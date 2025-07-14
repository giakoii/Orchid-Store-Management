# 🌺 Orchid Store - Hệ thống E-commerce Bán Hoa Lan

![License](https://img.shields.io/badge/license-MIT-blue.svg)
![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)
![Next.js](https://img.shields.io/badge/Next.js-15.3.5-black.svg)
![TypeScript](https://img.shields.io/badge/TypeScript-5.0-blue.svg)

## 📋 Mô tả dự án

**Orchid Store** là một hệ thống thương mại điện tử chuyên bán hoa lan với kiến trúc fullstack hiện đại. Dự án được xây dựng với mục tiêu cung cấp trải nghiệm mua sắm trực tuyến tốt nhất cho khách hàng yêu thích hoa lan.

### ✨ Tính năng chính

#### 🛍️ **Dành cho khách hàng:**
- 🔐 Đăng ký và đăng nhập tài khoản
- 🌸 Duyệt và tìm kiếm hoa lan theo danh mục
- 🛒 Thêm sản phẩm vào giỏ hàng
- 💳 Thanh toán trực tuyến qua MoMo
- 📦 Theo dõi đơn hàng
- 👤 Quản lý thông tin cá nhân

#### ⚙️ **Dành cho quản trị viên:**
- 📊 Dashboard quản lý tổng quan
- 🌺 Quản lý sản phẩm hoa lan (CRUD)
- 📂 Quản lý danh mục sản phẩm
- 👥 Quản lý tài khoản người dùng
- 📋 Quản lý đơn hàng
- ☁️ Upload hình ảnh lên Cloudinary

## 🏗️ Kiến trúc hệ thống

### 🔧 **Backend - Clean Architecture**
```
OrchidStore.API/          # API Layer - Controllers & Endpoints
OrchidStore.Application/  # Application Layer - Business Logic
OrchidStore.Domain/       # Domain Layer - Entities & Models
OrchidStore.Infrastructure/ # Infrastructure Layer - Data Access
```

### 🎨 **Frontend - Next.js App Router**
```
src/
├── app/           # App Router pages
├── components/    # Reusable components
├── contexts/      # React Context providers
├── config/        # Configuration files
└── types/         # TypeScript type definitions
```

## 🛠️ Công nghệ sử dụng

### 🔹 **Backend Technologies**
- **Framework**: ASP.NET Core 9.0
- **Architecture**: Clean Architecture + CQRS Pattern + Template method Pattern
- **Authentication**: OpenIddict (OAuth2/OpenID Connect)
- **Database**: PostgreSQL + Entity Framework Core
- **Document Store**: Marten (Event Sourcing)
- **Mediator**: MediatR
- **File Storage**: Cloudinary
- **Payment**: MoMo API
- **API Documentation**: Swagger/OpenAPI

### 🔸 **Frontend Technologies**
- **Framework**: Next.js 15.3.5 (App Router)
- **Language**: TypeScript 5.0
- **Styling**: Tailwind CSS 4.1.11
- **State Management**: React Context API
- **HTTP Client**: Fetch API

### 🔹 **Database & Storage**
- **Main Database**: PostgreSQL
- **ORM**: Entity Framework Core
- **Document Store**: Marten (PostgreSQL-based)
- **File Storage**: Cloudinary CDN
- **Caching**: In-memory caching

## 📁 Cấu trúc dự án chi tiết

### 🔧 **Backend Structure**
```
BackEnd/OrchidStore/
├── OrchidStore.API/
│   ├── Controllers/
│   │   ├── Accounts/         # Authentication & User management
│   │   ├── Admin/           # Admin-specific endpoints  
│   │   ├── Categories/      # Category management
│   │   ├── Orchids/         # Orchid product management
│   │   └── Orders/          # Order management
│   ├── Program.cs           # Application startup
│   └── appsettings.json     # Configuration
│
├── OrchidStore.Application/
│   ├── Features/            # Feature-based organization
│   ├── CQRS/               # Command/Query interfaces
│   ├── Logics/             # Business logic services
│   └── Repositories/       # Repository interfaces
│
├── OrchidStore.Domain/
│   ├── WriteModels/        # Domain entities
│   └── ReadModels/         # Read-only collections
│
└── OrchidStore.Infrastructure/
    ├── Data/               # DbContext & configurations
    ├── Repositories/       # Repository implementations
    ├── Logics/            # External service integrations
    └── Migrations/        # Database migrations
```

### 🎨 **Frontend Structure**
```
FrontEnd/orchid-store/
├── src/
│   ├── app/
│   │   ├── home/          # Homepage
│   │   ├── admin/         # Admin dashboard
│   │   ├── orchid/        # Product pages
│   │   └── OrderCallback/ # Payment callback
│   │
│   ├── components/
│   │   ├── Header.tsx     # Navigation component
│   │   ├── Footer.tsx     # Footer component
│   │   └── UI/           # Reusable UI components
│   │
│   ├── contexts/
│   │   ├── AuthContext.tsx   # Authentication state
│   │   └── CartContext.tsx   # Shopping cart state
│   │
│   ├── config/
│   │   └── api.ts         # API configuration
│   │
│   └── types/
│       ├── auth.ts        # Authentication types
│       └── orchid.ts      # Product types
```

## 📊 Database Schema

### 🔹 **Core Entities**
- **Account**: Thông tin tài khoản người dùng
- **Role**: Vai trò (Customer/Admin)
- **Category**: Danh mục hoa lan
- **Orchid**: Sản phẩm hoa lan
- **Order**: Đơn hàng
- **OrderDetail**: Chi tiết đơn hàng

### 🔸 **Key Relationships**
```
Account 1--* Order
Order 1--* OrderDetail
OrderDetail *--1 Orchid
Orchid *--1 Category
Account *--1 Role
```

## 🚀 Hướng dẫn cài đặt

### 📋 **Yêu cầu hệ thống**
- .NET 8.0 SDK
- Node.js 18+ & npm
- PostgreSQL 14+
- Visual Studio/VS Code (khuyên dùng)

### 🔧 **Cài đặt Backend**

1. **Clone repository**
```bash
git clone <repository-url>
cd LabPrn231/BackEnd/OrchidStore
```

2. **Cấu hình database**
```bash
# Tạo database PostgreSQL
createdb OrchidStoreDB

# Update connection string trong appsettings.json
"ConnectionStrings": {
    "OrchidStoreDB": "Server=localhost;Database=OrchidStoreDB;User Id=your_user;Password=your_password;"
}
```

3. **Restore packages & Run migrations**
```bash
dotnet restore
dotnet ef database update
```

4. **Chạy ứng dụng**
```bash
dotnet run --project OrchidStore.API
```

Backend sẽ chạy tại: `https://localhost:7157`

### 🎨 **Cài đặt Frontend**

1. **Di chuyển đến thư mục frontend**
```bash
cd LabPrn231/FrontEnd/orchid-store
```

2. **Cài đặt dependencies**
```bash
npm install
```

3. **Chạy development server**
```bash
npm run dev
```

Frontend sẽ chạy tại: `http://localhost:3000`

## ⚙️ Cấu hình môi trường

### 🔹 **Backend Configuration**
Cập nhật `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "OrchidStoreDB": "your_postgresql_connection_string"
  },
  "CloudinarySettings": {
    "CloudName": "your_cloudinary_cloud_name",
    "ApiKey": "your_cloudinary_api_key", 
    "ApiSecret": "your_cloudinary_api_secret"
  },
  "MomoAPI": {
    "MomoApiUrl": "https://test-payment.momo.vn/gw_payment/transactionProcessor",
    "SecretKey": "your_momo_secret_key",
    "AccessKey": "your_momo_access_key",
    "ReturnUrl": "http://localhost:3000/OrderCallback",
    "PartnerCode": "MOMO"
  }
}
```

### 🔸 **Frontend Configuration**
Cập nhật file cấu hình API trong `src/config/api.ts` nếu cần thay đổi endpoints.

## 📚 API Documentation

### 🔐 **Authentication Endpoints**
- `POST /api/v1/Auth/token` - Đăng nhập
- `GET /api/v1/Auth/SelectToken` - Lấy thông tin token
- `POST /api/v1/InsertAccount` - Đăng ký tài khoản

### 🌺 **Product Endpoints**
- `GET /api/v1/SelectOrchids` - Lấy danh sách hoa lan
- `GET /api/v1/SelectOrchidDetail/{id}` - Chi tiết hoa lan
- `POST /api/v1/InsertOrchid` - Thêm hoa lan (Admin)
- `PUT /api/v1/UpdateOrchid` - Cập nhật hoa lan (Admin)

### 📂 **Category Endpoints**
- `GET /api/v1/SelectCategories` - Lấy danh sách danh mục
- `POST /api/v1/InsertCategory` - Thêm danh mục (Admin)

### 📦 **Order Endpoints**
- `GET /api/v1/SelectOrders` - Lấy danh sách đơn hàng
- `POST /api/v1/InsertOrder` - Tạo đơn hàng mới
- `PUT /api/v1/UpdateOrder` - Cập nhật đơn hàng

### 💳 **Payment Endpoints**
- `POST /api/v1/MomoOrderLogic` - Tạo link thanh toán MoMo
- `POST /api/v1/MomoOrderLogicReturn` - Xử lý callback từ MoMo

## 🔒 Security Features

- **JWT Authentication**: Sử dụng OpenIddict cho OAuth2/OpenID Connect
- **Role-based Authorization**: Phân quyền Admin/Customer
- **CORS Policy**: Cấu hình CORS cho cross-origin requests
- **Password Hashing**: Mã hóa mật khẩu an toàn
- **SQL Injection Prevention**: Sử dụng Entity Framework Core
- **XSS Protection**: Validation đầu vào

## 🧪 Testing

### 🔧 **Backend Testing**
```bash
# Chạy unit tests
dotnet test

# Kiểm tra API với Swagger
# Truy cập: https://localhost:7157/swagger
```

### 🎨 **Frontend Testing**
```bash
# Lint checking
npm run lint

# Build production
npm run build
```

## 📈 Performance Optimizations

- **Database Indexing**: Tối ưu query với indexes
- **Caching**: In-memory caching cho data thường xuyên truy cập
- **Image Optimization**: Cloudinary auto-optimization
- **Pagination**: Phân trang cho danh sách sản phẩm
- **Lazy Loading**: Next.js dynamic imports
- **CDN**: Cloudinary CDN cho static assets

## 🐛 Troubleshooting

### 🔹 **Backend Issues**
- **Database Connection**: Kiểm tra PostgreSQL service
- **Port Conflicts**: Đổi port trong `launchSettings.json`
- **Migration Errors**: Chạy lại `dotnet ef database update`

### 🔸 **Frontend Issues**
- **CORS Errors**: Kiểm tra backend CORS configuration
- **API Connection**: Verify API endpoints trong `config/api.ts`
- **Build Errors**: Chạy `npm ci` để clean install

## 👥 Đóng góp

1. Fork the repository
2. Create feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📝 License

Distributed under the MIT License. See `LICENSE` for more information.

## 📞 Liên hệ

- **Developer**: [Your Name]
- **Email**: [your.email@example.com]
- **Project Link**: [https://github.com/your-username/orchid-store](https://github.com/your-username/orchid-store)

## 🙏 Acknowledgments

- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- [Next.js Documentation](https://nextjs.org/docs)
- [OpenIddict](https://documentation.openiddict.com/)
- [Marten Event Store](https://martendb.io/)
- [Cloudinary](https://cloudinary.com/)
- [MoMo Payment](https://developers.momo.vn/)

---

⭐ **Nếu dự án này hữu ích, hãy cho chúng tôi một star!** ⭐
