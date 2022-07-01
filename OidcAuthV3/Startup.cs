using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OidcAuthV3.DataAccess;
using OidcAuthV3.Utilities;
using OidcAuthV3.Models;


namespace OidcAuthV3
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
            // Cookie Policy
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => false;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
            // Cookie Policy

            // Cookie authentication start
            services.AddAuthentication(options =>
            {
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
                            .AddCookie(options =>
                            {
                                options.AccessDeniedPath = "/Home/ErrorForbidden";
                                options.LoginPath = "/Home/ErrorNotLoggedIn";
                                options.ExpireTimeSpan = new TimeSpan(4, 0, 0);
                                options.Cookie.Name = "EngineeringPermits";
                            });
            // Cookie authentication end

            // Set up database connections
            services.AddDbContext<OidcAuthDbContext>(options => options.UseSqlServer(Tools.DecryptString(Configuration["AppConfig:ConnStringOidcAuthDb"])));


            // Other services to be started
            services.AddScoped<IDataFunctions, DataFunctions>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddHttpContextAccessor();
            // services.AddScoped<IUserDataService, UserDataService>();
            services.AddScoped<IStaffDataService, StaffDataService>();
            services.AddScoped<IEmailService, EmailService>();

            services.AddHttpClient();

            Tools.EncryptionKey = Tools.DecryptString(Configuration["AppConfig:EncryptionKey"]);

            // session setup start
            ////Adds a default in-memory implementation of IDistributedCache
            //services.AddDistributedMemoryCache();
            //services.AddSession(options =>
            //{
            //    // Set a short timeout for easy testing.
            //    options.IdleTimeout = TimeSpan.FromMinutes(240);
            //    options.Cookie.HttpOnly = true;
            //    // Make the session cookie essential
            //    options.Cookie.IsEssential = true;
            //});

            // Adds a default in-database implementation of IDistributedCache
            services.AddDistributedSqlServerCache(options =>
            {
                options.ConnectionString =
                    Tools.DecryptString(Configuration["AppConfig:ConnStringOidcAuthDb"]);
                options.SchemaName = "dbo";
                options.TableName = "tSqlSessions";
            });
            services.AddSession();
            // session setup end


            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.EnvironmentName.ToUpper() != "PRODUCTION" && env.EnvironmentName.ToUpper() != "STAGING")
            //if (env.IsDevelopment() || env.IsEnvironment("Home") || env.IsEnvironment("Work"))
            {
                //app.UseExceptionHandler("/Error/ErrorAction");
                //app.UseExceptionHandler();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error/ErrorAction");
                //app.UseDeveloperExceptionPage();
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            // https://dotnetcoretutorials.com/2017/01/08/set-x-frame-options-asp-net-core/
            app.Use(async (context, next) =>
            {
                context.Response.Headers.Add("X-Frame-Options", "DENY");
                await next();
            });

            app.UseCookiePolicy(
                new CookiePolicyOptions
                {
                    Secure = CookieSecurePolicy.Always
                });

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseCors(x => x
               .AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader());

            app.UseSession();

            app.UseRouting();

            // the following two lines must come after app.UseRouting()
            app.UseAuthentication();
            app.UseAuthorization();  // this is needed if you are using role authorization

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
