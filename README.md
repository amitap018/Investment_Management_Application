# Investment Management Application

A comprehensive ASP.NET Core web application for managing mutual fund investments, systematic investment plans (SIPs), and customer relationships. Built with modern .NET technologies and designed for secure, reliable investment portfolio management.

## 📋 Table of Contents

- [Overview](#overview)
- [Key Features](#key-features)
- [Technology Stack](#technology-stack)
- [Project Structure](#project-structure)
- [Database Schema](#database-schema)
- [Getting Started](#getting-started)
- [Configuration](#configuration)
- [Running the Application](#running-the-application)
- [API Endpoints](#api-endpoints)
- [Services](#services)
- [Authentication & Security](#authentication--security)
- [Contributing](#contributing)

## 🎯 Overview

The Investment Management Application is a full-featured web platform that enables users to:
- Register and manage investment accounts
- Browse and invest in various mutual funds
- Set up Systematic Investment Plans (SIPs)
- Track investment performance and portfolio value
- Manage orders and redemptions
- Submit feedback and grievances
- Receive timely email notifications about their investments

The application uses **Entity Framework Core** with **SQL Server** for data persistence, **Cookie-based authentication** for security, and **background services** for automated operations like email delivery and NAV updates.

## ✨ Key Features

### 1. **User Authentication & Account Management**
- User registration with email verification
- Secure login/logout with cookie-based authentication
- One-Time Password (OTP) based password recovery
- Account deletion with data cleanup
- Unique Demat ID generation for each user

### 2. **Investment Dashboard**
- Comprehensive investment portfolio overview
- Real-time fund performance tracking
- Investment history and analytics
- Visual representation of fund performance

### 3. **Mutual Fund Management**
- Browse available mutual funds
- View detailed fund information and performance metrics
- Investment performance analysis
- Fund allocation management

### 4. **Investment Operations**
- **One-Time Investments**: Direct lump sum investments in mutual funds
- **Systematic Investment Plans (SIPs)**: Recurring monthly investments
- **Redemptions**: Withdraw funds from investments
- **SIP Cancellation**: Stop recurring investments
- Order history and status tracking
- PDF export of transaction history

### 5. **Customer Support**
- Submit and track feedback
- File grievances with status tracking
- Receive support notifications via email

### 6. **Background Processing**
- Automated email notifications for transactions and updates
- NAV (Net Asset Value) updates scheduler
- Email queue management

## 🛠 Technology Stack

| Layer | Technologies |
|-------|--------------|
| **Backend** | ASP.NET Core 8.0, C# |
| **Database** | SQL Server, Entity Framework Core |
| **Authentication** | Cookie-based Authentication |
| **Frontend** | Razor Views, HTML5, CSS3, Bootstrap, jQuery |
| **Logging** | Serilog with file and console outputs |
| **PDF Generation** | iText |
| **Email** | SMTP-based email service |
| **Pagination** | PagedList.Core |
| **ORM** | Entity Framework Core |

## 📁 Project Structure

```
Investment_Management_Application/
├── Login/
│   ├── Controllers/
│   │   ├── HomeController.cs
│   │   ├── SignUpController.cs          # Authentication
│   │   ├── DashboardController.cs       # Investment dashboard
│   │   ├── MutualFundsController.cs     # Mutual fund operations
│   │   ├── OrdersController.cs          # Order management
│   │   └── CustomerSupportController.cs # Feedback & grievances
│   │
│   ├── Models/
│   │   ├── Authentication.cs            # User credentials
│   │   ├── UserDetails.cs               # User profile info
│   │   ├── MutualFund.cs                # Mutual fund data
│   │   ├── Order.cs                     # Investment orders
│   │   ├── SIP.cs                       # Systematic Investment Plans
│   │   ├── UserFund.cs                  # User fund holdings
│   │   ├── Feedback.cs                  # User feedback
│   │   ├── Grievance.cs                 # User grievances
│   │   ├── Otp.cs                       # OTP data
│   │   └── ForgotPassword.cs            # Password recovery
│   │
│   ├── Views/
│   │   ├── SignUp/
│   │   │   ├── Login.cshtml
│   │   │   ├── Register.cshtml
│   │   │   ├── ForgotPassword.cshtml
│   │   │   ├── VerifyOtp.cshtml
│   │   │   └── ResetPassword.cshtml
│   │   ├── Dashboard/
│   │   │   ├── Index.cshtml
│   │   │   ├── OneTimeInvestment.cshtml
│   │   │   ├── StartOrEditSIP.cshtml
│   │   │   ├── Redeem.cshtml
│   │   │   └── CancelSIP.cshtml
│   │   ├── MutualFunds/
│   │   │   ├── Index.cshtml
│   │   │   ├── Details.cshtml
│   │   │   └── Invest.cshtml
│   │   ├── Orders/
│   │   │   ├── UserOrders.cshtml
│   │   │   └── OrderDetails.cshtml
│   │   ├── CustomerSupport/
│   │   │   ├── SubmitFeedback.cshtml
│   │   │   ├── SubmitGrievance.cshtml
│   │   │   └── UserGrievances.cshtml
│   │   └── Shared/
│   │       ├── _Layout.cshtml
│   │       └── Error.cshtml
│   │
│   ├── Data/
│   │   ├── ApplicationDbContext.cs       # EF Core DbContext
│   │   └── DbInitializer.cs             # Database seed data
│   │
│   ├── Services/
│   │   ├── EmailService.cs              # Email sending
│   │   ├── EmailBackgroundService.cs    # Background email processing
│   │   ├── NavUpdateScheduler.cs        # NAV update scheduler
│   │   └── HashingHelper.cs             # Password hashing
│   │
│   ├── Migrations/
│   │   ├── 20240807165319_InitialCreate.cs
│   │   └── ApplicationDbContextModelSnapshot.cs
│   │
│   ├── wwwroot/
│   │   ├── css/
│   │   ├── js/
│   │   ├── lib/
│   │   └── Images/
│   │
│   ├── Program.cs                        # Application entry point
│   ├── appsettings.json                 # Configuration
│   ├── Login.csproj                     # Project file
│   └── Properties/
│       └── launchSettings.json
│
└── README.md                            # This file
```

## 🗄 Database Schema

### Core Tables

**UserDetails**
- UserId (PK)
- Email
- FirstName, LastName
- PhoneNumber
- DematId (unique demat account identifier)
- Address, City, State, PinCode
- CreatedAt

**Authentication**
- UserId (PK, FK)
- PasswordHash
- CreatedAt

**MutualFund**
- FundId (PK)
- FundName
- Description
- CurrentNAV (Net Asset Value)
- MinimumInvestment
- YearlyPerformance

**Order**
- OrderId (PK)
- UserId (FK)
- FundId (FK)
- OrderType (OneTime/SIP)
- Amount
- OrderDate
- Status (Pending/Completed/Cancelled)

**SIP (Systematic Investment Plan)**
- SIPId (PK)
- UserId (FK)
- FundId (FK)
- MonthlyAmount
- StartDate
- EndDate
- Status (Active/Inactive/Completed)

**UserFund**
- UserFundId (PK)
- UserId (FK)
- FundId (FK)
- FolioNumber (unique investment account)
- Units
- AveragePrice
- CurrentValue

**Feedback**
- FeedbackId (PK)
- UserId (FK)
- Description
- Rating
- CreatedAt

**Grievance**
- GrievanceId (PK)
- UserId (FK)
- Description
- Status
- CreatedAt

**OTP**
- OtpId (PK)
- Email
- OtpCode
- ExpiresAt
- IsUsed

## 🚀 Getting Started

### Prerequisites

- .NET 8.0 SDK or later
- SQL Server 2019 or later
- Visual Studio 2022 or Visual Studio Code
- Git

### Installation

1. **Clone the Repository**
   ```bash
   git clone <repository-url>
   cd Investment_Management_Application
   ```

2. **Open the Project**
   ```bash
   cd Login
   ```

3. **Restore NuGet Packages**
   ```bash
   dotnet restore
   ```

4. **Set Up the Database**
   
   Update the connection string in `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=YOUR_SERVER;Database=InvestmentDB;Trusted_Connection=true;"
     }
   }
   ```

5. **Apply Migrations**
   ```bash
   dotnet ef database update
   ```

## ⚙️ Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=InvestmentDB;Trusted_Connection=true;"
  },
  "Serilog": {
    "MinimumLevel": "Information",
    "WriteTo": [
      {
        "Name": "Console"
      },
      {
        "Name": "File",
        "Args": {
          "path": "Logs/app-.txt",
          "rollingInterval": "Day"
        }
      }
    ]
  },
  "EmailSettings": {
    "SmtpServer": "your_smtp_server",
    "SmtpPort": 587,
    "SenderEmail": "your_email@example.com",
    "SenderPassword": "your_password"
  }
}
```

### launchSettings.json

Configure the application URL and launch settings:
```json
{
  "profiles": {
    "https": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "applicationUrl": "https://localhost:7001;http://localhost:5001",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

## ▶️ Running the Application

### Development Server

```bash
cd Login
dotnet run
```

The application will be available at: `https://localhost:7001`

### Production Build

```bash
dotnet publish -c Release -o ./publish
```

## 📡 API Endpoints

### Authentication (SignUp Controller)
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/SignUp` | Registration form |
| POST | `/SignUp/SignUp` | Create new user account |
| GET | `/SignUp/Login` | Login form |
| POST | `/SignUp/Login` | Authenticate user |
| GET | `/SignUp/Logout` | Logout user |
| GET | `/SignUp/ForgotPassword` | Password recovery form |
| POST | `/SignUp/ForgotPassword` | Send OTP for password reset |
| GET | `/SignUp/VerifyOtp` | OTP verification form |
| POST | `/SignUp/VerifyOtp` | Verify OTP |
| POST | `/SignUp/ResetPassword` | Reset password |
| POST | `/SignUp/DeleteAccount` | Delete user account |

### Dashboard (Dashboard Controller)
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/Dashboard` | User investment dashboard |
| GET | `/Dashboard/GetFundPerformance` | Fund performance data |
| GET | `/Dashboard/OneTimeInvestment/{folioNumber}` | One-time investment page |
| POST | `/Dashboard/OneTimeInvestment` | Create one-time investment |
| GET | `/Dashboard/Redeem/{folioNumber}` | Redeem investment page |
| POST | `/Dashboard/Redeem` | Process redemption |
| GET | `/Dashboard/StartOrEditSIP/{folioNumber}` | SIP creation/edit page |
| POST | `/Dashboard/StartOrEditSIP` | Create or edit SIP |
| GET | `/Dashboard/CancelSIP/{sipId}` | Cancel SIP |

### Mutual Funds (MutualFunds Controller)
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/mutual-funds` | List all mutual funds |
| GET | `/mutual-funds/details/{id}` | Fund details |
| POST | `/mutual-funds/invest` | Invest in fund |

### Orders (Orders Controller)
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/Orders/UserOrders/{userId}` | User order history |
| GET | `/Orders/OrderDetails/{id}` | Order details |
| GET | `/Orders/DownloadUserOrdersPdf` | Download orders as PDF |

### Customer Support (CustomerSupport Controller)
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/CustomerSupport/SubmitFeedback` | Feedback form |
| POST | `/CustomerSupport/SubmitFeedback` | Submit feedback |
| GET | `/CustomerSupport/SubmitGrievance` | Grievance form |
| POST | `/CustomerSupport/SubmitGrievance` | Submit grievance |
| GET | `/CustomerSupport/UserGrievances` | View user grievances |

## 🔧 Services

### EmailService
Handles email operations:
- Sending transactional emails (registration, password reset, order confirmations)
- Email templates for different scenarios
- SMTP configuration and error handling

**Usage:**
```csharp
await _emailService.SendEmailAsync(email, subject, message);
```

### EmailBackgroundService
Processes emails asynchronously in the background:
- Queues email operations
- Handles failed email attempts with retry logic
- Runs as a hosted service

### NavUpdateScheduler
Background scheduler for mutual fund NAV updates:
- Updates Net Asset Value periodically
- Calculates fund performance metrics
- Synchronizes fund data

### HashingHelper
Secure password hashing utilities:
- Hash passwords using industry-standard algorithms
- Verify passwords securely
- Salt-based hashing for enhanced security

## 🔐 Authentication & Security

### Authentication Method
- **Type**: Cookie-based Authentication
- **Scheme**: Default ASP.NET Core Cookie Authentication

### Security Features
1. **Password Security**
   - Passwords are hashed using secure algorithms
   - Salt-based hashing prevents rainbow table attacks

2. **Authorization**
   - `[Authorize]` attribute protects sensitive endpoints
   - Role-based access control (if implemented)

3. **CSRF Protection**
   - `[ValidateAntiForgeryToken]` on POST actions
   - Anti-forgery tokens in all forms

4. **Data Validation**
   - Server-side validation on all inputs
   - Model validation attributes

### Login Flow
1. User provides email and password
2. System verifies credentials against stored hashes
3. Creates authentication cookie on success
4. Redirects to dashboard
5. Cookie validated on each request

### Password Reset Flow
1. User enters email on Forgot Password page
2. System generates OTP and sends via email
3. User verifies OTP
4. User sets new password
5. Password hash is updated in database

## 📝 Logging

The application uses **Serilog** for structured logging:
- **File Logging**: Logs saved to `Logs/app-*.txt` with daily rolling
- **Console Logging**: Real-time log output during development
- **Structured Format**: Compact JSON format for easier parsing
- **Minimum Level**: Information level and above

### Log Locations
- Development: `Logs/` directory (created automatically)
- Production: Configure path in `appsettings.Production.json`

## 🤝 Contributing

1. **Fork** the repository
2. **Create** a feature branch: `git checkout -b feature/your-feature`
3. **Commit** your changes: `git commit -m 'Add your feature'`
4. **Push** to the branch: `git push origin feature/your-feature`
5. **Submit** a pull request

## 📋 Development Workflow

1. Create a new branch for each feature
2. Follow ASP.NET Core naming conventions
3. Add unit tests for business logic
4. Ensure all endpoints are properly secured with `[Authorize]`
5. Test database migrations in development environment
6. Update this README for new features

## 📦 Dependencies

Core NuGet packages:
- `Microsoft.AspNetCore.Mvc`
- `Microsoft.EntityFrameworkCore`
- `Microsoft.EntityFrameworkCore.SqlServer`
- `Microsoft.AspNetCore.Authentication.Cookies`
- `Serilog`
- `iText` (PDF generation)
- `PagedList.Core`
- `Newtonsoft.Json`

## 🐛 Troubleshooting

### Database Connection Issues
```
Error: "A network or instance-specific error occurred while establishing a connection"
Solution: Check connection string in appsettings.json and verify SQL Server is running
```

### Migration Errors
```
Error: "Unable to create migrations"
Solution: Ensure DbContext is properly configured and use: dotnet ef migrations add <name>
```

### Authentication Issues
```
Error: "User not authenticated"
Solution: Check if [Authorize] attribute is applied and user is logged in
```

## 📞 Support

For issues and questions:
- Check existing issues in the repository
- Create a detailed issue report
- Include error messages and reproduction steps

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🔄 Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0.0 | Aug 2024 | Initial database schema and migrations |
| Current | - | Active development |

---

**Last Updated**: May 2, 2026
