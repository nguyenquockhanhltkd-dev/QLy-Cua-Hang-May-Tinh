using Microsoft.Data.SqlClient;
using QLCuaHangMayTinh.Models;

namespace QLCuaHangMayTinh.DAL;

public class ExportOrderDAL
{
    public List<ExportOrder> GetAll()
    {
        var list = new List<ExportOrder>();
        using var conn = DBConnection.GetConnection();
        using var cmd = new SqlCommand(
            "SELECT eo.ExportID, eo.ExportCode, eo.UserID, eo.CustomerName, eo.CustomerPhone, " +
            "eo.ExportDate, eo.TotalAmount, eo.Status, eo.Note, u.FullName AS CreatedByFullName " +
            "FROM dbo.ExportOrders eo " +
            "INNER JOIN dbo.Users u ON eo.UserID = u.UserID " +
            "ORDER BY eo.ExportDate DESC", conn);
        conn.Open();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            list.Add(MapOrder(reader));
        return list;
    }

    public ExportOrder? GetByID(int exportID)
    {
        using var conn = DBConnection.GetConnection();
        using var cmd = new SqlCommand(
            "SELECT eo.ExportID, eo.ExportCode, eo.UserID, eo.CustomerName, eo.CustomerPhone, " +
            "eo.ExportDate, eo.TotalAmount, eo.Status, eo.Note, u.FullName AS CreatedByFullName " +
            "FROM dbo.ExportOrders eo " +
            "INNER JOIN dbo.Users u ON eo.UserID = u.UserID " +
            "WHERE eo.ExportID = @ExportID", conn);
        cmd.Parameters.AddWithValue("@ExportID", exportID);
        conn.Open();
        using var reader = cmd.ExecuteReader();
        if (reader.Read())
            return MapOrder(reader);
        return null;
    }

