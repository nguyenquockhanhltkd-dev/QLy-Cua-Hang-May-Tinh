namespace QLCuaHangMayTinh.Models;

public class ExportOrder
{
    public int ExportID { get; set; }
    public string ExportCode { get; set; } = string.Empty;
    public int UserID { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public DateTime ExportDate { get; set; } = DateTime.Now;
    public decimal TotalAmount { get; set; } = 0;
    public string Status { get; set; } = "Pending";
    public string? Note { get; set; }

    // Navigation
    public string? CreatedByFullName { get; set; }
}
