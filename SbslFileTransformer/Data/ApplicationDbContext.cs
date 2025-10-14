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

        //public DbSet<Job> Jobs { get; set; }
        public DbSet<Configuration> Configurations { get; set; }
        public DbSet<SftpUploadedFile> UploadedFiles { get; set; }
        public DbSet<AccountsLookup> Accounts { get; set; }
        public DbSet<EmailGroup> EmailGroups { get; set; }
        public DbSet<ProcessedReport> ProcessedReports { get; set; }
        public DbSet<VisionRecordCollection> VisionRecordCollections { get; set; }
        public DbSet<VisionRecordCreditSettlement> VisionRecordCreditSettlements { get; set; }
        public DbSet<VisionRecordDebtors> VisionRecordDebtors { get; set; }
        public DbSet<ReportConfiguration> ReportConfigurations { get; set; }
    }
}


