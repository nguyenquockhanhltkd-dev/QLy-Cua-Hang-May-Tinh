# 🖥️ QL Cửa Hàng Máy Tính

Phần mềm **Quản Lý Cửa Hàng Máy Tính** — ứng dụng desktop xây dựng bằng **C# WinForms** và **SQL Server**, theo kiến trúc 3 lớp (DAL / BLL / UI).

> Dự án môn Lập trình .NET — Sinh viên năm 2

---

## 📋 Tính Năng Chính

| Module | Chức năng |
|---|---|
| 🔐 **Đăng nhập** | Xác thực BCrypt, phân quyền Admin / Staff |
| 📦 **Sản phẩm** | CRUD sản phẩm + tab thông số kỹ thuật theo từng loại (Laptop, Desktop, Monitor…) |
| 🏷️ **Danh mục** | Quản lý danh mục sản phẩm |
| 🏭 **Nhà cung cấp** | CRUD + lịch sử nhập hàng theo NCC |
| 📥 **Nhập kho** | Tạo phiếu nhập → xác nhận → cập nhật tồn kho & giá vốn |
| 📤 **Xuất kho** | Tạo phiếu xuất → kiểm tra tồn khả dụng → xác nhận → trừ tồn |
| 🛒 **Đơn hàng** | Đơn bán online: chọn SP, nhập thông tin KH, xác nhận → trừ tồn |
| 📊 **Tồn kho** | Xem tồn kho, cảnh báo sắp hết, chi tiết lô hàng |
| 📈 **Báo cáo** | KPI tổng quan, biểu đồ doanh thu, thống kê nhập/xuất, top sản phẩm |
| 👤 **Người dùng** | Quản lý tài khoản (Admin only), reset mật khẩu |

### Phân Quyền

| Role | Quyền hạn |
|---|---|
| **Admin** | Toàn quyền: CRUD tất cả, xem báo cáo, quản lý user |
| **Staff** | Nhập/xuất kho, đơn hàng, xem sản phẩm & tồn kho |

---

## 🛠️ Công Nghệ Sử Dụng

| Thành phần | Chi tiết |
|---|---|
| Ngôn ngữ | C# (.NET 8) |
| UI | Windows Forms (WinForms) |
| Database | SQL Server 2016+ |
| ORM | ADO.NET thuần (không dùng Entity Framework) |
| Bảo mật | BCrypt.Net-Next (hash mật khẩu) |
| Kiến trúc | 3 lớp: Models → DAL → BLL → UI |

---

## 📁 Cấu Trúc Dự Án

```
QLCuaHangMayTinh/
├── database.sql                          ← Script tạo DB (chạy trong SSMS)
├── ai-docs/                              ← Tài liệu thiết kế
└── project/
    ├── QLCuaHangMayTinh.slnx             ← Solution file
    ├── QLCuaHangMayTinh.Models/          ← Entity & DTO models
    ├── QLCuaHangMayTinh.DAL/             ← Data Access Layer (ADO.NET)
    ├── QLCuaHangMayTinh.BLL/             ← Business Logic Layer
    └── QLCuaHangMayTinh.UI/              ← WinForms Presentation Layer
```

---

## ⚙️ Yêu Cầu Hệ Thống

