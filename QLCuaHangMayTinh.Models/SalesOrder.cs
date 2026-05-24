namespace QLCuaHangMayTinh.Models;

public class SalesOrder
{
    public int      OrderID           { get; set; }
    public string   OrderCode         { get; set; } = "";
    public int      UserID            { get; set; }
    public string?  CustomerName      { get; set; }
    public string?  CustomerPhone     { get; set; }
    public string?  CustomerAddress   { get; set; }
    public string   PaymentMethod     { get; set; } = "Tiền mặt";
    public DateTime OrderDate         { get; set; } = DateTime.Now;
    public decimal  TotalAmount       { get; set; }
    public string   Status            { get; set; } = "Pending";
    public string?  Note              { get; set; }

    // Navigation
    public string?  CreatedByFullName { get; set; }
}
