namespace QLCuaHangMayTinh.Models;

public class SalesOrderDetail
{
    public int     DetailID    { get; set; }
    public int     OrderID     { get; set; }
    public int     ProductID   { get; set; }
    public int     Quantity    { get; set; }
    public decimal UnitPrice   { get; set; }
    public decimal SubTotal    { get; set; }

    // Navigation
    public string? ProductName { get; set; }
    public string? ProductCode { get; set; }
}
