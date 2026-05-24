-- ============================================================
-- DATABASE: QLCuaHangMayTinh
-- Compatible: SQL Server 2016+
-- ============================================================

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'QLCuaHangMayTinh')
BEGIN
    CREATE DATABASE QLCuaHangMayTinh
    COLLATE Vietnamese_CI_AS;
END
GO

USE QLCuaHangMayTinh;
GO

-- ============================================================
-- CLEANUP: Drop tất cả bảng theo thứ tự reverse FK dependency
-- Đảm bảo script chạy lại an toàn trên DB đã có dữ liệu
-- ============================================================
IF OBJECT_ID('dbo.SalesOrderDetails',  'U') IS NOT NULL DROP TABLE dbo.SalesOrderDetails;
IF OBJECT_ID('dbo.SalesOrders',        'U') IS NOT NULL DROP TABLE dbo.SalesOrders;
IF OBJECT_ID('dbo.ExportOrderDetails', 'U') IS NOT NULL DROP TABLE dbo.ExportOrderDetails;
IF OBJECT_ID('dbo.ImportOrderDetails', 'U') IS NOT NULL DROP TABLE dbo.ImportOrderDetails;
IF OBJECT_ID('dbo.ProductSpecs',       'U') IS NOT NULL DROP TABLE dbo.ProductSpecs;
IF OBJECT_ID('dbo.ExportOrders',       'U') IS NOT NULL DROP TABLE dbo.ExportOrders;
IF OBJECT_ID('dbo.ImportOrders',       'U') IS NOT NULL DROP TABLE dbo.ImportOrders;
IF OBJECT_ID('dbo.Products',           'U') IS NOT NULL DROP TABLE dbo.Products;
IF OBJECT_ID('dbo.Categories',         'U') IS NOT NULL DROP TABLE dbo.Categories;
IF OBJECT_ID('dbo.Suppliers',          'U') IS NOT NULL DROP TABLE dbo.Suppliers;
IF OBJECT_ID('dbo.Users',              'U') IS NOT NULL DROP TABLE dbo.Users;
GO

-- ============================================================
-- TABLE 1: Users — Quản lý tài khoản người dùng
-- ============================================================
CREATE TABLE dbo.Users (
    UserID       INT           NOT NULL IDENTITY(1,1),
    Username     NVARCHAR(50)  NOT NULL,
    PasswordHash NVARCHAR(255) NOT NULL,
    FullName     NVARCHAR(100) NOT NULL,
    Role         NVARCHAR(20)  NOT NULL DEFAULT 'Staff',   -- 'Admin' | 'Staff'
    IsActive     BIT           NOT NULL DEFAULT 1,
    CreatedAt    DATETIME      NOT NULL DEFAULT GETDATE(),
    CONSTRAINT PK_Users        PRIMARY KEY (UserID),
    CONSTRAINT UQ_Users_Name   UNIQUE      (Username),
    CONSTRAINT CK_Users_Role   CHECK       (Role IN ('Admin', 'Staff'))
);
GO

-- ============================================================
-- TABLE 2: Categories — Danh mục sản phẩm
-- ============================================================
CREATE TABLE dbo.Categories (
    CategoryID   INT           NOT NULL IDENTITY(1,1),
    Name         NVARCHAR(100) NOT NULL,
    Description  NVARCHAR(255) NULL,
    CONSTRAINT PK_Categories   PRIMARY KEY (CategoryID),
    CONSTRAINT UQ_Categories_Name UNIQUE (Name)
);
GO

-- ============================================================
-- TABLE 3: Suppliers — Nhà cung cấp
-- ============================================================
CREATE TABLE dbo.Suppliers (
    SupplierID   INT           NOT NULL IDENTITY(1,1),
    Name         NVARCHAR(150) NOT NULL,
    Phone        NVARCHAR(20)  NULL,
    Email        NVARCHAR(100) NULL,
    Address      NVARCHAR(255) NULL,
    ContactPerson NVARCHAR(100) NULL,
    IsActive     BIT           NOT NULL DEFAULT 1,
    CONSTRAINT PK_Suppliers    PRIMARY KEY (SupplierID)
);
GO

-- ============================================================
-- TABLE 4: Products — Sản phẩm
-- ============================================================
CREATE TABLE dbo.Products (
    ProductID    INT            NOT NULL IDENTITY(1,1),
    CategoryID   INT            NOT NULL,
    ProductCode  NVARCHAR(30)   NOT NULL,   -- Mã sản phẩm (VD: LAP-001)
    Name         NVARCHAR(200)  NOT NULL,
    Brand        NVARCHAR(100)  NULL,       -- Hãng: Dell, HP, Asus...
    Description  NVARCHAR(1000) NULL,
    Unit         NVARCHAR(30)   NOT NULL DEFAULT N'Cái',
    CostPrice    DECIMAL(18,2)  NOT NULL DEFAULT 0,   -- Giá nhập
    SellPrice    DECIMAL(18,2)  NOT NULL DEFAULT 0,   -- Giá bán
    StockQty     INT            NOT NULL DEFAULT 0,   -- Số lượng tồn kho hiện tại
    MinStockQty  INT            NOT NULL DEFAULT 5,   -- Ngưỡng cảnh báo tồn thấp
    ImagePath    NVARCHAR(500)  NULL,                 -- Đường dẫn ảnh sản phẩm
    IsActive     BIT            NOT NULL DEFAULT 1,
    CreatedAt    DATETIME       NOT NULL DEFAULT GETDATE(),
    UpdatedAt    DATETIME       NOT NULL DEFAULT GETDATE(),
    CONSTRAINT PK_Products       PRIMARY KEY (ProductID),
    CONSTRAINT UQ_Products_Code  UNIQUE      (ProductCode),
    CONSTRAINT FK_Products_Cat   FOREIGN KEY (CategoryID) REFERENCES dbo.Categories(CategoryID),
    CONSTRAINT CK_Products_Cost  CHECK (CostPrice >= 0),
    CONSTRAINT CK_Products_Sell  CHECK (SellPrice >= 0),
    CONSTRAINT CK_Products_Stock CHECK (StockQty >= 0)
);
GO

