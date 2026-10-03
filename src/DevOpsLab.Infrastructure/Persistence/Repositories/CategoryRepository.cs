using DevOpsLab.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace DevOpsLab.Infrastructure.Persistence.Repositories;

public sealed class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _dbContext;

    public CategoryRepository(AppDbContext dbContext) => _dbContext = dbContext;

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        => _dbContext.Categories.AnyAsync(c => c.Id == id, cancellationToken);
}
