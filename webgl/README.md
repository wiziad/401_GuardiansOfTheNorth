# WebGL deployment folder

This folder is intended to be the Vercel project root for the Unity WebGL build.

Put the Unity WebGL export files directly here:

- `index.html`
- `Build/`
- `TemplateData/`

Recommended Vercel project settings:

- Root Directory: `webgl`
- Framework Preset: `Other`
- Build Command: leave empty
- Output Directory: leave empty

After the files are committed and pushed to the connected Git repository, Vercel will
create a new deployment automatically.
