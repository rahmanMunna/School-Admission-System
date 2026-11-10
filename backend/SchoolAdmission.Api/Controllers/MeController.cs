using Microsoft.AspNetCore.Mvc;
using SchoolAdmission.Business.Contracts;
using SchoolAdmission.Business.Services;
using SchoolAdmission.Business.Validation;

namespace SchoolAdmission.Api.Controllers;

[ApiController]
[Route("me")]
public class MeController : ControllerBase
{
    private readonly IHoldService _holdService;

    public MeController(IHoldService holdService)
    {
        _holdService = holdService;
    }

    [HttpGet("holds")]
    public async Task<IActionResult> MyHolds([FromHeader(Name = "X-Student-Id")] string? studentId,CancellationToken ct)
    {
        string validStudentId = StudentIdRules.Validate(studentId);
        IReadOnlyList<MyHoldResponse> holds = await _holdService.ListMineAsync(validStudentId, ct);
        return Ok(holds);                                            
    }
}