using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Softcoinp.ERP.Domain.Entities;
using Softcoinp.ERP.Domain.Enums;
using Softcoinp.ERP.Infrastructure.Persistence;
using Softcoinp.ERP.WebAPI.DTOs;

namespace Softcoinp.ERP.WebAPI.Services;

public class BulletinBoardService
{
    private readonly ApplicationDbContext _context;

    public BulletinBoardService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<BulletinBoardPostDto>> GetActivePostsAsync(string tenantId)
    {
        var now = DateTime.UtcNow;

        return await _context.BulletinBoardPosts
            .Where(p => p.TenantId == tenantId && !p.IsDeleted && p.PublishedAt <= now)
            .Where(p => p.ExpiresAt == null || p.ExpiresAt > now)
            .OrderByDescending(p => p.IsPinned)
            .ThenByDescending(p => p.PublishedAt)
            .Select(p => new BulletinBoardPostDto
            {
                Id = p.Id,
                Title = p.Title,
                Content = p.Content,
                PublishedAt = p.PublishedAt,
                ExpiresAt = p.ExpiresAt,
                IsPinned = p.IsPinned,
                Category = p.Category.ToString(),
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<List<BulletinBoardPostAdminDto>> GetAllPostsAsync(string tenantId, bool includeArchived = false)
    {
        var query = _context.BulletinBoardPosts.Where(p => p.TenantId == tenantId);

        if (!includeArchived)
            query = query.Where(p => !p.IsDeleted);

        return await query
            .OrderByDescending(p => p.IsPinned)
            .ThenByDescending(p => p.PublishedAt)
            .Select(p => new BulletinBoardPostAdminDto
            {
                Id = p.Id,
                Title = p.Title,
                Content = p.Content,
                PublishedAt = p.PublishedAt,
                ExpiresAt = p.ExpiresAt,
                IsPinned = p.IsPinned,
                Category = p.Category.ToString(),
                IsDeleted = p.IsDeleted,
                CreatedByUserId = p.CreatedByUserId,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<BulletinBoardPostAdminDto?> GetByIdAsync(Guid id, string tenantId)
    {
        var post = await _context.BulletinBoardPosts
            .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId);

        if (post == null) return null;

        return new BulletinBoardPostAdminDto
        {
            Id = post.Id,
            Title = post.Title,
            Content = post.Content,
            PublishedAt = post.PublishedAt,
            ExpiresAt = post.ExpiresAt,
            IsPinned = post.IsPinned,
            Category = post.Category.ToString(),
            IsDeleted = post.IsDeleted,
            CreatedByUserId = post.CreatedByUserId,
            CreatedAt = post.CreatedAt,
            UpdatedAt = post.UpdatedAt
        };
    }

    public async Task<BulletinBoardPostAdminDto> CreateAsync(CreateBulletinBoardPostRequest request, string tenantId, string userId)
    {
        var category = string.IsNullOrEmpty(request.Category)
            ? BulletinCategory.Administrative
            : (BulletinCategory)Enum.Parse(typeof(BulletinCategory), request.Category);

        var post = new BulletinBoardPost
        {
            TenantId = tenantId,
            Title = request.Title,
            Content = request.Content,
            PublishedAt = request.PublishedAt ?? DateTime.UtcNow,
            ExpiresAt = request.ExpiresAt,
            IsPinned = request.IsPinned,
            Category = category,
            CreatedByUserId = userId
        };

        _context.BulletinBoardPosts.Add(post);
        await _context.SaveChangesAsync();

        return new BulletinBoardPostAdminDto
        {
            Id = post.Id,
            Title = post.Title,
            Content = post.Content,
            PublishedAt = post.PublishedAt,
            ExpiresAt = post.ExpiresAt,
            IsPinned = post.IsPinned,
            Category = post.Category.ToString(),
            IsDeleted = post.IsDeleted,
            CreatedByUserId = post.CreatedByUserId,
            CreatedAt = post.CreatedAt,
            UpdatedAt = post.UpdatedAt
        };
    }

    public async Task<BulletinBoardPostAdminDto?> UpdateAsync(Guid id, UpdateBulletinBoardPostRequest request, string tenantId)
    {
        var post = await _context.BulletinBoardPosts
            .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId);

        if (post == null) return null;

        if (request.Title != null) post.Title = request.Title;
        if (request.Content != null) post.Content = request.Content;
        if (request.PublishedAt.HasValue) post.PublishedAt = request.PublishedAt.Value;
        if (request.ExpiresAt != null) post.ExpiresAt = request.ExpiresAt;
        if (request.IsPinned.HasValue) post.IsPinned = request.IsPinned.Value;
        if (request.Category != null)
            post.Category = (BulletinCategory)Enum.Parse(typeof(BulletinCategory), request.Category);

        await _context.SaveChangesAsync();

        return await GetByIdAsync(id, tenantId);
    }

    public async Task<bool> ArchiveAsync(Guid id, string tenantId)
    {
        var post = await _context.BulletinBoardPosts
            .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId);

        if (post == null) return false;

        post.IsDeleted = true;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task ArchiveExpiredPostsAsync()
    {
        var now = DateTime.UtcNow;
        var expired = await _context.BulletinBoardPosts
            .Where(p => !p.IsDeleted && p.ExpiresAt != null && p.ExpiresAt <= now)
            .ToListAsync();

        foreach (var post in expired)
        {
            post.IsDeleted = true;
        }

        if (expired.Count > 0)
            await _context.SaveChangesAsync();
    }
}
