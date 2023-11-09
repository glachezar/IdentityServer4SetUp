namespace Server
{
    using System.Reflection;
    using Data;
    using Microsoft.AspNetCore.Identity;
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