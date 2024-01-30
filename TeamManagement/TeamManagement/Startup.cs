using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using TeamMan.Interfaces;
using TeamMan.Models;
using TeamMan.Repositories;
using TeamMan.Services;
using Microsoft.Extensions.DependencyInjection.Abstractions;
using Lib.Net.Http.WebPush;
using static TeamMan.Models.AngularPushNotification;
using TeamMan.ConfigModels;

namespace TeamMan
{
    public class Startup
    {
        public class PushNotificationsOptions
        {
            public string PublicKey { get; set; }
            public string PrivateKey { get; set; }
        }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<PushNotificationsOptions>(Configuration.GetSection("PushNotifications"));
            //System jobs Injection
            services.AddHostedService<ApplicationJobs>();
            
            //Database Injection
            services.AddDbContext<ApplicationContext>(options =>
                    options.UseSqlServer(Configuration.GetConnectionString("TeamMan")));//Connection String
            services.AddControllersWithViews()
                .AddNewtonsoftJson(options =>
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
            );

            //Service Injection
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped(typeof(IPushSubscriptionsService), typeof(PushSubscriptionsService));
            services.AddScoped(typeof(ITaskService), typeof(TaskService));
            services.AddScoped(typeof(INotificationService), typeof(NotificationService));
            services.AddScoped(typeof(INotificationProducercs), typeof(NotificationProducer));
            services.AddScoped(typeof(IUserConnectionManager), typeof(UserConnectionManagerService));
            services.AddScoped(typeof(UserInterface<>), typeof(UserManagement<>));
            
            var secretKey = Configuration["AppSettings:Secret"];
            var urls = Configuration.GetSection("Origins:urls").Get<List<Origins>>();

            //Identity Injection
            services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<ApplicationContext>().AddDefaultTokenProviders(); ;//Identity injection 
            
            //Authentication Sercice Injection
            services.AddAuthentication(opt => { //JWT Auth
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
               {
                   options.SaveToken = true;
                   options.TokenValidationParameters = new TokenValidationParameters
                   {
                       ValidateIssuer = true,
                       ValidateAudience = true,
                       ValidateLifetime = true,
                       ValidateIssuerSigningKey = true,
                       ValidIssuer = "https://localhost:44342",
                       ValidAudience = "https://localhost:44342",
                       IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
                   };
               });
            //DBContextConfig.Initialize(services, configuration);
            services.AddControllers(options => options.EnableEndpointRouting = false);
            services.AddPushServiceClient(options =>
            {

                options.PublicKey = "";
                options.PrivateKey = "";
            });
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            services.AddControllers();
            services.Configure<FormOptions>(o => { //File Upload
                o.ValueLengthLimit = int.MaxValue;
                o.MultipartBodyLengthLimit = int.MaxValue;
                o.MemoryBufferThreshold = int.MaxValue;
            });
            services.AddMvc();
            services.AddCors(options =>
            {
            
                options.AddPolicy("EnableCORS",
                    builder => builder
                        .WithOrigins(urls[0].url.ToString())
                        .AllowAnyMethod().AllowCredentials()
                        .AllowAnyHeader());
            });
            //services.AddSpaStaticFiles(configuration =>
            //{
            //    configuration.RootPath = "ClientApp/dist";
            //});

            services.AddSignalR();


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseCors("EnableCORS");

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSignalR(routes =>
            { 
            routes.MapHub<NotificationHub>("/NotificationAction");
                routes.MapHub<NotificationUserHub>("/NotificationUserHub");
                }) ;

            //app.UseMvc();
            app.UseRouting();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            });
            //app.UseCors("CorsPolicy");
            app.UseStaticFiles();
            app.UseStaticFiles(new StaticFileOptions() //Static Files
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), @"Assets")),
                RequestPath = new PathString("/Assets"),

            });
            app.UseHttpsRedirection();
            //app.UseSpa(spa =>
            //{
            //    spa.Options.SourcePath = "ClientApp";
            //    if (env.IsDevelopment())
            //    {
            //        spa.UseProxyToSpaDevelopmentServer("http://localhost:4200");
            //    }
            //});
        }
    }
}
