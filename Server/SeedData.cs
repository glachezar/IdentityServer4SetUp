namespace Server
{
    using IdentityModel;
    using IdentityServer4.EntityFramework.DbContexts;
    using IdentityServer4.EntityFramework.Mappers;
    using IdentityServer4.EntityFramework.Storage;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using System.Security.Claims;
    using Data;

    public class SeedData
    {
        public static void EnsureSeedData(string connectionString)
        {
            var services = new ServiceCollection();
            services.AddLogging();

            services.AddDbContext<AspNetIdentityDbContext>(
                options => options.UseSqlServer(connectionString));

            services
                .AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<AspNetIdentityDbContext>()
                .AddDefaultTokenProviders();

            services.AddOperationalDbContext(
                options =>
                {
                    options.ConfigureDbContext = db =>
                        db.UseSqlServer(
                            connectionString,
                            sql => sql.MigrationsAssembly(typeof(SeedData).Assembly.FullName));
                });

            services.AddConfigurationDbContext(
                options =>
                {
                    options.ConfigureDbContext = db =>
                        db.UseSqlServer(
                            connectionString,
                            sql => sql.MigrationsAssembly(typeof(SeedData).Assembly.FullName));
                });

            var serviceProvider = services.BuildServiceProvider();

            using var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
            scope.ServiceProvider.GetService<PersistedGrantDbContext>().Database.Migrate();

            var context = scope.ServiceProvider.GetService<ConfigurationDbContext>();
            context.Database.Migrate();

            EnsureSeedData(context);


            var ctx = scope.ServiceProvider.GetService<AspNetIdentityDbContext>();
            ctx.Database.Migrate();
            EnsureUsers(scope);

        }

        private static void EnsureUsers(IServiceScope serviceScope)
        {
            var userMgr = serviceScope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

            var admin = userMgr.FindByNameAsync("admin").Result;
            if (admin == null)
            {
                admin = new IdentityUser()
                {
                    UserName = "admin",
                    Email = "admin@admin.com",
                    EmailConfirmed = true
                };
                var result = userMgr.CreateAsync(admin, "Admin123$").Result;
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }

                result = userMgr.AddClaimsAsync(
                        admin, new Claim[]
                        {
                            new Claim(JwtClaimTypes.Name, "Admin Admin"),
                            new Claim(JwtClaimTypes.GivenName, "Admin"),
                            new Claim(JwtClaimTypes.FamilyName, "Admin"),
                            new Claim(JwtClaimTypes.WebSite, "http://admin.com"),
                            new Claim("location", "somewhere")
                        })
                    .Result;
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }
            }
        }

        private static void EnsureSeedData(ConfigurationDbContext context)
        {
            if (!context.Clients.Any())
            {
                foreach (var client in Config.Clients.ToList())
                {
                    context.Clients.Add(client.ToEntity());
                }

                context.SaveChanges();
            }

            if (!context.ApiScopes.Any())
            {
                foreach (var resource in Config.ApiScopes.ToList())
                {
                    context.ApiScopes.Add(resource.ToEntity());
                }
                context.SaveChanges();
            }

        }
    }
}
