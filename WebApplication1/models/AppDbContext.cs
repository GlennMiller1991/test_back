using Microsoft.EntityFrameworkCore;

namespace WebApplication1.models;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<GuestEntry>GuestEntries { get; set; }
}