    public int Add(ExportOrder order)
    {
        using var conn = DBConnection.GetConnection();
        using var cmd = new SqlCommand(
            "INSERT INTO dbo.ExportOrders (ExportCode, UserID, CustomerName, CustomerPhone, TotalAmount, Status, Note) " +
            "VALUES (@ExportCode, @UserID, @CustomerName, @CustomerPhone, @TotalAmount, @Status, @Note); " +
            "SELECT SCOPE_IDENTITY();", conn);
        cmd.Parameters.AddWithValue("@ExportCode",    order.ExportCode);
        cmd.Parameters.AddWithValue("@UserID",        order.UserID);
        cmd.Parameters.AddWithValue("@CustomerName",  (object?)order.CustomerName  ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@CustomerPhone", (object?)order.CustomerPhone ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@TotalAmount",   order.TotalAmount);
        cmd.Parameters.AddWithValue("@Status",        order.Status);
        cmd.Parameters.AddWithValue("@Note",          (object?)order.Note          ?? DBNull.Value);
        conn.Open();
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public void AddDetail(ExportOrderDetail detail)
    {
        using var conn = DBConnection.GetConnection();
        using var cmd = new SqlCommand(
            "INSERT INTO dbo.ExportOrderDetails (ExportID, ProductID, Quantity, UnitPrice) " +
            "VALUES (@ExportID, @ProductID, @Quantity, @UnitPrice)", conn);
        cmd.Parameters.AddWithValue("@ExportID",  detail.ExportID);
        cmd.Parameters.AddWithValue("@ProductID", detail.ProductID);
        cmd.Parameters.AddWithValue("@Quantity",  detail.Quantity);
        cmd.Parameters.AddWithValue("@UnitPrice", detail.UnitPrice);
        conn.Open();
        cmd.ExecuteNonQuery();
    }

    public List<ExportOrderDetail> GetDetails(int exportID)
    {
        var list = new List<ExportOrderDetail>();
        using var conn = DBConnection.GetConnection();
        using var cmd = new SqlCommand(
            "SELECT d.DetailID, d.ExportID, d.ProductID, d.Quantity, d.UnitPrice, d.SubTotal, " +
            "p.Name AS ProductName, p.ProductCode " +
            "FROM dbo.ExportOrderDetails d " +
            "INNER JOIN dbo.Products p ON d.ProductID = p.ProductID " +
            "WHERE d.ExportID = @ExportID", conn);
        cmd.Parameters.AddWithValue("@ExportID", exportID);
        conn.Open();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new ExportOrderDetail
            {
                DetailID    = (int)reader["DetailID"],
                ExportID    = (int)reader["ExportID"],
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

    public void UpdateTotalAmount(int exportID, decimal totalAmount)
    {
        using var conn = DBConnection.GetConnection();
        using var cmd = new SqlCommand(
            "UPDATE dbo.ExportOrders SET TotalAmount = @TotalAmount WHERE ExportID = @ExportID", conn);
        cmd.Parameters.AddWithValue("@TotalAmount", totalAmount);
        cmd.Parameters.AddWithValue("@ExportID",    exportID);
        conn.Open();
        cmd.ExecuteNonQuery();
    }

    public void ConfirmExport(int exportID, int userID)
    {
        using var conn = DBConnection.GetConnection();
        using var cmd = new SqlCommand("dbo.sp_ConfirmExport", conn)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };
        cmd.Parameters.AddWithValue("@ExportID", exportID);
        cmd.Parameters.AddWithValue("@UserID",   userID);
        conn.Open();
        cmd.ExecuteNonQuery();
    }

    public int GetAvailableQty(int productID)
    {
        using var conn = DBConnection.GetConnection();
        using var cmd = new SqlCommand(
            "SELECT p.StockQty - ISNULL((" +
            "    SELECT SUM(ed.Quantity) " +
            "    FROM dbo.ExportOrderDetails ed " +
            "    INNER JOIN dbo.ExportOrders eo ON eo.ExportID = ed.ExportID " +
            "    WHERE ed.ProductID = @ProductID AND eo.Status = N'Pending'" +
            "), 0) FROM dbo.Products p WHERE p.ProductID = @ProductID", conn);
        cmd.Parameters.AddWithValue("@ProductID", productID);
        conn.Open();
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public void Cancel(int exportID)
    {
        using var conn = DBConnection.GetConnection();
        using var cmd = new SqlCommand(
            "UPDATE dbo.ExportOrders SET Status = N'Cancelled' " +
            "WHERE ExportID = @ExportID AND Status = N'Pending'", conn);
        cmd.Parameters.AddWithValue("@ExportID", exportID);
        conn.Open();
        cmd.ExecuteNonQuery();
    }

    public void Delete(int exportID)
    {
        using var conn = DBConnection.GetConnection();
        using var cmd = new SqlCommand(
            "DELETE FROM dbo.ExportOrders WHERE ExportID = @ExportID AND Status = N'Pending'", conn);
        cmd.Parameters.AddWithValue("@ExportID", exportID);
        conn.Open();
        cmd.ExecuteNonQuery();
    }

    private static ExportOrder MapOrder(SqlDataReader reader) => new()
    {
        ExportID          = (int)reader["ExportID"],
        ExportCode        = reader["ExportCode"].ToString()!,
        UserID            = (int)reader["UserID"],
        CustomerName      = reader["CustomerName"]  == DBNull.Value ? null : reader["CustomerName"].ToString(),
        CustomerPhone     = reader["CustomerPhone"] == DBNull.Value ? null : reader["CustomerPhone"].ToString(),
        ExportDate        = (DateTime)reader["ExportDate"],
        TotalAmount       = (decimal)reader["TotalAmount"],
        Status            = reader["Status"].ToString()!,
        Note              = reader["Note"] == DBNull.Value ? null : reader["Note"].ToString(),
        CreatedByFullName = reader["CreatedByFullName"].ToString()
    };
}
