using Microsoft.Data.SqlClient;
using QLCuaHangMayTinh.Models;

namespace QLCuaHangMayTinh.DAL;

public class ImportOrderDAL
{
    public List<ImportOrder> GetAll()
    {
        var list = new List<ImportOrder>();
        using var conn = DBConnection.GetConnection();
        using var cmd = new SqlCommand(
            "SELECT io.ImportID, io.ImportCode, io.SupplierID, io.UserID, io.ImportDate, " +
            "io.TotalAmount, io.Status, io.Note, s.Name AS SupplierName, u.FullName AS CreatedByFullName " +
            "FROM dbo.ImportOrders io " +
            "INNER JOIN dbo.Suppliers s ON io.SupplierID = s.SupplierID " +
            "INNER JOIN dbo.Users u ON io.UserID = u.UserID " +
            "ORDER BY io.ImportDate DESC", conn);
        conn.Open();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            list.Add(MapOrder(reader));
        return list;
    }

    public ImportOrder? GetByID(int importID)
    {
        using var conn = DBConnection.GetConnection();
        using var cmd = new SqlCommand(
            "SELECT io.ImportID, io.ImportCode, io.SupplierID, io.UserID, io.ImportDate, " +
            "io.TotalAmount, io.Status, io.Note, s.Name AS SupplierName, u.FullName AS CreatedByFullName " +
            "FROM dbo.ImportOrders io " +
            "INNER JOIN dbo.Suppliers s ON io.SupplierID = s.SupplierID " +
            "INNER JOIN dbo.Users u ON io.UserID = u.UserID " +
            "WHERE io.ImportID = @ImportID", conn);
        cmd.Parameters.AddWithValue("@ImportID", importID);
        conn.Open();
        using var reader = cmd.ExecuteReader();
        if (reader.Read())
            return MapOrder(reader);
        return null;
    }

