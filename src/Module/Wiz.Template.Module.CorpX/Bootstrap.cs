using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Wiz.Multitenant.Core.Common;
using Wiz.Multitenant.Core.Common.Interfaces;
using Wiz.Template.Domain.Interfaces.Identity;
using Wiz.Template.Domain.Interfaces.Notifications;
using Wiz.Template.Domain.Interfaces.Repository;
using Wiz.Template.Domain.Interfaces.UoW;
using Wiz.Template.Domain.Notifications;
using Wiz.Template.Infra.Context;
using Wiz.Template.Infra.Identity;
using Wiz.Template.Infra.Repository;
using Wiz.Template.Infra.UoW;
using Wiz.Template.Module.Base.Services;
using Wiz.Template.Module.Base.Services.Interfaces;
using Wiz.Template.Module.CorpX.Services;

namespace Wiz.Template.Module.CorpX
{
public class Bootstrap
    {
        public static Type GetTenantStoreType()
        {
            return typeof(TenantRepository);
        }

        private static void RegisterServices(IServiceCollection services, Tenant t)
        {
            #region Service

            services.AddScoped<ICustomerService, CustomerService>();
            //Mudamos um serviço aqui para especializar no contexto da CorpX
            services.AddScoped<ICorporateService, CorpXCorporateService>();
            services.AddScoped<IConfigurationService, ConfigurationService>();
            services.AddScoped(typeof(IFinancingMethodService), serviceProvider => {

                string s = t.Dns;
                ITenantStore<Tenant> tenantStore = serviceProvider.GetService<ITenantStore<Tenant>>();

                Tenant tenant = tenantStore.GetTenantAsync(t.Dns).GetAwaiter().GetResult();

                object instance = Activator.CreateInstance(Type.GetType(tenant.Items["financingMethod"].ToString()));
                return instance;
            });

            #endregion

            #region Domain

            services.AddScoped<IDomainNotification, DomainNotification>();

            #endregion

            #region Infra

            services.AddSingleton<ITenantStore<Tenant>, TenantRepository>();

            services.AddScoped<DapperContext>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<IIdentityService, IdentityService>();

            #endregion
        }

        public static void Init(IServiceCollection services, Tenant t)
        {
            RegisterServices(services,t);
        }
    }
}