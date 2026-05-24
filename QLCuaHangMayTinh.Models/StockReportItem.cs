namespace QLCuaHangMayTinh.Models;

public class StockReportItem
{
    public int     ProductID   { get; set; }
    public string  ProductCode { get; set; } = "";
    public string  ProductName { get; set; } = "";
    public string? Brand       { get; set; }
    public string? Category    { get; set; }
    public int     StockQty     { get; set; }
    public int     MinStockQty  { get; set; }
    public decimal CostPrice    { get; set; }
    public decimal SellPrice    { get; set; }
    public int     AvailableQty { get; set; }
    public string  StockStatus  { get; set; } = "";
}
