# Deploying the News Portal Demo

This repo is container-ready for [Render](https://render.com)'s free web service tier. Render was
chosen over Azure App Service's Free (F1) tier (60 CPU-minutes/day — exhausted almost immediately
under real use) and over Fly.io (no perpetual free tier anymore, only a 2-hour/7-day trial).

**What changed to make this deployable**, all verified locally before being written here:

- Added `Umbraco.Cms.Persistence.Sqlite` and `appsettings.Production.json`, so the app uses a
  file-based SQLite database in Production instead of SQL Server LocalDB (which only exists on
  Windows). Verified end-to-end against a real `dotnet publish` output: fresh install, full content
  seeding, Delivery API, and the contact POST endpoint all confirmed working on SQLite.
- `Dockerfile` — multi-stage build (.NET 10 SDK → ASP.NET 10 runtime), reads `$PORT` at container
  *start* (not baked in at build time) so it matches whatever the host injects, falling back to
  Render's own default of `10000`.
- `render.yaml` — a Render Blueprint so the service, Docker build, and required secret are defined
  in one file instead of manual dashboard clicking.
- `Program.cs` — CORS origins can now be extended via `AngularApp:AdditionalOrigins` config instead
  of being hardcoded to `localhost:4200`.

**What this means for the deployed demo, by design, not by accident**: Render's free tier has no
persistent disk — local files are wiped on every redeploy, restart, *and* spin-down (which happens
after 15 minutes with no traffic). This app's content architecture is entirely idempotent
code-first composers (research.md §1), so every cold start just reinstalls and reseeds itself from
nothing in under a minute. That's a real, visible characteristic of the free deployment, not a bug
— the first request after 15 minutes of inactivity will be slow (~1 extra minute) while it reseeds,
and any contact-form submissions made since the last spin-down won't survive it either.

## Step by step

1. **Push this branch** so Render can see `Dockerfile` and `render.yaml` (already done if you're
   reading this from the committed repo).

2. **Create a Render account** at [dashboard.render.com](https://dashboard.render.com) — sign in
   with GitHub is fastest, since the next step needs repo access anyway.

3. **New → Blueprint**, and point it at this GitHub repo. Render will read `render.yaml`
   automatically and show one service: `news-portal-demo`.

4. **Set the required secret** when prompted: `Umbraco__CMS__Unattended__UnattendedUserPassword`.
   Pick your own value, 10+ characters (Umbraco 18's minimum) — **do not reuse** the password
   committed in `appsettings.json`, since that one is in git history and this deployment will be
   publicly reachable. Optionally also set
   `Umbraco__CMS__Unattended__UnattendedUserEmail` to your own address for the backoffice admin
   login.

5. **Click Apply.** Render builds the Docker image and deploys it. First build takes a few minutes
   (restoring/publishing a full Umbraco solution); watch the build log in the Render dashboard.

6. **Get the URL** from the service's page in the Render dashboard — it'll be
   `https://news-portal-demo-<random>.onrender.com` (Render assigns the random suffix; a fixed
   `.onrender.com` subdomain costs nothing extra to claim from the dashboard if you want a cleaner
   one). Open it — the first load will be slow (cold start + full reseed), everything after that
   until the next 15-minute idle spin-down will be fast.

7. **Verify it**, the same way this was verified locally all session: hit
   `/umbraco/delivery/api/v2/content?filter=contentType:newsArticle`
   (should return the 8 seeded articles) and try the contact form POST from
   [docs/requirements/05-api-reference.md](docs/requirements/05-api-reference.md) or the
   [Postman collection](specs/001-news-portal-demo/postman_collection.json), pointed at the new
   URL instead of `localhost`.

## If a frontend needs to call this from a deployed origin

Set an additional Render environment variable (not in `render.yaml`, since it's deployment-specific
and optional):

```
AngularApp__AdditionalOrigins__0 = https://your-frontend.example.com
```

`localhost:4200` stays allowed automatically for local frontend development against the deployed
API.

## Known limitations of this specific deployment

- **No persistent storage** — covered above; expected behavior, not a defect.
- **No custom domain on the free plan** without upgrading.
- **Cold starts** after 15 minutes idle (~1 extra minute, full reseed).
- **750 free instance-hours/month** per Render workspace; if other free services share the
  workspace, they draw from the same pool.
