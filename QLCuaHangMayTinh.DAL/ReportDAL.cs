using Microsoft.Data.SqlClient;
using QLCuaHangMayTinh.Models;

namespace QLCuaHangMayTinh.DAL;

public class ReportDAL
{
    // ── Nhập kho ─────────────────────────────────────────────────────────────

    public List<ReportDayRow> GetImportByDay(DateTime from, DateTime to)
    {
        var list = new List<ReportDayRow>();
        using var conn = DBConnection.GetConnection();
        using var cmd = new SqlCommand(
            "SELECT FORMAT(CAST(ImportDate AS DATE), 'dd/MM/yyyy') AS DateLabel, " +
            "COUNT(*) AS TotalOrders, SUM(TotalAmount) AS TotalAmount " +
            "FROM dbo.ImportOrders " +
            "WHERE Status = N'Confirmed' AND ImportDate >= @From AND ImportDate < @To " +
            "GROUP BY CAST(ImportDate AS DATE) " +
            "ORDER BY CAST(ImportDate AS DATE)", conn);
        cmd.Parameters.AddWithValue("@From", from.Date);
        cmd.Parameters.AddWithValue("@To",   to.Date.AddDays(1));
        conn.Open();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            list.Add(new ReportDayRow
            {
                DateLabel   = reader["DateLabel"].ToString()!,
                TotalOrders = (int)reader["TotalOrders"],
                TotalAmount = (decimal)reader["TotalAmount"]
            });
        return list;
    }

    public List<ReportMonthRow> GetImportByMonth(DateTime from, DateTime to)
    {
        var list = new List<ReportMonthRow>();
        using var conn = DBConnection.GetConnection();
        using var cmd = new SqlCommand(
            "SELECT YEAR(ImportDate) AS [Year], MONTH(ImportDate) AS [Month], " +
            "COUNT(*) AS TotalOrders, SUM(TotalAmount) AS TotalAmount " +
            "FROM dbo.ImportOrders " +
            "WHERE Status = N'Confirmed' AND ImportDate >= @From AND ImportDate < @To " +
            "GROUP BY YEAR(ImportDate), MONTH(ImportDate) " +
            "ORDER BY [Year], [Month]", conn);
        cmd.Parameters.AddWithValue("@From", from.Date);
        cmd.Parameters.AddWithValue("@To",   to.Date.AddDays(1));
        conn.Open();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            list.Add(new ReportMonthRow
            {
                Year        = (int)reader["Year"],
                Month       = (int)reader["Month"],
                TotalOrders = (int)reader["TotalOrders"],
                TotalAmount = (decimal)reader["TotalAmount"]
            });
        return list;
    }

    // ── Xuất kho ─────────────────────────────────────────────────────────────

    public List<ReportDayRow> GetExportByDay(DateTime from, DateTime to)
    {
        var list = new List<ReportDayRow>();
        using var conn = DBConnection.GetConnection();
        using var cmd = new SqlCommand(
            "SELECT FORMAT(CAST(ExportDate AS DATE), 'dd/MM/yyyy') AS DateLabel, " +
            "COUNT(*) AS TotalOrders, SUM(TotalAmount) AS TotalAmount " +
            "FROM dbo.ExportOrders " +
            "WHERE Status = N'Confirmed' AND ExportDate >= @From AND ExportDate < @To " +
            "GROUP BY CAST(ExportDate AS DATE) " +
            "ORDER BY CAST(ExportDate AS DATE)", conn);
        cmd.Parameters.AddWithValue("@From", from.Date);
        cmd.Parameters.AddWithValue("@To",   to.Date.AddDays(1));
        conn.Open();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            list.Add(new ReportDayRow
            {
                DateLabel   = reader["DateLabel"].ToString()!,
                TotalOrders = (int)reader["TotalOrders"],
                TotalAmount = (decimal)reader["TotalAmount"]
            });
        return list;
    }

