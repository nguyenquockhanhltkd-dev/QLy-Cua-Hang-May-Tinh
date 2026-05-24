using System.Text.RegularExpressions;
using BCrypt.Net;
using QLCuaHangMayTinh.DAL;
using QLCuaHangMayTinh.Models;

namespace QLCuaHangMayTinh.BLL;

public class UserBLL
{
    private readonly UserDAL _userDAL = new();

    public User? Login(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return null;
        try
        {
            var user = _userDAL.GetByUsername(username.Trim());
            if (user == null) return null;
            return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash) ? user : null;
        }
        catch
        {
            return null;
        }
    }

    public List<User> GetAll()
    {
        try { return _userDAL.GetAll(); }
        catch (Exception ex) { throw new Exception("Không thể tải danh sách người dùng: " + ex.Message); }
    }

    public bool Add(User user, string plainPassword)
    {
        if (string.IsNullOrWhiteSpace(user.Username))
            throw new Exception("Tên đăng nhập không được để trống.");
        if (!Regex.IsMatch(user.Username.Trim(), @"^[a-zA-Z0-9_]+$"))
            throw new Exception("Tên đăng nhập chỉ được chứa chữ cái (a-z, A-Z), số (0-9) và dấu gạch dưới (_). Không được có khoảng trắng hoặc ký tự đặc biệt.");
        if (string.IsNullOrWhiteSpace(user.FullName))
            throw new Exception("Họ tên không được để trống.");
        if (string.IsNullOrWhiteSpace(plainPassword) || plainPassword.Length < 6)
            throw new Exception("Mật khẩu phải có ít nhất 6 ký tự.");
        if (user.Role != "Admin" && user.Role != "Staff")
            throw new Exception("Role không hợp lệ.");
        if (_userDAL.IsUsernameExists(user.Username.Trim()))
            throw new Exception($"Tên đăng nhập '{user.Username.Trim()}' đã tồn tại. Vui lòng chọn tên khác.");
        try
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(plainPassword);
            _userDAL.Add(user);
            return true;
        }
        catch (Exception ex) { throw new Exception("Thêm người dùng thất bại: " + ex.Message); }
    }

    public bool Update(User user)
    {
        if (string.IsNullOrWhiteSpace(user.FullName))
            throw new Exception("Họ tên không được để trống.");
        if (user.Role != "Admin" && user.Role != "Staff")
            throw new Exception("Role không hợp lệ.");

        // Không cho phép thay đổi vai trò của tài khoản 'admin'
        var existing = _userDAL.GetByID(user.UserID)
            ?? throw new Exception("Không tìm thấy người dùng.");
        if (existing.Username.Equals("admin", StringComparison.OrdinalIgnoreCase)
            && user.Role != existing.Role)
            throw new Exception("Không được phép thay đổi vai trò của tài khoản 'admin'.");

        try { _userDAL.Update(user); return true; }
        catch (Exception ex) { throw new Exception("Cập nhật người dùng thất bại: " + ex.Message); }
    }

    public bool ChangePassword(int userID, string oldPassword, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            throw new Exception("Mật khẩu mới phải có ít nhất 6 ký tự.");
        try
        {
            var user = _userDAL.GetByID(userID)
                ?? throw new Exception("Không tìm thấy người dùng.");
            if (!BCrypt.Net.BCrypt.Verify(oldPassword, user.PasswordHash))
                throw new Exception("Mật khẩu cũ không đúng.");
            _userDAL.UpdatePassword(userID, BCrypt.Net.BCrypt.HashPassword(newPassword));
            return true;
        }
        catch (Exception ex) when (ex.Message.StartsWith("Mật khẩu") || ex.Message.StartsWith("Không tìm"))
        { throw; }
        catch (Exception ex) { throw new Exception("Đổi mật khẩu thất bại: " + ex.Message); }
    }

    public bool AdminResetPassword(int userID, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            throw new Exception("Mật khẩu mới phải có ít nhất 6 ký tự.");
        try
        {
            _userDAL.UpdatePassword(userID, BCrypt.Net.BCrypt.HashPassword(newPassword));
            return true;
        }
        catch (Exception ex) { throw new Exception("Đặt lại mật khẩu thất bại: " + ex.Message); }
    }

    public bool Delete(int userID, int currentUserID)
    {
        var user = _userDAL.GetByID(userID)
            ?? throw new Exception("Không tìm thấy người dùng.");

        if (user.Username.Equals("admin", StringComparison.OrdinalIgnoreCase))
            throw new Exception("Không thể xóa tài khoản 'admin'.");

        if (userID == currentUserID)
            throw new Exception("Không thể xóa tài khoản đang đăng nhập.");

        if (_userDAL.HasLinkedOrders(userID))
            throw new Exception(
                $"Không thể xóa tài khoản '{user.Username}' vì đã có lịch sử giao dịch. " +
                "Hãy dùng 'Đổi trạng thái' để vô hiệu hóa thay thế.");

        try { _userDAL.Delete(userID); return true; }
        catch (Exception ex) { throw new Exception("Xóa tài khoản thất bại: " + ex.Message); }
    }

    public bool SetActive(int userID, bool isActive)
    {
        try { _userDAL.SetActive(userID, isActive); return true; }
        catch (Exception ex) { throw new Exception("Cập nhật trạng thái thất bại: " + ex.Message); }
    }
}
