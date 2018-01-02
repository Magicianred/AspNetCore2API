using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FormPdfEasy.Entities;
using FormPdfEasy.Interfaces;
using FormPdfEasy.Models;
using FormPdfEasy.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using NLog.Extensions.Logging;

namespace FormPdfEasy
{
    public class Startup
    {
        private IConfiguration _configuration { get; }

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            var connectionString = _configuration["Database:ConnectionString"];

            services.AddDbContext<FormPdfEasyContext>(o => o.UseSqlServer(connectionString));
          
            services.AddScoped<IUserRepository, UserRepository>();

            services.AddIdentity<CustomUser, IdentityRole>()
                .AddEntityFrameworkStores<FormPdfEasyContext>()
                .AddDefaultTokenProviders();

            services.AddAuthorization(options => options.DefaultPolicy = new AuthorizationPolicyBuilder(
                JwtBearerDefaults.AuthenticationScheme)
                .RequireAuthenticatedUser()
                .Build());

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["IssuerSigningKey"])),
                ValidateIssuer = true,
                ValidIssuer = _configuration["Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = tokenValidationParameters;
                options.RequireHttpsMetadata = Convert.ToBoolean(_configuration["RequireHttpsMetadata"]);
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("SuperUsers", policy => policy.RequireClaim("SuperUser", "True"));
                //TODO: Optional, add policy against the role
            });
        }
        
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
                        IApplicationBuilder app, 
                        IHostingEnvironment env, 
                        ILoggerFactory loggerFactory, 
                        FormPdfEasyContext formPdfEasyContext)
        {
            loggerFactory.AddNLog();    
            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //TODO: implement database seed

            AutoMapper.Mapper.Initialize(config => {
                config.CreateMap<CustomUser, CustomUserDto>();
            });

            app.UseAuthentication();

            app.UseMvc();
           
          }
    }
}
