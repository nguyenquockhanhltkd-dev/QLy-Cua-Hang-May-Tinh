namespace QLCuaHangMayTinh.Models;

public class ImportOrder
{
    public int ImportID { get; set; }
    public string ImportCode { get; set; } = string.Empty;
    public int SupplierID { get; set; }
    public int UserID { get; set; }
    public DateTime ImportDate { get; set; } = DateTime.Now;
    public decimal TotalAmount { get; set; } = 0;
    public string Status { get; set; } = "Pending";
    public string? Note { get; set; }

    // Navigation
    public string? SupplierName { get; set; }
    public string? CreatedByFullName { get; set; }
}
