using ASC.Business;
using ASC.Business.Interfaces;
using ASC.DataAccess;
using ASC.DataAccess.Interfaces;
using ASC.WebMVC.Data;
using ASC.WebMVC.Areas.Identity.Services;
using ASC.WebMVC.Configuration;
using ASC.WebMVC.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Serialization;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using Microsoft.Extensions.Caching.Distributed;
using ASC.WebMVC.Logger;
using ASC.WebMVC.Filters;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Mvc.Razor;

namespace ASC.WebMVC
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
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration["ConnectionStrings:DefaultConnection"]));

            services.AddIdentity<ApplicationUser, ApplicationRole>((options) =>
            {
                options.User.RequireUniqueEmail = true;                
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();
         
            services.AddOptions();
            services.Configure<ApplicationSettings>(Configuration.GetSection("AppSettings"));
            services.Configure<AuthMessageSenderOptions>(Configuration.GetSection("SendGrid"));

            
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddAutoMapper();

            // Add support to embedded views from ASC.Utilities project.
            var assembly = typeof(ASC.Utilities.Navigation.LeftNavigationViewComponent).GetTypeInfo().Assembly;
            var embeddedFileProvider = new EmbeddedFileProvider(assembly, "ASC.Utilities");
            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.FileProviders.Add(embeddedFileProvider);
            });


            services.AddDistributedRedisCache(options =>
            {
                options.Configuration = Configuration["CacheSettings:CacheConnectionString"];
                options.InstanceName =  Configuration["CacheSettings:CacheInstance"];
            });

            services.AddSession();
            services.AddAuthentication().AddGoogle((options) =>
            {
                options.ClientId = Configuration["Google:Identity:ClientId"];
                options.ClientSecret = Configuration["Google:Identity:ClientSecret"];
                options.Scope.Add("https://www.googleapis.com/auth/plus.login");
                options.Scope.Add("https://www.googleapis.com/auth/plus.profile.emails.read");
                options.Scope.Add("https://www.googleapis.com/auth/plus.me");
                options.ClaimActions.MapJsonKey(ClaimTypes.Gender, "gender");
                options.ClaimActions.MapJsonKey(ClaimTypes.Country, "country");
            });

            services.AddMvc((o => { o.Filters.Add(typeof(CustomExceptionFilter)); }))
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)             
                .AddJsonOptions(options => options.SerializerSettings.ContractResolver=new DefaultContractResolver()); 

            //services.AddTransient<IEmailSender, EmailSender>();     
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();
            services.AddSingleton<IIdentitySeed, IdentitySeed>();
            services.AddScoped<IServiceRequestOperations, ServiceRequestOperations>();
            services.AddScoped<IMasterDataOperations, MasterDataOperations>();
            services.AddScoped<ILogDataOperations, LogDataOperations>();
            services.AddScoped<IUnitOfWork>(p => new UnitOfWork("UseDevelopmentStorage=true;"));
            // services.AddSingleton<IMasterDataCacheOperations, MasterDataCacheOperations>();
            services.AddSingleton<INavigationCacheOperations, NavigationCacheOperations>();
            services.AddCors();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public async void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory,
            IIdentitySeed storageSeed,
            ILogDataOperations logDataOperations,IUnitOfWork unitOfWork,
             INavigationCacheOperations navigationCacheOperations
            )
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            // Configure Azure Logger to log all events except the ones that are generated by default by ASP.NET Core.
            loggerFactory.AddAzureTableStorageLog(logDataOperations,(categoryName, logLevel) => !categoryName.Contains("Microsoft") &&
                                         logLevel >= LogLevel.Information);

            if (env.IsDevelopment())
            {
                //app.UseDeveloperExceptionPage();
                //app.UseDatabaseErrorPage();
                //app.UseBrowserLink();
            }
            else
            {
                //app.UseExceptionHandler("/Home/Error");
            }

            app.UseStatusCodePagesWithRedirects("/Home/Error/{0}");
            app.UseSession();
            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseCors(builder =>
            {
                builder.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin().AllowCredentials();
            });



            app.UseMvc(routes =>
            {
                routes.MapRoute(name: "areaRoute",
                    template: "{area:exists}/{controller=Home}/{action=Index}");

                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            using (var scope = app.ApplicationServices.CreateScope())
            {
                //Resolve ASP .NET Core Identity with DI help
                var userManager = (UserManager<ApplicationUser>)scope.ServiceProvider.GetService(typeof(UserManager<ApplicationUser>));
                var roleManager = (RoleManager<ApplicationRole>)scope.ServiceProvider.GetService(typeof(RoleManager<ApplicationRole>));
                var options = (IOptions<ApplicationSettings>)scope.ServiceProvider.GetService(typeof(IOptions<ApplicationSettings>));

                await storageSeed.Seed(userManager, roleManager, options);
                // do you things here
            }

            var models = Assembly.Load(new AssemblyName("ASC.Models")).GetTypes().Where(type => type.Namespace == "ASC.Models.Models");
            foreach (var model in models)
            {
                var repositoryInstance = Activator.CreateInstance(typeof(Repository<>).MakeGenericType(model), unitOfWork);
                MethodInfo method = typeof(Repository<>).MakeGenericType(model).GetMethod("CreateTableAsync");
                method.Invoke(repositoryInstance, new object[0]);
            }


            //await masterDataCacheOperations.CreateMasterDataCacheAsync();
            await navigationCacheOperations.CreateNavigationCacheAsync();



        }

        // For demonstration purposes only.
        // This method searches up the directory tree until it
        // finds the KeyRing folder in the sample. Using this
        // approach allows the sample to run from a Debug
        // or Release location within the bin folder.
        private DirectoryInfo GetKeyRingDirInfo()
        {
            var startupAssembly = System.Reflection.Assembly.GetExecutingAssembly();
            var applicationBasePath = System.AppContext.BaseDirectory;
            var directoryInfo = new DirectoryInfo(applicationBasePath);
            do
            {
                directoryInfo = directoryInfo.Parent;

                var keyRingDirectoryInfo = new DirectoryInfo(Path.Combine(directoryInfo.FullName, "KeyRing"));
                if (keyRingDirectoryInfo.Exists)
                {
                    return keyRingDirectoryInfo;
                }
            }
            while (directoryInfo.Parent != null);

            throw new Exception($"KeyRing folder could not be located using the application root {applicationBasePath}.");
        }
    }


}
