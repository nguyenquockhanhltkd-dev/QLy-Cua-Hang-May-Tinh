using Microsoft.Data.SqlClient;
using QLCuaHangMayTinh.Models;

namespace QLCuaHangMayTinh.DAL;

public class ProductSpecDAL
{
    public ProductSpec? GetByProductID(int productID)
    {
        using var conn = DBConnection.GetConnection();
        using var cmd = new SqlCommand(
            "SELECT SpecID, ProductID, ProductType, CPU, RAM, Storage, GPU, " +
            "ScreenSize, BatteryWhr, OperatingSystem, ExtraSpecs " +
            "FROM dbo.ProductSpecs WHERE ProductID = @ProductID", conn);
        cmd.Parameters.AddWithValue("@ProductID", productID);
        conn.Open();
        using var reader = cmd.ExecuteReader();
        if (reader.Read())
            return MapSpec(reader);
        return null;
    }

    public void Add(ProductSpec spec)
    {
        using var conn = DBConnection.GetConnection();
        using var cmd = new SqlCommand(
            "INSERT INTO dbo.ProductSpecs " +
            "(ProductID, ProductType, CPU, RAM, Storage, GPU, ScreenSize, BatteryWhr, OperatingSystem, ExtraSpecs) " +
            "VALUES (@ProductID, @ProductType, @CPU, @RAM, @Storage, @GPU, @ScreenSize, @BatteryWhr, @OperatingSystem, @ExtraSpecs)",
            conn);
        SetSpecParams(cmd, spec);
        conn.Open();
        cmd.ExecuteNonQuery();
    }

    public void Update(ProductSpec spec)
    {
        using var conn = DBConnection.GetConnection();
        using var cmd = new SqlCommand(
            "UPDATE dbo.ProductSpecs SET " +
            "ProductType = @ProductType, CPU = @CPU, RAM = @RAM, Storage = @Storage, GPU = @GPU, " +
            "ScreenSize = @ScreenSize, BatteryWhr = @BatteryWhr, OperatingSystem = @OperatingSystem, " +
            "ExtraSpecs = @ExtraSpecs WHERE ProductID = @ProductID", conn);
        SetSpecParams(cmd, spec);
        conn.Open();
        cmd.ExecuteNonQuery();
    }

    private static void SetSpecParams(SqlCommand cmd, ProductSpec spec)
    {
        cmd.Parameters.AddWithValue("@ProductID",       spec.ProductID);
        cmd.Parameters.AddWithValue("@ProductType",     (object?)spec.ProductType     ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@CPU",             (object?)spec.CPU             ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@RAM",             (object?)spec.RAM             ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Storage",         (object?)spec.Storage         ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@GPU",             (object?)spec.GPU             ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ScreenSize",      (object?)spec.ScreenSize      ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@BatteryWhr",      (object?)spec.BatteryWhr      ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@OperatingSystem", (object?)spec.OperatingSystem ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ExtraSpecs",      (object?)spec.ExtraSpecs      ?? DBNull.Value);
    }

    private static ProductSpec MapSpec(SqlDataReader reader) => new()
    {
        SpecID          = (int)reader["SpecID"],
        ProductID       = (int)reader["ProductID"],
        ProductType     = reader["ProductType"]     == DBNull.Value ? null : reader["ProductType"].ToString(),
        CPU             = reader["CPU"]             == DBNull.Value ? null : reader["CPU"].ToString(),
        RAM             = reader["RAM"]             == DBNull.Value ? null : reader["RAM"].ToString(),
        Storage         = reader["Storage"]         == DBNull.Value ? null : reader["Storage"].ToString(),
        GPU             = reader["GPU"]             == DBNull.Value ? null : reader["GPU"].ToString(),
        ScreenSize      = reader["ScreenSize"]      == DBNull.Value ? null : (decimal?)reader["ScreenSize"],
        BatteryWhr      = reader["BatteryWhr"]      == DBNull.Value ? null : (int?)reader["BatteryWhr"],
        OperatingSystem = reader["OperatingSystem"] == DBNull.Value ? null : reader["OperatingSystem"].ToString(),
        ExtraSpecs      = reader["ExtraSpecs"]      == DBNull.Value ? null : reader["ExtraSpecs"].ToString()
    };
}
