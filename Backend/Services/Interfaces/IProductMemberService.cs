using Backend.Models;

namespace Backend.Services;

public interface IProductMemberService
{
    Task<AddMemberResult> AddMemberAsync(
        int productId,
        string userToken,
        CancellationToken cancellationToken
    );
    Task<GetMembersResult> GetMembersAsync(
        int productId,
        int page,
        int size,
        CancellationToken cancellationToken
    );
    Task<RemoveMemberStatus> RemoveMemberAsync(
        int productId,
        string memberId,
        CancellationToken cancellationToken
    );
    Task<SimilarMembersResult> SearchSimilarMembersAsync(
        int productId,
        string gender,
        SimilarMemberCurrentRequest current,
        CancellationToken cancellationToken
    );
}

public enum AddMemberStatus
{
    Created,
    ProductNotFound,
    AlreadyRegistered,
    MemberIdentityConflict,
    InBodyMemberNotFound,
    QuotaExceeded,
    InBodyApiError,
}

public sealed record AddMemberResult(AddMemberStatus Status, ProductMemberResponse? Member = null);

public enum GetMembersStatus
{
    Success,
    ProductNotFound,
}

public sealed record GetMembersResult(
    GetMembersStatus Status,
    PagedProductMembersResponse? Page = null
);

public enum RemoveMemberStatus
{
    Removed,
    ProductNotFound,
    ProductMemberNotFound,
}

public enum SimilarMembersStatus
{
    Success,
    ProductNotFound,
}

public sealed record SimilarMembersResult(
    SimilarMembersStatus Status,
    IReadOnlyList<SimilarMemberResponse>? Members = null
);
