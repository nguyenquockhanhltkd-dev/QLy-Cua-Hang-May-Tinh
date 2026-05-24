using QLCuaHangMayTinh.DAL;
using QLCuaHangMayTinh.Models;

namespace QLCuaHangMayTinh.BLL;

public class ReportBLL
{
    private readonly ReportDAL _reportDAL = new();

    // ── Nhập kho ─────────────────────────────────────────────────────────────

    public List<ReportDayRow> GetImportByDay(DateTime from, DateTime to)
    {
        ValidateDateRange(from, to);
        if ((to - from).TotalDays > 366)
            throw new Exception("Khoảng ngày 'Theo ngày' không được vượt quá 366 ngày.");
        try { return _reportDAL.GetImportByDay(from, to); }
        catch (Exception ex) { throw new Exception("Không thể tải báo cáo nhập kho: " + ex.Message); }
    }

    public List<ReportMonthRow> GetImportByMonth(DateTime from, DateTime to)
    {
        ValidateDateRange(from, to);
        try { return _reportDAL.GetImportByMonth(from, to); }
        catch (Exception ex) { throw new Exception("Không thể tải báo cáo nhập kho: " + ex.Message); }
    }

    // ── Xuất kho ─────────────────────────────────────────────────────────────

    public List<ReportDayRow> GetExportByDay(DateTime from, DateTime to)
    {
        ValidateDateRange(from, to);
        if ((to - from).TotalDays > 366)
            throw new Exception("Khoảng ngày 'Theo ngày' không được vượt quá 366 ngày.");
        try { return _reportDAL.GetExportByDay(from, to); }
        catch (Exception ex) { throw new Exception("Không thể tải báo cáo xuất kho: " + ex.Message); }
    }

    public List<ReportMonthRow> GetExportByMonth(DateTime from, DateTime to)
    {
        ValidateDateRange(from, to);
        try { return _reportDAL.GetExportByMonth(from, to); }
        catch (Exception ex) { throw new Exception("Không thể tải báo cáo xuất kho: " + ex.Message); }
    }

    // ── KPI Tổng quan ────────────────────────────────────────────────────────

    public KpiSummary GetKpiSummary(int year, int month)
    {
        if (month < 1 || month > 12) throw new Exception("Tháng không hợp lệ.");
        try { return _reportDAL.GetKpiSummary(year, month); }
        catch (Exception ex) { throw new Exception("Không thể tải KPI: " + ex.Message); }
    }

    public decimal[] GetMonthlyRevenue(int year)
    {
        if (year < 2000 || year > DateTime.Today.Year + 1)
            throw new Exception("Năm không hợp lệ.");
        try { return _reportDAL.GetMonthlyRevenue(year); }
        catch (Exception ex) { throw new Exception("Không thể tải doanh thu theo tháng: " + ex.Message); }
    }

    // ── Top sản phẩm ─────────────────────────────────────────────────────────

    public List<ReportTopProductRow> GetTopExportProducts(
        DateTime from, DateTime to, int? categoryID = null, int top = 10)
    {
        ValidateDateRange(from, to);
        if (top <= 0) throw new Exception("Số lượng top phải lớn hơn 0.");
        try { return _reportDAL.GetTopExportProducts(from, to, categoryID, top); }
        catch (Exception ex) { throw new Exception("Không thể tải top sản phẩm: " + ex.Message); }
    }

    // ── Helper ───────────────────────────────────────────────────────────────

    private static void ValidateDateRange(DateTime from, DateTime to)
    {
        if (from > to)
            throw new Exception("Ngày bắt đầu không được lớn hơn ngày kết thúc.");
    }
}
