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
                  .AddTwitter(twitterOptions =>
                  {
                      twitterOptions.ConsumerKey = Configuration["Authentication:Twitter:ConsumerKey"];
                      twitterOptions.ConsumerSecret = Configuration["Authentication:Twitter:ConsumerSecret"];
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

            services.AddHttpClient("auth", client =>
            {
                client.BaseAddress = new Uri(Configuration["Services:Auth"]);
            });

            services.AddGrpcClient<OrderStatusClient>(c =>
            {
                c.Address = new Uri(Configuration["Services:Orders"]);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
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
