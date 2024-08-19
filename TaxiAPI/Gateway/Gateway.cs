using System;
using System.Collections.Generic;
using System.Fabric;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Communication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using UserStateful;
using Microsoft.EntityFrameworkCore;
using Common.Models;
using Microsoft.AspNetCore.SignalR;
using Gateway.Hubs;
using Communcation;
using Gateway.Services;

namespace Gateway
{
    internal sealed class Gateway : StatelessService
    {
        public Gateway(StatelessServiceContext context)
            : base(context) { }

        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[]
            {
        new ServiceInstanceListener(serviceContext =>
            new KestrelCommunicationListener(serviceContext, "ServiceEndpoint", (url, listener) =>
            {
                ServiceEventSource.Current.ServiceMessage(serviceContext, $"Starting Kestrel on {url}");

                var builder = WebApplication.CreateBuilder();

                builder.Services.AddSignalR();
                builder.Services.AddSingleton<StatelessServiceContext>(serviceContext);
                builder.Services.AddSingleton<IRideCommunication, RideService>();
                builder.Services.AddSingleton<IRatingCommunication, RatingService>();


                builder.WebHost
                       .UseKestrel()
                       .UseContentRoot(Directory.GetCurrentDirectory())
                       .UseServiceFabricIntegration(listener, ServiceFabricIntegrationOptions.None)
                       .UseUrls(url);

                builder.Services.AddCors(options =>
                {
                    options.AddPolicy("AllowSpecificOrigins",
                        builder =>
                        {
                            builder.WithOrigins("http://localhost:3000")
                                   .AllowAnyMethod()
                                   .AllowAnyHeader()
                                   .AllowCredentials();
                        });
                });

                builder.Services.AddControllers();
                builder.Services.AddEndpointsApiExplorer();

                builder.Services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Gateway API", Version = "v1" });

                    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                    {
                        In = ParameterLocation.Header,
                        Description = "Please enter into field the word 'Bearer' followed by a space and the JWT value",
                        Name = "Authorization",
                        Type = SecuritySchemeType.ApiKey
                    });

                    c.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            },
                            Array.Empty<string>()
                        }
                    });
                });

                builder.Services.AddSingleton<IUserCommunication, UserService>();

                builder.Services.AddDbContext<UserDbContext>(options =>
                    options.UseSqlServer(builder.Configuration.GetConnectionString("UserDatabase")));

                builder.Services.AddDbContext<RideDbContext>(options =>
                    options.UseSqlServer(builder.Configuration.GetConnectionString("RideDatabase")));

                var key = Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"]);
                builder.Services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(key)
                    };
                });

                builder.Services.AddAuthorization();

                builder.Services.AddHttpContextAccessor();

                var app = builder.Build();

                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI(c =>
                    {
                        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Gateway API V1");
                    });
                }

                app.UseRouting();
                app.UseCors("AllowSpecificOrigins");
                app.UseAuthentication();
                app.UseAuthorization();

                app.MapControllers();
                app.MapHub<RideHub>("/rideHub");

                app.UseStaticFiles();

                app.Run();

                return app;
            }))
            };
        }

    }
}
