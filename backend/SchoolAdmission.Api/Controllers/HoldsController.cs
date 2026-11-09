using Microsoft.AspNetCore.Mvc;
using SchoolAdmission.Api.Http;
using SchoolAdmission.Business.Services;

namespace SchoolAdmission.Api.Controllers;

[ApiController]
[Route("holds")]
public class HoldsController : ControllerBase
{
    private readonly IHoldService _holds;
    public HoldsController(IHoldService holds)
    {
        _holds = holds;
    }

    [HttpGet("{holdId}")]
    public async Task<IActionResult> Get(string holdId, CancellationToken ct)
    {
        var studentId = StudentIdHeader.Optional(Request);

        var result = await _holds.GetAsync(holdId, studentId, ct);

        return Ok(result);
    }

    [HttpPost("{holdId}/confirm")]
    public async Task<IActionResult> Confirm(string holdId, CancellationToken ct)
    {
        var studentId = StudentIdHeader.Require(Request);

        var result = await _holds.ConfirmAsync(holdId, studentId, ct);

        return Ok(result);
    }

    [HttpDelete("{holdId}")]
    public async Task<IActionResult> Cancel(string holdId, CancellationToken ct)
    {
        var studentId = StudentIdHeader.Require(Request);

        await _holds.CancelAsync(holdId, studentId, ct);

        return NoContent(); 
    }
}