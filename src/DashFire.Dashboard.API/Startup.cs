using System;
using System.Threading;
using System.Threading.Tasks;
using DashFire.Dashboard.Framework.Cache;
using DashFire.Dashboard.Framework.Options;
using DashFire.Dashboard.Framework.Services.Job;
using DashFire.Dashboard.Framework.Services.Log;
using DashFire.Dashboard.Service.Job;
using DashFire.Dashboard.Service.Log;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Caching.Distributed;

namespace DashFire.Dashboard.API
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
            services.AddControllers();
            services.AddHealthChecks();
            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen();

            services.AddApiVersioning(options =>
            {
                // reporting api versions will return the headers "api-supported-versions" and "api-deprecated-versions"
                options.ReportApiVersions = true;
            });

            services.AddVersionedApiExplorer(options =>
            {
                // add the versioned api explorer, which also adds IApiVersionDescriptionProvider service
                // note: the specified format code will format the version as "'v'major[.minor][-status]"
                options.GroupNameFormat = "'v'VVV";

                // note: this option is only necessary when versioning by url segment. the SubstitutionFormat
                // can also be used to control the format of the API version in route templates
                options.SubstituteApiVersionInUrl = true;
            });

            services.AddOptions();
            services.Configure<ApplicationOptions>(options => Configuration.GetSection("ApplicationOptions").Bind(options));

            services.AddEntityFrameworkNpgsql().AddDbContext<Domain.AppDbContext>((sp, options) =>
            {
                options.UseNpgsql(Configuration.GetConnectionString("DashFireDatabase"));
                options.UseInternalServiceProvider(sp);
            }, ServiceLifetime.Scoped);

            services.AddScoped<IJobService, JobService>();
            services.AddScoped<ILogService, LogService>();

            services.Add(ServiceDescriptor.Singleton<IDistributedCache, RedisCache>());

            services.AddStackExchangeRedisCache(option =>
            {
                option.Configuration = Configuration.GetValue<string>("ApplicationOptions:RedisOptions:ConnectionString");
                option.InstanceName = Configuration.GetValue<string>("ApplicationOptions:RedisOptions:InstanceName");

                option.ConfigurationOptions = new StackExchange.Redis.ConfigurationOptions()
                {
                    AbortOnConnectFail = false,
                    EndPoints =
                    {
                        {
                            Configuration.GetValue<string>("ApplicationOptions:RedisOptions:Ip"),
                            int.Parse(Configuration.GetValue<string>("ApplicationOptions:RedisOptions:Port"))
                        }
                    },
                    ConnectRetry = 99,
                    ConnectTimeout = 5000,
                    SyncTimeout = 5000,
                    DefaultDatabase = 3
                };
            });

            services.AddSingleton<DashFireCacheManager>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var cacheManager = app.ApplicationServices.CreateScope().ServiceProvider.GetService<DashFireCacheManager>();
            cacheManager.InitializeAsync(CancellationToken.None).GetAwaiter().GetResult();

            Task.Delay(TimeSpan.FromSeconds(10), CancellationToken.None).GetAwaiter().GetResult();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            // HealthCheck middleware
            app.UseHealthChecks("/health/live", new HealthCheckOptions()
            {
                Predicate = _ => true
            });

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
