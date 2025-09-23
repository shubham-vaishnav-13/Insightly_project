using Insightly_project.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Insightly_project
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
            options.UseSqlServer(Configuration.GetConnectionString("BusinessCon")));

            //services.AddIdentity<IdentityUser, IdentityRole>()
            //    .AddEntityFrameworkStores<ApplicationDbContext>()
            //    .AddDefaultTokenProviders()
            //    .AddDefaultUI();   
            //;
            //services.AddControllersWithViews();
            //services.AddRazorPages(); // Add this

            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
            })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders()
                .AddDefaultUI(); 

            services.AddControllersWithViews();
            services.AddRazorPages(); 


            // Force claims to refresh on each request and validate security stamp immediately
            services.ConfigureApplicationCookie(options =>
            {
                options.Events.OnValidatePrincipal = async context =>
                {
                    var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
                    var signInManager = context.HttpContext.RequestServices.GetRequiredService<SignInManager<ApplicationUser>>();
                    var user = await userManager.GetUserAsync(context.Principal);
                    if (user != null)
                    {
                        var newPrincipal = await signInManager.CreateUserPrincipalAsync(user);
                        context.ReplacePrincipal(newPrincipal);
                        context.ShouldRenew = true;
                    }
                };
            });

            services.Configure<SecurityStampValidatorOptions>(o =>
            {
                // Zero makes every request check the security stamp so role changes take effect immediately
                o.ValidationInterval = TimeSpan.Zero;
            });


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages(); // Identity UI

            });
        }
    }
}