-- ============================================================
-- TABLE 5: ProductSpecs — Thông số kỹ thuật chi tiết sản phẩm
-- (Mỗi loại sản phẩm: Laptop, Desktop, Màn hình, Linh kiện...)
-- ============================================================
CREATE TABLE dbo.ProductSpecs (
    SpecID       INT            NOT NULL IDENTITY(1,1),
    ProductID    INT            NOT NULL,

    -- Thông tin chung (dùng cho nhiều loại)
    ProductType  NVARCHAR(50)   NULL,  -- 'Laptop','Desktop','Monitor','Component','Peripheral'

    -- ── CPU / Processor ──────────────────────────────────────
    CPU          NVARCHAR(200)  NULL,  -- VD: Intel Core i5-1235U
    CPUSpeed     NVARCHAR(50)   NULL,  -- VD: 3.3 GHz ~ 4.4 GHz
    CPUCores     INT            NULL,  -- Số nhân

    -- ── RAM ──────────────────────────────────────────────────
    RAM          NVARCHAR(100)  NULL,  -- VD: 16GB DDR4
    RAMSlots     INT            NULL,  -- Số khe RAM
    RAMMaxGB     INT            NULL,  -- RAM tối đa hỗ trợ (GB)

    -- ── Lưu trữ ──────────────────────────────────────────────
    Storage      NVARCHAR(150)  NULL,  -- VD: 512GB SSD NVMe
    StorageType  NVARCHAR(30)   NULL,  -- 'SSD','HDD','SSD+HDD'
    StorageSlots INT            NULL,  -- Số khe lưu trữ

    -- ── Màn hình (Laptop / Monitor) ──────────────────────────
    ScreenSize   DECIMAL(5,1)   NULL,  -- Inch, VD: 15.6
    Resolution   NVARCHAR(50)   NULL,  -- VD: 1920x1080 (FHD)
    PanelType    NVARCHAR(30)   NULL,  -- IPS, VA, TN, OLED
    RefreshRate  INT            NULL,  -- Hz, VD: 144
    Brightness   INT            NULL,  -- nits

    -- ── Card đồ họa ──────────────────────────────────────────
    GPU          NVARCHAR(200)  NULL,  -- VD: NVIDIA GeForce RTX 4060
    VRAM         NVARCHAR(50)   NULL,  -- VD: 8GB GDDR6

    -- ── Kết nối ──────────────────────────────────────────────
    Ports        NVARCHAR(300)  NULL,  -- VD: 2x USB-A, 1x USB-C, HDMI, SD
    Wireless     NVARCHAR(100)  NULL,  -- VD: WiFi 6, Bluetooth 5.2
    HasEthernet  BIT            NULL,

    -- ── Pin (Laptop) ─────────────────────────────────────────
    BatteryWhr   INT            NULL,  -- Wh, VD: 72
    BatteryLife  NVARCHAR(50)   NULL,  -- VD: Lên đến 9 giờ

    -- ── Hệ điều hành ─────────────────────────────────────────
    OperatingSystem NVARCHAR(100) NULL, -- VD: Windows 11 Home

    -- ── Thông tin vật lý ─────────────────────────────────────
    WeightKg     DECIMAL(5,2)   NULL,  -- Kg
    Dimensions   NVARCHAR(100)  NULL,  -- VD: 35.9 x 23.3 x 1.99 cm
    Color        NVARCHAR(50)   NULL,  -- Màu sắc

    -- ── Bảo hành ─────────────────────────────────────────────
    WarrantyMonths INT          NULL,  -- Số tháng bảo hành, VD: 24
    WarrantyNote NVARCHAR(200)  NULL,  -- VD: Bảo hành hãng toàn quốc

    -- ── Thông số mạng (Router, Switch) ───────────────────────
    NetworkSpeed NVARCHAR(100)  NULL,  -- VD: 1Gbps
    WifiStandard NVARCHAR(50)   NULL,  -- VD: Wi-Fi 6E (802.11ax)

    -- ── Thông số nguồn điện (PSU) / Linh kiện ────────────────
    PowerWatt    INT            NULL,  -- Công suất (W)
    Socket       NVARCHAR(50)   NULL,  -- VD: LGA1700, AM5

    -- ── Ghi chú kỹ thuật thêm ────────────────────────────────
    ExtraSpecs   NVARCHAR(1000) NULL,  -- Các thông số khác không có cột riêng

    CONSTRAINT PK_ProductSpecs     PRIMARY KEY (SpecID),
    CONSTRAINT FK_Specs_Product    FOREIGN KEY (ProductID) REFERENCES dbo.Products(ProductID)
        ON DELETE CASCADE
);
GO

-- ============================================================
-- TABLE 6: ImportOrders — Phiếu nhập kho
-- ============================================================
CREATE TABLE dbo.ImportOrders (
    ImportID     INT            NOT NULL IDENTITY(1,1),
    ImportCode   NVARCHAR(30)   NOT NULL,   -- Mã phiếu nhập (VD: NK-20240417-001)
    SupplierID   INT            NOT NULL,
    UserID       INT            NOT NULL,
    ImportDate   DATETIME       NOT NULL DEFAULT GETDATE(),
    TotalAmount  DECIMAL(18,2)  NOT NULL DEFAULT 0,
    Status       NVARCHAR(20)   NOT NULL DEFAULT N'Pending', -- Pending | Confirmed | Cancelled
    Note         NVARCHAR(500)  NULL,
    CONSTRAINT PK_ImportOrders     PRIMARY KEY (ImportID),
    CONSTRAINT UQ_ImportOrders_Code UNIQUE     (ImportCode),
    CONSTRAINT FK_Import_Supplier  FOREIGN KEY (SupplierID) REFERENCES dbo.Suppliers(SupplierID),
    CONSTRAINT FK_Import_User      FOREIGN KEY (UserID)     REFERENCES dbo.Users(UserID),
    CONSTRAINT CK_Import_Status    CHECK (Status IN (N'Pending', N'Confirmed', N'Cancelled'))
);
GO

