using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SecurityDemo1.Data;
using SecurityDemo1.Models;
using SecurityDemo1.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication;

namespace SecurityDemo1
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));

            services.Configure<EmailConfiguration>(Configuration.GetSection("Email"));
            services.Configure<SiteConfiguration>(Configuration.GetSection("Site"));

            services.AddAuthentication()
                .AddMicrosoftAccount(options =>
                {
                    options.ClaimActions.MapJsonKey(System.Security.Claims.ClaimTypes.DateOfBirth, "dateofbirth");
                    options.ClaimActions.MapJsonKey(System.Security.Claims.ClaimTypes.Country, "country");
                    options.ClientId = Configuration["WEBSITE_AUTH_MSA_CLIENT_ID_FIX"];
                    options.ClientSecret = Configuration["WEBSITE_AUTH_MSA_CLIENT_SECRET_FIX"];
                    options.SaveTokens = true;
                    options.Validate();
                });

            services.AddIdentity<ApplicationUser, IdentityRole>(config =>
                {
                    config.SignIn.RequireConfirmedEmail = false;
                    config.Password.RequireNonAlphanumeric = false;
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddAuthorization(config =>
            {

                config.AddPolicy(ApplicationUser.AppPolicies.King, policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireUserName("smabrookes@hotmail.com");
                    policy.Build();
                });

                config.AddPolicy(ApplicationUser.AppPolicies.Admin, policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireRole(ApplicationUser.AppRoles.Admin);
                    policy.Build();
                });

                config.AddPolicy(ApplicationUser.AppPolicies.Treasurer, policy =>
                {
                    policy.RequireClaim(ApplicationUser.AppClaims.CommitteeMember, "treasurer");
                    policy.Build();
                });

                config.AddPolicy(ApplicationUser.AppPolicies.Chairman, policy =>
                {
                    policy.RequireClaim(ApplicationUser.AppClaims.CommitteeMember, "chairman");
                    policy.Build();
                });

                config.AddPolicy(ApplicationUser.AppPolicies.CommitteeMember, policy =>
                {
                    policy.RequireClaim(ApplicationUser.AppClaims.CommitteeMember, new[] { "treasurer", "secretary", "chairman" });
                    policy.Build();
                });
            });

            // Add application services.
            services.AddTransient<IEmailSender, EmailSender>();

            services.AddMvc(options =>
            {
                options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
                options.Filters.Add(new RequireHttpsAttribute());
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app,
            IHostingEnvironment env, 
            ILoggerFactory loggerFactory,
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
            });

            // From NuGet package NWebSec
            // Registered before static files to always set header
            // Closes Trust-On-First-Use loophole
            app.UseHsts(options => options.MaxAge(days: 365).IncludeSubdomains());
            app.UseXXssProtection(options => options.EnabledWithBlockMode());
            app.UseXContentTypeOptions();
            app.UseReferrerPolicy(options => options.SameOrigin());

            app.UseStaticFiles();

            // Registered after static files, to set headers for dynamic content.
            app.UseXfo(xfo => xfo.Deny());
            app.UseRedirectValidation(options => 
            {
                options.AllowSameHostRedirectsToHttps();
                options.AllowedDestinations(new[] 
                    {   "https://login.microsoftonline.com/",
                        "https://microsoftonline.com/",
                        "https://microsoft.com/" });
            }); //Register this earlier if there's middleware that might redirect.

            app.UseAuthentication();
            
            SampleData.InitializeData(app.ApplicationServices, loggerFactory);

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
