namespace QLCuaHangMayTinh.Models;

public class KpiSummary
{
    public int     Month               { get; set; }
    public int     Year                { get; set; }
    public decimal RevenueThisMonth    { get; set; }  // ExportOrders + SalesOrders Confirmed
    public decimal ImportCostThisMonth { get; set; }  // ImportOrders Confirmed
    public decimal CostOfGoodsSold     { get; set; }  // Giá vốn hàng đã bán (ExportDetails + SalesDetails × CostPrice)
    public decimal EstimatedProfit     => RevenueThisMonth - CostOfGoodsSold;
    public int     PendingOrders       { get; set; }  // ExportOrders + SalesOrders Pending
    public int     LowStockCount       { get; set; }  // Products StockQty <= MinStockQty
}
