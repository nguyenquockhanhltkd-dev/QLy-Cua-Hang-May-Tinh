using QLCuaHangMayTinh.DAL;
using QLCuaHangMayTinh.Models;

namespace QLCuaHangMayTinh.BLL;

public class SupplierBLL
{
    private readonly SupplierDAL _supplierDAL = new();

    public List<Supplier> GetAll()
    {
        try { return _supplierDAL.GetAll(); }
        catch (Exception ex) { throw new Exception("Không thể tải nhà cung cấp: " + ex.Message); }
    }

    public Supplier? GetByID(int supplierID)
    {
        try { return _supplierDAL.GetByID(supplierID); }
        catch (Exception ex) { throw new Exception("Không thể tải nhà cung cấp: " + ex.Message); }
    }

    public bool Add(Supplier supplier)
    {
        if (string.IsNullOrWhiteSpace(supplier.Name))
            throw new Exception("Tên nhà cung cấp không được để trống.");
        try { _supplierDAL.Add(supplier); return true; }
        catch (Exception ex) { throw new Exception("Thêm nhà cung cấp thất bại: " + ex.Message); }
    }

    public bool Update(Supplier supplier)
    {
        if (string.IsNullOrWhiteSpace(supplier.Name))
            throw new Exception("Tên nhà cung cấp không được để trống.");
        try { _supplierDAL.Update(supplier); return true; }
        catch (Exception ex) { throw new Exception("Cập nhật nhà cung cấp thất bại: " + ex.Message); }
    }

    public bool SetActive(int supplierID, bool isActive)
    {
        try { _supplierDAL.SetActive(supplierID, isActive); return true; }
        catch (Exception ex) { throw new Exception("Cập nhật trạng thái thất bại: " + ex.Message); }
    }

    public bool Delete(int supplierID)
    {
        if (_supplierDAL.HasImportOrders(supplierID))
            throw new Exception("Không thể xóa nhà cung cấp này vì đã có phiếu nhập liên quan.");

        try { _supplierDAL.Delete(supplierID); return true; }
        catch (Exception ex) { throw new Exception("Xóa nhà cung cấp thất bại: " + ex.Message); }
    }
}
