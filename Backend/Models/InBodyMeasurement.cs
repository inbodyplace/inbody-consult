namespace Backend.Models;

public sealed class InBodyMeasurement
{
    public long Id { get; set; }
    public string MemberId { get; set; } = string.Empty;
    public DateTime MeasuredAt { get; set; }
    public string EquipSerial { get; set; } = string.Empty;
    public decimal? Smm { get; set; }
    public decimal? Bfm { get; set; }
    public decimal? Weight { get; set; }
    public decimal? Pbf { get; set; }
}