    public int Add(ImportOrder order)
    {
        using var conn = DBConnection.GetConnection();
        using var cmd = new SqlCommand(
            "INSERT INTO dbo.ImportOrders (ImportCode, SupplierID, UserID, TotalAmount, Status, Note) " +
            "VALUES (@ImportCode, @SupplierID, @UserID, @TotalAmount, @Status, @Note); " +
            "SELECT SCOPE_IDENTITY();", conn);
        cmd.Parameters.AddWithValue("@ImportCode",   order.ImportCode);
        cmd.Parameters.AddWithValue("@SupplierID",   order.SupplierID);
        cmd.Parameters.AddWithValue("@UserID",       order.UserID);
        cmd.Parameters.AddWithValue("@TotalAmount",  order.TotalAmount);
        cmd.Parameters.AddWithValue("@Status",       order.Status);
        cmd.Parameters.AddWithValue("@Note",         (object?)order.Note ?? DBNull.Value);
        conn.Open();
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public void AddDetail(ImportOrderDetail detail)
    {
        using var conn = DBConnection.GetConnection();
        using var cmd = new SqlCommand(
            "INSERT INTO dbo.ImportOrderDetails (ImportID, ProductID, Quantity, UnitPrice) " +
            "VALUES (@ImportID, @ProductID, @Quantity, @UnitPrice)", conn);
        cmd.Parameters.AddWithValue("@ImportID",  detail.ImportID);
        cmd.Parameters.AddWithValue("@ProductID", detail.ProductID);
        cmd.Parameters.AddWithValue("@Quantity",  detail.Quantity);
        cmd.Parameters.AddWithValue("@UnitPrice", detail.UnitPrice);
        conn.Open();
        cmd.ExecuteNonQuery();
    }

    public List<ImportOrderDetail> GetDetails(int importID)
    {
        var list = new List<ImportOrderDetail>();
        using var conn = DBConnection.GetConnection();
        using var cmd = new SqlCommand(
            "SELECT d.DetailID, d.ImportID, d.ProductID, d.Quantity, d.UnitPrice, d.SubTotal, " +
            "p.Name AS ProductName, p.ProductCode " +
            "FROM dbo.ImportOrderDetails d " +
            "INNER JOIN dbo.Products p ON d.ProductID = p.ProductID " +
            "WHERE d.ImportID = @ImportID", conn);
        cmd.Parameters.AddWithValue("@ImportID", importID);
        conn.Open();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new ImportOrderDetail
            {
                DetailID    = (int)reader["DetailID"],
                ImportID    = (int)reader["ImportID"],
                ProductID   = (int)reader["ProductID"],
                Quantity    = (int)reader["Quantity"],
                UnitPrice   = (decimal)reader["UnitPrice"],
                SubTotal    = (decimal)reader["SubTotal"],
                ProductName = reader["ProductName"].ToString(),
                ProductCode = reader["ProductCode"].ToString()
            });
        }
        return list;
    }

    public void UpdateTotalAmount(int importID, decimal totalAmount)
    {
        using var conn = DBConnection.GetConnection();
        using var cmd = new SqlCommand(
            "UPDATE dbo.ImportOrders SET TotalAmount = @TotalAmount WHERE ImportID = @ImportID", conn);
        cmd.Parameters.AddWithValue("@TotalAmount", totalAmount);
        cmd.Parameters.AddWithValue("@ImportID",    importID);
        conn.Open();
        cmd.ExecuteNonQuery();
    }

    public void ConfirmImport(int importID, int userID)
    {
        using var conn = DBConnection.GetConnection();
        using var cmd = new SqlCommand("dbo.sp_ConfirmImport", conn)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };
        cmd.Parameters.AddWithValue("@ImportID", importID);
        cmd.Parameters.AddWithValue("@UserID",   userID);
        conn.Open();
        cmd.ExecuteNonQuery();
    }

    public List<ImportOrder> GetBySupplierID(int supplierID)
    {
        var list = new List<ImportOrder>();
        using var conn = DBConnection.GetConnection();
        using var cmd = new SqlCommand(
            "SELECT io.ImportID, io.ImportCode, io.SupplierID, io.UserID, io.ImportDate, " +
            "io.TotalAmount, io.Status, io.Note, s.Name AS SupplierName, u.FullName AS CreatedByFullName " +
            "FROM dbo.ImportOrders io " +
            "INNER JOIN dbo.Suppliers s ON io.SupplierID = s.SupplierID " +
            "INNER JOIN dbo.Users u ON io.UserID = u.UserID " +
            "WHERE io.SupplierID = @SupplierID " +
            "ORDER BY io.ImportDate DESC", conn);
        cmd.Parameters.AddWithValue("@SupplierID", supplierID);
        conn.Open();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            list.Add(MapOrder(reader));
        return list;
    }

    public void Cancel(int importID)
    {
        using var conn = DBConnection.GetConnection();
        using var cmd = new SqlCommand(
            "UPDATE dbo.ImportOrders SET Status = N'Cancelled' " +
            "WHERE ImportID = @ImportID AND Status = N'Pending'", conn);
        cmd.Parameters.AddWithValue("@ImportID", importID);
        conn.Open();
        cmd.ExecuteNonQuery();
    }

    public void Delete(int importID)
    {
        using var conn = DBConnection.GetConnection();
        using var cmd = new SqlCommand(
            "DELETE FROM dbo.ImportOrders WHERE ImportID = @ImportID AND Status = N'Pending'", conn);
        cmd.Parameters.AddWithValue("@ImportID", importID);
        conn.Open();
        cmd.ExecuteNonQuery();
    }

    private static ImportOrder MapOrder(SqlDataReader reader) => new()
    {
        ImportID          = (int)reader["ImportID"],
        ImportCode        = reader["ImportCode"].ToString()!,
        SupplierID        = (int)reader["SupplierID"],
        UserID            = (int)reader["UserID"],
        ImportDate        = (DateTime)reader["ImportDate"],
        TotalAmount       = (decimal)reader["TotalAmount"],
        Status            = reader["Status"].ToString()!,
        Note              = reader["Note"] == DBNull.Value ? null : reader["Note"].ToString(),
        SupplierName      = reader["SupplierName"].ToString(),
        CreatedByFullName = reader["CreatedByFullName"].ToString()
    };
}
