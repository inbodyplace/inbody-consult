namespace Backend.Models;

public sealed class Member
{
    public string Id { get; set; } = string.Empty;
    public string UserToken { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
}
