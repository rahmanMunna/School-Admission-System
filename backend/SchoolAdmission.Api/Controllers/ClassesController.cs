using Microsoft.AspNetCore.Mvc;
using SchoolAdmission.Business.Contracts;
using SchoolAdmission.Business.Services;
using SchoolAdmission.Business.Validation;

namespace SchoolAdmission.Api.Controllers;

[ApiController]
[Route("classes")]
public class ClassesController : ControllerBase
{
    private readonly IClassService _classService;
    private readonly IHoldService _holdService;

    public ClassesController(IClassService classService, IHoldService holdService)
    {
        _classService = classService;
        _holdService = holdService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateClassRequest request, CancellationToken ct)
    {
        ClassCreatedResponse created = await _classService.CreateAsync(request, ct);
        return Created($"/classes/{created.Id}", created);       
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        IReadOnlyList<ClassResponse> classes = await _classService.ListAsync(ct);
        return Ok(classes);                                           
    }

    [HttpGet("{classId}")]
    public async Task<IActionResult> Get(string classId, CancellationToken ct)
    {
        ClassResponse schoolClass = await _classService.GetAsync(classId, ct);
        return Ok(schoolClass);                                     
    }

    [HttpPost("{classId}/holds")]
    public async Task<IActionResult> CreateHold(string classId,[FromHeader(Name = "X-Student-Id")] string? studentId,CancellationToken ct)
    {
        string validStudentId = StudentIdRules.Validate(studentId);  
        HoldCreatedResponse hold = await _holdService.CreateAsync(classId, validStudentId, ct);
        return Created($"/holds/{hold.HoldId}", hold);             
    }
}