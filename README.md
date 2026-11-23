<<<<<<< HEAD
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
=======
# Online Food Ordering and Restaurant Management System

A Zomato-inspired full-stack web application that allows customers to explore restaurants, browse food menus, manage their shopping carts, and place food orders. The platform also provides restaurant management features for owners and administrative controls for administrators.

## 🚀 Features

### 👤 Customer
- Register and log in securely.
- Browse available restaurants.
- View restaurant food menus.
- Add food items to the cart.
- Update item quantities and remove items.
- Place and manage food orders.
- View order details and order status.

### 🏪 Restaurant Owner
- Manage assigned restaurant details.
- Add, update, and manage food items.
- View incoming customer orders.
- Manage order status.

### 🛡️ Admin
- Create and manage restaurant owner accounts.
- Create and update restaurants.
- Assign owners to restaurants.
- Manage food items.
- Deactivate restaurants and remove food items.
- Manage platform operations.

## 🛠️ Technology Stack

### Frontend
- Angular
- TypeScript
- HTML5
- CSS3

### Backend
- ASP.NET Core Web API (.NET 10)
- C#
- Entity Framework Core
- RESTful APIs

### Database
- MySQL

### Authentication & Security
- JWT (JSON Web Token) Authentication
- Role-Based Authorization
- BCrypt Password Hashing

### Tools
- Visual Studio Code
- Git & GitHub
- Postman
- MySQL

## 🏗️ Project Architecture

The application follows a client-server architecture.

```text
Angular Frontend
       |
       | HTTP Requests (REST API)
       |
ASP.NET Core Web API
       |
       | Entity Framework Core
       |
     MySQL
```

## 🔐 Authentication and Authorization

The application uses JWT-based authentication to secure API endpoints.

Three user roles are supported:

| Role | Access |
|---|---|
| Customer | Browse restaurants, manage cart, and place orders |
| Owner | Manage assigned restaurants, food items, and orders |
| Admin | Manage users, restaurants, owners, and food items |

Protected endpoints use role-based authorization to ensure that users can access only the functionality permitted for their roles.

## 📂 Backend Structure

```text
FoodDelivery.Api/
│
├── Controllers/
│   ├── AuthController.cs
│   ├── AdminController.cs
│   ├── CartController.cs
│   ├── RestaurantController.cs
│   └── OrderController.cs
│
├── Data/
│   └── FoodDbContext.cs
│
├── DTOs/
│
├── Models/
│
├── Migrations/
│
├── Program.cs
└── appsettings.json
```

*The structure above represents the main backend organization; filenames may vary according to the final implementation.*

## ⚙️ Getting Started

### Prerequisites

Make sure you have installed:

- .NET 10 SDK
- Node.js and npm
- Angular CLI
- MySQL Server
- Git

### 1. Clone the Repository

```bash
git clone <YOUR_GITHUB_REPOSITORY_URL>
cd <YOUR_PROJECT_FOLDER>
```

### 2. Configure the Database

Create a MySQL database and update the connection string in `appsettings.json`.

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=FoodDeliveryDB;User=root;Password=YOUR_PASSWORD;"
  }
}
```

Use your own MySQL credentials.

### 3. Run the Backend

Navigate to the backend project directory:

```bash
cd FoodDelivery.Api
```

Restore dependencies:

```bash
dotnet restore
```

Apply database migrations:

```bash
dotnet ef database update
```

Run the API:

```bash
dotnet run
```

### 4. Run the Frontend

Open another terminal and navigate to the Angular frontend directory:

```bash
cd <YOUR_FRONTEND_FOLDER>
```

Install dependencies:

```bash
npm install
```

Start the Angular application:

```bash
ng serve
```

Open the application in your browser at:

```text
http://localhost:4200
```

## 🔗 API Functionality

The backend exposes RESTful API endpoints for:

- User registration and login
- Restaurant listing and management
- Food menu management
- Shopping cart operations
- Order placement and management
- Admin operations and owner assignments

## 🔒 Security Practices

- Passwords are hashed using BCrypt.
- JWT tokens are used for authenticated API requests.
- Role-based authorization protects restricted endpoints.
- Customers can access only their own cart data.
- API endpoints validate resource ownership where required.

## 🎯 Project Objective

The main objective of this project is to develop a full-stack food ordering platform while applying practical concepts of Angular, ASP.NET Core Web API, C#, Entity Framework Core, MySQL, RESTful API development, JWT authentication, and role-based access control.

## 👨‍💻 Author

**Nipun Kumar**

- GitHub: [NIPUNYADAV1](https://github.com/NIPUNYADAV1)
- LinkedIn: [Nipun Kumar](https://linkedin.com/in/nipun-kumar-60232233b/)

---

⭐ If you find this project useful, consider giving it a star!
>>>>>>> 1f8807acdbb1b31c9246b9436d1e66af4cbd4fb1
