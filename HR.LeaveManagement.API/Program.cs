using HR.LeaveManagement.Api.Middleware;
using HR.LeaveManagement.Application;
using HR.LeaveManagement.Identity;
using HR.LeaveManagement.Infrastructure;
using HR.LeaveManagement.Persistence;
using Serilog;

namespace HR.LeaveManagement.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Host.UseSerilog((context, loggerConfig) => loggerConfig
                .WriteTo.Console()
                .ReadFrom.Configuration(context.Configuration));

            builder.Services.AddApplicationServices();
            builder.Services.AddPersistenceServices(builder.Configuration);
            builder.Services.AddInfrastructureServices(builder.Configuration);
            builder.Services.AddIdentityServices(builder.Configuration);

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddHttpContextAccessor();

            builder.Services.AddCors(opt =>
            {
                opt.AddPolicy("CorsPolicy", policy =>
                {
                    policy.AllowAnyHeader()
                    .AllowAnyOrigin()
                    .AllowAnyOrigin()
                    .AllowAnyMethod();
                });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            app.UseExceptionMiddleware();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseSerilogRequestLogging();

            app.UseCors("CorsPolicy");
            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}