using Microsoft.AspNetCore.Mvc;
using UnitofWork.WebApi.Domain;

namespace UnitofWork.WebApi.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class TestController : ControllerBase
{
    private readonly DomainService _domain;

    public TestController(DomainService domain)
    {
        _domain = domain;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var result = await _domain.SaveAsync();
        return Ok(result);
    }
}
