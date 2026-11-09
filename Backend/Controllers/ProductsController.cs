using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/products")]
public sealed class ProductsController(
    IProductService productService,
    IProductMemberService productMemberService) : ControllerBase
{
    // Get programs
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ProductListItemResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductListItemResponse>>> GetProducts(CancellationToken cancellationToken)
    {
        var products = await productService.GetProductsAsync(cancellationToken);
        return Ok(products);
    }

    // Get program details and gender statistics
    [HttpGet("{productId:int}")]
    [ProducesResponseType(typeof(ProductDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDetailResponse>> GetProduct(
        int productId,
        CancellationToken cancellationToken)
    {
        var product = await productService.GetProductAsync(productId, cancellationToken);
        return product is null
            ? NotFound(new { code = "PRODUCT_NOT_FOUND", message = "Program not found." })
            : Ok(product);
    }

    // Create program
    [HttpPost]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductResponse>> CreateProduct(
        [FromBody] CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        var name = request.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name) || name.Length > 200)
        {
            return BadRequest(new { code = "INVALID_PRODUCT_NAME", message = "Enter a program name of up to 200 non-whitespace characters." });
        }

        var product = await productService.CreateProductAsync(name, cancellationToken);
        return Created("/api/products", product);
    }

    [HttpPost("{productId:int}/members")]
    [ProducesResponseType(typeof(ProductMemberResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<ProductMemberResponse>> AddMember(
        int productId,
        [FromBody] AddProductMemberRequest request,
        CancellationToken cancellationToken)
    {
        if (!PhoneToken.TryNormalize(request.UserToken, out var userToken))
            return BadRequest(new { code = "INVALID_USER_TOKEN", message = "Enter a phone number with 7 to 15 digits." });

        var result = await productMemberService.AddMemberAsync(productId, userToken, cancellationToken);
        return result.Status switch
        {
            AddMemberStatus.Created => StatusCode(StatusCodes.Status201Created, result.Member),
            AddMemberStatus.ProductNotFound => NotFound(new { code = "PRODUCT_NOT_FOUND", message = "Program not found." }),
            AddMemberStatus.AlreadyRegistered => Conflict(new { code = "MEMBER_ALREADY_REGISTERED", message = "Member is already registered for this program." }),
            AddMemberStatus.MemberIdentityConflict => Conflict(new { code = "MEMBER_IDENTITY_CONFLICT", message = "Member ID and phone number do not match existing registration." }),
            AddMemberStatus.InBodyMemberNotFound => NotFound(new { code = "INBODY_MEMBER_NOT_FOUND", message = "InBody member not found." }),
            AddMemberStatus.QuotaExceeded => StatusCode(StatusCodes.Status429TooManyRequests, new { code = "INBODY_DAILY_QUOTA_EXCEEDED", message = "The daily InBody API call limit has been reached. Try again after the quota resets." }),
            _ => StatusCode(StatusCodes.Status502BadGateway, new { code = "INBODY_API_ERROR", message = "Failed to retrieve the InBody member." })
        };
    }

    [HttpGet("{productId:int}/members")]
    [ProducesResponseType(typeof(PagedProductMembersResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedProductMembersResponse>> GetMembers(
        int productId,
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        CancellationToken cancellationToken = default)
    {
        // Validate pagination request
        if (page < 1 || size < 1 || size > 100)
            return BadRequest(new { code = "INVALID_PAGINATION", message = "Page must be at least 1 and size must be between 1 and 100." });

        var result = await productMemberService.GetMembersAsync(productId, page, size, cancellationToken);
        return result.Status switch
        {
            GetMembersStatus.ProductNotFound => NotFound(new { code = "PRODUCT_NOT_FOUND", message = "Program not found." }),
            _ => Ok(result.Page)
        };
    }

    [HttpDelete("{productId:int}/members/{memberId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveMember(
        int productId,
        string memberId,
        CancellationToken cancellationToken)
    {
        var result = await productMemberService.RemoveMemberAsync(productId, memberId, cancellationToken);
        return result switch
        {
            RemoveMemberStatus.Removed => NoContent(),
            RemoveMemberStatus.ProductNotFound => NotFound(new { code = "PRODUCT_NOT_FOUND", message = "Program not found." }),
            _ => NotFound(new { code = "PRODUCT_MEMBER_NOT_FOUND", message = "Member is not registered for this program." })
        };
    }

    [HttpPost("{productId:int}/similar-members/search")]
    [ProducesResponseType(typeof(IReadOnlyList<SimilarMemberResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<SimilarMemberResponse>>> SearchSimilarMembers(
        int productId,
        [FromBody] SearchSimilarMembersRequest request,
        CancellationToken cancellationToken)
    {
        var gender = request.Gender?.ToUpperInvariant();
        var current = request.Current;
        if (gender is not ("M" or "F")
            || current?.Smm is null
            || current.Bfm is null
            || current.Weight is null
            || current.Pbf is null)
            return BadRequest(new { code = "INVALID_SEARCH_INPUT", message = "Enter gender, skeletal muscle mass, body fat mass, weight, and body fat percentage." });

        var result = await productMemberService.SearchSimilarMembersAsync(productId, gender, current, cancellationToken);
        return result.Status switch
        {
            SimilarMembersStatus.ProductNotFound => NotFound(new { code = "PRODUCT_NOT_FOUND", message = "Program not found." }),
            _ => Ok(result.Members)
        };
    }
}
