namespace QLCuaHangMayTinh.Models;

public class ReportDayRow
{
    public string  DateLabel   { get; set; } = "";  // "dd/MM/yyyy" hoặc "MM/yyyy"
    public int     TotalOrders { get; set; }
    public decimal TotalAmount { get; set; }
}

public class ReportMonthRow
{
    public int     Year        { get; set; }
    public int     Month       { get; set; }
    public string  MonthLabel  => $"Tháng {Month:D2}/{Year}";
    public int     TotalOrders { get; set; }
    public decimal TotalAmount { get; set; }
}

public class ReportTopProductRow
{
    public int     Rank         { get; set; }
    public string  ProductCode  { get; set; } = "";
    public string  ProductName  { get; set; } = "";
    public string  CategoryName { get; set; } = "";
    public string? Brand        { get; set; }
    public int     TotalQty     { get; set; }
    public decimal TotalAmount  { get; set; }
}
