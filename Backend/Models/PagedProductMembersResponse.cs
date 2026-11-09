namespace Backend.Models;

public sealed record PagedProductMembersResponse(
    IReadOnlyList<ProductMemberResponse> Items,
    int Page,
    int Size,
    int TotalItems,
    int TotalPages);
