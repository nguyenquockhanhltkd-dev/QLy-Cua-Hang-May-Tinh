using Microsoft.Data.SqlClient;
using QLCuaHangMayTinh.Models;

namespace QLCuaHangMayTinh.DAL;

public class SalesOrderDAL
{
    public List<SalesOrder> GetAll()
    {
        var list = new List<SalesOrder>();
        using var conn = DBConnection.GetConnection();
        using var cmd = new SqlCommand(
            "SELECT so.OrderID, so.OrderCode, so.UserID, so.CustomerName, so.CustomerPhone, " +
            "so.CustomerAddress, so.PaymentMethod, so.OrderDate, so.TotalAmount, so.Status, so.Note, " +
            "u.FullName AS CreatedByFullName " +
            "FROM dbo.SalesOrders so " +
            "INNER JOIN dbo.Users u ON so.UserID = u.UserID " +
            "ORDER BY so.OrderDate DESC", conn);
        conn.Open();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            list.Add(MapOrder(reader));
        return list;
    }

    public SalesOrder? GetByID(int orderID)
    {
        using var conn = DBConnection.GetConnection();
        using var cmd = new SqlCommand(
            "SELECT so.OrderID, so.OrderCode, so.UserID, so.CustomerName, so.CustomerPhone, " +
            "so.CustomerAddress, so.PaymentMethod, so.OrderDate, so.TotalAmount, so.Status, so.Note, " +
            "u.FullName AS CreatedByFullName " +
            "FROM dbo.SalesOrders so " +
            "INNER JOIN dbo.Users u ON so.UserID = u.UserID " +
            "WHERE so.OrderID = @OrderID", conn);
        cmd.Parameters.AddWithValue("@OrderID", orderID);
        conn.Open();
        using var reader = cmd.ExecuteReader();
        if (reader.Read()) return MapOrder(reader);
        return null;
    }