    public List<ReportMonthRow> GetExportByMonth(DateTime from, DateTime to)
    {
        var list = new List<ReportMonthRow>();
        using var conn = DBConnection.GetConnection();
        using var cmd = new SqlCommand(
            "SELECT YEAR(ExportDate) AS [Year], MONTH(ExportDate) AS [Month], " +
            "COUNT(*) AS TotalOrders, SUM(TotalAmount) AS TotalAmount " +
            "FROM dbo.ExportOrders " +
            "WHERE Status = N'Confirmed' AND ExportDate >= @From AND ExportDate < @To " +
            "GROUP BY YEAR(ExportDate), MONTH(ExportDate) " +
            "ORDER BY [Year], [Month]", conn);
        cmd.Parameters.AddWithValue("@From", from.Date);
        cmd.Parameters.AddWithValue("@To",   to.Date.AddDays(1));
        conn.Open();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            list.Add(new ReportMonthRow
            {
                Year        = (int)reader["Year"],
                Month       = (int)reader["Month"],
                TotalOrders = (int)reader["TotalOrders"],
                TotalAmount = (decimal)reader["TotalAmount"]
            });
        return list;
    }

    // ── KPI Tổng quan ────────────────────────────────────────────────────────

    public KpiSummary GetKpiSummary(int year, int month)
    {
        using var conn = DBConnection.GetConnection();
        using var cmd = new SqlCommand(
            "SELECT " +
            "  (SELECT ISNULL(SUM(TotalAmount),0) FROM dbo.ExportOrders " +
            "   WHERE Status=N'Confirmed' AND YEAR(ExportDate)=@Year AND MONTH(ExportDate)=@Month) " +
            "+ (SELECT ISNULL(SUM(TotalAmount),0) FROM dbo.SalesOrders " +
            "   WHERE Status=N'Confirmed' AND YEAR(OrderDate)=@Year  AND MONTH(OrderDate)=@Month) " +
            "  AS RevenueThisMonth, " +
            "  (SELECT ISNULL(SUM(TotalAmount),0) FROM dbo.ImportOrders " +
            "   WHERE Status=N'Confirmed' AND YEAR(ImportDate)=@Year AND MONTH(ImportDate)=@Month) " +
            "  AS ImportCostThisMonth, " +
            "  (SELECT ISNULL(SUM(ed.Quantity * p.CostPrice),0) " +
            "   FROM dbo.ExportOrderDetails ed " +
            "   INNER JOIN dbo.ExportOrders eo ON ed.ExportID = eo.ExportID " +
            "   INNER JOIN dbo.Products     p  ON ed.ProductID = p.ProductID " +
            "   WHERE eo.Status=N'Confirmed' AND YEAR(eo.ExportDate)=@Year AND MONTH(eo.ExportDate)=@Month) " +
            "+ (SELECT ISNULL(SUM(sd.Quantity * p.CostPrice),0) " +
            "   FROM dbo.SalesOrderDetails sd " +
            "   INNER JOIN dbo.SalesOrders so ON sd.OrderID = so.OrderID " +
            "   INNER JOIN dbo.Products    p  ON sd.ProductID = p.ProductID " +
            "   WHERE so.Status=N'Confirmed' AND YEAR(so.OrderDate)=@Year  AND MONTH(so.OrderDate)=@Month) " +
            "  AS CostOfGoodsSold, " +
            "  (SELECT COUNT(*) FROM dbo.ExportOrders WHERE Status=N'Pending') " +
            "+ (SELECT COUNT(*) FROM dbo.SalesOrders  WHERE Status=N'Pending') " +
            "  AS PendingOrders, " +
            "  (SELECT COUNT(*) FROM dbo.Products WHERE StockQty <= MinStockQty AND IsActive=1) " +
            "  AS LowStockCount", conn);
        cmd.Parameters.AddWithValue("@Year",  year);
        cmd.Parameters.AddWithValue("@Month", month);
        conn.Open();
        using var r = cmd.ExecuteReader();
        if (!r.Read()) return new KpiSummary { Year = year, Month = month };
        return new KpiSummary
        {
            Year                = year,
            Month               = month,
            RevenueThisMonth    = (decimal)r["RevenueThisMonth"],
            ImportCostThisMonth = (decimal)r["ImportCostThisMonth"],
            CostOfGoodsSold     = (decimal)r["CostOfGoodsSold"],
            PendingOrders       = (int)r["PendingOrders"],
            LowStockCount       = (int)r["LowStockCount"]
        };
    }

