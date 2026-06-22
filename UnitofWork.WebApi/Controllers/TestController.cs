using Microsoft.AspNetCore.Mvc;
using UnitofWork.Domain;

namespace UnitofWork.WebApi.Controllers;


[ApiController]
[Route("[controller]/[action]")]
public class TestController : ControllerBase
{
    private readonly DomainService domain;

    public TestController(DomainService domain)
    {
        this.domain = domain;
    }
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var result = await domain.SaveAsync();
        return Ok(result);
    }
}
