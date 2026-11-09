namespace Backend.Models;

public sealed class ProductMember
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string MemberId { get; set; } = string.Empty;
    public DateTime AddedAt { get; set; }
    public long? BeforeMeasurementId { get; set; }
    public long? AfterMeasurementId { get; set; }
}
