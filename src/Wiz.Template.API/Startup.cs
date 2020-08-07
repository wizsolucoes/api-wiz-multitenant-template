using Autofac;
using Autofac.Extensions.DependencyInjection;
using AutoMapper;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using NSwag;
using NSwag.SwaggerGeneration.Processors.Security;
using Polly;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Claims;
using Wiz.Multitenant.Core.Common;
using Wiz.Multitenant.Core.Extensions;
using Wiz.Multitenant.Core.Strategies;
using Wiz.Template.API.Extensions;
using Wiz.Template.API.Filters;
using Wiz.Template.API.Middlewares;
using Wiz.Template.API.Settings;
using Wiz.Template.API.Swagger;
using Wiz.Template.Domain.Settings;
using Wiz.Template.Infra.Context;
using Wiz.Template.Module.Base.Services.Interfaces;

[assembly: ApiConventionType(typeof(MyApiConventions))]
namespace Wiz.Template.API
{
    public class Startup
    {
        //TODO: Mudar para App configuration
        public static readonly string SQUAD = "devz";

        public Startup(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            Configuration = configuration;
            WebHostEnvironment = webHostEnvironment;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment WebHostEnvironment { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddMvc(options =>
            {
                options.Filters.Add<DomainNotificationFilter>();
                options.EnableEndpointRouting = false;
            }).AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.IgnoreNullValues = true;
            });

            //Work with Multi-Tenants
            services.AddMultiTenancy()
                .WithResolutionStrategy<HostTenantResolutionStrategy>()
                // .WithResolutionStrategy<HeaderTenantResolutionStrategy>()
                .WithStore(Module.Base.Bootstrap.GetTenantStoreType());

            services.Configure<GzipCompressionProviderOptions>(x => x.Level = CompressionLevel.Optimal);
            services.AddResponseCompression(x =>
            {
                x.Providers.Add<GzipCompressionProvider>();
            });

