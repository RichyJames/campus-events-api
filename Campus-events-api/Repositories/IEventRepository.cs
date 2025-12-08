using Campus_events_api.Models;

namespace Campus_events_api.Repositories;

public interface IEventRepository
{
    Task<List<Event>> GetAllAsync();
    Task<Event?> GetByIdAsync(int id);
    Task AddAsync(Event evnt);
    Task UpdateAsync(Event evnt);
    Task DeleteAsync(Event evnt);
    Task<bool> ExistsAsync(int id);
}