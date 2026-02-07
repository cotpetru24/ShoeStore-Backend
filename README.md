# ShoeStore Backend API

A modern, scalable e-commerce backend API built with ASP.NET Core 8.0, PostgreSQL, and Entity Framework Core. This API powers a full-featured shoe store with admin management, order processing, payment integration (Stripe), and content management capabilities.

## Architecture

### Technology Stack
- **Framework**: ASP.NET Core 8.0
- **Database**: PostgreSQL with Entity Framework Core
- **Authentication**: JWT Bearer Tokens with ASP.NET Core Identity
- **Payment Processing**: Stripe Integration
- **API Style**: RESTful with Problem Details (RFC 7807) error responses

### Project Structure
```
ShoeStore-Backend/
├── ShoeStore/                        # API (Controllers, Services, DTOs)
├── ShoeStore.DataContext.PostgreSQL/ # EF Core DbContext and Models
└── Tests/                            # Unit + Integration tests
```

### Design Principles
- **Separation of Concerns**: Controllers handle HTTP, Services contain business logic
- **Single Responsibility**: Each service/controller focuses on one domain area
- **Centralized Error Handling**: Global exception middleware with proper HTTP status codes
- **Request Validation**: Paging parameters and model validation at controller level
- **Dependency Injection**: All services registered in `Program.cs`

## Getting Started

### Prerequisites
- .NET 8.0 SDK
- PostgreSQL 12+ (or Docker)
- Stripe account (for payment processing)

### Configuration

1. **Database Connection**
   Connection strings must be set as environment variables:
   ```bash
   ConnectionStrings__ShoeStoreConnection=Host=localhost;Database=shoestore;Username=postgres;Password=yourpassword
   ConnectionStrings__DefaultConnection=Host=localhost;Database=shoestore;Username=postgres;Password=yourpassword
   ```

2. **JWT Settings**
   ```json
   {
     "JwtSettings": {
       "Secret": "your-secret-key-min-32-characters",
       "Issuer": "ShoeStore",
       "Audience": "AngularClient",
       "ExpiryMinutes": 60
     }
   }
   ```

3. **Stripe Configuration**
   Set environment variable:
   ```bash
   Stripe__SecretKey=sk_test_your_stripe_secret_key
   ```

4. **Run Migrations**
   ```bash
   dotnet ef database update --project ShoeStore.DataContext.PostgreSQL
   ```

### Running the Application

```bash
# Development
dotnet run --project ShoeStore

# Production
dotnet publish -c Release
```

The API will be available at:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`
- Swagger UI: `https://localhost:5001/swagger` (Development only)

## API Endpoints

### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login and receive JWT token

### Products (Public)
- `GET /api/product` - List products with filtering/pagination
- `GET /api/product/featured` - Get featured products
- `GET /api/product/{id}` - Get product details

### Orders (Authenticated)
- `GET /api/order` - Get user's orders
- `GET /api/order/{id}` - Get order details
- `POST /api/order` - Place new order
- `PUT /api/order/cancel-order/{id}` - Cancel order

### Addresses (Authenticated)
- `GET /api/address`
- `GET /api/address/{id}`
- `POST /api/address`
- `PUT /api/address/{id}`
- `DELETE /api/address/{id}`

### Payments (Authenticated)
- `POST /api/payment/createPaymentIntent` - Create Stripe payment intent
- `POST /api/payment/storePaymentDetails` - Store payment details

### Admin Endpoints (Administrator Role Required)

#### Dashboard
- `GET /api/admin/dashboard` - Get dashboard statistics

#### User Management
- `GET /api/admin/users` - List users with filtering/pagination
- `GET /api/admin/users/{id}` - Get user details
- `PUT /api/admin/users/{id}` - Update user
- `DELETE /api/admin/users/{id}` - Delete user
- `PUT /api/admin/users/{id}/password` - Update user password
- `GET /api/admin/users/{id}/orders` - Get user's orders

#### Order Management
- `GET /api/admin/orders` - List orders with filtering/pagination
- `GET /api/admin/orders/{id}` - Get order details
- `PUT /api/admin/orders/{id}/status` - Update order status

#### Product Management
- `GET /api/admin/products` - List products with filtering/pagination
- `GET /api/admin/products/{id}` - Get product details
- `POST /api/admin/products` - Create product
- `PUT /api/admin/products/{id}` - Update product
- `DELETE /api/admin/products/{id}` - Delete product
- `GET /api/admin/products/brands` - Get all brands
- `GET /api/admin/products/audience` - Get all audience types

#### Content Management
- `GET /api/cms/navAndFooter` - Get navigation and footer (public)
- `GET /api/cms/landing` - Get landing page content (public)
- `GET /api/cms/profiles` - List CMS profiles (admin)
- `GET /api/cms/{id}` - Get CMS profile (admin)
- `POST /api/cms` - Create CMS profile (admin)
- `PUT /api/cms` - Update CMS profile (admin)
- `DELETE /api/cms/{id}` - Delete CMS profile (admin)
- `POST /api/cms/activate/{id}` - Activate CMS profile (admin)

## Authentication & Authorization

