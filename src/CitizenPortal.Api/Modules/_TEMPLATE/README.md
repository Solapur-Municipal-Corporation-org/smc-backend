# Module template — use this ONLY if the generic form system isn't enough

Most departments do **not** need anything in this folder. If your service's form
is just a set of fields (text/number/date/select/textarea/email/tel) and a list
of required documents, you don't need a custom table at all — just add a
`{DeptCode}ServicesSeeder.cs` file in `Data/Seeders/` (copy
`Data/Seeders/_TemplateServicesSeeder.cs.txt`). The existing `Applications`
table already stores whatever the citizen submits (as JSON) and the existing
`ApplicationsController` already handles create / track / list / certificate
download / payment for you — zero backend code required.

Use a custom table (this folder's pattern) only when your service genuinely
needs **structured, queryable data** that a flat form can't represent well —
e.g. a repeating list of family members, a linked survey/assessment record,
geo-coordinates, or data your department's other systems need to read
directly from the database.

## How to add a custom table

1. Copy this `_TEMPLATE` folder, rename it to your department code, e.g. `PROP`.
2. Rename `TemplateDetail.cs` → `{YourEntity}.cs` and edit the fields you need.
3. Rename `TemplateDetailConfiguration.cs` → `{YourEntity}Configuration.cs` and
   update the table name (see the `ToTable(...)` call — **always prefix your
   table name with your department code**, e.g. `PROP_AssessmentDetails`, so
   table names never collide between departments).
4. Register your DbSet in `Data/AppDbContext.cs` — add ONE line under the
   `// ---- MODULES ----` comment. That's the only shared file you touch, and
   it's a single additive line, so conflicts with other developers are rare.
5. Still seed your Service/ServiceField/ServiceDocument rows the normal way
   (step above) — the custom table is always a 1:1 **extension** of an
   `Application` row via `ApplicationId`, never a replacement for it. This
   keeps status tracking, payments, and certificate generation working
   automatically through the shared `Applications` table.
6. In your controller, after the citizen submits (or in a custom endpoint if
   you need one), create your detail row with the same `ApplicationId` as the
   `Application` that was created.
7. Generate and apply the migration:
   ```
   dotnet ef migrations add Add_{YourDeptCode}_{YourTableName}
   dotnet ef database update
   ```
   Coordinate with the team before running `database update` against the
   shared DB/server — one person should apply migrations at a time.

See `INTEGRATION_GUIDE.md` at the project root for the full explanation.
