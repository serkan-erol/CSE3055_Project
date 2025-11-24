using Kismet.DataAccess.Persistence;
using Kismet.Entities.Models;
using Kismet.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Kismet.Repository.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly ApplicationDbContext _context;

    public CustomerRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Customer.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<Customer> AddAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        await _context.Customer.AddAsync(customer, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return customer;
    }

    public async Task<bool> DeleteAsync(int customerId, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Customer.FindAsync(new object[] { customerId }, cancellationToken);
        if (entity is null)
        {
            return false;
        }

        _context.Customer.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}

