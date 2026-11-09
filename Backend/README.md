# Backend

This .NET 8 Web API handles requests in `Controllers`, program logic in `Services`, and MSSQL access through EF Core and `ApplicationDbContext`.

## Local configuration

Copy `Backend/secrets.example.json` to `Backend/secrets.json`, then replace every placeholder with your MSSQL and InBody API settings. `secrets.json` is ignored by Git. Use the API URLs for your InBody account and region.

In a hosted environment, provide the same settings through environment variables such as `ConnectionStrings__DefaultConnection`, `InBody__GetUserUrl`, `InBody__GetDatetimesByIdUrl`, `InBody__GetInBodyDataByIdUrl`, `InBody__ApiKey`, and `InBody__Account`. Do not publish a populated `secrets.json`.

Set `InBody:QuotaTimeZone` to the provider's actual daily reset time zone before production use. The example uses `UTC` only as a placeholder. `DailyCallLimit` defaults to 500. The quota counts attempted member-registration calls before sending them, including failed requests. Webhook measurement lookups are excluded from this counter according to the stated InBody limit. All instances sharing an account must use the same database and quota settings. Calls made by other applications using that account are outside this counter.

## Database

Use an existing local `InBodyConsult` MSSQL database or run `Database/001_InitialSchema.sql` once in a new environment. If all member-related tables are empty, `Database/002_UseInBodyStringMemberId.sql` can switch member IDs to InBody's string `iD`. If any member, measurement, or program association exists, map existing IDs before migrating. Program data is retained. Run `Database/003_ExpandEquipSerial.sql` when upgrading a schema whose device serial column is 32 characters long. Run `Database/004_ExpandUserToken.sql` on existing databases before accepting international phone numbers. Run `Database/005_InBodyApiUsage.sql` before deploying the quota-aware API clients. EF Core does not change the schema automatically.

Initial member synchronization fetches the latest measurement and the closest earlier qualifying measurement at least 30 days apart. It fetches additional dates only when a candidate is incomplete. Existing stored measurements are reused for later program additions. If the member-registration quota is exhausted, member addition returns `429`. Webhook lookups continue without using this counter.

## Run and inspect

```sh
dotnet run --project Backend/Backend.csproj
```

In Development, `/swagger` displays the API. `GET /health` checks the server; `GET /health/database` checks the MSSQL connection.

## Register the InBody webhook URL

Deploy the backend at a public HTTPS origin, then register this complete URL in the InBody webhook configuration:

```text
https://your-backend.example.com/webhook/inbody/measurements
```

Replace `your-backend.example.com` with the domain of the deployed **backend**, not the frontend. The path is fixed in the app; it is not a value to put in `secrets.json` or `Frontend/.env`. Make sure any reverse proxy forwards `POST /webhook/inbody/measurements` and its JSON body to the backend. The older `/webhooks/inbody/measurements` path also works, but use the singular `/webhook` path for new registrations. A `localhost` URL cannot receive InBody's remote notifications.

To verify that the public route reaches the backend without triggering an InBody lookup, send an empty JSON object and expect `400 INVALID_WEBHOOK_PAYLOAD`:

```sh
curl -i -X POST https://your-backend.example.com/webhook/inbody/measurements \
  -H 'Content-Type: application/json' -d '{}'
```

The webhook currently has no signature or shared-secret check. Restrict who can call it at the deployment edge if InBody provides an allowlist or authentication mechanism. A successful notification returns `202`; an unknown member also returns `202`.

## Implemented endpoints

- `GET /api/products`
- `GET /api/products/{productId}`
- `GET /api/products/{productId}/members?page=1&size=20`
- `POST /api/products`
- `POST /api/products/{productId}/members`
- `DELETE /api/products/{productId}/members/{memberId}`
- `POST /api/products/{productId}/similar-members/search`
- `POST /webhook/inbody/measurements` (also accepts `/webhooks/inbody/measurements`)

Program statistics are computed from complete measurement pairs at least 30 days apart. The frontend displays these averages and uses them for consultation forecasts.
