using System.Collections.Generic;
using System.Threading.Tasks;
using Backend.Models;

namespace Backend.Repository;

public interface IPaymentRepository
{
    Task<IEnumerable<Payment>> GetAllAsync();
    Task<Payment?> GetByIdAsync(int id);
    Task<IEnumerable<Payment>> GetByUserIdAsync(int userId);
    Task<Payment> CreateAsync(Payment payment);
    Task<Payment?> UpdateAsync(Payment payment);
    Task<bool> DeleteAsync(int id);
}