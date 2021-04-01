using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SbslFileTransformer.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SbslFileTransformer.Infrastructure.Helpers
{
    public class ApplicationSeeding
    {
        internal static async Task CreateDatabase(IServiceProvider serviceProvider, ILogger<Startup> logger)
        {
            var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();

            logger.LogInformation("Creating Database tables...");

            await dbContext.Database.EnsureCreatedAsync();
        }
    }
}
