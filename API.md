# InBody Consult API

This document defines the frontend/backend contract. See [DESIGN.md](DESIGN.md) for data flow, database rules, and calculations.

## Conventions

- Standard endpoints use `/api`. The InBody webhook uses `/webhook`; `/webhooks` is also accepted for compatibility.
- Requests and responses use JSON, except a bodyless `204 No Content` response.
- Gender is `M` or `F`. Dates use `yyyy-MM-dd`; InBody timestamps use `yyyyMMddHHmmss`.
- Measurements and average changes are numbers. An unavailable average is `null`.
- `userToken` accepts common international phone formatting: an optional leading `+`, digits, spaces, parentheses, hyphens, and periods. The backend stores 7 to 15 digits without separators. Include a country code when it is part of the number registered with InBody. Member `id` and `memberId` use the string returned as `GetUser.iD`; program `id` is numeric.
- Measurement fields are `smm` (skeletal muscle mass, kg), `bfm` (body fat mass, kg), `weight` (kg), and `pbf` (body fat percentage, %). A change in `pbf` is expressed in percentage points.
- Error responses have `{ "code": "PRODUCT_NOT_FOUND", "message": "Program not found." }` shape.

## Endpoints

| Method | Path | Purpose |
| --- | --- | --- |
| `GET` | `/api/products` | List programs with overall statistics |
| `GET` | `/api/products/{productId}` | Program details with overall and gender statistics |
| `GET` | `/api/products/{productId}/members` | Paged member measurement pairs |
| `POST` | `/api/products` | Create a program |
| `POST` | `/api/products/{productId}/members` | Add an existing InBody member to a program |
| `DELETE` | `/api/products/{productId}/members/{memberId}` | Remove a member from a program |
| `POST` | `/api/products/{productId}/similar-members/search` | Find similar members |
| `POST` | `/webhook/inbody/measurements` | Receive a new InBody measurement |

## Program list

`GET /api/products` computes overall statistics from measurement history on each request. It does not return gender statistics or member records. An empty list returns `[]`. The forecast selector uses each program's `id` and `name`.

```json
[
  {
    "id": 4,
    "name": "8-week body composition program",
    "registeredAt": "2026-11-05",
    "memberCount": 12,
    "completedCount": 12,
    "monthlyAverageChange": { "smm": 1.33, "bfm": -0.8, "weight": -0.5, "pbf": -0.7 }
  }
]
```

`completedCount` counts members included in the average. When there are none, it is `0` and `monthlyAverageChange` is `null`.

## Program details

`GET /api/products/{productId}` returns program information and statistics for `overall`, `M`, and `F`. Each statistics group has `completedCount` and `monthlyAverageChange` with the same shape as the list response. A group with no complete members has count `0` and average `null`. Member records are returned by a separate endpoint. A missing program returns `404 PRODUCT_NOT_FOUND`.

## Member records

`GET /api/products/{productId}/members?page=1&size=20` returns `items`, `page`, `size`, `totalItems`, and `totalPages`. Page numbering starts at 1; size defaults to 20 and cannot exceed 100. Invalid pagination returns `400 INVALID_PAGINATION`; a missing program returns `404 PRODUCT_NOT_FOUND`.

Each item contains string `id`, `name`, `gender`, `startDate`, `endDate`, `before`, `after`, and `includedInAverage`. The `before` and `after` objects have `smm`, `bfm`, `weight`, and `pbf`. Missing dates and measurements are `null`. When the server finds a qualifying pair among stored measurements, it returns that pair and sets `includedInAverage` to `true`.

## Create a program

`POST /api/products` accepts `{ "name": "New program" }` and returns `201 Created` with numeric `id`, `name`, and `registeredAt`. A blank name returns `400 INVALID_PRODUCT_NAME`.

## Add an existing InBody member

`POST /api/products/{productId}/members` accepts `{ "userToken": "+1 (415) 555-0123" }`. It links an existing InBody member; it does not create a new InBody user. A successful `201 Created` response has the same member measurement fields as an item in the member list.

On first addition, the server stores identity details from `GetUser`, gets measurement dates from `GetDatetimesByID`, and fetches the latest and nearest qualifying earlier measurements from `GetInBodyDataByID`. It fetches more dates only when candidates are incomplete. Later program additions reuse stored measurements. The response prefers a pair that qualifies for statistics; otherwise it returns recent records. If there is only one measurement, it appears in both `before` and `after`. Measurements less than 30 days apart remain in the record but are excluded from program averages. Only phone number, name, and gender are stored from `GetUser`.

| Condition | Status and code |
| --- | --- |
| Invalid phone format | `400 INVALID_USER_TOKEN` |
| Program missing | `404 PRODUCT_NOT_FOUND` |
| InBody member missing | `404 INBODY_MEMBER_NOT_FOUND` |
| Already linked to this program | `409 MEMBER_ALREADY_REGISTERED` |
| Member ID conflicts with an existing phone association | `409 MEMBER_IDENTITY_CONFLICT` |
| InBody API failure | `502 INBODY_API_ERROR` |
| Daily InBody API quota exhausted | `429 INBODY_DAILY_QUOTA_EXCEEDED` |

## Remove a member

`DELETE /api/products/{productId}/members/{memberId}` removes only the program association; member identity and stored measurements remain. Success returns `204 No Content`. A missing program returns `404 PRODUCT_NOT_FOUND`; a missing association returns `404 PRODUCT_MEMBER_NOT_FOUND`.

## Similar-member search

`POST /api/products/{productId}/similar-members/search` accepts gender and current measurements:

```json
{
  "gender": "F",
  "current": { "smm": 22.8, "bfm": 21.4, "weight": 74.6, "pbf": 28.7 }
}
```

It returns up to three closest members of the same program and gender with complete measurement pairs. Results are ordered by similarity. Each result uses a response-local alias such as `Member 1` and an alias ID such as `similar-1`; neither field contains the stored name or member ID. An empty match set returns `[]`. Each result has `id`, `name`, `gender`, `startDate`, `endDate`, `months`, `before`, and `after`. Invalid gender or missing measurements return `400 INVALID_SEARCH_INPUT`; a missing program returns `404 PRODUCT_NOT_FOUND`.

## InBody measurement webhook

`POST /webhook/inbody/measurements` is called by InBody, not by the frontend. The legacy `/webhooks/inbody/measurements` path is also accepted. The request carries `EquipSerial`, `TelHP`, `UserID`, `TestDatetimes`, `Account`, `Equip`, `Type`, and `IsTempData`. Accepted requests return `202` with `{ "accepted": true }`.

Register `https://<your-public-backend-domain>/webhook/inbody/measurements` in the InBody webhook configuration. The frontend URL and `VITE_API_BASE_URL` do not determine the webhook URL. See [Backend/README.md](Backend/README.md) for deployment checks.

The server looks up the member by `TelHP`, fetches measurement data using `TestDatetimes`, stores it, and updates linked programs' latest measurement references. An unknown member also returns `202` to acknowledge webhook delivery. A duplicate member/time/device combination is not stored again. Statistics are recalculated on subsequent program reads. Invalid required fields return `400 INVALID_WEBHOOK_PAYLOAD`; an InBody fetch failure returns `502 INBODY_API_ERROR`.
Webhook measurement lookups do not use the member-registration daily quota counter.

## Frontend calls

The program list uses `GET /api/products`; details also load `/members`. Adding or removing members uses the corresponding program endpoint. The forecast form loads the program list, fetches gender statistics from program details, calculates projected values in the frontend, and calls the similar-member search endpoint.
