using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Net.Http;

namespace WebApplication3
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
            services.AddRazorPages();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime hostApplicationLifetime)
        {
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

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });

            hostApplicationLifetime.ApplicationStopping.Register(() =>
            {
                var instanceId = Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID");
                Console.WriteLine($"Stopping Application on instance {instanceId} at {DateTime.UtcNow}");

                try
                {
                    using var writter = File.AppendText($"/home/site/wwwroot/{instanceId}.txt");
                    writter.WriteLine($"{DateTime.UtcNow} => ApplicationStopping");
                    writter.Flush();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

                try
                {
                    var client = new HttpClient();
                    client.Send(new HttpRequestMessage
                    {
                        Method = HttpMethod.Get,
                        RequestUri = new Uri("https://cjaliaga-psf.azurewebsites.net/api/HttpTrigger1?name=DOTNETSTOPPING")
                    });
                }
                catch (Exception ex)
                {
                    using var writter = File.AppendText($"/home/site/wwwroot/{instanceId}-errors.txt");
                    writter.WriteLine(ex);
                    writter.Flush();
                }

            });

            hostApplicationLifetime.ApplicationStopped.Register(() =>
            {
                var instanceId = Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID");
                Console.WriteLine($"Application Stopped on instance {instanceId} at {DateTime.UtcNow}");

                try
                {
                    using var writter = File.AppendText($"/home/site/wwwroot/{instanceId}.txt");
                    writter.WriteLine($"{DateTime.UtcNow} => ApplicationStopped");
                    writter.Flush();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

                try
                {
                    var client = new HttpClient();
                    client.Send(new HttpRequestMessage
                    {
                        Method = HttpMethod.Get,
                        RequestUri = new Uri("https://cjaliaga-psf.azurewebsites.net/api/HttpTrigger1?name=DOTNETSTOPPED")
                    });
                }
                catch (Exception ex)
                {
                    using var writter = File.AppendText($"/home/site/wwwroot/{instanceId}-errors.txt");
                    writter.WriteLine(ex);
                    writter.Flush();
                }
            });
        }
    }
}
