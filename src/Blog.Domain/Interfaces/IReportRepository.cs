using Blog.Domain.Entities;

namespace Blog.Domain.Interfaces;

public interface IReportRepository
{
    // Returns a list of all reports
    Task<List<Report>> GetAllAsync();
    // Returns a report by its ID
    Task<Report?> GetByIdAsync(int id);
    
    Task AddAsync(Report report);
    Task UpdateAsync(Report report);
}
