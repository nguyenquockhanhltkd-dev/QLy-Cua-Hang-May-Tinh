using QLCuaHangMayTinh.DAL;
using QLCuaHangMayTinh.Models;

namespace QLCuaHangMayTinh.BLL;

public class SalesOrderBLL
{
    private readonly SalesOrderDAL _orderDAL   = new();
    private readonly ProductDAL    _productDAL = new();

    public List<SalesOrder> GetAll()
    {
        try { return _orderDAL.GetAll(); }
        catch (Exception ex) { throw new Exception("Không thể tải danh sách đơn hàng: " + ex.Message); }
    }

    public SalesOrder? GetByID(int orderID)
    {
        try { return _orderDAL.GetByID(orderID); }
        catch (Exception ex) { throw new Exception("Không thể tải đơn hàng: " + ex.Message); }
    }

    public List<SalesOrderDetail> GetDetails(int orderID)
    {
        try { return _orderDAL.GetDetails(orderID); }
        catch (Exception ex) { throw new Exception("Không thể tải chi tiết đơn hàng: " + ex.Message); }
    }

    public int CreateOrder(SalesOrder order, List<SalesOrderDetail> details)
    {
        if (details == null || details.Count == 0)
            throw new Exception("Đơn hàng phải có ít nhất một sản phẩm.");
        if (string.IsNullOrWhiteSpace(order.CustomerName))
            throw new Exception("Vui lòng nhập tên khách hàng.");
        if (string.IsNullOrWhiteSpace(order.CustomerPhone))
            throw new Exception("Vui lòng nhập số điện thoại khách hàng.");
        foreach (var d in details)
        {
            if (d.Quantity <= 0) throw new Exception("Số lượng phải lớn hơn 0.");
            if (d.UnitPrice < 0) throw new Exception("Đơn giá không được âm.");
        }

        try
        {
            foreach (var d in details)
            {
                var product = _productDAL.GetByID(d.ProductID)
                    ?? throw new Exception($"Sản phẩm ID {d.ProductID} không tồn tại.");
                int available = _orderDAL.GetAvailableQty(d.ProductID);
                if (d.Quantity > available)
                    throw new Exception(
                        $"Sản phẩm '{product.Name}' chỉ còn {available} {product.Unit} khả dụng, " +
                        $"không đủ để xuất {d.Quantity}.");
            }

            order.OrderCode  = GenerateOrderCode();
            order.TotalAmount = details.Sum(d => d.Quantity * d.UnitPrice);
            order.Status     = "Pending";
            int orderID      = _orderDAL.Add(order);
            foreach (var d in details)
            {
                d.OrderID = orderID;
                _orderDAL.AddDetail(d);
            }
            return orderID;
        }
        catch (Exception ex) when (ex.Message.StartsWith("Sản phẩm"))
        { throw; }
        catch (Exception ex) { throw new Exception("Tạo đơn hàng thất bại: " + ex.Message); }
    }

    public bool ConfirmOrder(int orderID, int userID)
    {
        try
        {
            var order = _orderDAL.GetByID(orderID)
                ?? throw new Exception("Không tìm thấy đơn hàng.");
            if (order.Status != "Pending")
                throw new Exception($"Không thể xác nhận đơn có trạng thái '{order.Status}'.");
            _orderDAL.ConfirmOrder(orderID, userID);
            return true;
        }
        catch (Exception ex) when (ex.Message.StartsWith("Không") || ex.Message.Contains("tồn kho"))
        { throw; }
        catch (Exception ex) { throw new Exception("Xác nhận đơn hàng thất bại: " + ex.Message); }
    }

    public int GetAvailableQty(int productID)
    {
        try { return _orderDAL.GetAvailableQty(productID); }
        catch (Exception ex) { throw new Exception("Không thể lấy tồn khả dụng: " + ex.Message); }
    }

    public bool Cancel(int orderID)
    {
        try
        {
            var order = _orderDAL.GetByID(orderID)
                ?? throw new Exception("Không tìm thấy đơn hàng.");
            if (order.Status != "Pending")
                throw new Exception($"Không thể huỷ đơn có trạng thái '{order.Status}'.");
            _orderDAL.Cancel(orderID);
            return true;
        }
        catch (Exception ex) when (ex.Message.StartsWith("Không"))
        { throw; }
        catch (Exception ex) { throw new Exception("Huỷ đơn hàng thất bại: " + ex.Message); }
    }

    public bool Delete(int orderID)
    {
        try
        {
            var order = _orderDAL.GetByID(orderID)
                ?? throw new Exception("Không tìm thấy đơn hàng.");
            if (order.Status != "Pending")
                throw new Exception($"Chỉ được xóa đơn hàng ở trạng thái Pending (đơn này đang '{order.Status}').");
            _orderDAL.Delete(orderID);
            return true;
        }
        catch (Exception ex) when (ex.Message.StartsWith("Không") || ex.Message.StartsWith("Chỉ"))
        { throw; }
        catch (Exception ex) { throw new Exception("Xóa đơn hàng thất bại: " + ex.Message); }
    }

    private static string GenerateOrderCode()
        => "DH" + DateTime.Now.ToString("yyyyMMddHHmmss");
}
