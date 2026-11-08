using Microsoft.AspNetCore.Mvc;
using SchoolAdmission.Api.Identity;
using SchoolAdmission.Api.Services;

namespace SchoolAdmission.Api.Controllers;

[ApiController]
[Route("holds")]
public class HoldsController(IHoldService holds) : ControllerBase
{
    // 5. GET /holds/{holdId}  (header optional)
    [HttpGet("{holdId}")]
    public async Task<IActionResult> Get(string holdId, CancellationToken ct) =>
        Ok(await holds.GetAsync(holdId, StudentId.Optional(Request), ct));

    // 6. POST /holds/{holdId}/confirm  (header required)
    [HttpPost("{holdId}/confirm")]
    public async Task<IActionResult> Confirm(string holdId, CancellationToken ct) =>
        Ok(await holds.ConfirmAsync(holdId, StudentId.Require(Request), ct));

    // 7. DELETE /holds/{holdId}  (header required)
    [HttpDelete("{holdId}")]
    public async Task<IActionResult> Cancel(string holdId, CancellationToken ct)
    {
        await holds.CancelAsync(holdId, StudentId.Require(Request), ct);
        return NoContent();                                              // 204
    }
}