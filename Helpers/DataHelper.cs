using MoviePro.Data;
using Microsoft.EntityFrameworkCore;

namespace TheBlogProject.Helpers
{
    public static class DataHelper
    {
        public static async Task ManageDataAsync(IServiceProvider svcProvider)
        {
            // get and instance of the db application context
            var dbContextSvc = svcProvider.GetRequiredService<ApplicationDbContext>();

            // migration: This is equivalent to update-database
            await dbContextSvc.Database.MigrateAsync();
        }
    }
}
