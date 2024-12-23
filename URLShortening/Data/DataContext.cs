using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using URLShortening.Models;

namespace URLShortening.Data;

public class DataContext(DbContextOptions<DataContext> options) :
    IdentityDbContext<User>
    (options)
{
    public DbSet<Url> Urls { set; get; }
    public DbSet<AccessLog> AccessLogs { set; get; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Url>().HasIndex(u => u.ShortId).IsUnique();
        builder.Entity<Url>().HasIndex(u => u.LongUrl).IsUnique();
        builder.Entity<Url>().HasIndex(u => u.CustomAlias).IsUnique();
        base.OnModelCreating(builder);
    }
}
