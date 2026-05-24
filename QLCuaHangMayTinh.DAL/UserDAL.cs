using Microsoft.Data.SqlClient;
using QLCuaHangMayTinh.Models;

namespace QLCuaHangMayTinh.DAL;

public class UserDAL
{
    public User? GetByUsername(string username)
    {
        using var conn = DBConnection.GetConnection();
        using var cmd = new SqlCommand(
            "SELECT UserID, Username, PasswordHash, FullName, Role, IsActive, CreatedAt " +
            "FROM dbo.Users WHERE Username = @Username AND IsActive = 1", conn);
        cmd.Parameters.AddWithValue("@Username", username);
        conn.Open();
        using var reader = cmd.ExecuteReader();
        if (reader.Read())
            return MapUser(reader);
        return null;
    }

    public List<User> GetAll()
    {
        var list = new List<User>();
        using var conn = DBConnection.GetConnection();
        using var cmd = new SqlCommand(
            "SELECT UserID, Username, PasswordHash, FullName, Role, IsActive, CreatedAt " +
            "FROM dbo.Users ORDER BY FullName", conn);
        conn.Open();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            list.Add(MapUser(reader));
        return list;
    }

    public User? GetByID(int userID)
    {
        using var conn = DBConnection.GetConnection();
        using var cmd = new SqlCommand(
            "SELECT UserID, Username, PasswordHash, FullName, Role, IsActive, CreatedAt " +
            "FROM dbo.Users WHERE UserID = @UserID", conn);
        cmd.Parameters.AddWithValue("@UserID", userID);
        conn.Open();
        using var reader = cmd.ExecuteReader();
        if (reader.Read())
            return MapUser(reader);
        return null;
    }

    public bool IsUsernameExists(string username)
    {
        using var conn = DBConnection.GetConnection();
        using var cmd = new SqlCommand(
            "SELECT COUNT(1) FROM dbo.Users WHERE Username = @Username", conn);
        cmd.Parameters.AddWithValue("@Username", username);
        conn.Open();
        return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
    }

    public bool HasLinkedOrders(int userID)
    {
        using var conn = DBConnection.GetConnection();
        using var cmd = new SqlCommand(
            "SELECT CASE WHEN EXISTS (" +
            "    SELECT 1 FROM dbo.ImportOrders WHERE UserID = @UserID" +
            "    UNION ALL" +
            "    SELECT 1 FROM dbo.ExportOrders  WHERE UserID = @UserID" +
            "    UNION ALL" +
            "    SELECT 1 FROM dbo.SalesOrders   WHERE UserID = @UserID" +
            ") THEN 1 ELSE 0 END", conn);
        cmd.Parameters.AddWithValue("@UserID", userID);
        conn.Open();
        return Convert.ToInt32(cmd.ExecuteScalar()) == 1;
    }

    public void Delete(int userID)
    {
        using var conn = DBConnection.GetConnection();
        using var cmd = new SqlCommand(
            "DELETE FROM dbo.Users WHERE UserID = @UserID", conn);
        cmd.Parameters.AddWithValue("@UserID", userID);
        conn.Open();
        cmd.ExecuteNonQuery();
    }

    public void Add(User user)
    {
        using var conn = DBConnection.GetConnection();
        using var cmd = new SqlCommand(
            "INSERT INTO dbo.Users (Username, PasswordHash, FullName, Role, IsActive) " +
            "VALUES (@Username, @PasswordHash, @FullName, @Role, @IsActive)", conn);
        cmd.Parameters.AddWithValue("@Username", user.Username);
        cmd.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
        cmd.Parameters.AddWithValue("@FullName", user.FullName);
        cmd.Parameters.AddWithValue("@Role", user.Role);
        cmd.Parameters.AddWithValue("@IsActive", user.IsActive);
        conn.Open();
        cmd.ExecuteNonQuery();
    }

    public void Update(User user)
    {
        using var conn = DBConnection.GetConnection();
        using var cmd = new SqlCommand(
            "UPDATE dbo.Users SET FullName = @FullName, Role = @Role " +
            "WHERE UserID = @UserID", conn);
        cmd.Parameters.AddWithValue("@FullName", user.FullName);
        cmd.Parameters.AddWithValue("@Role", user.Role);
        cmd.Parameters.AddWithValue("@UserID", user.UserID);
        conn.Open();
        cmd.ExecuteNonQuery();
    }

    public void UpdatePassword(int userID, string passwordHash)
    {
        using var conn = DBConnection.GetConnection();
        using var cmd = new SqlCommand(
            "UPDATE dbo.Users SET PasswordHash = @PasswordHash WHERE UserID = @UserID", conn);
        cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);
        cmd.Parameters.AddWithValue("@UserID", userID);
        conn.Open();
        cmd.ExecuteNonQuery();
    }

    public void SetActive(int userID, bool isActive)
    {
        using var conn = DBConnection.GetConnection();
        using var cmd = new SqlCommand(
            "UPDATE dbo.Users SET IsActive = @IsActive WHERE UserID = @UserID", conn);
        cmd.Parameters.AddWithValue("@IsActive", isActive);
        cmd.Parameters.AddWithValue("@UserID", userID);
        conn.Open();
        cmd.ExecuteNonQuery();
    }

    private static User MapUser(SqlDataReader reader) => new()
    {
        UserID       = (int)reader["UserID"],
        Username     = reader["Username"].ToString()!,
        PasswordHash = reader["PasswordHash"].ToString()!,
        FullName     = reader["FullName"].ToString()!,
        Role         = reader["Role"].ToString()!,
        IsActive     = (bool)reader["IsActive"],
        CreatedAt    = (DateTime)reader["CreatedAt"]
    };
}