            services.Configure<KestrelServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });

            services.Configure<IISServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });

            if (PlatformServices.Default.Application.ApplicationName != "testhost")
            {
                var healthCheck = services.AddHealthChecksUI(setupSettings: setup =>
                {
                    setup.AddWebhookNotification("Teams", Configuration["Webhook:Teams"],
                        payload: File.ReadAllText(Path.Combine(".", "MessageCard", "ServiceDown.json")),
                        restorePayload: File.ReadAllText(Path.Combine(".", "MessageCard", "ServiceRestore.json")),
                        customMessageFunc: report =>
                            {
                                var failing = report.Entries.Where(e => e.Value.Status == UIHealthStatus.Unhealthy);
                                return $"{AppDomain.CurrentDomain.FriendlyName}: {failing.Count()} healthchecks are failing";
                            }
                        );
                }).AddHealthChecks();

                //500 mb
                healthCheck.AddProcessAllocatedMemoryHealthCheck(500 * 1024 * 1024, "Process Memory", tags: new[] { "self" });
                //500 mb
                healthCheck.AddPrivateMemoryHealthCheck(1500 * 1024 * 1024, "Private memory", tags: new[] { "self" });

                healthCheck.AddSqlServer(Configuration["ConnectionStrings:CustomerDB"], tags: new[] { "services" });

                //dotnet add <Project> package AspNetCore.HealthChecks.Redis
                //healthCheck.AddRedis(Configuration["Data:ConnectionStrings:Redis"], tags: new[] {"services"});

                //dotnet add <Project> package AspNetCore.HealthChecks.OpenIdConnectServer
                //healthCheck.AddIdentityServer(new Uri(Configuration["WizID:Authority"]), "SSO Wiz", tags: new[] { "services" });

                //if (WebHostEnvironment.IsProduction())
                //{
                //dotnet add <Project> package AspNetCore.HealthChecks.AzureKeyVault
                //healthChecks.AddAzureKeyVault(options =>
                //{
                //    options.UseKeyVaultUrl($"{Configuration["Azure:KeyVaultUrl"]}");
                //}, name: "azure-key-vault",tags: new[] {"services"});
                //}

                healthCheck.AddApplicationInsightsPublisher();
            }

            if (!WebHostEnvironment.IsProduction())
            {
                services.AddSwaggerDocument(document =>
                {
                    document.DocumentName = "v1";
                    document.Version = "v1";
                    document.Title = "Whitelabel API";
                    document.Description = "API de Whitelabel";
                    document.OperationProcessors.Add(new OperationSecurityScopeProcessor("JWT"));
                    document.AddSecurity("JWT", Enumerable.Empty<string>(), new SwaggerSecurityScheme
                    {
                        Type = SwaggerSecuritySchemeType.ApiKey,
                        Name = HeaderNames.Authorization,
                        Description = "Token de autenticação via SSO",
                        In = SwaggerSecurityApiKeyLocation.Header
                    });
                });
            }

            services.AddAutoMapper(typeof(Startup));
            services.AddHttpContextAccessor();
            services.AddApplicationInsightsTelemetry();

            RegisterServices(services);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IOptions<ApplicationInsightsSettings> options)
        {
            if (!env.IsProduction())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseAzureAppConfiguration();

            app.UseRouting();
            app.UseHttpsRedirection();
            app.UseResponseCompression();

            app.UseCors(builder =>
            {
                builder.AllowAnyOrigin();
                builder.AllowAnyMethod();
                builder.AllowAnyHeader();
            });

            if (!env.IsProduction())
            {
                app.UseSwagger();
                app.UseSwaggerUi3();
            }

            app.UseMultiTenant<Tenant>().UseMultiTenantAuthentication();

            app.UseAuthorization();
            app.UseLogMiddleware();

            app.UseExceptionHandler(new ExceptionHandlerOptions
            {
                ExceptionHandler = new ErrorHandlerMiddleware(options, env).Invoke
            });

            app.UseEndpoints(endpoints =>
            {
                if (PlatformServices.Default.Application.ApplicationName != "testhost")
                {
                    //Para cada sistema de terceiro ou API da Wiz (incluir URL em appsettings.json)
                    //endpoints.MapHealthChecks("{sistema}", ...);
                    endpoints.MapHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("self"),
                        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                    });
                    endpoints.MapHealthChecks("/ready", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("services"),
                        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                    });
                    endpoints.MapHealthChecksUI(setup =>
                    {
                        setup.UIPath = "/health-ui";
                    });
                }

                endpoints.MapControllers();
            });
        }

        private void RegisterServices(IServiceCollection services)
        {
            services.Configure<ApplicationInsightsSettings>(Configuration.GetSection("ApplicationInsights"));
            services.AddDbContext<EntityContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("CustomerDB")));
        }

        private static void ConfigureHttpServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpClient("viacep", (s, c) =>
            {
                c.BaseAddress = new Uri(configuration["API:ViaCEP"]);
                c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            })
            .ConfigurePrimaryHttpMessageHandler(() => new System.Net.Http.HttpClientHandler
            {
                AllowAutoRedirect = false,
                AutomaticDecompression = System.Net.DecompressionMethods.Deflate | System.Net.DecompressionMethods.GZip,
                CheckCertificateRevocationList = false,
            })
                .AddTransientHttpErrorPolicy(policyBuilder => policyBuilder.OrResult(response =>
                     (int)response.StatusCode == (int)HttpStatusCode.InternalServerError)
               .WaitAndRetryAsync(3, retry =>
                    TimeSpan.FromSeconds(Math.Pow(2, retry)) +
                    TimeSpan.FromMilliseconds(new Random().Next(0, 100))))
              .AddTransientHttpErrorPolicy(policyBuilder => policyBuilder.CircuitBreakerAsync(
                   handledEventsAllowedBeforeBreaking: 3,
                   durationOfBreak: TimeSpan.FromSeconds(30)
            ));
        }

        internal static void ConfigureTenantServices(Tenant t, ContainerBuilder c, IHttpContextAccessor httpContextAccessor)
        {
            IConfiguration configuration = httpContextAccessor.HttpContext.RequestServices.GetRequiredService<IConfiguration>();

            ServiceCollection tenantServices = new ServiceCollection();

            //precisa configurar o WizIDSettings no App Configuration
            string wizId = configuration.GetSection($"{SQUAD}:{t.Id}:WizID").Value;

            if (wizId is string)
            {
                WizIDSettings wizIDSettings = JsonConvert.DeserializeObject<WizIDSettings>(wizId);
                tenantServices.AddAuthentication(options =>
                {
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                }).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.Authority = wizIDSettings.Authority;
                    options.Audience = wizIDSettings.Audience;
                    options.RequireHttpsMetadata = false;
                    options.Events = new JwtBearerEvents
                    {
                        //Remover warning caso há alguma validação do token assíncrona (async/await)
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
                        OnTokenValidated = async ctx =>
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
                        {
                            //Exemplo para recuperar informações do token JWT e utilizar no serviço: IIdentityService
                            var jwtClaimScope = ctx.Principal.Claims.FirstOrDefault(x => x.Type == "scope")?.Value;

                            var claims = new List<Claim>
                        {
                                new Claim(ClaimTypes.System, jwtClaimScope),
                                new Claim(ClaimTypes.Authentication, ((JwtSecurityToken)ctx.SecurityToken).RawData)
                        };

                            var claimsIdentity = new ClaimsIdentity(claims);
                            ctx.Principal.AddIdentity(claimsIdentity);
                            ctx.Success();
                        }
                    };
                });
            }

            ConfigureHttpServices(tenantServices, configuration);

            Type type = Type.GetType(t.Items["module"].ToString());
            MethodInfo staticMethodInfo = type.GetMethod("Init");
            staticMethodInfo.Invoke(null, new object[] { tenantServices, t });

            c.Populate(tenantServices);
        }
    }
}
