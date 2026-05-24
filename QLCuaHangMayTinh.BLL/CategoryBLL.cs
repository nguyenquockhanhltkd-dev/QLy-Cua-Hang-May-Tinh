using QLCuaHangMayTinh.DAL;
using QLCuaHangMayTinh.Models;

namespace QLCuaHangMayTinh.BLL;

public class CategoryBLL
{
    private readonly CategoryDAL _categoryDAL = new();

    public List<Category> GetAll()
    {
        try { return _categoryDAL.GetAll(); }
        catch (Exception ex) { throw new Exception("Không thể tải danh mục: " + ex.Message); }
    }

    public bool Add(Category category)
    {
        if (string.IsNullOrWhiteSpace(category.Name))
            throw new Exception("Tên danh mục không được để trống.");
        try { _categoryDAL.Add(category); return true; }
        catch (Exception ex) { throw new Exception("Thêm danh mục thất bại: " + ex.Message); }
    }

    public bool Update(Category category)
    {
        if (string.IsNullOrWhiteSpace(category.Name))
            throw new Exception("Tên danh mục không được để trống.");
        try { _categoryDAL.Update(category); return true; }
        catch (Exception ex) { throw new Exception("Cập nhật danh mục thất bại: " + ex.Message); }
    }

    public bool Delete(int categoryID)
    {
        if (_categoryDAL.HasProducts(categoryID))
            throw new Exception("Không thể xóa danh mục này vì vẫn còn sản phẩm thuộc danh mục đó.");

        try { _categoryDAL.Delete(categoryID); return true; }
        catch (Exception ex) { throw new Exception("Xóa danh mục thất bại: " + ex.Message); }
    }
}
