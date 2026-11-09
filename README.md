# InBody Consult

InBody Consult is a sample app for exploring body composition changes across training programs. It shows program averages, compares initial and follow-up measurements, and prepares a consultation forecast from the selected program and a client's current measurements.

## Project structure

- `Frontend/`: Vue and Vite app
- `Backend/`: .NET 8 Web API and EF Core data access
- `API.md`: frontend and backend API contract
- `DESIGN.md`: data flow and calculation rules

## Run locally

1. Copy `Backend/secrets.example.json` to the ignored `Backend/secrets.json` file, fill in your settings, and follow [Backend/README.md](Backend/README.md) for database setup.
2. Run `dotnet run --project Backend/Backend.csproj`.
3. Run `npm ci --prefix Frontend`, then `npm run dev --prefix Frontend`. Copy `Frontend/.env.example` to `Frontend/.env` only if the browser must call an API at another origin.

The frontend uses a Vite development proxy for `/api` requests. Production deployments need to route `/api` to the backend.

## Sample data and privacy

The frontend mock files contain fictional names and measurements. Real member data may be returned by the backend when it is connected to an InBody account. Do not publish a connected backend or database with real member records as a public sample.

Keep `Backend/secrets.json` and local `.env` files out of source control. Both are ignored by Git. Variables prefixed with `VITE_` are included in the browser bundle and must not contain secrets.
