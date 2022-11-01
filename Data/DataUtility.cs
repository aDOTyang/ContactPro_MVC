using ContactPro_MVC.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ContactPro_MVC.Data
{
    public static class DataUtility
    {
        // static only instantiated once at build, performs same function each time its called directly
        public static string GetConnectionString(IConfiguration configuration)
        {
            string? connectionString = configuration.GetConnectionString("DefaultConnection");
            string? databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
            
            // is database url present in Environment? if yes, connection is local & return connectionString -> else, build the connection string from provided database url
            return string.IsNullOrEmpty(databaseUrl) ? connectionString : BuildConnectionString(databaseUrl);
            // can be reformatted as the below if statement

            //if (string.IsNullOrEmpty(databaseUrl))
            //{ return connectionString;} else {
            //  return BuildConnectionString(databaseUrl);}
        }

        private static string BuildConnectionString(string databaseUrl)
        {
            var databaseUri = new Uri(databaseUrl);
            var userInfo = databaseUri.UserInfo.Split(':');
            var builder = new NpgsqlConnectionStringBuilder()
            {
                Host = databaseUri.Host,
                Port = databaseUri.Port,
                Username = userInfo[0],
                Password = userInfo[1],
                Database = databaseUri.LocalPath.TrimStart('/'),
                SslMode = SslMode.Require,
                TrustServerCertificate = true
            };
            return builder.ToString();
        }

        public static async Task ManageDataAsync(IServiceProvider svcProvider)
        {
            // this is an injection, but injection engine hasn't started yet -> small workaround to allow database access
            // obtains necessary services based on the IServiceProvider parameter 
            var dbContextSvc = svcProvider.GetRequiredService<ApplicationDbContext>();
            var userManagerSvc = svcProvider.GetRequiredService<UserManager<AppUser>>();

            // aligns local/production databases by checking migrations
            await dbContextSvc.Database.MigrateAsync();

            await SeedDemoUserAsync(userManagerSvc);
        }
        
        private static async Task SeedDemoUserAsync(UserManager<AppUser> userManager)
        {
            AppUser demoUser = new AppUser()
            {
                UserName = "demologin@contactpro.com",
                Email = "demologin@contactpro.com",
                FirstName = "Demo",
                LastName = "User",
                EmailConfirmed = true
            };

            try
            {
                AppUser user = await userManager.FindByEmailAsync(demoUser.Email);

                if(user == null)
                {
                    await userManager.CreateAsync(demoUser, "Abc&123!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("******** ERROR ********");
                Console.WriteLine("Error Seeding Demo Login User.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************");
                throw;
            }

        }
    }
}
