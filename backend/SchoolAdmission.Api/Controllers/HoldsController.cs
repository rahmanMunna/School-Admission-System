using Microsoft.AspNetCore.Mvc;
using SchoolAdmission.Api.Http;
using SchoolAdmission.Business.Services;

namespace SchoolAdmission.Api.Controllers;

[ApiController]
[Route("holds")]
public class HoldsController(IHoldService holds) : ControllerBase
{
    // 5. GET /holds/{holdId}  (header optional)
    [HttpGet("{holdId}")]
    public async Task<IActionResult> Get(string holdId, CancellationToken ct) =>
        Ok(await holds.GetAsync(holdId, StudentIdHeader.Optional(Request), ct));

    // 6. POST /holds/{holdId}/confirm  (header required)
    [HttpPost("{holdId}/confirm")]
    public async Task<IActionResult> Confirm(string holdId, CancellationToken ct) =>
        Ok(await holds.ConfirmAsync(holdId, StudentIdHeader.Require(Request), ct));

    // 7. DELETE /holds/{holdId}  (header required)
    [HttpDelete("{holdId}")]
    public async Task<IActionResult> Cancel(string holdId, CancellationToken ct)
    {
        await holds.CancelAsync(holdId, StudentIdHeader.Require(Request), ct);
        return NoContent();                                              // 204
    }
}