using System.Collections.Generic;

namespace Kismet.Entities.DTOs;

public class CustomerResponseDto
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public IReadOnlyCollection<string>? Errors { get; set; }
    public IEnumerable<CustomerDto>? Customer { get; set; }
}