### JWT Token Flow
1. User registers/logs in via `/api/auth/register` or `/api/auth/login`
2. API returns JWT token
3. Client includes token in `Authorization: Bearer {token}` header
4. API validates token and extracts user claims

### Roles
- **Customer**: Default role for registered users
- **Administrator**: Full access to admin endpoints

## Order Statuses

Order statuses are implemented as backend enums and stored as integers in the database.
They are not configurable at runtime.

### OrderStatusEnum

| Value | Name           | Description                      | Terminal |
|------:|----------------|----------------------------------|----------|
| 2     | Processing     | Payment confirmed / order active | No       |
| 3     | Shipped        | Order shipped                    | No       |
| 4     | Delivered      | Order delivered                  | No       |
| 5     | Cancelled      | Order cancelled and refunded     | Yes      |
| 6     | PaymentFailed  | Payment failed                   | Yes      |
| 7     | Returned       | Order returned and refunded      | Yes      |

### Valid Transitions

- Processing → Shipped, Cancelled
- Shipped → Delivered
- Delivered → Returned

Terminal statuses cannot be changed.

## Payment Statuses

Payment statuses are implemented as backend enums and stored as integers in the database.
They represent the lifecycle of a payment and are **not configurable at runtime**.

### PaymentStatusEnum

| Value | Name       | Description                                           | Terminal |
|------:|------------|-------------------------------------------------------|----------|
| 1     | Pending    | PaymentIntent created, awaiting confirmation          | No       |
| 3     | Authorised | Payment authorised but not captured                   | No       |
| 4     | Failed     | Payment failed or expired without capturing funds     | Yes      |
| 6     | Refunded   | Payment fully refunded                                | Yes      |
| 12    | Paid       | Payment captured and successful                       | Yes*     |

\* `Paid` is terminal from a payment perspective, but may still lead to a refund.

### Payment → Order Relationship

- A payment in `Pending` or `Authorised` is associated with an order in `Processing` once confirmed.
- A payment moving to `Paid` allows the order to move forward in the fulfillment flow.
- A payment in `Refunded` forces the order into `Cancelled` or `Returned`.
- A payment in `Failed` forces the order into `PaymentFailed`.

### Invariants

- Refunded payments cannot be amended.
- Failed payments cannot be retried without creating a new PaymentIntent.
- Payment state changes driven by external providers (e.g., Stripe) should be handled via webhooks.
- Order status transitions must respect payment status compatibility.

### Authorization Attributes
- `[Authorize]` - Requires authenticated user
- `[Authorize(Roles = "Administrator")]` - Requires admin role

## Error Handling

The API uses a centralized exception handling middleware that:
- Maps exceptions to appropriate HTTP status codes:
  - `UnauthorizedAccessException` → 401 Unauthorized
  - `KeyNotFoundException` → 404 Not Found
  - `ArgumentException` → 400 Bad Request
  - `InvalidOperationException` → 409 Conflict
  - Others → 500 Internal Server Error
- Returns RFC 7807 Problem Details format.

## Request Validation

### Paging Parameters
All list endpoints validate and normalize paging:
- `PageNumber`: Minimum 1, defaults to 1
- `PageSize`: Range 1-100, defaults to 10

### Model Validation
- ASP.NET Core model validation via `[FromBody]` attributes
- Invalid models return 400 Bad Request with validation errors

## Testing

### Unit Tests
Located in `Tests/` directory:
- `AdminServiceTests.cs`
- `CmsServiceTests.cs`
- `OrderServiceTests.cs`
- `ProductServiceTests.cs`

Run tests:
```bash
dotnet test
```

### Integration Tests
See `Tests/IntegrationTests/` for full-stack tests using `WebApplicationFactory`.

## Development

### Adding a New Endpoint
1. Add method to appropriate service (e.g., `AdminProductService`)
2. Create DTOs in `Dto/` folder
3. Add controller action with proper attributes
4. Register service in `Program.cs` (if new service)
5. Add validation and error handling

### Database Migrations
```bash
# Create migration
dotnet ef migrations add MigrationName --project ShoeStore.DataContext.PostgreSQL

# Apply migration
dotnet ef database update --project ShoeStore.DataContext.PostgreSQL
```

## Environment Variables

Required for production:
- `ConnectionStrings__DefaultConnection` - PostgreSQL connection string
- `Stripe__SecretKey` - Stripe secret API key

## Deployment

1. Set environment variables
2. Run database migrations
3. Build and publish:
   ```bash
   dotnet publish -c Release -o ./publish
   ```
4. Deploy to hosting platform (Azure, AWS, etc.)

## License

Petru Cotorobai

## Contributors

Petru Cotorobai

## Frontend Integration (Brief)
- Frontend should call this API base URL (example): `https://localhost:5001`.
- Auth is JWT-based; send `Authorization: Bearer <token>` on protected endpoints.
- Ensure CORS in `ShoeStore/Program.cs` allows your frontend origin.

## Deployment / Runtime Notes (Lightweight)
- Configure required environment variables listed above (DB, JWT, Stripe) for each environment.
- Use HTTPS in production and set secure secrets outside source control.
- Run EF Core migrations as part of deployment (if applicable).
- Health check endpoint: `GET /health` for readiness monitoring.
