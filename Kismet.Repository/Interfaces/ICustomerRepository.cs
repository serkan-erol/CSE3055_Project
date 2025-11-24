using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kismet.Entities.Models;

namespace Kismet.Repository.Interfaces;

public interface ICustomerRepository
{
    Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Customer> AddAsync(Customer customer, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int customerId, CancellationToken cancellationToken = default);
}

