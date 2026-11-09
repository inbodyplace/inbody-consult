namespace Backend.Models;

public sealed class SearchSimilarMembersRequest
{
    public string? Gender { get; set; }
    public SimilarMemberCurrentRequest? Current { get; set; }
}

public sealed class SimilarMemberCurrentRequest
{
    public decimal? Smm { get; set; }
    public decimal? Bfm { get; set; }
    public decimal? Weight { get; set; }
    public decimal? Pbf { get; set; }
}
