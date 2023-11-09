namespace Server
{
    using System.Reflection;
    using Microsoft.EntityFrameworkCore;

    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var assembly = typeof(Program).GetTypeInfo().Assembly.GetName().Name;
            var defaultConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            builder.Services.AddIdentityServer()
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

            app.UseIdentityServer();

            app.Run();
        }
    }
}