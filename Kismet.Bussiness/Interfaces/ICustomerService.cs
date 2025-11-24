using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kismet.Core.Common;
using Kismet.Entities.DTOs;
using Kismet.Entities.Models;

namespace Kismet.Bussiness.Interfaces;

public interface ICustomerService
{
    Task<Result<IEnumerable<Customer>>> GetCustomersAsync(CancellationToken cancellationToken = default);
    Task<Result<CustomerDto>> CreateCustomerAsync(CreateCustomerDto dto, CancellationToken cancellationToken = default);
    Task<Result> DeleteCustomerAsync(DeleteCustomerDto dto, CancellationToken cancellationToken = default);
}

