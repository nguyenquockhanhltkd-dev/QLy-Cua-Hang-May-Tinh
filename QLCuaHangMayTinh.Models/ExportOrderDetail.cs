namespace QLCuaHangMayTinh.Models;

public class ExportOrderDetail
{
    public int DetailID { get; set; }
    public int ExportID { get; set; }
    public int ProductID { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal SubTotal { get; set; }

    // Navigation
    public string? ProductName { get; set; }
    public string? ProductCode { get; set; }
}
