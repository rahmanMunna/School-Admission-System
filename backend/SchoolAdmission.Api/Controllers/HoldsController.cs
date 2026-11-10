using Microsoft.AspNetCore.Mvc;
using SchoolAdmission.Business.Contracts;
using SchoolAdmission.Business.Services;
using SchoolAdmission.Business.Validation;

namespace SchoolAdmission.Api.Controllers;

[ApiController]
[Route("holds")]
public class HoldsController : ControllerBase
{
    private readonly IHoldService _holdService;

    public HoldsController(IHoldService holdService)
    {
        _holdService = holdService;
    }

    [HttpGet("{holdId}")]
    public async Task<IActionResult> Get(string holdId,[FromHeader(Name = "X-Student-Id")] string? studentId,CancellationToken ct)
    {
        if (studentId != null)
        {
            studentId = StudentIdRules.Validate(studentId);
        }

        HoldResponse hold = await _holdService.GetAsync(holdId, studentId, ct);
        return Ok(hold);                                             
    }

  
    [HttpPost("{holdId}/confirm")]
    public async Task<IActionResult> Confirm(string holdId,[FromHeader(Name = "X-Student-Id")] string? studentId,CancellationToken ct)
    {
        string validStudentId = StudentIdRules.Validate(studentId);
        ConfirmResponse result = await _holdService.ConfirmAsync(holdId, validStudentId, ct);
        return Ok(result);                                           
    }

    [HttpDelete("{holdId}")]
    public async Task<IActionResult> Cancel(string holdId,[FromHeader(Name = "X-Student-Id")] string? studentId,CancellationToken ct)
    {
        string validStudentId = StudentIdRules.Validate(studentId);
        await _holdService.CancelAsync(holdId, validStudentId, ct);
        return NoContent();                                          
    }
}