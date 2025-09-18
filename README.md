# ApiBase

A simple .NET API with JWT authentication and user management.

## Features

- User registration and authentication
- JWT token-based authentication
- User CRUD operations
- Swagger UI documentation

## Endpoints

### Public Endpoints

- `POST /users/register` - Register a new user
- `POST /auth/login` - Authenticate and get JWT token

### Protected Endpoints (requires JWT authentication)

- `GET /users` - Get all users
- `GET /users/{id}` - Get user by ID
- `PUT /users/{id}` - Update user
- `DELETE /users/{id}` - Delete user

## Getting Started

1. Clone the repository
2. Install .NET 9.0 SDK
3. Run the application:
   ```bash
   dotnet run
   ```
4. Navigate to `https://localhost:5001/swagger` to view the Swagger UI

## Authentication

To authenticate:

1. Register a new user using the `/users/register` endpoint
2. Login using the `/auth/login` endpoint to get a JWT token
3. Use the token in the Authorization header for protected endpoints:
   ```
   Authorization: Bearer your.jwt.token
   ```