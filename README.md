# Really Not Instant

A "slow messenger": a web app where messages do **not** arrive instantly. Instead, a message
"travels" from the sender to the recipient at walking pace, and the recipient can see that
something is on its way but cannot read it until it has "arrived".

This is deliberately not a clone of an existing messenger — every part of the delivery model is a design decision made for this app.

This project is not being created agentically as it's being used as a learning project. AI has been used for generating this README though and sometimes as a search engine.

## How it works

- The sender and recipient are currently two fixed users:
  - **Rose Tyler** — at Land's End, Cornwall (≈ 50.06°N, 5.72°W)
  - **Donna Noble** — at John O'Groats, Scotland (≈ 58.64°N, 3.07°W)
- When a message is sent, the backend asks the OpenRouteService foot-walking matrix API for the
  walking duration between the two locations and stores that as the message's planned delivery
  time (`SentAtUtc + walkingDuration`).
- The message is persisted to PostgreSQL and a status is returned. With roughly 1,400 km between
  the two endpoints, a message takes on the order of two weeks to "arrive" — that is the point.
- The frontend lets a user compose and send a message and shows its current status, retrying on
  failure.

## Tech stack

### Backend (`ReallyNotInstantService/`)
- **.NET 10** / **ASP.NET Core** minimal APIs (C#)
- **NHibernate** for ORM / data access
- **FluentMigrator** for database migrations
- **Npgsql** + **PostgreSQL** for persistence
- **OpenRouteService** (foot-walking matrix API) for delivery-time calculation
- **Docker** (multi-stage Dockerfile) and **docker-compose** (Postgres + pgAdmin)
- **Render** for hosting (see `render.yaml`), region Frankfurt, free tier

### Frontend (`ReallyNotInstantFrontend/`)
- Plain **HTML5 / CSS3 / JavaScript** (ES modules, no framework)
- **Google Fonts** (Roboto, Source Code Pro)
- `fetch` against the backend API with CORS

### Tooling
- Visual Studio solution (`ReallyNotInstantService.slnx`)
- REST client file (`ReallyNotInstantService.http`)
- Environment-based configuration (`.env`, `appsettings*.json`)

## Project structure

```
really-not-instant/
├── README.md
├── ReallyNotInstantFrontend/     # Static frontend
│   ├── index.html
│   ├── script.js
│   ├── styles.css
│   └── config.js                 # Local vs. production API base URL
└── ReallyNotInstantService/      # .NET backend
    ├── docker-compose.yml
    ├── render.yaml
    └── ReallyNotInstantService/
        ├── Program.cs
        ├── Models/                # Message, User, Location, MatrixResponse
        ├── Endpoints/             # SendMessage, GetMessageStatus
        ├── DeliveryCalculation/   # DeliveryTimeCalculator, DistanceApiService
        ├── Data/                  # IPersistence, Persistence, Migrations
        ├── Mappings/              # NHibernate mappings
        └── Dockerfile
```

## Current features

- Compose and send a text message from the browser.
- Delivery time calculated once at send time from a real walking route (OpenRouteService).
- Message persisted to PostgreSQL (`Messages` table created via FluentMigrator migration).
- Minimal API endpoints:
  - `PUT /sendmessage` — accepts a message body string, returns the created `Message`.
  - `POST /getmessagestatus` — accepts a message id (GUID), returns its status string.
- Defined message status model: `Sent`, `InTransit`, `WaitingForDelivery`, `Delivered`, `Read`, `Failed`.
- Frontend shows `Sending…` → `Sent`, or a retry button on failure.
- Local vs. production API base URL switching (`config.js`).
- CORS policy restricted to a configured allowed origin.
- Containerised backend + Render deployment blueprint with managed PostgreSQL.

## Running locally

### Backend

Prerequisites: .NET 10 SDK, Docker.

1. Start Postgres (and optionally pgAdmin):
   ```sh
   cd ReallyNotInstantService
   docker compose up -d reallynotinstantservice_db
   ```
2. Create a `.env` file in `ReallyNotInstantService/` (see the existing one for shape) providing
   `POSTGRES_CONNECTION_STRING`, `OPEN_ROUTE_SERVICE_KEY`, `ALLOWED_ORIGIN` (e.g.
   `http://127.0.0.1:5500`), and `ASPNETCORE_ENVIRONMENT=Development`.
3. Run the API:
   ```sh
   cd ReallyNotInstantService/ReallyNotInstantService
   dotnet run
   ```
   The API listens on `http://localhost:8080` by default (see `launchSettings.json`).

### Frontend

Serve `ReallyNotInstantFrontend/` with any static server (e.g. VS Code Live Server on port 5500)
so that `ALLOWED_ORIGIN` matches. Open `index.html` in the browser.

## Deployment

- Backend: Docker image built from `ReallyNotInstantService/Dockerfile`, deployed to Render
  (`render.yaml`), with a managed Postgres database in Frankfurt.
- Frontend: static files, intended to be hosted separately (Vercel/Netlify/GitHub Pages). Set
  `API_URL` in `config.js` to the live backend URL.

## Roadmap / planned

The challenge is structured as steps. Steps already covered: design decisions, minimal API,
delivery-time calculation, persistence, an initial static frontend, and deployment. The
following are planned next:

- **Recipient experience** — show incoming messages and reveal a message only once it has
  arrived; messages wait if the recipient is away.
- **Status progression over time** — move messages through `InTransit` → `WaitingForDelivery` →
  `Delivered`, with polling (and later push) for arrival.
- **Make waiting visible** (Step 6) — progress percentage, ETA, and/or position along the route.
- **Live updates** (Step 7, stretch) — replace polling with SignalR push.
- **Make it feel alive** (Step 9) — multiple courier types with different speed/reliability,
  external factors (time of day, simulated weather, random delays), a conversation history/journal
  of pacing, and notifications on arrival.
- **Second implementation** (Step 8, stretch) — reimplement the delivery calculation as a
  serverless function and compare cold starts/statelessness/cost.
- **Configurable users & locations** — replace the two hardcoded users with real accounts,
  user-entered addresses or browser geolocation, instead of fixed coordinates.
- **Frontend polish** — incoming-message view, read receipts, and richer in-transit UI.

## Challenge step status

| Step | Description | Status |
|------|-------------|--------|
| 0 | Design before code | Decisions recorded in code (walking pace, single courier, waits for recipient) |
| 1 | Minimal API, no persistence | Done (superseded by persistence) |
| 2 | Delivery time calculation | Done (OpenRouteService foot-walking) |
| 3 | Persistence | Done (PostgreSQL + NHibernate + FluentMigrator) |
| 4 | Static HTML frontend | Partial (send + status; incoming/reveal + polling pending) |
| 5 | Deploy | Partial (Render blueprint + Docker) |
| 6 | Make waiting visible | Planned |
| 7 | Live updates (SignalR) | Planned |
| 8 | Serverless reimplementation | Planned |
| 9 | Make it feel alive | Planned |

## Notes

- Delivery time is computed **once at send time** and stored, so showing progress later is just
  arithmetic on the frontend. This was an early design decision; switching to continuous
  recalculation would be a much larger change.
- The message content is sent as a JSON string body (`PUT /sendmessage`), not a JSON object.
- `ALLOWED_ORIGIN` must be provided (env var or config) or the app throws on startup.