-- ============================================================
-- TABLE 7: ImportOrderDetails — Chi tiết phiếu nhập
-- ============================================================
CREATE TABLE dbo.ImportOrderDetails (
    DetailID     INT            NOT NULL IDENTITY(1,1),
    ImportID     INT            NOT NULL,
    ProductID    INT            NOT NULL,
    Quantity     INT            NOT NULL,
    UnitPrice    DECIMAL(18,2)  NOT NULL,
    SubTotal     AS (Quantity * UnitPrice) PERSISTED,  -- Cột tính tự động
    CONSTRAINT PK_ImportDetails       PRIMARY KEY (DetailID),
    CONSTRAINT FK_ImpDetail_Order     FOREIGN KEY (ImportID)   REFERENCES dbo.ImportOrders(ImportID)
        ON DELETE CASCADE,
    CONSTRAINT FK_ImpDetail_Product   FOREIGN KEY (ProductID)  REFERENCES dbo.Products(ProductID),
    CONSTRAINT CK_ImpDetail_Qty       CHECK (Quantity > 0),
    CONSTRAINT CK_ImpDetail_Price     CHECK (UnitPrice >= 0)
);
GO

-- ============================================================
-- TABLE 8: ExportOrders — Phiếu xuất kho
-- ============================================================
CREATE TABLE dbo.ExportOrders (
    ExportID      INT            NOT NULL IDENTITY(1,1),
    ExportCode    NVARCHAR(30)   NOT NULL,   -- Mã phiếu xuất (VD: XK-20240417-001)
    UserID        INT            NOT NULL,
    CustomerName  NVARCHAR(150)  NULL,
    CustomerPhone NVARCHAR(20)   NULL,
    ExportDate    DATETIME       NOT NULL DEFAULT GETDATE(),
    TotalAmount   DECIMAL(18,2)  NOT NULL DEFAULT 0,
    Status        NVARCHAR(20)   NOT NULL DEFAULT N'Pending',
    Note          NVARCHAR(500)  NULL,
    CONSTRAINT PK_ExportOrders       PRIMARY KEY (ExportID),
    CONSTRAINT UQ_ExportOrders_Code  UNIQUE     (ExportCode),
    CONSTRAINT FK_Export_User        FOREIGN KEY (UserID) REFERENCES dbo.Users(UserID),
    CONSTRAINT CK_Export_Status      CHECK (Status IN (N'Pending', N'Confirmed', N'Cancelled'))
);
GO

-- ============================================================
-- TABLE 9: ExportOrderDetails — Chi tiết phiếu xuất
-- ============================================================
CREATE TABLE dbo.ExportOrderDetails (
    DetailID     INT            NOT NULL IDENTITY(1,1),
    ExportID     INT            NOT NULL,
    ProductID    INT            NOT NULL,
    Quantity     INT            NOT NULL,
    UnitPrice    DECIMAL(18,2)  NOT NULL,
    SubTotal     AS (Quantity * UnitPrice) PERSISTED,
    CONSTRAINT PK_ExportDetails       PRIMARY KEY (DetailID),
    CONSTRAINT FK_ExpDetail_Order     FOREIGN KEY (ExportID)   REFERENCES dbo.ExportOrders(ExportID)
        ON DELETE CASCADE,
    CONSTRAINT FK_ExpDetail_Product   FOREIGN KEY (ProductID)  REFERENCES dbo.Products(ProductID),
    CONSTRAINT CK_ExpDetail_Qty       CHECK (Quantity > 0),
    CONSTRAINT CK_ExpDetail_Price     CHECK (UnitPrice >= 0)
);
GO

-- ============================================================
-- SEED DATA MẪU
-- ============================================================

-- Tài khoản Admin mặc định (password: Admin@123 — hash bằng BCrypt)
INSERT INTO dbo.Users (Username, PasswordHash, FullName, Role)
VALUES (N'admin', N'$2a$11$wIvqMmxlzm0qcU4/l5RcV.h4Us.g2cpWT0GJYb55UgzodUhIvlCbS', N'Quản trị viên', N'Admin');
GO

-- Danh mục
INSERT INTO dbo.Categories (Name, Description) VALUES
    (N'Laptop',       N'Máy tính xách tay'),
    (N'Desktop',      N'Máy tính để bàn'),
    (N'Màn hình',     N'Monitor'),
    (N'Linh kiện',    N'CPU, RAM, SSD, VGA...'),
    (N'Ngoại vi',     N'Bàn phím, chuột, tai nghe'),
    (N'Mạng',         N'Router, Switch, Access Point'),
    (N'Nguồn & Case', N'Nguồn máy tính, vỏ case');
GO

-- Nhà cung cấp mẫu
INSERT INTO dbo.Suppliers (Name, Phone, Email, Address, ContactPerson) VALUES
    (N'Công ty TNHH Dell Việt Nam',  N'0909111222', N'supply@dell.vn',  N'TP.HCM', N'Nguyễn Văn A'),
    (N'Phân phối HP Chính hãng',     N'0909333444', N'hp@supply.vn',    N'Hà Nội', N'Trần Thị B'),
    (N'Asus Authorized Distributor', N'0909555666', N'asus@dist.vn',    N'Đà Nẵng', N'Lê Văn C');
GO

