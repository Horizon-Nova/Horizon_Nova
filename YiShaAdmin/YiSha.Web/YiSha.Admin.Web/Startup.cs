using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using Newtonsoft.Json.Serialization;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using YiSha.Util;
using YiSha.Util.Model;
using YiSha.Admin.Web.Controllers;
using Microsoft.Extensions.Logging;

namespace YiSha.Admin.Web
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IWebHostEnvironment WebHostEnvironment { get; set; }

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            WebHostEnvironment = env;
            GlobalContext.LogWhenStart(env);
            GlobalContext.HostingEnvironment = env;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            if (WebHostEnvironment.IsDevelopment())
            {
                services.AddRazorPages().AddRazorRuntimeCompilation();
            }
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddSingleton(HtmlEncoder.Create(UnicodeRanges.All));
            services.AddControllersWithViews(options =>
            {
                options.Filters.Add<GlobalExceptionFilter>();
                options.ModelMetadataDetailsProviders.Add(new ModelBindingMetadataProvider());
            }).AddNewtonsoftJson(options =>
            {
                // 返回資料首字母不小寫，CamelCasePropertyNamesContractResolver是小寫
                options.SerializerSettings.ContractResolver = new DefaultContractResolver();
            });

            services.AddMemoryCache();
            services.AddSession();
            services.AddHttpContextAccessor();

            services.AddOptions();

            services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(GlobalContext.HostingEnvironment.ContentRootPath + Path.DirectorySeparatorChar + "DataProtection"));

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);  // 注册Encoding

            GlobalContext.SystemConfig = Configuration.GetSection("SystemConfig").Get<SystemConfig>();
            GlobalContext.Services = services;
            GlobalContext.Configuration = Configuration;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (!string.IsNullOrEmpty(GlobalContext.SystemConfig.VirtualDirectory))
            {
                app.UsePathBase(new PathString(GlobalContext.SystemConfig.VirtualDirectory)); // 讓 Pathbase 中間件成為第一個處理請求的中間件， 才能正確的模擬虛擬路徑
            }
            if (WebHostEnvironment.IsDevelopment())
            {
                GlobalContext.SystemConfig.Debug = true;
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            string resource = Path.Combine(env.ContentRootPath, "Resource");
            FileHelper.CreateDirectory(resource);

            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = GlobalContext.SetCacheControl
            });
            app.UseStaticFiles(new StaticFileOptions
            {
                RequestPath = "/Resource",
                FileProvider = new PhysicalFileProvider(resource),
                OnPrepareResponse = GlobalContext.SetCacheControl
            });
            app.UseSession();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute("areas", "{area:exists}/{controller=Home}/{action=Index}/{id?}");
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });
            GlobalContext.ServiceProvider = app.ApplicationServices;
        }
    }
}