- **Windows** 10/11
- **.NET 8 SDK** — [Tải tại đây](https://dotnet.microsoft.com/download/dotnet/8.0)
- **SQL Server 2016+** (hoặc SQL Server Express) — [Tải tại đây](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
- **SQL Server Management Studio (SSMS)** — [Tải tại đây](https://aka.ms/ssmsfullsetup)
- **Visual Studio 2022** (khuyên dùng) hoặc VS Code

---

## 🚀 Hướng Dẫn Cài Đặt & Chạy

### Bước 1 — Clone repository

```bash
git clone https://github.com/huymonsterhuman-eng/Project-qu-n-l-c-a-h-ng-m-y-t-nh-m-n.Net-.git
cd Project-qu-n-l-c-a-h-ng-m-y-t-nh-m-n.Net-
```

### Bước 2 — Tạo Database

1. Mở **SQL Server Management Studio (SSMS)**
2. Kết nối vào SQL Server instance của bạn
3. Mở file `database.sql` (File → Open → File…)
4. Nhấn **F5** hoặc bấm **Execute** để chạy toàn bộ script

> Script sẽ tự động tạo database `QLCuaHangMayTinh`, tất cả bảng, stored procedures, view và dữ liệu mẫu.

### Bước 3 — Cấu Hình Chuỗi Kết Nối

Mở file `project/QLCuaHangMayTinh.DAL/DBConnection.cs`, tìm dòng connection string và sửa cho phù hợp:

```csharp
// Thay "YOUR_SERVER_NAME" bằng tên SQL Server instance của bạn
// Ví dụ: "localhost", ".\SQLEXPRESS", "DESKTOP-ABC\SQLEXPRESS"
private const string ConnectionString =
    "Server=YOUR_SERVER_NAME;Database=QLCuaHangMayTinh;Trusted_Connection=True;TrustServerCertificate=True;";
```

**Các giá trị Server phổ biến:**

| Loại SQL Server | Giá trị Server |
|---|---|
| SQL Server Developer/Express mặc định | `localhost` hoặc `.` |
| SQL Server Express | `.\SQLEXPRESS` hoặc `localhost\SQLEXPRESS` |
| Named instance | `TEN_MAY\TEN_INSTANCE` |

### Bước 4 — Mở & Chạy Dự Án

**Cách 1 — Visual Studio 2022 (khuyên dùng):**
1. Mở Visual Studio 2022
2. Chọn **File → Open → Project/Solution**
3. Chọn file `project/QLCuaHangMayTinh.slnx`
4. Nhấn **F5** hoặc bấm nút ▶ **Start** để build và chạy

**Cách 2 — Command Line:**
```bash
cd project
dotnet build
dotnet run --project QLCuaHangMayTinh.UI
```

### Bước 5 — Đăng Nhập

Sau khi chạy, dùng tài khoản mặc định:

| Thông tin | Giá trị |
|---|---|
| Username | `admin` |
| Password | `Admin@123` |

---

## 📸 Giao Diện

| Màn hình | Mô tả |
|---|---|
| Dashboard | KPI tổng quan tháng, biểu đồ doanh thu 12 tháng |
| Sản phẩm | Danh sách, tìm kiếm, highlight hàng sắp hết |
| Nhập / Xuất kho | Form chi tiết phiếu, kiểm tra tồn khả dụng real-time |
| Báo cáo | 4 tab: Tổng quan, Nhập, Xuất, Top sản phẩm |

---

## 🗄️ Sơ Đồ Database (Tóm Tắt)

```
Users ──────────────────────────────────────┐
Categories ──── Products ─── ProductSpecs   │
                    │                        │
Suppliers ──── ImportOrders ── ImportOrderDetails
                Products
                    │
              ExportOrders ── ExportOrderDetails
              SalesOrders  ── SalesOrderDetails
```

**11 bảng chính** + 3 Stored Procedures + 1 View (`vw_StockReport`)

---

## 📦 NuGet Packages

| Package | Project | Mục đích |
|---|---|---|
| `Microsoft.Data.SqlClient` 5.x | DAL | Kết nối SQL Server |
| `BCrypt.Net-Next` 4.x | BLL | Hash mật khẩu |

---

## 🐛 Lỗi Thường Gặp

**Lỗi kết nối database:**
> `A network-related or instance-specific error occurred...`

→ Kiểm tra lại tên Server trong `DBConnection.cs`. Đảm bảo SQL Server đang chạy (Services → SQL Server).

**Lỗi build thiếu package:**
```bash
cd project
dotnet restore
```

**SQL Server không nhận Windows Auth:**
→ Thêm `Integrated Security=True` hoặc dùng SQL Auth:
```
Server=localhost;Database=QLCuaHangMayTinh;User Id=sa;Password=YourPassword;TrustServerCertificate=True;
```

---

## 👨‍💻 Tác Giả

**Lương Quốc Huy** — Sinh viên ngành Công nghệ thông tin

---

## 📄 License

Dự án học thuật — MIT License
