using Blog.Domain.Entities;
using Blog.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Blog.Infrastructure.Repositories;

public class ReportRepository : IReportRepository
{
    private readonly AppDbContext _context;

    public ReportRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Report>> GetAllAsync()
    {
        return await _context.Reports
            .Include(r => r.Reporter)
            .Include(r => r.ReportedUser)
            .Include(r => r.Post)
            .Include(r => r.Reviewer)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<Report?> GetByIdAsync(int id)
    {
        return await _context.Reports.FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task AddAsync(Report report)
    {
        await _context.Reports.AddAsync(report);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Report report)
    {
        _context.Reports.Update(report);
        await _context.SaveChangesAsync();
    }
}
