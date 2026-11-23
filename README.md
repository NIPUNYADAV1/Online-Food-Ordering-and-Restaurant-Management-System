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
