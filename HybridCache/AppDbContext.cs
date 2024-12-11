using System;
using HybridCache.Model;
using Microsoft.EntityFrameworkCore;

namespace HybridCache;

public class AppDbContext: DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options): base(options)
    {
    }

    public DbSet<Product> Products { get; set; }
}

