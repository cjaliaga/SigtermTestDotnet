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
            OperationId = Guid.NewGuid();
        }

        public IConfiguration Configuration { get; }
        public Guid OperationId { get; }

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
                StreamWriter writer = null;

                try
                {
                    writer = File.AppendText($"/home/site/wwwroot/{instanceId}.txt");
                }
                catch(Exception ex)
                {
                    LogMessage(writer, ex.ToString());
                }

                LogMessage(writer, $"Stopping Application on instance {instanceId}");

                if (Configuration.GetValue<bool>("MakeExternalCall"))
                {
                    try
                    {
                        var url = "https://cjaliaga-psf.azurewebsites.net/api/HttpTrigger1?name=DOTNETSTOPPING";
                        LogMessage(writer, $"Doing HTTP Call to {url}");

                        var client = new HttpClient();
                        var response = client.Send(new HttpRequestMessage
                        {
                            Method = HttpMethod.Get,
                            RequestUri = new Uri(url)
                        });

                        LogMessage(writer,  $"HTTP Call Response: {response.StatusCode}");
                    }
                    catch (Exception ex)
                    {
                        LogMessage(writer, ex.ToString());
                    }
                } 
                else
                {
                    LogMessage(writer, "External Call Disabled");
                }

                LogMessage(writer, "Stopping operation ended.");
            });

            hostApplicationLifetime.ApplicationStopped.Register(() =>
            {
                var instanceId = Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID");
                var message = $"{DateTime.UtcNow} | {OperationId} => Stopped Application on instance {instanceId}";
                Console.WriteLine(message);
                StreamWriter writer = null;

                try
                {
                    writer = File.AppendText($"/home/site/wwwroot/{instanceId}.txt");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

                writer?.WriteLine(message);
                writer?.Flush();

                if (Configuration.GetValue<bool>("MakeExternalCall"))
                {
                    try
                    {
                        var url = "https://cjaliaga-psf.azurewebsites.net/api/HttpTrigger1?name=DOTNETSTOPPED";
                        LogMessage(writer, $"Doing HTTP Call to {url}");

                        var client = new HttpClient();
                        var response = client.Send(new HttpRequestMessage
                        {
                            Method = HttpMethod.Get,
                            RequestUri = new Uri(url)
                        });

                        LogMessage(writer, $"HTTP Call Response: {response.StatusCode}");
                    }
                    catch (Exception ex)
                    {
                        LogMessage(writer, ex.ToString());
                    }
                }
                else
                {
                    LogMessage(writer, "External Call Disabled");
                }

                LogMessage(writer, "Stopped operation ended.");
            });
        }

        private void LogMessage(StreamWriter writer, string message)
        {
            message = $"{DateTime.UtcNow} | {OperationId} => {message}";
            Console.WriteLine(message);
            writer?.WriteLine(message);
            writer?.Flush();
        }
    }
}
