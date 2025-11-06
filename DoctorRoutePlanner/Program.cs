using DoctorRoutePlanner.Interfaces;
using DoctorRoutePlanner.Models;
using DoctorRoutePlanner.Services;
using Microsoft.Extensions.Configuration;
using Serilog;
using System.Reflection;

namespace DoctorRoutePlanner
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorPages();

            //Dependency injection entries
            builder.Services.AddSingleton<IRoutePlanner, LocalRoutePlanner>();
            builder.Services.Configure<LoginCredentials>(builder.Configuration.GetSection("LoginCredentials"));
            builder.Services.AddScoped<ILoginService, LoginService>();

            // Add Serilog for logging purposes
            Environment.SetEnvironmentVariable("BaseDir", Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            builder.Host.UseSerilog((ctx, lc) => lc
                .ReadFrom.Configuration(ctx.Configuration));

            builder.Services.AddSession();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapRazorPages();

            app.UseSession();

            app.Run();
        }
    }
}
