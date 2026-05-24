using QLCuaHangMayTinh.DAL;
using QLCuaHangMayTinh.Models;

namespace QLCuaHangMayTinh.BLL;

public class ImportOrderBLL
{
    private readonly ImportOrderDAL _importDAL = new();

    public List<ImportOrder> GetAll()
    {
        try { return _importDAL.GetAll(); }
        catch (Exception ex) { throw new Exception("Không thể tải danh sách phiếu nhập: " + ex.Message); }
    }

    public ImportOrder? GetByID(int importID)
    {
        try { return _importDAL.GetByID(importID); }
        catch (Exception ex) { throw new Exception("Không thể tải phiếu nhập: " + ex.Message); }
    }

    public List<ImportOrderDetail> GetDetails(int importID)
    {
        try { return _importDAL.GetDetails(importID); }
        catch (Exception ex) { throw new Exception("Không thể tải chi tiết phiếu nhập: " + ex.Message); }
    }

    public int CreateOrder(ImportOrder order, List<ImportOrderDetail> details)
    {
        if (order.SupplierID <= 0)
            throw new Exception("Vui lòng chọn nhà cung cấp.");
        if (details == null || details.Count == 0)
            throw new Exception("Phiếu nhập phải có ít nhất một sản phẩm.");
        foreach (var d in details)
        {
            if (d.Quantity <= 0)  throw new Exception("Số lượng phải lớn hơn 0.");
            if (d.UnitPrice < 0)  throw new Exception("Đơn giá không được âm.");
        }
        try
        {
            order.ImportCode  = GenerateImportCode();
            order.TotalAmount = details.Sum(d => d.Quantity * d.UnitPrice);
            order.Status      = "Pending";
            int importID      = _importDAL.Add(order);
            foreach (var d in details)
            {
                d.ImportID = importID;
                _importDAL.AddDetail(d);
            }
            return importID;
        }
        catch (Exception ex) { throw new Exception("Tạo phiếu nhập thất bại: " + ex.Message); }
    }

    public bool ConfirmImport(int importID, int userID)
    {
        try
        {
            var order = _importDAL.GetByID(importID)
                ?? throw new Exception("Không tìm thấy phiếu nhập.");
            if (order.Status != "Pending")
                throw new Exception($"Không thể xác nhận phiếu có trạng thái '{order.Status}'.");
            _importDAL.ConfirmImport(importID, userID);
            return true;
        }
        catch (Exception ex) when (ex.Message.StartsWith("Không"))
        { throw; }
        catch (Exception ex) { throw new Exception("Xác nhận nhập kho thất bại: " + ex.Message); }
    }

    public bool Cancel(int importID)
    {
        try
        {
            var order = _importDAL.GetByID(importID)
                ?? throw new Exception("Không tìm thấy phiếu nhập.");
            if (order.Status != "Pending")
                throw new Exception($"Không thể huỷ phiếu có trạng thái '{order.Status}'.");
            _importDAL.Cancel(importID);
            return true;
        }
        catch (Exception ex) when (ex.Message.StartsWith("Không"))
        { throw; }
        catch (Exception ex) { throw new Exception("Huỷ phiếu nhập thất bại: " + ex.Message); }
    }

    public List<ImportOrder> GetBySupplierID(int supplierID)
    {
        try { return _importDAL.GetBySupplierID(supplierID); }
        catch (Exception ex) { throw new Exception("Không thể tải lịch sử nhập: " + ex.Message); }
    }

    public bool Delete(int importID)
    {
        try
        {
            var order = _importDAL.GetByID(importID)
                ?? throw new Exception("Không tìm thấy phiếu nhập.");
            if (order.Status != "Pending")
                throw new Exception($"Chỉ được xóa phiếu nhập ở trạng thái Pending (phiếu này đang '{order.Status}').");
            _importDAL.Delete(importID);
            return true;
        }
        catch (Exception ex) when (ex.Message.StartsWith("Không") || ex.Message.StartsWith("Chỉ"))
        { throw; }
        catch (Exception ex) { throw new Exception("Xóa phiếu nhập thất bại: " + ex.Message); }
    }

    private static string GenerateImportCode()
        => "NK" + DateTime.Now.ToString("yyyyMMddHHmmss");
}
