using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

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

                var waitTime = Configuration.GetValue<int>("WaitTime");

                if(waitTime > 0)
                {
                    LogMessage(writer, $"Waiting {waitTime}ms to simulate a long operation to see if the platform kills it before ending.");
                    Task.Delay(waitTime).Wait();
                    LogMessage(writer, $"Waited {waitTime}ms");
                }

                if (Configuration.GetValue<bool>("MakeExternalCall"))
                {
                    try
                    {
                        var cancelTime = Configuration.GetValue<int>("ExternalCallCancellationTime");
                        var url = "https://cjaliaga-psf.azurewebsites.net/api/HttpTrigger1?name=DOTNETSTOPPING";
                        LogMessage(writer, $"Doing HTTP Call to {url} that will cancelled after {cancelTime}ms");
                        CancellationTokenSource source = new(cancelTime);

                        var client = new HttpClient();
                        var response = client.Send(new HttpRequestMessage
                        {
                            Method = HttpMethod.Get,
                            RequestUri = new Uri(url)
                        }, source.Token);

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
                StreamWriter writer = null;

                try
                {
                    writer = File.AppendText($"/home/site/wwwroot/{instanceId}.txt");
                }
                catch (Exception ex)
                {
                    LogMessage(writer, ex.ToString());
                }

                LogMessage(writer, $"Stopped Application on instance {instanceId}");

                var waitTime = Configuration.GetValue<int>("WaitTime");

                if (waitTime > 0)
                {
                    LogMessage(writer, $"Waiting {waitTime}ms to simulate a long operation to see if the platform kills it before ending.");
                    Task.Delay(waitTime).Wait();
                    LogMessage(writer, $"Waited {waitTime}ms");
                }

                if (Configuration.GetValue<bool>("MakeExternalCall"))
                {
                    try
                    {
                        var cancelTime = Configuration.GetValue<int>("ExternalCallCancellationTime");
                        var url = "https://cjaliaga-psf.azurewebsites.net/api/HttpTrigger1?name=DOTNETSTOPPED";
                        LogMessage(writer, $"Doing HTTP Call to {url} that will cancelled after {cancelTime}ms");
                        CancellationTokenSource source = new(cancelTime);

                        var client = new HttpClient();
                        var response = client.Send(new HttpRequestMessage
                        {
                            Method = HttpMethod.Get,
                            RequestUri = new Uri(url)
                        }, source.Token);

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
            message = $"{DateTime.UtcNow:o} | {OperationId} => {message}";
            Console.WriteLine(message);
            writer?.WriteLine(message);
            writer?.Flush();
        }
    }
}
