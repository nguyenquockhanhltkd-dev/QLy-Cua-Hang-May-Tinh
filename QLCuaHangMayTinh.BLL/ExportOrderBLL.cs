using QLCuaHangMayTinh.DAL;
using QLCuaHangMayTinh.Models;

namespace QLCuaHangMayTinh.BLL;

public class ExportOrderBLL
{
    private readonly ExportOrderDAL _exportDAL  = new();
    private readonly ProductDAL     _productDAL = new();

    public List<ExportOrder> GetAll()
    {
        try { return _exportDAL.GetAll(); }
        catch (Exception ex) { throw new Exception("Không thể tải danh sách phiếu xuất: " + ex.Message); }
    }

    public ExportOrder? GetByID(int exportID)
    {
        try { return _exportDAL.GetByID(exportID); }
        catch (Exception ex) { throw new Exception("Không thể tải phiếu xuất: " + ex.Message); }
    }

    public List<ExportOrderDetail> GetDetails(int exportID)
    {
        try { return _exportDAL.GetDetails(exportID); }
        catch (Exception ex) { throw new Exception("Không thể tải chi tiết phiếu xuất: " + ex.Message); }
    }

    public int CreateOrder(ExportOrder order, List<ExportOrderDetail> details)
    {
        if (details == null || details.Count == 0)
            throw new Exception("Phiếu xuất phải có ít nhất một sản phẩm.");
        foreach (var d in details)
        {
            if (d.Quantity <= 0)  throw new Exception("Số lượng phải lớn hơn 0.");
            if (d.UnitPrice < 0)  throw new Exception("Đơn giá không được âm.");
        }

        // Kiểm tra tồn khả dụng trước khi tạo phiếu
        // (AvailableQty = StockQty − tổng số lượng các phiếu xuất đang Pending)
        try
        {
            foreach (var d in details)
            {
                var product = _productDAL.GetByID(d.ProductID)
                    ?? throw new Exception($"Sản phẩm ID {d.ProductID} không tồn tại.");
                int available = _exportDAL.GetAvailableQty(d.ProductID);
                if (d.Quantity > available)
                    throw new Exception($"Sản phẩm '{product.Name}' chỉ còn {available} {product.Unit} khả dụng (đang có phiếu xuất chờ xử lý), không đủ để xuất {d.Quantity}.");
            }

            order.ExportCode  = GenerateExportCode();
            order.TotalAmount = details.Sum(d => d.Quantity * d.UnitPrice);
            order.Status      = "Pending";
            int exportID      = _exportDAL.Add(order);
            foreach (var d in details)
            {
                d.ExportID = exportID;
                _exportDAL.AddDetail(d);
            }
            return exportID;
        }
        catch (Exception ex) when (ex.Message.StartsWith("Sản phẩm"))
        { throw; }
        catch (Exception ex) { throw new Exception("Tạo phiếu xuất thất bại: " + ex.Message); }
    }

    public bool ConfirmExport(int exportID, int userID)
    {
        try
        {
            var order = _exportDAL.GetByID(exportID)
                ?? throw new Exception("Không tìm thấy phiếu xuất.");
            if (order.Status != "Pending")
                throw new Exception($"Không thể xác nhận phiếu có trạng thái '{order.Status}'.");
            _exportDAL.ConfirmExport(exportID, userID);
            return true;
        }
        catch (Exception ex) when (ex.Message.StartsWith("Không") || ex.Message.Contains("tồn kho"))
        { throw; }
        catch (Exception ex) { throw new Exception("Xác nhận xuất kho thất bại: " + ex.Message); }
    }

    public int GetAvailableQty(int productID)
    {
        try { return _exportDAL.GetAvailableQty(productID); }
        catch (Exception ex) { throw new Exception("Không thể lấy tồn khả dụng: " + ex.Message); }
    }

    public bool Cancel(int exportID)
    {
        try
        {
            var order = _exportDAL.GetByID(exportID)
                ?? throw new Exception("Không tìm thấy phiếu xuất.");
            if (order.Status != "Pending")
                throw new Exception($"Không thể huỷ phiếu có trạng thái '{order.Status}'.");
            _exportDAL.Cancel(exportID);
            return true;
        }
        catch (Exception ex) when (ex.Message.StartsWith("Không"))
        { throw; }
        catch (Exception ex) { throw new Exception("Huỷ phiếu xuất thất bại: " + ex.Message); }
    }

    public bool Delete(int exportID)
    {
        try
        {
            var order = _exportDAL.GetByID(exportID)
                ?? throw new Exception("Không tìm thấy phiếu xuất.");
            if (order.Status != "Pending")
                throw new Exception($"Chỉ được xóa phiếu xuất ở trạng thái Pending (phiếu này đang '{order.Status}').");
            _exportDAL.Delete(exportID);
            return true;
        }
        catch (Exception ex) when (ex.Message.StartsWith("Không") || ex.Message.StartsWith("Chỉ"))
        { throw; }
        catch (Exception ex) { throw new Exception("Xóa phiếu xuất thất bại: " + ex.Message); }
    }

    private static string GenerateExportCode()
        => "XK" + DateTime.Now.ToString("yyyyMMddHHmmss");
}
