using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SbslFileTransformer.Models;

namespace SbslFileTransformer.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Plugin> Plugins { get; set; }
        public DbSet<Configuration> Configurations { get; set; }
        public DbSet<SftpUploadedFile> UploadedFiles { get; set; }
        public DbSet<AccountsLookup> Accounts { get; set; }
        public DbSet<EmailGroup> EmailGroups { get; set; }
        public DbSet<ProcessedReport> ProcessedReports { get; set; }
    }
}
