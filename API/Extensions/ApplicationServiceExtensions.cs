using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.Helpers;
using API.Interfaces;
using API.Services;
using API.SignalR;
using Microsoft.EntityFrameworkCore;

namespace API.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddSingleton<PresenceTracker>();
            services.Configure<CloudinarySettings>(config.GetSection("CloudinarySettings"));
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IPhotoService, PhotoService>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<LogUserActivity>();
            services.AddAutoMapper(typeof(AutoMapperProfiles).Assembly);
            services.AddDbContext<DataContext>(opt =>
            {
                AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
                var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

                string connStr;

                //Depending on if in devlopment or production, use either heroku-provided
                //connection string, development connection string from env var.
                if (env == "Development")
                {
                    //use connection string from file.
                    connStr = config.GetConnectionString("DefaultConnection");
                }
                else
                {
                    //Use connection string provided at runtime by Heroku
                    var connURL = Environment.GetEnvironmentVariable("DATABASE_URL");

                    //Parse connection URL to connection string for npgsql
                    connURL = connURL.Replace("postgres://", string.Empty);
                    var pgUserPass = connURL.Split("@")[0];
                    var pgHostPortDb = connURL.Split("@")[1];
                    var pgHostPort = pgHostPortDb.Split("/")[0];
                    var pgDb = pgHostPortDb.Split("/")[1];
                    var pgUser = pgUserPass.Split(":")[0];
                    var pgPass = pgUserPass.Split(":")[1];
                    var pgHost = pgHostPort.Split(":")[0];
                    var pgPort = pgHostPort.Split(":")[1];

                    connStr = $"User ID={pgUser};Password={pgPass};Host={pgHost};Port={pgPort};Database={pgDb}";
                }

                //Whether the connection came from the local development configuration file or from the environment variable from Heroku, use it to set up your DbContext.
                opt.UseNpgsql(connStr);
            });
            return services;
        }
    }
}