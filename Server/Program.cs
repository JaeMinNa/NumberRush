using GameServer.Module.ServerManager.DataBase;
using GameServer.Module.ServerManager;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Test_Server
{
    public class Program
    {
        public async static Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Cors
            builder.Services.AddCors(p => p.AddPolicy("corsapp", builder =>
            {
                builder.WithOrigins("*").AllowAnyMethod().AllowAnyHeader();
            }));

            // Blazor
            builder.Services.AddRazorPages();
            builder.Services.AddServerSideBlazor();
            builder.Services.AddControllers();
            builder.Services.AddHttpClient();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);
            builder.Configuration.AddEnvironmentVariables();

            var app = builder.Build();
            app.UseExceptionHandler("/Error");
            app.UseStaticFiles();
            app.UseCors("corsapp");
            app.UseRouting();
            app.MapControllers();
            app.MapBlazorHub();
            app.MapFallbackToPage("/_Host");

            // DB
            Console.WriteLine("MongoDB Init");

            var connectString = builder.Configuration["MongoDB"];

            if (string.IsNullOrWhiteSpace(connectString))
            {
                throw new InvalidOperationException(
                    $"MongoDB 설정을 찾지 못했습니다. ContentRootPath: {builder.Environment.ContentRootPath}");
            }

            Console.WriteLine($"ConnectString = {connectString}");

            await ServerDataBase.Init(connectString);

            Console.WriteLine("Start Run");

            app.Run();
        }
    }
}