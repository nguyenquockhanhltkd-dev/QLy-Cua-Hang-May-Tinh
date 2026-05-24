namespace QLCuaHangMayTinh.Models;

public class Product
{
    public int ProductID { get; set; }
    public int CategoryID { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string? Description { get; set; }
    public string Unit { get; set; } = "Cái";
    public decimal CostPrice { get; set; } = 0;
    public decimal SellPrice { get; set; } = 0;
    public int StockQty { get; set; } = 0;
    public int MinStockQty { get; set; } = 5;
    public string? ImagePath { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    // Navigation (chỉ dùng cho hiển thị, không lưu DB)
    public string? CategoryName { get; set; }
}