-- Sản phẩm mẫu — StockQty khởi tạo = 0, tồn kho sẽ được cộng qua sp_ConfirmImport
-- CategoryID: 1=Laptop, 2=Desktop, 3=Màn hình, 4=Linh kiện, 5=Ngoại vi, 6=Mạng, 7=Nguồn & Case
INSERT INTO dbo.Products (CategoryID, ProductCode, Name, Brand, Description, Unit, CostPrice, SellPrice, StockQty, MinStockQty, IsActive) VALUES
    (1, N'LTP-001', N'Dell Inspiron 15 3520',          N'Dell',      N'Intel Core i5-1235U, RAM 8GB, SSD 512GB, 15.6" FHD',       N'Cái',  12500000, 15990000, 0, 3,  1),
    (1, N'LTP-002', N'Asus VivoBook 14 X1404',         N'Asus',      N'Intel Core i3-1215U, RAM 8GB, SSD 256GB, 14" FHD',         N'Cái',  10200000, 12990000, 0, 3,  1),
    (1, N'LTP-003', N'HP Pavilion 15-eg3068TX',        N'HP',        N'Intel Core i7-1355U, RAM 16GB, SSD 512GB, 15.6" FHD IPS',  N'Cái',  18000000, 22490000, 0, 3,  1),
    (2, N'DSK-001', N'PC Gaming Intel Core i5-13400F', N'Assembled', N'Core i5-13400F, RAM 16GB DDR4, SSD 512GB, RTX 3060',       N'Bộ',   18000000, 22000000, 0, 2,  1),
    (3, N'MON-001', N'LG 24MK430H-B 24"',             N'LG',        N'24" IPS FHD, 75Hz, AMD FreeSync, HDMI, VGA',               N'Cái',   2800000,  3590000, 0, 5,  1),
    (3, N'MON-002', N'Samsung 27" LS27C310EAEXXV',    N'Samsung',   N'27" IPS FHD, 75Hz, Eye Saver Mode, HDMI',                  N'Cái',   3500000,  4390000, 0, 5,  1),
    (4, N'CMP-001', N'RAM Kingston 8GB DDR4 3200',     N'Kingston',  N'DDR4 3200MHz, CL22, DIMM, tương thích Desktop',            N'Thanh',  550000,   750000, 0, 10, 1),
    (4, N'CMP-002', N'SSD Samsung 870 EVO 500GB',      N'Samsung',   N'SATA III 2.5", đọc 560MB/s, ghi 530MB/s',                 N'Cái',   1500000,  1990000, 0, 5,  1),
    (5, N'PHR-001', N'Chuột Logitech M235',            N'Logitech',  N'Không dây 2.4GHz, Nano receiver, pin AA, 12 tháng',        N'Cái',    220000,   320000, 0, 10, 1),
    (5, N'PHR-002', N'Bàn phím cơ Dare-U EK87',       N'Dare-U',    N'TKL 87 phím, switch xanh, đèn LED RGB, USB',              N'Cái',    380000,   520000, 0, 8,  1);
GO


-- ============================================================
-- SEED DATA: ProductSpecs — Thông số kỹ thuật cho 10 sản phẩm mẫu
-- ============================================================

-- LTP-001: Dell Inspiron 15 3520
INSERT INTO dbo.ProductSpecs (ProductID, ProductType, CPU, CPUSpeed, CPUCores, RAM, RAMSlots, RAMMaxGB, Storage, StorageType, StorageSlots, ScreenSize, Resolution, PanelType, RefreshRate, GPU, Ports, Wireless, HasEthernet, BatteryWhr, BatteryLife, OperatingSystem, WeightKg, Dimensions, WarrantyMonths, WarrantyNote)
SELECT ProductID, N'Laptop', N'Intel Core i5-1235U', N'1.3 ~ 4.4 GHz', 10,
       N'8GB DDR4 3200MHz', 2, 32, N'512GB SSD NVMe', N'SSD', 1,
       15.6, N'1920x1080 (FHD)', N'WVA', 60,
       N'Intel Iris Xe Graphics',
       N'1x USB 3.2 Gen1 Type-A, 1x USB 2.0, 1x USB-C 3.2, 1x HDMI 1.4, 1x SD Card, 1x RJ-45, 1x 3.5mm',
       N'WiFi 5 (802.11ac), Bluetooth 4.2', 1,
       54, N'Lên đến 8 giờ', N'Windows 11 Home',
       1.70, N'35.8 x 23.6 x 1.99 cm', 12, N'Bảo hành hãng Dell 12 tháng toàn quốc'
FROM dbo.Products WHERE ProductCode = N'LTP-001';

-- LTP-002: Asus VivoBook 14 X1404
INSERT INTO dbo.ProductSpecs (ProductID, ProductType, CPU, CPUSpeed, CPUCores, RAM, RAMSlots, RAMMaxGB, Storage, StorageType, StorageSlots, ScreenSize, Resolution, PanelType, RefreshRate, GPU, Ports, Wireless, HasEthernet, BatteryWhr, BatteryLife, OperatingSystem, WeightKg, Dimensions, WarrantyMonths, WarrantyNote)
SELECT ProductID, N'Laptop', N'Intel Core i3-1215U', N'1.2 ~ 4.4 GHz', 6,
       N'8GB DDR4 3200MHz', 1, 8, N'256GB SSD NVMe', N'SSD', 1,
       14.0, N'1920x1080 (FHD)', N'IPS', 60,
       N'Intel UHD Graphics',
       N'1x USB 3.2 Gen1 Type-A, 2x USB 2.0, 1x USB-C 3.2, 1x HDMI 1.4, 1x 3.5mm',
       N'WiFi 5 (802.11ac), Bluetooth 4.2', 0,
       42, N'Lên đến 7 giờ', N'Windows 11 Home',
       1.50, N'32.1 x 21.1 x 1.99 cm', 24, N'Bảo hành hãng Asus 24 tháng toàn quốc'
FROM dbo.Products WHERE ProductCode = N'LTP-002';

