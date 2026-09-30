# Bhoomi Malmatta API integration

This solution is isolated under `src/BhoomiMalmatta`. Keep its SQL Server database separate from every organization database. The API currently applies this module's own EF migrations during startup, so do not start it until `ConnectionStrings:DefaultConnection` has been verified to point to a disposable/local Bhoomi database. Never point it at the organization Citizen Portal database.

## Local setup

1. Copy `src/SMC.API/appsettings.example.json` to the ignored `src/SMC.API/appsettings.Development.json` and set local values there, or use environment variables. Do not commit local appsettings files.
2. Set `ASPNETCORE_URLS=http://localhost:5000` when launching the API. Its API base is `http://localhost:5000/api`; the frontend uses `NEXT_PUBLIC_BHOOMI_API_BASE_URL` with that fallback.
3. Configure the JWT issuer, audience, and signing key to validate the existing Department JWT. Do not copy the key into frontend configuration. The same signing key and matching issuer/audience are required because the Bhoomi API accepts the existing `dept_token` as a bearer token.
4. Department JWT roles `SystemAdmin` and `DepartmentAdmin` map to Bhoomi `Admin`. `DepartmentEmployee` defaults to least-privilege `Staff`. For approved Bhoomi assignments, add the same user ID to `BhoomiAuth:DepartmentRoleMappings` in the ignored backend settings and to `NEXT_PUBLIC_BHOOMI_DEPARTMENT_ROLE_MAP` in the frontend's ignored local environment. Allowed values are `Admin`, `Officer`, `Staff`, `JE`, `OS`, and `AssistantCommissioner`. The backend enforces this mapping from the validated JWT subject; the frontend mapping only controls the displayed Bhoomi role.
5. Set `Integrations:Sms:PublicBaseUrl` to the public Citizen Bhoomi base URL. The API appends `/application-status` when it creates document-request links.
6. Configure `Cors:AllowedOrigins` for the Citizen and Department frontend origins. Keep SMS and Razorpay disabled until their credentials are supplied through ignored settings or a secret store.

The tracked `appsettings.example.json` contains placeholders only. Health is available at `/health`; Swagger is enabled in Development at `/swagger`.
