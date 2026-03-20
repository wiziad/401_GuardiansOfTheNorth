# Frontend Integration

This document explains how the Unity client should communicate with the backend in `backend/`.

The backend is an HTTP JSON API deployed separately from the Unity game. The Unity project is the client, and the backend is the server.

## Communication Model

```text
Unity Client
  -> HTTPS requests
Backend API on Render
  -> Prisma queries
Render PostgreSQL
```

## Base URL

Use one base URL and append the route path.

- local development: `http://localhost:3000`
- deployed backend: `https://four01-guardiansofthenorth.onrender.com`

Examples:

- `https://four01-guardiansofthenorth.onrender.com/api/auth/login`
- `https://four01-guardiansofthenorth.onrender.com/api/saves`

## Auth Flow

Recommended login flow:

1. Player registers once with `POST /api/auth/register`
2. Player logs in with `POST /api/auth/login`
3. Backend returns a `token`
4. Unity stores that token in memory, or in a local secure place if needed
5. Unity includes the token in the `Authorization` header for protected routes

Header format:

```text
Authorization: Bearer <token>
```

Protected routes:

- `GET /api/auth/me`
- `GET /api/saves`
- `POST /api/saves`
- `GET /api/saves/:id`
- `PUT /api/saves/:id`

## Response Contract

### Success

```json
{
  "success": true,
  "data": {}
}
```

### Error

```json
{
  "success": false,
  "message": "Something went wrong"
}
```

Frontend should always check:

- `request.result` in Unity
- HTTP status code
- `success` field in the JSON body

## API Endpoints

### Register

```http
POST /api/auth/register
Content-Type: application/json
```

Request body:

```json
{
  "email": "player@example.com",
  "username": "player1",
  "password": "secret123"
}
```

Typical response:

```json
{
  "success": true,
  "data": {
    "token": "jwt-token",
    "user": {
      "id": "user-id",
      "email": "player@example.com",
      "username": "player1",
      "createdAt": "2026-03-17T00:00:00.000Z"
    }
  }
}
```

### Login

```http
POST /api/auth/login
Content-Type: application/json
```

Request body:

```json
{
  "email": "player@example.com",
  "password": "secret123"
}
```

### Get Current User

```http
GET /api/auth/me
Authorization: Bearer <token>
```

Use this to verify an existing token and fetch the logged-in player's profile.

### Get All Saves

```http
GET /api/saves
Authorization: Bearer <token>
```

Typical response:

```json
{
  "success": true,
  "data": [
    {
      "id": "save-id",
      "userId": "user-id",
      "slotName": "Slot 1",
      "mapId": "starter-map",
      "playerX": 0,
      "playerY": 0,
      "hp": 100,
      "mana": 50,
      "level": 1,
      "createdAt": "2026-03-17T00:00:00.000Z",
      "updatedAt": "2026-03-17T00:00:00.000Z"
    }
  ]
}
```

### Create A Save

```http
POST /api/saves
Authorization: Bearer <token>
Content-Type: application/json
```

Request body:

```json
{
  "slotName": "Slot 1",
  "mapId": "starter-map",
  "playerX": 0,
  "playerY": 0,
  "hp": 100,
  "mana": 50,
  "level": 1
}
```

Notes:

- each user currently has a max of 3 save slots
- use this when a player starts a new game or creates a new save file

### Get One Save

```http
GET /api/saves/:id
Authorization: Bearer <token>
```

Use this when the player selects one specific save slot.

### Update A Save

```http
PUT /api/saves/:id
Authorization: Bearer <token>
Content-Type: application/json
```

Request body can be partial:

```json
{
  "playerX": 24.5,
  "playerY": 9.75,
  "hp": 90
}
```

Use this when:

- the player manually saves
- the game autosaves
- the player changes map or key stats

### Health Check

```http
GET /api/health
```

Use this for:

- checking whether the backend is up
- checking whether the database is reachable
- debugging deployment problems

## Unity Integration Notes

Recommended Unity-side structure:

```text
Scripts/
  Network/
    ApiClient.cs
    AuthService.cs
    SaveService.cs
    Models/
```

Responsibilities:

- `ApiClient.cs`: low-level HTTP requests
- `AuthService.cs`: register, login, token management
- `SaveService.cs`: load saves, create saves, update saves
- `Models/`: request and response DTOs

## Unity Example

```csharp
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class ApiClient : MonoBehaviour
{
    private const string BaseUrl = "https://four01-guardiansofthenorth.onrender.com";
    private string authToken;

    public void SetToken(string token)
    {
        authToken = token;
    }

    public IEnumerator Login(string email, string password)
    {
        var json = JsonUtility.ToJson(new LoginRequest
        {
            email = email,
            password = password
        });

        using var request = new UnityWebRequest(BaseUrl + "/api/auth/login", "POST");
        var body = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log(request.downloadHandler.text);
        }
        else
        {
            Debug.LogError(request.error);
        }
    }

    public IEnumerator GetSaves()
    {
        using var request = UnityWebRequest.Get(BaseUrl + "/api/saves");
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Authorization", "Bearer " + authToken);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log(request.downloadHandler.text);
        }
        else
        {
            Debug.LogError(request.error);
        }
    }
}

[System.Serializable]
public class LoginRequest
{
    public string email;
    public string password;
}
```

## Frontend Error Handling Advice

Unity should handle these cases cleanly:

- `400`: invalid request body
- `401`: invalid login or invalid token
- `404`: save slot not found
- `409`: email or username already exists
- `500`: unexpected server problem

Suggested UI behavior:

- show a user-friendly message
- do not crash the game scene
- keep raw backend error text in debug logs

## Recommended Client Flow

At game launch:

1. Player logs in
2. Unity stores token
3. Unity calls `GET /api/saves`
4. Player chooses a save
5. Unity loads that save into game state

During gameplay:

1. Unity tracks player stats and position locally
2. On save or autosave, Unity sends `PUT /api/saves/:id`
3. Backend persists the latest data in PostgreSQL

When starting a brand new game:

1. Unity calls `POST /api/saves`
2. Backend creates a new save slot
3. Unity stores the returned save id locally for later updates

## Deployment Reminder

The backend is deployed on Render at:

**`https://four01-guardiansofthenorth.onrender.com`**

Auto-deploy is enabled — any push to the `main` branch in `backend/` triggers a redeploy.

To avoid unnecessary rebuilds from non-backend changes, set **Build Filters → Included Paths** to `backend` in the Render service settings.

If you want to keep integration docs outside of `backend/` so they never touch the deploy pipeline, move them to the root-level `docs/` folder instead.
