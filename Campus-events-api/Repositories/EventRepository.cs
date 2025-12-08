using Campus_events_api.Data;
using Campus_events_api.Models;
using Microsoft.EntityFrameworkCore;

namespace Campus_events_api.Repositories;

public class EventRepository : IEventRepository
{
    private readonly ApplicationDbContext _context;

    public EventRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Event>> GetAllAsync()
    {
        return await _context.Events
            .Include(e => e.Category)
            .ToListAsync();
    }

    public async Task<Event?> GetByIdAsync(int id)
    {
        return await _context.Events
            .Include(e => e.Category)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task AddAsync(Event evnt)
    {
        _context.Events.Add(evnt);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Event evnt)
    {
        _context.Events.Update(evnt);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Event evnt)
    {
        _context.Events.Remove(evnt);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Events.AnyAsync(e => e.Id == id);
    }
}