-- LTP-003: HP Pavilion 15-eg3068TX
INSERT INTO dbo.ProductSpecs (ProductID, ProductType, CPU, CPUSpeed, CPUCores, RAM, RAMSlots, RAMMaxGB, Storage, StorageType, StorageSlots, ScreenSize, Resolution, PanelType, RefreshRate, GPU, Ports, Wireless, HasEthernet, BatteryWhr, BatteryLife, OperatingSystem, WeightKg, Dimensions, WarrantyMonths, WarrantyNote)
SELECT ProductID, N'Laptop', N'Intel Core i7-1355U', N'1.7 ~ 5.0 GHz', 10,
       N'16GB DDR4 3200MHz', 2, 32, N'512GB SSD NVMe', N'SSD', 1,
       15.6, N'1920x1080 (FHD)', N'IPS', 60,
       N'Intel Iris Xe Graphics',
       N'2x USB 3.2 Gen1 Type-A, 1x USB 2.0, 1x USB-C 3.2, 1x HDMI 1.4, 1x SD Card, 1x 3.5mm',
       N'WiFi 5 (802.11ac), Bluetooth 5.0', 0,
       70, N'Lên đến 10 giờ', N'Windows 11 Home',
       1.75, N'35.8 x 23.9 x 1.99 cm', 12, N'Bảo hành hãng HP 12 tháng toàn quốc'
FROM dbo.Products WHERE ProductCode = N'LTP-003';

-- DSK-001: PC Gaming Intel Core i5-13400F
INSERT INTO dbo.ProductSpecs (ProductID, ProductType, CPU, CPUSpeed, CPUCores, RAM, RAMSlots, RAMMaxGB, Storage, StorageType, StorageSlots, GPU, VRAM, Ports, Wireless, HasEthernet, OperatingSystem, Socket, PowerWatt, WarrantyMonths, WarrantyNote)
SELECT ProductID, N'Desktop', N'Intel Core i5-13400F', N'2.5 ~ 4.6 GHz', 10,
       N'16GB DDR4 3200MHz', 4, 128, N'512GB SSD NVMe', N'SSD', 2,
       N'NVIDIA GeForce RTX 3060', N'12GB GDDR6',
       N'2x USB 3.2 Gen1, 4x USB 2.0, 1x HDMI 2.1, 1x DisplayPort 1.4, 1x RJ-45, 1x 3.5mm',
       N'WiFi 5 (802.11ac), Bluetooth 5.0', 1,
       N'Windows 11 Home', N'LGA1700', 650,
       12, N'Bảo hành linh kiện 12 tháng'
FROM dbo.Products WHERE ProductCode = N'DSK-001';

-- MON-001: LG 24MK430H-B 24"
INSERT INTO dbo.ProductSpecs (ProductID, ProductType, ScreenSize, Resolution, PanelType, RefreshRate, Brightness, Ports, HasEthernet, WarrantyMonths, WarrantyNote)
SELECT ProductID, N'Monitor', 24.0, N'1920x1080 (FHD)', N'IPS', 75, 250,
       N'1x HDMI 1.4, 1x VGA (D-Sub), 1x 3.5mm Audio Out',
       0, 36, N'Bảo hành hãng LG 36 tháng'
FROM dbo.Products WHERE ProductCode = N'MON-001';

-- MON-002: Samsung 27" LS27C310EAEXXV
INSERT INTO dbo.ProductSpecs (ProductID, ProductType, ScreenSize, Resolution, PanelType, RefreshRate, Brightness, Ports, HasEthernet, WarrantyMonths, WarrantyNote, ExtraSpecs)
SELECT ProductID, N'Monitor', 27.0, N'1920x1080 (FHD)', N'IPS', 75, 250,
       N'1x HDMI 1.4, 1x D-Sub (VGA)',
       0, 36, N'Bảo hành hãng Samsung 36 tháng',
       N'Eye Saver Mode, Flicker Free, AMD FreeSync'
FROM dbo.Products WHERE ProductCode = N'MON-002';

-- CMP-001: RAM Kingston 8GB DDR4 3200
INSERT INTO dbo.ProductSpecs (ProductID, ProductType, RAM, WarrantyMonths, WarrantyNote, ExtraSpecs)
SELECT ProductID, N'Component', N'8GB DDR4 3200MHz',
       36, N'Bảo hành Kingston 36 tháng',
       N'CL22, DIMM 288-pin, 1.35V, không tản nhiệt, tương thích Desktop'
FROM dbo.Products WHERE ProductCode = N'CMP-001';

-- CMP-002: SSD Samsung 870 EVO 500GB
INSERT INTO dbo.ProductSpecs (ProductID, ProductType, Storage, StorageType, WarrantyMonths, WarrantyNote, ExtraSpecs)
SELECT ProductID, N'Component', N'500GB SSD SATA III', N'SSD',
       60, N'Bảo hành Samsung 60 tháng',
       N'Đọc tuần tự 560MB/s, Ghi tuần tự 530MB/s, form factor 2.5", TLC V-NAND, có DRAM Cache'
FROM dbo.Products WHERE ProductCode = N'CMP-002';

-- PHR-001: Chuột Logitech M235
INSERT INTO dbo.ProductSpecs (ProductID, ProductType, Wireless, HasEthernet, WeightKg, Color, WarrantyMonths, WarrantyNote, ExtraSpecs)
SELECT ProductID, N'Peripheral',
       N'2.4GHz Nano USB Receiver', 0,
       0.08, N'Đen/Xám', 12, N'Bảo hành Logitech 12 tháng',
       N'Chuột không dây, DPI 1000, pin AA (ước tính 12 tháng), tương thích Windows/Mac/Chrome OS'
FROM dbo.Products WHERE ProductCode = N'PHR-001';

-- PHR-002: Bàn phím cơ Dare-U EK87
INSERT INTO dbo.ProductSpecs (ProductID, ProductType, Ports, Wireless, HasEthernet, Color, WarrantyMonths, WarrantyNote, ExtraSpecs)
SELECT ProductID, N'Peripheral',
       N'1x USB Type-A', NULL, 0,
       N'Đen', 12, N'Bảo hành Dare-U 12 tháng',
       N'TKL layout 87 phím, switch Blue (tactile + clicky), đèn LED RGB per-key, chống nước IPX4, N-Key Rollover'
