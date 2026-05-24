using QLCuaHangMayTinh.DAL;
using QLCuaHangMayTinh.Models;

namespace QLCuaHangMayTinh.BLL;

public class InventoryBatchBLL
{
    private readonly InventoryBatchDAL _batchDAL = new();

    public List<BatchDetail> GetByProduct(int productID)
    {
        try { return _batchDAL.GetByProductID(productID); }
        catch (Exception ex) { throw new Exception("Không thể tải chi tiết lô hàng: " + ex.Message); }
    }
}
