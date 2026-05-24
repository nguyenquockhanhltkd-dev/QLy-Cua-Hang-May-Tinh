using Microsoft.Data.SqlClient;
using QLCuaHangMayTinh.Models;

namespace QLCuaHangMayTinh.DAL;

public class ProductDAL
{
    public List<Product> GetAll(bool includeInactive = false)
    {
        var list = new List<Product>();
        using var conn = DBConnection.GetConnection();
        string where = includeInactive ? "" : "WHERE p.IsActive = 1 ";
        using var cmd = new SqlCommand(
            "SELECT p.ProductID, p.CategoryID, p.ProductCode, p.Name, p.Brand, p.Description, " +
            "p.Unit, p.CostPrice, p.SellPrice, p.StockQty, p.MinStockQty, p.ImagePath, " +
            "p.IsActive, p.CreatedAt, p.UpdatedAt, c.Name AS CategoryName " +
            "FROM dbo.Products p " +
            "INNER JOIN dbo.Categories c ON p.CategoryID = c.CategoryID " +
            where + "ORDER BY p.IsActive DESC, p.Name", conn);
        conn.Open();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            list.Add(MapProduct(reader));
        return list;
    }

    public Product? GetByID(int productID)
    {
        using var conn = DBConnection.GetConnection();
        using var cmd = new SqlCommand(
            "SELECT p.ProductID, p.CategoryID, p.ProductCode, p.Name, p.Brand, p.Description, " +
            "p.Unit, p.CostPrice, p.SellPrice, p.StockQty, p.MinStockQty, p.ImagePath, " +
            "p.IsActive, p.CreatedAt, p.UpdatedAt, c.Name AS CategoryName " +
            "FROM dbo.Products p " +
            "INNER JOIN dbo.Categories c ON p.CategoryID = c.CategoryID " +
            "WHERE p.ProductID = @ProductID", conn);
        cmd.Parameters.AddWithValue("@ProductID", productID);
        conn.Open();
        using var reader = cmd.ExecuteReader();
        if (reader.Read())
            return MapProduct(reader);
        return null;
    }