FROM dbo.Products WHERE ProductCode = N'PHR-002';
GO

-- ============================================================
-- STORED PROCEDURE: Xác nhận phiếu nhập → cập nhật tồn kho
-- ============================================================
CREATE OR ALTER PROCEDURE dbo.sp_ConfirmImport
    @ImportID INT,
    @UserID   INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;

    -- Kiểm tra phiếu hợp lệ
    IF NOT EXISTS (SELECT 1 FROM dbo.ImportOrders WHERE ImportID = @ImportID AND Status = N'Pending')
    BEGIN
        RAISERROR(N'Phiếu nhập không tồn tại hoặc đã xử lý.', 16, 1);
        ROLLBACK;
        RETURN;
    END

    -- Cập nhật tồn kho
    UPDATE p
    SET p.StockQty  = p.StockQty + d.Quantity,
        p.CostPrice = d.UnitPrice,           -- Cập nhật giá nhập mới nhất
        p.UpdatedAt = GETDATE()
    FROM dbo.Products p
    INNER JOIN dbo.ImportOrderDetails d ON p.ProductID = d.ProductID
    WHERE d.ImportID = @ImportID;

    -- Cập nhật trạng thái phiếu
    UPDATE dbo.ImportOrders
    SET Status = N'Confirmed'
    WHERE ImportID = @ImportID;

    COMMIT;
END;
GO

-- ============================================================
-- STORED PROCEDURE: Xác nhận phiếu xuất → giảm tồn kho
-- ============================================================
CREATE OR ALTER PROCEDURE dbo.sp_ConfirmExport
    @ExportID INT,
    @UserID   INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;

    IF NOT EXISTS (SELECT 1 FROM dbo.ExportOrders WHERE ExportID = @ExportID AND Status = N'Pending')
    BEGIN
        RAISERROR(N'Phiếu xuất không tồn tại hoặc đã xử lý.', 16, 1);
        ROLLBACK;
        RETURN;
    END

    -- Kiểm tra tồn kho thực đủ không
    -- (AvailableQty đã được validate ở BLL khi tạo phiếu Pending;
    --  ở đây kiểm tra StockQty để đảm bảo an toàn trong transaction)
    IF EXISTS (
        SELECT 1
        FROM dbo.ExportOrderDetails d
        INNER JOIN dbo.Products p ON d.ProductID = p.ProductID
        WHERE d.ExportID = @ExportID AND d.Quantity > p.StockQty
    )
    BEGIN
        RAISERROR(N'Tồn kho không đủ cho một hoặc nhiều sản phẩm.', 16, 1);
        ROLLBACK;
        RETURN;
    END

    -- Trừ tồn kho thực (Products.StockQty)
    -- FIFO được thể hiện qua vw_InventoryBatchDetail (tính động từ tổng đã xuất confirmed)
    UPDATE p
    SET p.StockQty  = p.StockQty - d.Quantity,
        p.UpdatedAt = GETDATE()
    FROM dbo.Products p
    INNER JOIN dbo.ExportOrderDetails d ON p.ProductID = d.ProductID
    WHERE d.ExportID = @ExportID;

    -- Xác nhận phiếu
    UPDATE dbo.ExportOrders
    SET Status = N'Confirmed'
    WHERE ExportID = @ExportID;

    COMMIT;
END;
GO

-- ============================================================
-- VIEW: Báo cáo tồn kho hiện tại
-- ============================================================
CREATE OR ALTER VIEW dbo.vw_StockReport AS
    SELECT
        p.ProductID,
        p.ProductCode,
        p.Name         AS ProductName,
        p.Brand,
        c.Name         AS Category,
        p.StockQty,
        p.MinStockQty,
        p.CostPrice,
        p.SellPrice,
        p.StockQty - ISNULL((
            SELECT SUM(ed.Quantity)
            FROM dbo.ExportOrderDetails ed
            INNER JOIN dbo.ExportOrders eo ON eo.ExportID = ed.ExportID
            WHERE ed.ProductID = p.ProductID AND eo.Status = N'Pending'
        ), 0) AS AvailableQty,
        CASE
            WHEN p.StockQty = 0              THEN N'✗ Hết hàng'
            WHEN p.StockQty <= p.MinStockQty THEN N'⚠ Sắp hết hàng'
            ELSE                                  N'✓ Còn hàng'
        END AS StockStatus
    FROM dbo.Products     p
    INNER JOIN dbo.Categories c ON p.CategoryID = c.CategoryID
    WHERE p.IsActive = 1;
GO

-- ============================================================
-- VIEW: Chi tiết lô hàng theo phiếu nhập (FIFO — tính động)
-- RemainingQty = OriginalQty trừ đi phần đã xuất confirmed (FIFO tích lũy)
-- ============================================================
CREATE OR ALTER VIEW dbo.vw_InventoryBatchDetail AS
WITH BatchesCumulative AS (
    SELECT
        d.DetailID,
        d.ImportID,
        d.ProductID,
        io.ImportCode,
        io.ImportDate,
        d.Quantity                                             AS OriginalQty,
        d.UnitPrice,
        SUM(d.Quantity) OVER (
            PARTITION BY d.ProductID
            ORDER BY io.ImportDate ASC, d.DetailID ASC
            ROWS UNBOUNDED PRECEDING)                         AS CumQty,
        SUM(d.Quantity) OVER (
            PARTITION BY d.ProductID
            ORDER BY io.ImportDate ASC, d.DetailID ASC
            ROWS UNBOUNDED PRECEDING) - d.Quantity            AS PrevCumQty
    FROM dbo.ImportOrderDetails d
    INNER JOIN dbo.ImportOrders io ON io.ImportID = d.ImportID
    WHERE io.Status = N'Confirmed'
),
TotalExported AS (
    SELECT ed.ProductID,
           ISNULL(SUM(ed.Quantity), 0) AS TotalExp
    FROM dbo.ExportOrderDetails ed
    INNER JOIN dbo.ExportOrders eo ON eo.ExportID = ed.ExportID
    WHERE eo.Status = N'Confirmed'
    GROUP BY ed.ProductID
)
SELECT
    b.DetailID,
    b.ImportID,
    b.ProductID,
    p.ProductCode,
    p.Name        AS ProductName,
    b.ImportCode,
    b.ImportDate,
    b.OriginalQty,
    CASE
        WHEN b.PrevCumQty >= ISNULL(te.TotalExp, 0) THEN b.OriginalQty
        WHEN b.CumQty     <= ISNULL(te.TotalExp, 0) THEN 0
        ELSE b.CumQty - ISNULL(te.TotalExp, 0)
    END           AS RemainingQty,
    b.UnitPrice
