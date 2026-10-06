# User Management API (.NET 8)

A simple Minimal API for managing users, built with the help of Microsoft Copilot
(Coursera: Back-End Development with .NET).

## Features
- CRUD endpoints: `GET /users`, `GET /users/{id}`, `POST /users`, `PUT /users/{id}`, `DELETE /users/{id}`
- Validation (DataAnnotations): name, email format, age range, department, duplicate email check
- Middleware: error handling, token authentication, request logging
- In-memory storage (no database needed)

## Run
```bash
dotnet run
```
Then use `UserManagementAPI.http` (VS Code REST Client / Visual Studio) or curl:
```bash
curl http://localhost:5000/users -H "Authorization: Bearer secret-token-123"
```
Check the port printed in the console after `dotnet run` and update it if different.

## Middleware order
1. `ErrorHandlingMiddleware` - returns JSON 500 on unhandled exceptions
2. `AuthenticationMiddleware` - requires `Authorization: Bearer <token>` (token in `appsettings.json`)
3. `RequestLoggingMiddleware` - logs method, path, status code, duration

## How Copilot was used
- Generated the initial CRUD endpoints and the User model
- Debugged issues (null handling, duplicate emails, 404 cases)
- Suggested and refined the middleware components
