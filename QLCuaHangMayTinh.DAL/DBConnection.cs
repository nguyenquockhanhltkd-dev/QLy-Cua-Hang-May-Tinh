using Microsoft.Data.SqlClient;
using System.Configuration;
namespace QLCuaHangMayTinh.DAL;

public static class DBConnection
{
    // Lấy chuỗi kết nối từ App.config của project UI
    private static readonly string ConnectionString =
        ConfigurationManager.ConnectionStrings["ConnStr"].ConnectionString;

    public static SqlConnection GetConnection()
    {
        return new SqlConnection(ConnectionString);
    }
}
