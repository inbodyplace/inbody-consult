using System.Globalization;
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("webhook/inbody")]
public sealed class InBodyWebhookController(IInBodyWebhookService webhookService) : ControllerBase
{
    [HttpPost("measurements")]
    [HttpPost("/webhooks/inbody/measurements")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> ReceiveMeasurement(
        [FromBody] InBodyMeasurementWebhookRequest request,
        CancellationToken cancellationToken
    )
    {
        if (
            !PhoneToken.TryNormalize(request.TelHP, out var userToken)
            || string.IsNullOrWhiteSpace(request.EquipSerial)
            || !DateTime.TryParseExact(
                request.TestDatetimes,
                "yyyyMMddHHmmss",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out _
            )
        )
            return BadRequest(
                new
                {
                    code = "INVALID_WEBHOOK_PAYLOAD",
                    message = "Check the required webhook fields.",
                }
            );

        var result = await webhookService.HandleMeasurementAsync(
            userToken,
            request.EquipSerial.Trim(),
            request.TestDatetimes!,
            cancellationToken
        );
        return result.Status switch
        {
            InBodyWebhookStatus.Processed
            or InBodyWebhookStatus.Duplicate
            or InBodyWebhookStatus.MemberNotFound => Accepted(new { accepted = true }),
            _ => StatusCode(
                StatusCodes.Status502BadGateway,
                new
                {
                    code = "INBODY_API_ERROR",
                    message = "Could not retrieve measurement data from InBody.",
                }
            ),
        };
    }
}
