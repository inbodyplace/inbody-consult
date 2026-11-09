namespace Backend.Models;

public sealed class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateOnly RegisteredAt { get; set; }
}
