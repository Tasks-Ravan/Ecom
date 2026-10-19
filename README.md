# E-Commerce API

A .NET 9 ASP.NET Core Minimal API using PostgreSQL, EF Core, ASP.NET Core Identity, JWT authentication, FluentValidation, Scalar API documentation, Brevo SMTP through MailKit, ClosedXML, and Hangfire.

## Architecture

The API uses vertical slices. Auth registration, login, and refresh token each live under `src/ECommerce.Api/Features/Auth/` with an endpoint, request/response records, validator, and handler. Handlers are plain DI services; there is no mediator. `IEndpoint` implementations are discovered from the API assembly and mapped automatically. Shared HTTP concerns live in `Common`, user and refresh-token entities in `Domain`, EF Core and its initial Identity migration in `Persistence`, and JWT issuance and identity seeding in `Infrastructure`.

Refresh tokens are cryptographically random, stored only as SHA-256 hashes, expire after seven days, and rotate on use. Access tokens include the user's roles. Product translations are stored as PostgreSQL `jsonb`. Product Excel export queries are no-tracking projections so they load only the export columns.

## Run with Docker

1. Copy `.env.example` to `.env` and set unique local values for `POSTGRES_PASSWORD`, `JWT_SECRET_KEY` (at least 32 bytes), `ADMIN_EMAIL`, and `ADMIN_PASSWORD` (at least 12 characters; uppercase, lowercase, digit, and symbol).
2. Run `docker compose up --build`.
3. Open [http://localhost:8080/scalar/v1](http://localhost:8080/scalar/v1). In Development the API applies EF Core migrations and seeds the Admin and Customer roles, configured Admin user, and sample catalog on startup.

### Connect with DataGrip

PostgreSQL is published to the host on `localhost:5432` by default (`POSTGRES_PORT` in `.env` can change the host port). In DataGrip, add a PostgreSQL data source and use:

- Host: `localhost`
- Port: `5432` (or your configured `POSTGRES_PORT`)
- Database: value of `POSTGRES_DB` in `.env`
- User: value of `POSTGRES_USER` in `.env`
- Password: value of `POSTGRES_PASSWORD` in `.env`
- SSL: disable for this local Docker connection

Click **Test Connection** and allow DataGrip to download its PostgreSQL driver if prompted. The PostgreSQL container must be running (`docker compose up -d postgres`).

Use Scalar's authentication control to enter a bearer access token. Register via `POST /api/auth/register`, log in via `POST /api/auth/login`, then use the returned access token as `Bearer <token>`.

### Email and product exports

For real delivery, create a Brevo SMTP key and verify the sender address in Brevo. The sender (`EMAIL__FROMEMAIL`) is a verified address and is separate from the SMTP login (`EMAIL__USERNAME`). Copy the Brevo values in `.env.example` to `.env`, replace `EMAIL__PASSWORD` with your SMTP key, and set `EMAIL__FROMEMAIL` to the verified sender. SMTP uses STARTTLS on port 587 by default. Never commit `.env` or the SMTP key.

If either `EMAIL__HOST` or `EMAIL__PASSWORD` is empty, the API selects `LoggingEmailSender`: no email is sent, and the log records recipient, subject, and attachment name/size. The export job completes successfully in this mode, which is useful for local development. Registration sends a welcome email after account creation; an SMTP failure is logged and does not undo registration.

The API generates `.xlsx` workbooks with SKU, Azerbaijani/English/Russian names, category, brand, price, stock, active state, and creation time. A Hangfire worker stores its state in the same PostgreSQL database in the separate `hangfire` schema. A recurring export email is scheduled using `PRODUCT_EXPORT_CRON` (default `0 8 * * *`, evaluated in Hangfire's configured/default time zone). Failed jobs retry up to three times.

Log in as the seeded Admin and authorize Scalar with its JWT to use:

- `GET /api/products/export/download` — downloads `products.xlsx`.
- `POST /api/products/export/send-mail` — enqueue an export email, for example `{"recipientEmail":"ops@example.com"}`. Returns `202 Accepted` and a Hangfire job link.
- `/hangfire` — Hangfire dashboard; requires an authenticated Admin bearer token.

The scheduled recipient is `PRODUCT_EXPORT_RECIPIENT`. On-demand recipients are validated as email addresses.

## Frontend

The React 18 + TypeScript storefront and Admin panel live in `frontend/`. The UI uses feature-based API clients and pages, React Router, TanStack Query/Table, React Hook Form + Zod, three persisted languages (az/en/ru), and a persisted light/dark theme. Public catalog browsing and customer registration are available alongside the Admin catalog tools.

For local development, start the API on `http://localhost:8080`, then run:

```sh
cd frontend
copy .env.example .env
npm install
npm run dev
```

Vite serves the frontend on port 5173 and proxies `/api`, `/scalar`, `/openapi`, and `/hangfire` to `VITE_DEV_PROXY_TARGET` (default `http://localhost:8080`). Set `VITE_API_BASE_URL` to `/api` when using the proxy. For Docker, set `FRONTEND_PORT` (default `3010`) in the root `.env`, then run `docker compose up --build`; open `http://localhost:3010`.

Frontend checks:

```sh
cd frontend
npm run build
npm run lint
npm test
```

Admin features include catalog statistics, product CRUD with az/en/ru translations, category hierarchy management, brands, Excel download, and queued export mail. Admin credentials are seeded from `ADMIN_EMAIL` and `ADMIN_PASSWORD` in the root `.env`.

### Cart and sandbox checkout

The storefront includes a persistent cart and authenticated checkout. Open a product, add it to the cart, sign in as a customer, and continue to checkout. Product prices, active status, and stock are re-read and locked in PostgreSQL by the API before an order is created. An approved sandbox order reduces stock; a declined sandbox attempt records a declined order without changing stock. The UI never asks for card details and does not contact a payment provider or collect real money. The sandbox is available only when the API runs in Development and `PAYMENTS__SANDBOX__ENABLED=true`; keep it disabled in every production environment.

- `POST /api/checkout` — authenticated sandbox checkout with product IDs, quantities, language, and the explicit `simulateDecline` test switch.
- `GET /api/orders/{id}` — authenticated owner-only order details and saved price snapshots.
- Frontend: product detail **Add to cart** → **Cart** → **Checkout**. Choose the approved or declined simulation to verify both outcomes.

New databases apply the `20261019000000_Orders` migration automatically on startup in Development. Existing databases receive the same migration before the API starts serving requests.

## Catalog endpoints

Public reads are available at `GET /api/categories`, `GET /api/categories/{id}`, `GET /api/brands`, `GET /api/brands/{id}`, `GET /api/products`, and `GET /api/products/{id}`. Admin-role users can create, update, and delete categories, brands, and products with POST/PUT/DELETE on those paths. Product listing supports `search`, `categoryId`, `brandId`, `minPrice`, `maxPrice`, `page`, `pageSize`, and `sort=price_asc|price_desc|created_desc`. Product reads use `?lang=az` (or `en`/`ru`) first, then the `Accept-Language` header, and fall back to English or the first stored translation. Translations live in one indexed `jsonb` field.

Example:

```http
GET /api/products?lang=az&search=telefon&minPrice=100&maxPrice=900&sort=price_desc&page=1&pageSize=20
```

### Seed data

When `Seed:Enabled` is true, startup seeds a 12-node hierarchical category catalog, 10 brands, 24 multilingual products, and `customer@example.com`. Compose enables seed data by default; set `SEED_ENABLED=false` to disable it and set `SEED_CUSTOMER_PASSWORD` to configure the sample customer's password. Development defaults seeding on and Production defaults it off when `Seed:Enabled` is not configured. Catalog seeding skips if any category, brand, or product already exists.

## Configuration

Configuration uses the standard ASP.NET Core environment variable mapping (`__` for nested keys). Docker Compose supplies these values from `.env`:

| Variable | Purpose |
| --- | --- |
| `POSTGRES_DB`, `POSTGRES_USER`, `POSTGRES_PASSWORD` | PostgreSQL database and credentials |
| `POSTGRES_PORT` | Host port mapped to PostgreSQL container port 5432; defaults to `5432` |
| `JWT_ISSUER`, `JWT_AUDIENCE`, `JWT_SECRET_KEY`, `JWT_EXPIRATION_MINUTES` | JWT validation and issuance; use a random secret of at least 32 bytes |
| `ADMIN_EMAIL`, `ADMIN_PASSWORD`, `ADMIN_FIRST_NAME`, `ADMIN_LAST_NAME` | Startup-seeded administrator |
| `API_PORT` | Host port mapped to the API container's port 8080 |
| `EMAIL__HOST`, `EMAIL__PORT` | SMTP server and port; Brevo uses `smtp-relay.brevo.com:587` |
| `EMAIL__USERNAME`, `EMAIL__PASSWORD` | Brevo SMTP login and SMTP key (password) |
| `EMAIL__FROMEMAIL`, `EMAIL__FROMNAME` | Verified Brevo sender email and display name; sender is not the SMTP login |
| `EMAIL__USESTARTTLS` | Use STARTTLS; defaults to `true` |
| `PRODUCT_EXPORT_RECIPIENT` | Recipient for the recurring product export email |
| `PRODUCT_EXPORT_CRON` | Hangfire cron expression; defaults to daily at 08:00 |
| `PAYMENTS__SANDBOX__ENABLED` | Enables simulated checkout in Development only; set `true` for local/test use and `false` in production |

Outside Docker, set `ConnectionStrings__DefaultConnection`, `Jwt__Issuer`, `Jwt__Audience`, `Jwt__SecretKey`, `Jwt__ExpirationMinutes`, and `Admin__Email`/`Admin__Password`/`Admin__FirstName`/`Admin__LastName` in the process environment. No secrets are committed in appsettings.

## Build

With the .NET 9 SDK installed, run `dotnet build ECommerce.sln` and `dotnet test ECommerce.sln`. The tests cover catalog validation, Excel workbook contents and formatting, and the background job's email attachment. Docker Compose builds and starts the API and PostgreSQL together.
