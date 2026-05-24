using Microsoft.Data.SqlClient;
using QLCuaHangMayTinh.Models;

namespace QLCuaHangMayTinh.DAL;

public class CategoryDAL
{
    public List<Category> GetAll()
    {
        var list = new List<Category>();
        using var conn = DBConnection.GetConnection();
        using var cmd = new SqlCommand(
            "SELECT CategoryID, Name, Description FROM dbo.Categories ORDER BY Name", conn);
        conn.Open();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            list.Add(MapCategory(reader));
        return list;
    }

    public Category? GetByID(int categoryID)
    {
        using var conn = DBConnection.GetConnection();
        using var cmd = new SqlCommand(
            "SELECT CategoryID, Name, Description FROM dbo.Categories WHERE CategoryID = @CategoryID", conn);
        cmd.Parameters.AddWithValue("@CategoryID", categoryID);
        conn.Open();
        using var reader = cmd.ExecuteReader();
        if (reader.Read())
            return MapCategory(reader);
        return null;
    }

    public void Add(Category category)
    {
        using var conn = DBConnection.GetConnection();
        using var cmd = new SqlCommand(
            "INSERT INTO dbo.Categories (Name, Description) VALUES (@Name, @Description)", conn);
        cmd.Parameters.AddWithValue("@Name", category.Name);
        cmd.Parameters.AddWithValue("@Description", (object?)category.Description ?? DBNull.Value);
        conn.Open();
        cmd.ExecuteNonQuery();
    }

    public void Update(Category category)
    {
        using var conn = DBConnection.GetConnection();
        using var cmd = new SqlCommand(
            "UPDATE dbo.Categories SET Name = @Name, Description = @Description " +
            "WHERE CategoryID = @CategoryID", conn);
        cmd.Parameters.AddWithValue("@Name", category.Name);
        cmd.Parameters.AddWithValue("@Description", (object?)category.Description ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@CategoryID", category.CategoryID);
        conn.Open();
        cmd.ExecuteNonQuery();
    }

    public void Delete(int categoryID)
    {
        using var conn = DBConnection.GetConnection();
        using var cmd = new SqlCommand(
            "DELETE FROM dbo.Categories WHERE CategoryID = @CategoryID", conn);
        cmd.Parameters.AddWithValue("@CategoryID", categoryID);
        conn.Open();
        cmd.ExecuteNonQuery();
    }

    public bool HasProducts(int categoryID)
    {
        using var conn = DBConnection.GetConnection();
        using var cmd = new SqlCommand(
            "SELECT COUNT(1) FROM dbo.Products WHERE CategoryID = @CategoryID", conn);
        cmd.Parameters.AddWithValue("@CategoryID", categoryID);
        conn.Open();
        return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
    }

    private static Category MapCategory(SqlDataReader reader) => new()
    {
        CategoryID  = (int)reader["CategoryID"],
        Name        = reader["Name"].ToString()!,
        Description = reader["Description"] == DBNull.Value ? null : reader["Description"].ToString()
    };
}
