# Goodfood — food delivery

Food delivery demo with an ASP.NET Core Web API, MySQL, JWT role-based access, Razorpay test checkout, and an Angular storefront.

## Requirements

- .NET 10 SDK and x64 runtime
- MySQL 8+
- Node.js 20.19+ or 22.12+
- Razorpay test-mode account for checkout

## Configure and run the API

Create a MySQL database or allow the API's EF Core `EnsureCreated` startup setup to create `FoodApp`. Configure secrets in the .NET user-secrets store from `backend` (do not put real passwords, JWT signing keys, or Razorpay secrets in source control):

```powershell
cd backend
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:Default" "Server=localhost;Port=3306;Database=FoodApp;User ID=root;Password=YOUR_MYSQL_PASSWORD;"
dotnet user-secrets set "Jwt:Key" "REPLACE_WITH_A_RANDOM_SECRET_OF_AT_LEAST_32_CHARACTERS"
dotnet user-secrets set "Billing:KeyId" "YOUR_RAZORPAY_TEST_KEY_ID"
dotnet user-secrets set "Billing:KeySecret" "YOUR_RAZORPAY_TEST_KEY_SECRET"
dotnet user-secrets set "SeedAdmin:Email" "admin@example.com"
dotnet user-secrets set "SeedAdmin:Password" "A_STRONG_ADMIN_PASSWORD"
dotnet run --urls http://localhost:5000
```

The configured admin account is created the first time the API starts. The API seeds a sample restaurant and menu item. Public registration creates customer accounts; only an administrator can create restaurant-owner accounts and assign owners to restaurants.

The connection string, JWT key, billing credentials, and admin bootstrap credentials can also be supplied through environment variables using the standard .NET `__` separator, such as `ConnectionStrings__Default` and `Billing__KeySecret`. The checked-in `appsettings.json` contains a placeholder database password only; it does not contain a JWT signing or payment secret.

## Configure and run the Angular app

In another terminal:

```powershell
cd frontend
npm install
npm start
```

Open `http://localhost:4200`. The Angular app expects the API at `http://localhost:5000/api`.

## Features

- Customer registration and login with signed JWTs and BCrypt password hashes.
- Restaurant and menu browsing, food-name search, and category filters.
- Authenticated add, quantity update, remove, and clear cart operations. A cart is limited to one restaurant per checkout.
- Razorpay test-mode order creation and server-side HMAC-SHA256 payment-signature verification.
- Customer order history/status tracking; restaurant owners can advance their own orders through the fulfillment states.
- Admin tools for restaurants, menu items, and restaurant-owner accounts/assignments.
- Responsive Angular UI with a medium 16px base font size.

## Main API routes

| Route | Access | Purpose |
|---|---|---|
| `POST /api/auth/register`, `POST /api/auth/login` | Public | Customer account and JWT |
| `GET /api/restaurants?search=&category=` | Public | Browse and search restaurants/menus |
| `GET /api/cart`, `POST /api/cart/items`, `PUT /api/cart/items/{id}`, `DELETE /api/cart/items/{id}` | Customer | Cart management |
| `POST /api/orders/payment/order`, `POST /api/orders/payment/verify` | Customer | Test checkout |
| `GET /api/orders` | Customer | Order history and status |
| `GET /api/orders/management`, `PUT /api/orders/{id}/status` | Owner/Admin | Fulfillment management |
| `/api/admin/owners`, `/api/admin/restaurants`, `/api/admin/food-items` | Admin | Restaurant and menu administration |

EF Core creates the `Users`, `Restaurants`, `FoodItems`, `Cart`, `CartItems`, `Orders`, and `OrderItems` tables on first run. For an existing schema or production deployment, replace `EnsureCreated` with versioned EF Core migrations before applying schema changes.

## Important

The Razorpay key ID is returned to the Angular client for checkout; the key secret remains on the API. Never use live billing credentials in this test-mode demo or commit secrets to source control. Supply a strong JWT signing key (at least 32 UTF-8 bytes) through user-secrets or environment configuration.
