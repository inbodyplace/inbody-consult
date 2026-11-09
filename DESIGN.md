# InBody Consult design

This document describes the intended data flow and calculation rules. See [Backend/README.md](Backend/README.md) for the implemented endpoints and [API.md](API.md) for the frontend/backend contract.

## Data flow

1. **Add a member:** Validate the phone number and program, reject duplicate links, call `GetUser`, fetch the latest and nearest qualifying earlier InBody measurements on first registration, and create the program/member association. Fetch additional dates only when candidates are incomplete. Reuse stored measurements for later program additions. Update the member's name and gender even if the member already exists in another program. Database unique constraints resolve concurrent duplicate attempts.
2. **Receive a measurement:** Validate the webhook payload, look up the member by phone number, check for a duplicate member/time/device combination, call `GetInBodyDataByID`, store the measurement, and update the latest measurement references on linked program members in one transaction. A duplicate or unknown member is acknowledged with `202`; an InBody API failure returns `502`.
3. **Read and consult:** Program reads calculate statistics from stored measurement history. The consultation form fetches program statistics and similar members, then calculates projections on the frontend when enough complete records exist.

Statistics are not stored separately. New measurements affect the next program list and detail responses.

## InBody API integration

| API | When used |
| --- | --- |
| `GetUser` | Adding an existing member to a program |
| `GetDatetimesByID` | Initial synchronization or recovery of missing history |
| `GetInBodyDataByID` | Initial synchronization, history recovery, or webhook processing |

This service does not call `InsertUser` or `InsertUserByID`. Adding a member links an existing InBody user to a program.

The `GetUser` request sends `userToken`. Store the returned `userToken` or `phone` as the digits-only `Members.UserToken`, `iD` as the string primary key `Members.MemberId`, and the returned name and gender. Other identity fields are not needed. Use `GetUser.iD` as `userID` for measurement API calls. Phone, `MobileNumber`, `UserToken`, and `TelHP` refer to the same digits-only mobile number.

Store `DATETIMES` as `MeasuredAt`, `SMM` as `Smm`, `BFM` as `Bfm`, `WT` as `Weight`, and `PBF` as `Pbf`. Initial sync stores selected measurement candidates rather than the full history. Program member references point to the latest two stored measurements, or the same measurement in both positions when only one exists. The 30-day and completeness rules apply when computing statistics.

## Program statistics

The backend calculates overall, male, and female completed counts and average monthly changes on each program list or detail request. A complete member needs an initial and later measurement with `SMM`, `BFM`, `WT`, and `PBF` on both dates, separated by at least 30 days. If several pairs qualify, use the pair with the latest end date; on a tie, prefer spacing closest to 30 days.

For each metric, normalize a member's change to 30 days:

```text
normalized change = (later value - initial value) * 30 / elapsed days
```

The group average is the mean of normalized changes among complete members. A group without complete members has a `null` average. The overall average is computed directly across all complete members, equivalent to weighting gender averages by completed counts.

The frontend calculates the consultation forecast:

```text
projected value = current value + gender monthly average * selected months
```

Do not show a projection unless at least three members of the selected program and gender have complete records.

## Similar-member search

Search only members in the selected program with the same gender and a complete measurement pair. Compare the client's current measurements with each member's initial values:

```text
distance = (SMM difference / 5 kg)^2
         + (BFM difference / 5 kg)^2
         + (weight difference / 10 kg)^2
         + (PBF difference / 5 percentage points)^2
```

Return up to three members in ascending distance order. Return response-local aliases instead of stored names and member IDs.

## Webhook handling

Use `TelHP` as the member token and `TestDatetimes` as the measurement time. Webhook processing is synchronous in the request; there is no separate job queue. A member/time/device combination is stored only once. An unknown member still returns `202` to acknowledge delivery. A failed `GetInBodyDataByID` call returns `502`; automatic server retry is not implemented. Do not assume a signature or replay feature is provided. `IsTempData` does not create a separate branch in this implementation.

## Database

| Table | Contents |
| --- | --- |
| `Products` | Program ID, name, creation date |
| `Members` | InBody string member ID, user token, name, gender |
| `InBodyMeasurements` | Member ID, measurement time, `Smm`, `Bfm`, `Weight`, `Pbf` |
| `ProductMembers` | Program ID, member ID, initial and follow-up measurement references |

Primary keys are `Products.ProductId`, `Members.MemberId`, `InBodyMeasurements.MeasurementId`, and `ProductMembers.ProductMemberId`. Foreign keys use the referenced primary key's name and type. API responses use `id` according to the UI contract. Unique constraints prevent duplicate associations and duplicate measurements.