    public int Add(SalesOrder order)
    {
        using var conn = DBConnection.GetConnection();
        using var cmd = new SqlCommand(
            "INSERT INTO dbo.SalesOrders " +
            "(OrderCode, UserID, CustomerName, CustomerPhone, CustomerAddress, PaymentMethod, TotalAmount, Status, Note) " +
            "VALUES (@OrderCode, @UserID, @CustomerName, @CustomerPhone, @CustomerAddress, @PaymentMethod, @TotalAmount, @Status, @Note); " +
            "SELECT SCOPE_IDENTITY();", conn);
        cmd.Parameters.AddWithValue("@OrderCode",       order.OrderCode);
        cmd.Parameters.AddWithValue("@UserID",          order.UserID);
        cmd.Parameters.AddWithValue("@CustomerName",    (object?)order.CustomerName    ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@CustomerPhone",   (object?)order.CustomerPhone   ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@CustomerAddress", (object?)order.CustomerAddress ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@PaymentMethod",   order.PaymentMethod);
        cmd.Parameters.AddWithValue("@TotalAmount",     order.TotalAmount);
        cmd.Parameters.AddWithValue("@Status",          order.Status);
        cmd.Parameters.AddWithValue("@Note",            (object?)order.Note            ?? DBNull.Value);
        conn.Open();
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public void AddDetail(SalesOrderDetail detail)
    {
        using var conn = DBConnection.GetConnection();
        using var cmd = new SqlCommand(
            "INSERT INTO dbo.SalesOrderDetails (OrderID, ProductID, Quantity, UnitPrice) " +
            "VALUES (@OrderID, @ProductID, @Quantity, @UnitPrice)", conn);
        cmd.Parameters.AddWithValue("@OrderID",   detail.OrderID);
        cmd.Parameters.AddWithValue("@ProductID", detail.ProductID);
        cmd.Parameters.AddWithValue("@Quantity",  detail.Quantity);
        cmd.Parameters.AddWithValue("@UnitPrice", detail.UnitPrice);
        conn.Open();
        cmd.ExecuteNonQuery();
    }

    public List<SalesOrderDetail> GetDetails(int orderID)
    {
        var list = new List<SalesOrderDetail>();
        using var conn = DBConnection.GetConnection();
        using var cmd = new SqlCommand(
            "SELECT d.DetailID, d.OrderID, d.ProductID, d.Quantity, d.UnitPrice, d.SubTotal, " +
            "p.Name AS ProductName, p.ProductCode " +
            "FROM dbo.SalesOrderDetails d " +
            "INNER JOIN dbo.Products p ON d.ProductID = p.ProductID " +
            "WHERE d.OrderID = @OrderID", conn);
        cmd.Parameters.AddWithValue("@OrderID", orderID);
        conn.Open();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new SalesOrderDetail
            {
                DetailID    = (int)reader["DetailID"],
                OrderID     = (int)reader["OrderID"],
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

    public void UpdateTotalAmount(int orderID, decimal totalAmount)
    {
        using var conn = DBConnection.GetConnection();
        using var cmd = new SqlCommand(
            "UPDATE dbo.SalesOrders SET TotalAmount = @TotalAmount WHERE OrderID = @OrderID", conn);
        cmd.Parameters.AddWithValue("@TotalAmount", totalAmount);
        cmd.Parameters.AddWithValue("@OrderID",     orderID);
        conn.Open();
        cmd.ExecuteNonQuery();
    }

    public void ConfirmOrder(int orderID, int userID)
    {
        using var conn = DBConnection.GetConnection();
        using var cmd = new SqlCommand("dbo.sp_ConfirmOrder", conn)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };
        cmd.Parameters.AddWithValue("@OrderID", orderID);
        cmd.Parameters.AddWithValue("@UserID",  userID);
        conn.Open();
        cmd.ExecuteNonQuery();
    }

    // Tồn khả dụng = StockQty − pending ExportOrders − pending SalesOrders
    public int GetAvailableQty(int productID)
    {
        using var conn = DBConnection.GetConnection();
        using var cmd = new SqlCommand(
            "SELECT p.StockQty " +
            "  - ISNULL((" +
            "      SELECT SUM(ed.Quantity) FROM dbo.ExportOrderDetails ed " +
            "      INNER JOIN dbo.ExportOrders eo ON eo.ExportID = ed.ExportID " +
            "      WHERE ed.ProductID = @ProductID AND eo.Status = N'Pending'" +
            "  ), 0) " +
            "  - ISNULL((" +
            "      SELECT SUM(sd.Quantity) FROM dbo.SalesOrderDetails sd " +
            "      INNER JOIN dbo.SalesOrders so ON so.OrderID = sd.OrderID " +
            "      WHERE sd.ProductID = @ProductID AND so.Status = N'Pending'" +
            "  ), 0) " +
            "FROM dbo.Products p WHERE p.ProductID = @ProductID", conn);
        cmd.Parameters.AddWithValue("@ProductID", productID);
        conn.Open();
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public void Cancel(int orderID)
    {
        using var conn = DBConnection.GetConnection();
        using var cmd = new SqlCommand(
            "UPDATE dbo.SalesOrders SET Status = N'Cancelled' " +
            "WHERE OrderID = @OrderID AND Status = N'Pending'", conn);
        cmd.Parameters.AddWithValue("@OrderID", orderID);
        conn.Open();
        cmd.ExecuteNonQuery();
    }

    public void Delete(int orderID)
    {
        using var conn = DBConnection.GetConnection();
        using var cmd = new SqlCommand(
            "DELETE FROM dbo.SalesOrders WHERE OrderID = @OrderID AND Status = N'Pending'", conn);
        cmd.Parameters.AddWithValue("@OrderID", orderID);
        conn.Open();
        cmd.ExecuteNonQuery();
    }

    private static SalesOrder MapOrder(SqlDataReader reader) => new()
    {
        OrderID           = (int)reader["OrderID"],
        OrderCode         = reader["OrderCode"].ToString()!,
        UserID            = (int)reader["UserID"],
        CustomerName      = reader["CustomerName"]    == DBNull.Value ? null : reader["CustomerName"].ToString(),
        CustomerPhone     = reader["CustomerPhone"]   == DBNull.Value ? null : reader["CustomerPhone"].ToString(),
        CustomerAddress   = reader["CustomerAddress"] == DBNull.Value ? null : reader["CustomerAddress"].ToString(),
        PaymentMethod     = reader["PaymentMethod"].ToString()!,
        OrderDate         = (DateTime)reader["OrderDate"],
        TotalAmount       = (decimal)reader["TotalAmount"],
        Status            = reader["Status"].ToString()!,
        Note              = reader["Note"] == DBNull.Value ? null : reader["Note"].ToString(),
        CreatedByFullName = reader["CreatedByFullName"].ToString()
    };
}
