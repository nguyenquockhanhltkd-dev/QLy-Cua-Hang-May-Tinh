namespace QLCuaHangMayTinh.Models;

public class ProductSpec
{
    public int SpecID { get; set; }
    public int ProductID { get; set; }
    public string? ProductType { get; set; }
    public string? CPU { get; set; }
    public string? RAM { get; set; }
    public string? Storage { get; set; }
    public string? GPU { get; set; }
    public decimal? ScreenSize { get; set; }
    public int? BatteryWhr { get; set; }
    public string? OperatingSystem { get; set; }
    public string? ExtraSpecs { get; set; }
}
