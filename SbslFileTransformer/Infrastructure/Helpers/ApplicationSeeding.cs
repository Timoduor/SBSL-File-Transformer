using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SbslFileTransformer.Data;

namespace SbslFileTransformer.Infrastructure.Helpers
{
    public class ApplicationSeeding
    {
        internal static async Task CreateDatabase(IServiceProvider serviceProvider, ILogger<Startup> logger)
        {
            ApplicationDbContext dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();

            logger.LogInformation("Creating Database tables...");

            //this must always be called before EnsureCreated which bypasses it
            dbContext.Database.Migrate();

            await dbContext.Database.EnsureCreatedAsync();
        }
    }
}