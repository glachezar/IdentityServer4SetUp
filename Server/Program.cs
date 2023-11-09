namespace Server
{
    using System.Reflection;
    using Data;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    public class Program
    {
        public static void Main(string[] args)
        {
            var seed = args.Contains("/seed");
            if (seed)
            {
                args = args.Except(new[] { "/seed" }).ToArray();
            }

            var builder = WebApplication.CreateBuilder(args);

            var assembly = typeof(Program).GetTypeInfo().Assembly.GetName().Name;
            var defaultConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            if (seed)
            {
                SeedData.EnsureSeedData(defaultConnectionString);
            }

            builder.Services.AddDbContext<AspNetIdentityDbContext>(options =>
                options.UseSqlServer(defaultConnectionString,
                    b => b.MigrationsAssembly(assembly)));

            builder.Services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<AspNetIdentityDbContext>();

            builder.Services.AddIdentityServer()
                .AddAspNetIdentity<IdentityUser>()
                .AddInMemoryApiScopes(Config.ApiScopes)
                .AddInMemoryIdentityResources(Config.IdentityResources())
                .AddInMemoryClients(Config.Clients)
                .AddConfigurationStore(options =>
                {
                    options.ConfigureDbContext = b =>
                        b.UseSqlServer(defaultConnectionString,
                            opt => opt.MigrationsAssembly(assembly));

                })
                .AddOperationalStore(options =>
                {
                    options.ConfigureDbContext = b =>
                        b.UseSqlServer(defaultConnectionString,
                            opt => opt.MigrationsAssembly(assembly));
                })
                .AddDeveloperSigningCredential();

            builder.Services.ConfigureApplicationCookie(config =>
            {
                config.Cookie.Name = "CoffeeShopCookie";
                config.LoginPath = "/Auth/Login";
            });

            builder.Services.AddAuthorization();
            builder.Services.AddControllersWithViews();

            var app = builder.Build();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseIdentityServer();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
            app.Run();
        }
    }
}