    /// <summary>Trả về mảng 12 phần tử — doanh thu tháng 1..12 của năm chỉ định.</summary>
    public decimal[] GetMonthlyRevenue(int year)
    {
        var result = new decimal[12];
        using var conn = DBConnection.GetConnection();
        using var cmd = new SqlCommand(
            "SELECT m.Month, " +
            "  ISNULL((SELECT SUM(TotalAmount) FROM dbo.ExportOrders " +
            "          WHERE Status=N'Confirmed' AND YEAR(ExportDate)=@Year AND MONTH(ExportDate)=m.Month),0) " +
            "+ ISNULL((SELECT SUM(TotalAmount) FROM dbo.SalesOrders  " +
            "          WHERE Status=N'Confirmed' AND YEAR(OrderDate)=@Year  AND MONTH(OrderDate)=m.Month),0) " +
            "  AS Revenue " +
            "FROM (VALUES(1),(2),(3),(4),(5),(6),(7),(8),(9),(10),(11),(12)) AS m(Month) " +
            "ORDER BY m.Month", conn);
        cmd.Parameters.AddWithValue("@Year", year);
        conn.Open();
        using var r = cmd.ExecuteReader();
        while (r.Read())
        {
            int idx = (int)r["Month"] - 1;          // 0-based
            result[idx] = (decimal)r["Revenue"];
        }
        return result;
    }

    // ── Top sản phẩm ─────────────────────────────────────────────────────────

    public List<ReportTopProductRow> GetTopExportProducts(
        DateTime from, DateTime to, int? categoryID, int top)
    {
        var list = new List<ReportTopProductRow>();
        using var conn = DBConnection.GetConnection();
        using var cmd = new SqlCommand(
            "SELECT TOP (@Top) " +
            "p.ProductCode, p.Name AS ProductName, c.Name AS CategoryName, p.Brand, " +
            "SUM(s.Quantity) AS TotalQty, SUM(s.SubTotal) AS TotalAmount " +
            "FROM ( " +
            "    SELECT ed.ProductID, ed.Quantity, ed.SubTotal " +
            "    FROM dbo.ExportOrderDetails ed " +
            "    INNER JOIN dbo.ExportOrders eo ON ed.ExportID = eo.ExportID " +
            "    WHERE eo.Status = N'Confirmed' " +
            "      AND eo.ExportDate >= @From AND eo.ExportDate < @To " +
            "    UNION ALL " +
            "    SELECT sd.ProductID, sd.Quantity, sd.SubTotal " +
            "    FROM dbo.SalesOrderDetails sd " +
            "    INNER JOIN dbo.SalesOrders so ON sd.OrderID = so.OrderID " +
            "    WHERE so.Status = N'Confirmed' " +
            "      AND so.OrderDate >= @From AND so.OrderDate < @To " +
            ") s " +
            "INNER JOIN dbo.Products   p ON s.ProductID  = p.ProductID " +
            "INNER JOIN dbo.Categories c ON p.CategoryID = c.CategoryID " +
            "WHERE (@CategoryID IS NULL OR p.CategoryID = @CategoryID) " +
            "GROUP BY p.ProductID, p.ProductCode, p.Name, c.Name, p.Brand " +
            "ORDER BY TotalQty DESC", conn);

        cmd.Parameters.Add("@Top",  System.Data.SqlDbType.Int).Value = top;
        cmd.Parameters.AddWithValue("@From", from.Date);
        cmd.Parameters.AddWithValue("@To",   to.Date.AddDays(1));
        cmd.Parameters.Add("@CategoryID", System.Data.SqlDbType.Int).Value =
            categoryID.HasValue ? categoryID.Value : DBNull.Value;

        conn.Open();
        using var reader = cmd.ExecuteReader();
        int rank = 1;
        while (reader.Read())
        {
            list.Add(new ReportTopProductRow
            {
                Rank         = rank++,
                ProductCode  = reader["ProductCode"].ToString()!,
                ProductName  = reader["ProductName"].ToString()!,
                CategoryName = reader["CategoryName"].ToString()!,
                Brand        = reader["Brand"] == DBNull.Value ? null : reader["Brand"].ToString(),
                TotalQty     = (int)reader["TotalQty"],
                TotalAmount  = (decimal)reader["TotalAmount"]
            });
        }
        return list;
    }
}