FROM BatchesCumulative b
LEFT JOIN TotalExported      te ON te.ProductID = b.ProductID
INNER JOIN dbo.Products       p  ON p.ProductID  = b.ProductID;
GO

-- ============================================================
-- SEED DATA: ImportOrders + ImportOrderDetails + Confirm
-- SP đã được tạo ở trên → gọi được ở đây
-- 3 phiếu nhập theo nhà cung cấp — giải thích tồn kho ban đầu
-- ============================================================

-- Phiếu nhập 1 (Dell): LTP-001, DSK-001, PHR-001
INSERT INTO dbo.ImportOrders (ImportCode, SupplierID, UserID, ImportDate, TotalAmount, Status, Note)
VALUES (N'NK-20250101-001',
        (SELECT SupplierID FROM dbo.Suppliers WHERE Name = N'Công ty TNHH Dell Việt Nam'),
        (SELECT UserID     FROM dbo.Users     WHERE Username = N'admin'),
        '2025-01-01', 0, N'Pending', N'Nhập hàng đầu năm 2025');

INSERT INTO dbo.ImportOrderDetails (ImportID, ProductID, Quantity, UnitPrice) VALUES
    ((SELECT ImportID  FROM dbo.ImportOrders WHERE ImportCode  = N'NK-20250101-001'),
     (SELECT ProductID FROM dbo.Products     WHERE ProductCode = N'LTP-001'), 10, 12500000),
    ((SELECT ImportID  FROM dbo.ImportOrders WHERE ImportCode  = N'NK-20250101-001'),
     (SELECT ProductID FROM dbo.Products     WHERE ProductCode = N'DSK-001'),  4, 18000000),
    ((SELECT ImportID  FROM dbo.ImportOrders WHERE ImportCode  = N'NK-20250101-001'),
     (SELECT ProductID FROM dbo.Products     WHERE ProductCode = N'PHR-001'), 50,   220000);

UPDATE dbo.ImportOrders
SET TotalAmount = (SELECT SUM(SubTotal) FROM dbo.ImportOrderDetails
                   WHERE ImportID = (SELECT ImportID FROM dbo.ImportOrders WHERE ImportCode = N'NK-20250101-001'))
WHERE ImportCode = N'NK-20250101-001';

-- Phiếu nhập 2 (HP): LTP-003, MON-002, CMP-002
INSERT INTO dbo.ImportOrders (ImportCode, SupplierID, UserID, ImportDate, TotalAmount, Status, Note)
VALUES (N'NK-20250101-002',
        (SELECT SupplierID FROM dbo.Suppliers WHERE Name = N'Phân phối HP Chính hãng'),
        (SELECT UserID     FROM dbo.Users     WHERE Username = N'admin'),
        '2025-01-01', 0, N'Pending', N'Nhập hàng đầu năm 2025');

INSERT INTO dbo.ImportOrderDetails (ImportID, ProductID, Quantity, UnitPrice) VALUES
    ((SELECT ImportID  FROM dbo.ImportOrders WHERE ImportCode  = N'NK-20250101-002'),
     (SELECT ProductID FROM dbo.Products     WHERE ProductCode = N'LTP-003'),  5, 18000000),
    ((SELECT ImportID  FROM dbo.ImportOrders WHERE ImportCode  = N'NK-20250101-002'),
     (SELECT ProductID FROM dbo.Products     WHERE ProductCode = N'MON-002'), 10,  3500000),
    ((SELECT ImportID  FROM dbo.ImportOrders WHERE ImportCode  = N'NK-20250101-002'),
     (SELECT ProductID FROM dbo.Products     WHERE ProductCode = N'CMP-002'), 20,  1500000);

UPDATE dbo.ImportOrders
SET TotalAmount = (SELECT SUM(SubTotal) FROM dbo.ImportOrderDetails
                   WHERE ImportID = (SELECT ImportID FROM dbo.ImportOrders WHERE ImportCode = N'NK-20250101-002'))
WHERE ImportCode = N'NK-20250101-002';

-- Phiếu nhập 3 (Asus): LTP-002, MON-001, CMP-001, PHR-002
INSERT INTO dbo.ImportOrders (ImportCode, SupplierID, UserID, ImportDate, TotalAmount, Status, Note)
VALUES (N'NK-20250101-003',
        (SELECT SupplierID FROM dbo.Suppliers WHERE Name = N'Asus Authorized Distributor'),
        (SELECT UserID     FROM dbo.Users     WHERE Username = N'admin'),
        '2025-01-01', 0, N'Pending', N'Nhập hàng đầu năm 2025');

INSERT INTO dbo.ImportOrderDetails (ImportID, ProductID, Quantity, UnitPrice) VALUES
    ((SELECT ImportID  FROM dbo.ImportOrders WHERE ImportCode  = N'NK-20250101-003'),
     (SELECT ProductID FROM dbo.Products     WHERE ProductCode = N'LTP-002'),  8, 10200000),
    ((SELECT ImportID  FROM dbo.ImportOrders WHERE ImportCode  = N'NK-20250101-003'),
     (SELECT ProductID FROM dbo.Products     WHERE ProductCode = N'MON-001'), 15,  2800000),
    ((SELECT ImportID  FROM dbo.ImportOrders WHERE ImportCode  = N'NK-20250101-003'),
     (SELECT ProductID FROM dbo.Products     WHERE ProductCode = N'CMP-001'), 30,   550000),
    ((SELECT ImportID  FROM dbo.ImportOrders WHERE ImportCode  = N'NK-20250101-003'),
     (SELECT ProductID FROM dbo.Products     WHERE ProductCode = N'PHR-002'), 25,   380000);

