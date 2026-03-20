# 后端 Demo 讲解稿

这份文档是给 TA 或组员讲解时用的中文版本，主要说明我为这个项目完成了哪些后端工作、为什么这样设计，以及现在项目已经到了什么状态。

## 1. 我负责的部分

我这次主要负责的是这个 Unity 项目的后端和数据库部分，目标是让前端之后可以直接连后端接口完成：

- 注册
- 登录
- 创建存档
- 读取存档
- 更新存档

因为这个项目当前前端是 Unity 客户端，不是传统网页，所以我把系统设计成：

```text
Unity 客户端
  -> HTTP/JSON 请求
Next.js 后端
  -> Prisma
Render PostgreSQL
```

也就是说，前端只需要请求我部署好的后端 API，不需要直接接数据库。

## 2. 我做了什么

### 2.1 创建了独立后端

我在项目里新建了一个 `backend/` 文件夹，做成一个独立的后端服务。技术栈是：

- Next.js
- Prisma
- PostgreSQL
- JWT
- Zod
- Vitest

这样做的原因是：

- 部署到 Render 比较方便
- API 结构清楚
- 后续加接口容易
- Prisma 连 PostgreSQL 很适合做存档系统

### 2.2 设计并实现了 API

目前已经完成的接口有：

- `POST /api/auth/register`
- `POST /api/auth/login`
- `GET /api/auth/me`
- `GET /api/saves`
- `POST /api/saves`
- `GET /api/saves/:id`
- `PUT /api/saves/:id`
- `GET /api/health`

这些接口已经足够支持第一阶段的核心功能：

- 用户注册和登录
- 用 token 识别当前用户
- 每个用户管理自己的存档

### 2.3 做了鉴权

我用了 Bearer Token，也就是前端登录成功后会拿到一个 `token`，之后每次请求受保护接口时都带上：

```text
Authorization: Bearer <token>
```

这样前端不用直接传用户 id，后端也能知道“是谁在请求”。

### 2.4 设计并建立了数据库表

我不只是写了代码，也已经真的把数据库表建到了 Render PostgreSQL 里。

现在主要表有：

- `users`
- `save_slots`
- `_prisma_migrations`

其中：

`users` 负责存：

- 用户 id
- 邮箱
- 用户名
- 加密后的密码
- 创建时间

`save_slots` 负责存：

- 属于哪个用户
- 存档名
- 地图 id
- 玩家坐标
- hp
- mana
- level
- 创建和更新时间

当前还做了一个限制：

- 每个用户最多 3 个存档

### 2.5 加了 Render 部署配置

我加了根目录的 `render.yaml`，让 Render 可以只部署 `backend/` 这个服务，不会去部署整个 Unity 项目。

也就是说：

- `render.yaml` 在仓库根目录
- 但真正部署的是 `backend/`

### 2.6 加了数据库保活

因为 Render 数据库可能会有空闲问题，所以我加了一个 keep-alive 机制：

- 后端启动时会 ping 一次数据库
- 默认每 30 分钟再执行一次 `SELECT 1`

这样可以从应用侧尽量保持数据库连接活跃。

### 2.7 加了数据库备份/恢复脚本

为了防止数据丢失，我还写了脚本：

- `npm run db:list`
- `npm run db:backup`
- `npm run db:restore -- ./backups/xxx.dump`
- `npm run db:restore -- ./backups/xxx.sql`

这些脚本的作用是：

- 查看当前数据库里的表
- 备份数据库
- 用备份恢复数据库

## 3. 我怎么保证它不是只写了代码

这个部分很重要，因为我不只是把文件搭起来，我还实际做了验证。

### 3.1 数据库真实落表

我已经实际连接到 Render PostgreSQL，并执行了 Prisma 的同步，所以真实数据库里已经有表了，不是只有本地 schema。

### 3.2 测试已经跑过

我加了完整的测试系统，`npm test` 现在会跑：

- validator 测试
- auth 路由测试
- saves 路由测试
- 真实数据库 integration test

特别是最后一个，不是 mock，而是真的连 Render PostgreSQL 做：

- 插入用户
- 插入存档
- 查询数据
- 更新数据
- 清理测试数据

所以这说明：

- 数据库连得上
- 表结构是对的
- CRUD 真能跑

### 3.3 编译和部署路径也验证过

我还跑过：

- `npm run build`

这说明这个后端不是只适合开发环境，生产构建也是能通过的。

## 4. 前端之后怎么接

前端是 Unity，所以他们接入方式很简单：

1. 调登录接口
2. 拿到 token
3. 把 token 存起来
4. 之后请求存档接口时带 `Authorization`

例如：

- 登录：`POST /api/auth/login`
- 读所有存档：`GET /api/saves`
- 新建存档：`POST /api/saves`
- 更新存档：`PUT /api/saves/:id`

所以我这边的设计是：

- 前端只管发请求
- 后端负责鉴权和业务逻辑
- 数据库存储长期数据

## 5. 为什么我这样设计

这个项目现在还在早期阶段，所以我没有做很复杂的微服务，而是做了一个“单体但分层清楚”的后端。

这样做的优点是：

- 开发成本低
- 部署简单
- 对课程项目很够用
- 后面加排行榜、任务、背包都容易扩展

也就是说，这套结构既够简单，又不是乱写。

## 6. 如果 TA 问“你具体完成了哪些文件”

可以重点提这些：

- `backend/app/api/...`
- `backend/lib/...`
- `backend/prisma/schema.prisma`
- `backend/tests/...`
- `backend/scripts/...`
- `backend/README.md`
- `backend/FRONTEND_INTEGRATION.md`
- `render.yaml`

## 7. 如果 TA 问“现在项目到什么程度了”

你可以直接说：

目前后端已经完成了第一阶段可用版本，包含：

- 基础账号系统
- 基础存档系统
- 数据库真实建表
- 自动化测试
- 真实数据库集成测试
- Render 部署配置
- 数据库备份恢复脚本

也就是说，前端现在已经可以开始对接，不需要等后端再补基础设施。

## 8. 一个简短口头版本

如果你明天想讲得更口语一点，可以直接说：

“我这次主要把这个 Unity 项目的后端搭起来了。我新建了一个独立的 backend，用 Next.js + Prisma + Render PostgreSQL 做了注册、登录、读存档、写存档这些接口。数据库表我已经真实建到 Render 上了，不只是本地代码。我还加了测试，里面包括真实数据库的 integration test，所以能证明它不是纸面设计。另外我也补了 Render 部署配置、数据库保活，还有备份恢复脚本。前端后面只需要调我的 API，然后把 token 带上，就能直接联调。” 

## 9. 你明天可以重点强调的三句话

- 我不只是写了后端代码，也已经把真实数据库表建到 Render PostgreSQL 里了。
- 我不只是做了 mock test，还加了真实数据库 integration test，并且已经跑通。
- 前端现在只要按接口文档发请求，就可以直接和后端联调。
