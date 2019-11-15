using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using static BlazingPizza.Orders.OrderStatus;

namespace BlazingPizza.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

            services.AddResponseCompression(options =>
            {
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                    new[] { MediaTypeNames.Application.Octet });
            });

            services.AddScoped<OrderState>();

            services.AddMvc()
                    .AddNewtonsoftJson();

            services.AddDistributedMemoryCache();

            // Add auth services
            services
                  .AddAuthentication(options =>
                  {
                      options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                  })
                  .AddCookie()
                  .AddMicrosoftAccount(options =>
                  {
                      options.ClientId = Configuration["Authentication:Twitter:ConsumerKey"];
                      options.ClientSecret = Configuration["Authentication:Twitter:ConsumerSecret"];
                  });

            services.AddServerSideBlazor();
            services.AddRazorPages();

            services.AddHttpClient("menu", client =>
            {
                client.BaseAddress = new Uri(Configuration["Services:Menu"]);
            });

            services.AddHttpClient("orders", client =>
            {
                client.BaseAddress = new Uri(Configuration["Services:Orders"]);
                client.DefaultRequestVersion = HttpVersion.Version20;
            });

            services.AddGrpcClient<OrderStatusClient>(c =>
            {
                c.Address = new Uri(Configuration["Services:Orders"]);
            });

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                //TODO: This should be configuration from the cluster telling the app what
                //IP ranges are possible for proxies in the cluster.
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });

            services.AddApplicationInsightsTelemetry(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseForwardedHeaders();
            app.UseResponseCompression();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
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
                endpoints.MapControllers();
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
