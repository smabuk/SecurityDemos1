using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SecurityDemo1.Data;
using SecurityDemo1.Models;
using SecurityDemo1.Services;
using Ardalis.ListStartupServices;

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
                    options.ClientId = Configuration["WEBSITE_AUTH_MSA_CLIENT_ID"];
                    options.ClientSecret = Configuration["WEBSITE_AUTH_MSA_CLIENT_SECRET"];
                    options.SaveTokens = true;
                    options.Validate();
                })
				.AddTwitter(options =>
				{
					options.ClaimActions.MapJsonKey(System.Security.Claims.ClaimTypes.DateOfBirth, "dateofbirth");
					options.ClaimActions.MapJsonKey(System.Security.Claims.ClaimTypes.Country, "country");
					options.ConsumerKey = Configuration["WEBSITE_AUTH_TWITTER_CONSUMER_KEY"];
					options.ConsumerSecret = Configuration["WEBSITE_AUTH_TWITTER_CONSUMER_SECRET"];
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
                    policy.RequireUserName(Configuration["Rules:KingAddress"]);
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
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.Configure<ServiceConfig>(config =>
            {
                config.Services = new List<ServiceDescriptor>(services);

                // optional - default path to view services is /listallservices - recommended to choose your own path
                config.Path = "/listallservices";
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

            app.UseShowAllServicesMiddleware();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
				app.UseHsts(options => options.MaxAge(days: 365).IncludeSubdomains());
            }

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
            });

            // From NuGet package NWebSec. docs at https://docs.nwebsec.com/en/latest
            // Implementing recommendations found at https://securityheaders.io
            // Article https://damienbod.com/2018/02/08/adding-http-headers-to-improve-security-in-an-asp-net-mvc-core-application/
            // Registered before static files to always set header
            app.UseXXssProtection(options => options.EnabledWithBlockMode());
            app.UseXContentTypeOptions();
            app.UseReferrerPolicy(options => options.SameOrigin());
            app.UseCsp(opts => opts
                .BlockAllMixedContent()
                //.StyleSources(s => s.Self())            // remove to allow CDNs to function properly
                //.StyleSources(s => s.UnsafeInline())    // really messes up general Admin pages
                .FontSources(s => s.Self())
                //.FormActions(s => s.Self())             // external logins don't work with this FormActions blocked
                .FrameAncestors(s => s.Self())
				.ImageSources(s => s.Self())			  // remove to allow Bootstrap SVG (menu etc.) to load
                //.ScriptSources(s => s.Self())           // remove to allow CDNs to function properly
            );

			app.UseHttpsRedirection();
			app.UseStaticFiles();

            // Registered after static files, to set headers for dynamic content.
            app.UseXfo(xfo => xfo.Deny());
            app.UseRedirectValidation(options => 
            {
                options.AllowSameHostRedirectsToHttps();
                options.AllowedDestinations(new[] 
                {
					"https://login.microsoftonline.com/",
                    "https://microsoftonline.com/",
                    "https://microsoft.com/",
					"https://api.twitter.com/"
				});
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
