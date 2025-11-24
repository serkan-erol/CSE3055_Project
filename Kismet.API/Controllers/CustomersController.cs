using Kismet.Bussiness.Interfaces;
using Kismet.Entities.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Kismet.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAsync(CancellationToken cancellationToken)
    {
        var result = await _customerService.GetCustomersAsync(cancellationToken);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result.Data);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] CreateCustomerDto dto, CancellationToken cancellationToken)
    {
        var result = await _customerService.CreateCustomerAsync(dto, cancellationToken);

        if (!result.Success || result.Data is null)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetAsync), new { id = result.Data.CustomerId }, result.Data);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var result = await _customerService.DeleteCustomerAsync(new DeleteCustomerDto { CustomerId = id }, cancellationToken);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }
}

