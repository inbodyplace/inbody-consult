namespace Backend.Models;

public sealed class InBodyMeasurementWebhookRequest
{
    public string? EquipSerial { get; set; }
    public string? TelHP { get; set; }
    public string? UserID { get; set; }
    public string? TestDatetimes { get; set; }
    public string? Account { get; set; }
    public string? Equip { get; set; }
    public string? Type { get; set; }
    public string? IsTempData { get; set; }
}
