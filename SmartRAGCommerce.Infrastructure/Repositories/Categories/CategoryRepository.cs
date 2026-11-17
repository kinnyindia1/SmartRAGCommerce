
using Microsoft.EntityFrameworkCore;
using SmartRAGCommerce.Application.Interfaces.Repositories;
using SmartRAGCommerce.Domain.Entities;
using SmartRAGCommerce.Infrastructure.Persistence.Context;

namespace SmartRAGCommerce.Infrastructure.Repositories.Categories;

public class CategoryRepository : ICategoryRepository
{
    private readonly ApplicationDbContext _context;

    public CategoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Category>> GetAllAsync()
    {
        return await _context.Categories
            .AsNoTracking()
            .Include(c => c.Products)
            .ToListAsync();
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        return await _context.Categories
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Category> AddAsync(Category category)
    {
        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();
        return category;
    }

    public async Task UpdateAsync(Category category)
    {
        _context.Categories.Update(category);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Category category)
    {
        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> NameExistsAsync(
        string name,
        int? excludeCategoryId = null)
    {
        return await _context.Categories.AnyAsync(c =>
            c.Name == name &&
            (!excludeCategoryId.HasValue ||
             c.Id != excludeCategoryId.Value));
    }

    public async Task<bool> HasProductsAsync(int categoryId)
    {
        return await _context.Products
            .AnyAsync(p => p.CategoryId == categoryId);
    }
}
