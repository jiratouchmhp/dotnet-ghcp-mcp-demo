using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Backend.Models;

namespace Backend.Repository;

public interface IPaymentRepository
{
    Task<IEnumerable<Payment>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Payment?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Payment> CreateAsync(Payment payment, CancellationToken cancellationToken = default);
    Task<Payment?> UpdateAsync(Payment payment, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Payment>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
}