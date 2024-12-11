using System;
using HybridCacheSample.Model;
using Microsoft.EntityFrameworkCore;

namespace HybridCacheSample;

public class AppDbContext: DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options): base(options)
    {
    }

    public DbSet<Product> Products { get; set; }
}

