using Microsoft.Data.SqlClient;
using QLCuaHangMayTinh.Models;

namespace QLCuaHangMayTinh.DAL;

public class InventoryBatchDAL
{
    public List<BatchDetail> GetByProductID(int productID)
    {
        var list = new List<BatchDetail>();
        using var conn = DBConnection.GetConnection();
        using var cmd = new SqlCommand(
            "SELECT DetailID, ImportID, ImportCode, ImportDate, OriginalQty, RemainingQty, UnitPrice " +
            "FROM dbo.vw_InventoryBatchDetail " +
            "WHERE ProductID = @ProductID " +
            "ORDER BY ImportDate ASC, DetailID ASC", conn);
        cmd.Parameters.AddWithValue("@ProductID", productID);
        conn.Open();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new BatchDetail
            {
                DetailID     = (int)reader["DetailID"],
                ImportID     = (int)reader["ImportID"],
                ImportCode   = reader["ImportCode"].ToString()!,
                ImportDate   = (DateTime)reader["ImportDate"],
                OriginalQty  = (int)reader["OriginalQty"],
                RemainingQty = (int)reader["RemainingQty"],
                UnitPrice    = (decimal)reader["UnitPrice"]
            });
        }
        return list;
    }
}
