namespace QLCuaHangMayTinh.Models;

public class BatchDetail
{
    public int      DetailID     { get; set; }
    public int      ImportID     { get; set; }
    public string   ImportCode   { get; set; } = "";
    public DateTime ImportDate   { get; set; }
    public int      OriginalQty  { get; set; }
    public int      RemainingQty { get; set; }
    public decimal  UnitPrice    { get; set; }
}
