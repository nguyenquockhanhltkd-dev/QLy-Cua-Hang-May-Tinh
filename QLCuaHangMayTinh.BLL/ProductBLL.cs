using QLCuaHangMayTinh.DAL;
using QLCuaHangMayTinh.Models;

namespace QLCuaHangMayTinh.BLL;

public class ProductBLL
{
    private readonly ProductDAL     _productDAL  = new();
    private readonly ProductSpecDAL _specDAL     = new();

    public List<Product> GetAll(bool includeInactive = false)
    {
        try { return _productDAL.GetAll(includeInactive); }
        catch (Exception ex) { throw new Exception("Không thể tải danh sách sản phẩm: " + ex.Message); }
    }

    public Product? GetByID(int productID)
    {
        try { return _productDAL.GetByID(productID); }
        catch (Exception ex) { throw new Exception("Không thể tải sản phẩm: " + ex.Message); }
    }

    public ProductSpec? GetSpec(int productID)
    {
        try { return _specDAL.GetByProductID(productID); }
        catch (Exception ex) { throw new Exception("Không thể tải thông số kỹ thuật: " + ex.Message); }
    }

    public bool Add(Product product, ProductSpec? spec)
    {
        if (_productDAL.IsProductCodeExists(product.ProductCode, 0))
            throw new Exception($"Mã sản phẩm '{product.ProductCode}' đã tồn tại.");
        ValidateProduct(product);
        try
        {
            int newID = _productDAL.Add(product);
            if (spec != null)
            {
                spec.ProductID = newID;
                _specDAL.Add(spec);
            }
            return true;
        }
        catch (Exception ex) { throw new Exception("Thêm sản phẩm thất bại: " + ex.Message); }
    }

    public bool Update(Product product, ProductSpec? spec)
    {
        if (_productDAL.IsProductCodeExists(product.ProductCode, product.ProductID))
            throw new Exception($"Mã sản phẩm '{product.ProductCode}' đã được dùng bởi sản phẩm khác.");
        ValidateProduct(product);
        try
        {
            _productDAL.Update(product);
            if (spec != null)
            {
                spec.ProductID = product.ProductID;
                var existing = _specDAL.GetByProductID(product.ProductID);
                if (existing != null)
                    _specDAL.Update(spec);
                else
                    _specDAL.Add(spec);
            }
            return true;
        }
        catch (Exception ex) { throw new Exception("Cập nhật sản phẩm thất bại: " + ex.Message); }
    }

    public bool SetActive(int productID, bool isActive)
    {
        if (!isActive)
        {
            var product = _productDAL.GetByID(productID)
                ?? throw new Exception("Không tìm thấy sản phẩm.");

            if (_productDAL.HasPendingOutOrders(productID))
                throw new Exception(
                    $"Không thể vô hiệu hóa '{product.Name}': " +
                    "sản phẩm đang có trong phiếu xuất hoặc đơn hàng chưa được duyệt.");

            if (_productDAL.HasPendingImportOrders(productID))
                throw new Exception(
                    $"Không thể vô hiệu hóa '{product.Name}': " +
                    "sản phẩm đang có trong phiếu nhập chưa được duyệt.");
        }
        try { _productDAL.SetActive(productID, isActive); return true; }
        catch (Exception ex) { throw new Exception("Cập nhật trạng thái sản phẩm thất bại: " + ex.Message); }
    }

    public bool IsReferencedInOrders(int productID)
    {
        try { return _productDAL.IsReferencedInOrders(productID); }
        catch (Exception ex) { throw new Exception("Không thể kiểm tra giao dịch: " + ex.Message); }
    }

    public bool Delete(int productID)
    {
        var product = _productDAL.GetByID(productID)
            ?? throw new Exception("Không tìm thấy sản phẩm.");

        if (product.IsActive)
            throw new Exception(
                $"Sản phẩm '{product.Name}' đang hoạt động. " +
                "Vui lòng vô hiệu hóa trước khi xóa vĩnh viễn.");

        if (product.StockQty > 0)
            throw new Exception(
                $"Không thể xóa sản phẩm '{product.Name}' vì còn {product.StockQty} {product.Unit} trong kho.");

        if (_productDAL.IsReferencedInOrders(productID))
            throw new Exception(
                $"Không thể xóa sản phẩm '{product.Name}' vì đã xuất hiện trong phiếu nhập, phiếu xuất hoặc đơn hàng.");

        try
        {
            _productDAL.Delete(productID);
            return true;
        }
        catch (Exception ex)
        {
            throw new Exception("Xóa sản phẩm thất bại: " + ex.Message);
        }
    }

    public string GetNextCode(string prefix)
    {
        try
        {
            int next = _productDAL.GetNextSequentialNumber(prefix) + 1;
            return $"{prefix}-{next:D3}";
        }
        catch (Exception ex) { throw new Exception("Không thể tạo mã sản phẩm: " + ex.Message); }
    }

    public List<StockReportItem> GetStockReport()
    {
        try { return _productDAL.GetStockReport(); }
        catch (Exception ex) { throw new Exception("Không thể tải báo cáo tồn kho: " + ex.Message); }
    }

    private static void ValidateProduct(Product p)
    {
        if (string.IsNullOrWhiteSpace(p.ProductCode))
            throw new Exception("Mã sản phẩm không được để trống.");
        if (string.IsNullOrWhiteSpace(p.Name))
            throw new Exception("Tên sản phẩm không được để trống.");
        if (p.CategoryID <= 0)
            throw new Exception("Vui lòng chọn danh mục.");
        if (p.CostPrice < 0)
            throw new Exception("Giá vốn không được âm.");
        if (p.SellPrice < 0)
            throw new Exception("Giá bán không được âm.");
        if (p.MinStockQty < 0)
            throw new Exception("Tồn kho tối thiểu không được âm.");
        if (p.SellPrice < p.CostPrice)
            throw new Exception("Giá bán không được thấp hơn giá vốn.");
    }
}