    public int Add(Product product)
    {
        using var conn = DBConnection.GetConnection();
        using var cmd = new SqlCommand(
            "INSERT INTO dbo.Products (CategoryID, ProductCode, Name, Brand, Description, Unit, " +
            "CostPrice, SellPrice, StockQty, MinStockQty, ImagePath, IsActive) " +
            "VALUES (@CategoryID, @ProductCode, @Name, @Brand, @Description, @Unit, " +
            "@CostPrice, @SellPrice, @StockQty, @MinStockQty, @ImagePath, @IsActive); " +
            "SELECT SCOPE_IDENTITY();", conn);
        cmd.Parameters.AddWithValue("@CategoryID",  product.CategoryID);
        cmd.Parameters.AddWithValue("@ProductCode", product.ProductCode);
        cmd.Parameters.AddWithValue("@Name",        product.Name);
        cmd.Parameters.AddWithValue("@Brand",       (object?)product.Brand       ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Description", (object?)product.Description ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Unit",        product.Unit);
        cmd.Parameters.AddWithValue("@CostPrice",   product.CostPrice);
        cmd.Parameters.AddWithValue("@SellPrice",   product.SellPrice);
        cmd.Parameters.AddWithValue("@StockQty",    product.StockQty);
        cmd.Parameters.AddWithValue("@MinStockQty", product.MinStockQty);
        cmd.Parameters.AddWithValue("@ImagePath",   (object?)product.ImagePath   ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@IsActive",    product.IsActive);
        conn.Open();
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public void Update(Product product)
    {
        using var conn = DBConnection.GetConnection();
        using var cmd = new SqlCommand(
            "UPDATE dbo.Products SET CategoryID = @CategoryID, ProductCode = @ProductCode, " +
            "Name = @Name, Brand = @Brand, Description = @Description, Unit = @Unit, " +
            "CostPrice = @CostPrice, SellPrice = @SellPrice, MinStockQty = @MinStockQty, " +
            "ImagePath = @ImagePath, UpdatedAt = GETDATE() " +
            "WHERE ProductID = @ProductID", conn);
        cmd.Parameters.AddWithValue("@CategoryID",  product.CategoryID);
        cmd.Parameters.AddWithValue("@ProductCode", product.ProductCode);
        cmd.Parameters.AddWithValue("@Name",        product.Name);
        cmd.Parameters.AddWithValue("@Brand",       (object?)product.Brand       ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Description", (object?)product.Description ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Unit",        product.Unit);
        cmd.Parameters.AddWithValue("@CostPrice",   product.CostPrice);
        cmd.Parameters.AddWithValue("@SellPrice",   product.SellPrice);
        cmd.Parameters.AddWithValue("@MinStockQty", product.MinStockQty);
        cmd.Parameters.AddWithValue("@ImagePath",   (object?)product.ImagePath   ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ProductID",   product.ProductID);
        conn.Open();
        cmd.ExecuteNonQuery();
    }

    public void SetActive(int productID, bool isActive)
    {
        using var conn = DBConnection.GetConnection();
        using var cmd = new SqlCommand(
            "UPDATE dbo.Products SET IsActive = @IsActive, UpdatedAt = GETDATE() " +
            "WHERE ProductID = @ProductID", conn);
        cmd.Parameters.AddWithValue("@IsActive",  isActive);
        cmd.Parameters.AddWithValue("@ProductID", productID);
        conn.Open();
        cmd.ExecuteNonQuery();
    }

    public bool HasPendingOutOrders(int productID)
    {
        using var conn = DBConnection.GetConnection();
        using var cmd = new SqlCommand(
            "SELECT CASE WHEN EXISTS (" +
            "    SELECT 1 FROM dbo.ExportOrderDetails ed " +
            "    INNER JOIN dbo.ExportOrders eo ON ed.ExportID = eo.ExportID " +
            "    WHERE ed.ProductID = @ProductID AND eo.Status = N'Pending' " +
            "    UNION ALL " +
            "    SELECT 1 FROM dbo.SalesOrderDetails sd " +
            "    INNER JOIN dbo.SalesOrders so ON sd.OrderID = so.OrderID " +
            "    WHERE sd.ProductID = @ProductID AND so.Status = N'Pending' " +
            ") THEN 1 ELSE 0 END", conn);
        cmd.Parameters.AddWithValue("@ProductID", productID);
        conn.Open();
        return Convert.ToInt32(cmd.ExecuteScalar()) == 1;
    }

    public bool HasPendingImportOrders(int productID)
    {
        using var conn = DBConnection.GetConnection();
        using var cmd = new SqlCommand(
            "SELECT CASE WHEN EXISTS (" +
            "    SELECT 1 FROM dbo.ImportOrderDetails id " +
            "    INNER JOIN dbo.ImportOrders io ON id.ImportID = io.ImportID " +
            "    WHERE id.ProductID = @ProductID AND io.Status = N'Pending' " +
            ") THEN 1 ELSE 0 END", conn);
        cmd.Parameters.AddWithValue("@ProductID", productID);
        conn.Open();
        return Convert.ToInt32(cmd.ExecuteScalar()) == 1;
    }

    public bool IsProductCodeExists(string productCode, int excludeProductID)
    {
        using var conn = DBConnection.GetConnection();
        using var cmd = new SqlCommand(
            "SELECT COUNT(1) FROM dbo.Products " +
            "WHERE ProductCode = @ProductCode AND ProductID <> @ExcludeID", conn);
        cmd.Parameters.AddWithValue("@ProductCode", productCode);
        cmd.Parameters.AddWithValue("@ExcludeID",   excludeProductID);
        conn.Open();
        return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
    }

    public bool IsReferencedInOrders(int productID)
    {
        using var conn = DBConnection.GetConnection();
        using var cmd = new SqlCommand(
            "SELECT CASE WHEN EXISTS (" +
            "    SELECT 1 FROM dbo.ImportOrderDetails  WHERE ProductID = @ProductID" +
            "    UNION ALL" +
            "    SELECT 1 FROM dbo.ExportOrderDetails  WHERE ProductID = @ProductID" +
            "    UNION ALL" +
            "    SELECT 1 FROM dbo.SalesOrderDetails   WHERE ProductID = @ProductID" +
            ") THEN 1 ELSE 0 END", conn);
        cmd.Parameters.AddWithValue("@ProductID", productID);
        conn.Open();
        return Convert.ToInt32(cmd.ExecuteScalar()) == 1;
    }

    public void Delete(int productID)
    {
        using var conn = DBConnection.GetConnection();
        using var cmd = new SqlCommand(
            "DELETE FROM dbo.Products WHERE ProductID = @ProductID", conn);
        cmd.Parameters.AddWithValue("@ProductID", productID);
        conn.Open();
        cmd.ExecuteNonQuery();
    }

    public List<StockReportItem> GetStockReport()
    {
        var list = new List<StockReportItem>();
        using var conn = DBConnection.GetConnection();
        using var cmd = new SqlCommand("SELECT * FROM dbo.vw_StockReport ORDER BY ProductName", conn);
        conn.Open();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new StockReportItem
            {
                ProductID   = (int)reader["ProductID"],
                ProductCode = reader["ProductCode"].ToString()!,
                ProductName = reader["ProductName"].ToString()!,
                Brand       = reader["Brand"]    == DBNull.Value ? null : reader["Brand"].ToString(),
                Category    = reader["Category"] == DBNull.Value ? null : reader["Category"].ToString(),
                StockQty     = (int)reader["StockQty"],
                MinStockQty  = (int)reader["MinStockQty"],
                CostPrice    = (decimal)reader["CostPrice"],
                SellPrice    = (decimal)reader["SellPrice"],
                AvailableQty = (int)reader["AvailableQty"],
                StockStatus  = reader["StockStatus"].ToString()!
            });
        }
        return list;
    }

    public int GetNextSequentialNumber(string prefix)
    {
        using var conn = DBConnection.GetConnection();
        using var cmd = new SqlCommand(
            "SELECT ISNULL(MAX(" +
            "    CASE WHEN ISNUMERIC(SUBSTRING(ProductCode, @Offset, 10)) = 1" +
            "         THEN CAST(SUBSTRING(ProductCode, @Offset, 10) AS INT)" +
            "         ELSE 0 END" +
            "), 0) FROM dbo.Products WHERE ProductCode LIKE @Pattern", conn);
        cmd.Parameters.AddWithValue("@Offset",  prefix.Length + 2);
        cmd.Parameters.AddWithValue("@Pattern", prefix + "-[0-9]%");
        conn.Open();
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    private static Product MapProduct(SqlDataReader reader) => new()
    {
        ProductID    = (int)reader["ProductID"],
        CategoryID   = (int)reader["CategoryID"],
        ProductCode  = reader["ProductCode"].ToString()!,
        Name         = reader["Name"].ToString()!,
        Brand        = reader["Brand"]       == DBNull.Value ? null : reader["Brand"].ToString(),
        Description  = reader["Description"] == DBNull.Value ? null : reader["Description"].ToString(),
        Unit         = reader["Unit"].ToString()!,
        CostPrice    = (decimal)reader["CostPrice"],
        SellPrice    = (decimal)reader["SellPrice"],
        StockQty     = (int)reader["StockQty"],
        MinStockQty  = (int)reader["MinStockQty"],
        ImagePath    = reader["ImagePath"]   == DBNull.Value ? null : reader["ImagePath"].ToString(),
        IsActive     = (bool)reader["IsActive"],
        CreatedAt    = (DateTime)reader["CreatedAt"],
        UpdatedAt    = (DateTime)reader["UpdatedAt"],
        CategoryName = reader["CategoryName"].ToString()
    };
}
