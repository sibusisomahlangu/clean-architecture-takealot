using OrderingService.Domain.Entities;

namespace OrderingService.Domain.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id);
    Task<Order> AddAsync(Order order);
    Task UpdateAsync(Order order);
    Task<List<Order>> GetByCustomerIdAsync(string customerId);
    Task<List<Order>> GetAllAsync();
}