UPDATE dbo.ImportOrders
SET TotalAmount = (SELECT SUM(SubTotal) FROM dbo.ImportOrderDetails
                   WHERE ImportID = (SELECT ImportID FROM dbo.ImportOrders WHERE ImportCode = N'NK-20250101-003'))
WHERE ImportCode = N'NK-20250101-003';
GO

-- Xác nhận 3 phiếu nhập → SP cộng tồn kho + set CostPrice
DECLARE @adminID   INT = (SELECT UserID   FROM dbo.Users         WHERE Username   = N'admin');
DECLARE @importID1 INT = (SELECT ImportID FROM dbo.ImportOrders  WHERE ImportCode = N'NK-20250101-001');
DECLARE @importID2 INT = (SELECT ImportID FROM dbo.ImportOrders  WHERE ImportCode = N'NK-20250101-002');
DECLARE @importID3 INT = (SELECT ImportID FROM dbo.ImportOrders  WHERE ImportCode = N'NK-20250101-003');

EXEC dbo.sp_ConfirmImport @ImportID = @importID1, @UserID = @adminID;
EXEC dbo.sp_ConfirmImport @ImportID = @importID2, @UserID = @adminID;
EXEC dbo.sp_ConfirmImport @ImportID = @importID3, @UserID = @adminID;
GO

-- ============================================================
-- TABLE: SalesOrders — Quản lý đơn hàng bán
-- ============================================================
CREATE TABLE dbo.SalesOrders (
    OrderID         INT             NOT NULL IDENTITY(1,1),
    OrderCode       NVARCHAR(30)    NOT NULL,
    UserID          INT             NOT NULL,
    CustomerName    NVARCHAR(150)   NULL,
    CustomerPhone   NVARCHAR(20)    NULL,
    CustomerAddress NVARCHAR(255)   NULL,
    PaymentMethod   NVARCHAR(20)    NOT NULL DEFAULT N'Tiền mặt',
    OrderDate       DATETIME        NOT NULL DEFAULT GETDATE(),
    TotalAmount     DECIMAL(18,2)   NOT NULL DEFAULT 0,
    Status          NVARCHAR(20)    NOT NULL DEFAULT N'Pending',
    Note            NVARCHAR(500)   NULL,
    CONSTRAINT PK_SalesOrders           PRIMARY KEY (OrderID),
    CONSTRAINT UQ_SalesOrders_Code      UNIQUE      (OrderCode),
    CONSTRAINT FK_SalesOrders_Users     FOREIGN KEY (UserID) REFERENCES dbo.Users(UserID),
    CONSTRAINT CK_SalesOrders_Status    CHECK (Status        IN (N'Pending', N'Confirmed', N'Cancelled')),
    CONSTRAINT CK_SalesOrders_Payment   CHECK (PaymentMethod IN (N'Tiền mặt', N'Chuyển khoản', N'COD'))
);
GO

-- ============================================================
-- TABLE: SalesOrderDetails — Chi tiết dòng sản phẩm trong đơn
-- ============================================================
CREATE TABLE dbo.SalesOrderDetails (
    DetailID  INT           NOT NULL IDENTITY(1,1),
    OrderID   INT           NOT NULL,
    ProductID INT           NOT NULL,
    Quantity  INT           NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    SubTotal  AS (Quantity * UnitPrice) PERSISTED,
    CONSTRAINT PK_SalesOrderDetails         PRIMARY KEY (DetailID),
    CONSTRAINT FK_SalesOrderDetails_Order   FOREIGN KEY (OrderID)    REFERENCES dbo.SalesOrders(OrderID) ON DELETE CASCADE,
    CONSTRAINT FK_SalesOrderDetails_Product FOREIGN KEY (ProductID)  REFERENCES dbo.Products(ProductID),
    CONSTRAINT CK_SalesOrderDetails_Qty     CHECK (Quantity  > 0),
    CONSTRAINT CK_SalesOrderDetails_Price   CHECK (UnitPrice >= 0)
);
GO

-- ============================================================
-- SP: sp_ConfirmOrder — Xác nhận đơn hàng, trừ tồn kho
-- ============================================================
IF OBJECT_ID('dbo.sp_ConfirmOrder', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_ConfirmOrder;
GO
CREATE PROCEDURE dbo.sp_ConfirmOrder
    @OrderID INT,
    @UserID  INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    BEGIN TRY
        IF NOT EXISTS (
            SELECT 1 FROM dbo.SalesOrders
            WHERE OrderID = @OrderID AND Status = N'Pending'
        )
            RAISERROR(N'Đơn hàng không ở trạng thái Pending hoặc không tồn tại.', 16, 1);

        IF EXISTS (
            SELECT 1
            FROM dbo.SalesOrderDetails d
            INNER JOIN dbo.Products p ON p.ProductID = d.ProductID
            WHERE d.OrderID = @OrderID AND p.StockQty < d.Quantity
        )
            RAISERROR(N'Tồn kho không đủ cho một hoặc nhiều sản phẩm trong đơn hàng.', 16, 1);

        UPDATE p SET
            p.StockQty  = p.StockQty - d.Quantity,
            p.UpdatedAt = GETDATE()
        FROM dbo.Products p
        INNER JOIN dbo.SalesOrderDetails d ON d.ProductID = p.ProductID
        WHERE d.OrderID = @OrderID;

        UPDATE dbo.SalesOrders SET Status = N'Confirmed' WHERE OrderID = @OrderID;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO
GO
