using Microsoft.Data.SqlClient;
using QLCuaHangMayTinh.Models;

namespace QLCuaHangMayTinh.DAL;

public class SupplierDAL
{
    public List<Supplier> GetAll()
    {
        var list = new List<Supplier>();
        using var conn = DBConnection.GetConnection();
        using var cmd = new SqlCommand(
            "SELECT SupplierID, Name, Phone, Email, Address, ContactPerson, IsActive " +
            "FROM dbo.Suppliers WHERE IsActive = 1 ORDER BY Name", conn);
        conn.Open();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            list.Add(MapSupplier(reader));
        return list;
    }

    public Supplier? GetByID(int supplierID)
    {
        using var conn = DBConnection.GetConnection();
        using var cmd = new SqlCommand(
            "SELECT SupplierID, Name, Phone, Email, Address, ContactPerson, IsActive " +
            "FROM dbo.Suppliers WHERE SupplierID = @SupplierID", conn);
        cmd.Parameters.AddWithValue("@SupplierID", supplierID);
        conn.Open();
        using var reader = cmd.ExecuteReader();
        if (reader.Read())
            return MapSupplier(reader);
        return null;
    }

    public void Add(Supplier supplier)
    {
        using var conn = DBConnection.GetConnection();
        using var cmd = new SqlCommand(
            "INSERT INTO dbo.Suppliers (Name, Phone, Email, Address, ContactPerson, IsActive) " +
            "VALUES (@Name, @Phone, @Email, @Address, @ContactPerson, @IsActive)", conn);
        cmd.Parameters.AddWithValue("@Name", supplier.Name);
        cmd.Parameters.AddWithValue("@Phone",         (object?)supplier.Phone         ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Email",         (object?)supplier.Email         ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Address",       (object?)supplier.Address       ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ContactPerson", (object?)supplier.ContactPerson ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@IsActive", supplier.IsActive);
        conn.Open();
        cmd.ExecuteNonQuery();
    }

    public void Update(Supplier supplier)
    {
        using var conn = DBConnection.GetConnection();
        using var cmd = new SqlCommand(
            "UPDATE dbo.Suppliers SET Name = @Name, Phone = @Phone, Email = @Email, " +
            "Address = @Address, ContactPerson = @ContactPerson WHERE SupplierID = @SupplierID", conn);
        cmd.Parameters.AddWithValue("@Name", supplier.Name);
        cmd.Parameters.AddWithValue("@Phone",         (object?)supplier.Phone         ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Email",         (object?)supplier.Email         ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Address",       (object?)supplier.Address       ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ContactPerson", (object?)supplier.ContactPerson ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@SupplierID", supplier.SupplierID);
        conn.Open();
        cmd.ExecuteNonQuery();
    }

    public void SetActive(int supplierID, bool isActive)
    {
        using var conn = DBConnection.GetConnection();
        using var cmd = new SqlCommand(
            "UPDATE dbo.Suppliers SET IsActive = @IsActive WHERE SupplierID = @SupplierID", conn);
        cmd.Parameters.AddWithValue("@IsActive", isActive);
        cmd.Parameters.AddWithValue("@SupplierID", supplierID);
        conn.Open();
        cmd.ExecuteNonQuery();
    }

    public bool HasImportOrders(int supplierID)
    {
        using var conn = DBConnection.GetConnection();
        using var cmd = new SqlCommand(
            "SELECT COUNT(1) FROM dbo.ImportOrders WHERE SupplierID = @SupplierID", conn);
        cmd.Parameters.AddWithValue("@SupplierID", supplierID);
        conn.Open();
        return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
    }

    public void Delete(int supplierID)
    {
        using var conn = DBConnection.GetConnection();
        using var cmd = new SqlCommand(
            "DELETE FROM dbo.Suppliers WHERE SupplierID = @SupplierID", conn);
        cmd.Parameters.AddWithValue("@SupplierID", supplierID);
        conn.Open();
        cmd.ExecuteNonQuery();
    }

    private static Supplier MapSupplier(SqlDataReader reader) => new()
    {
        SupplierID    = (int)reader["SupplierID"],
        Name          = reader["Name"].ToString()!,
        Phone         = reader["Phone"]         == DBNull.Value ? null : reader["Phone"].ToString(),
        Email         = reader["Email"]         == DBNull.Value ? null : reader["Email"].ToString(),
        Address       = reader["Address"]       == DBNull.Value ? null : reader["Address"].ToString(),
        ContactPerson = reader["ContactPerson"] == DBNull.Value ? null : reader["ContactPerson"].ToString(),
        IsActive      = (bool)reader["IsActive"]
    };
}
