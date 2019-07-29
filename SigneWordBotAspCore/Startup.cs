using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SigneWordBotAspCore.Services;

namespace SigneWordBotAspCore
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IServiceProvider serviceProvider)
        {
            Configuration = configuration;
        }

        
        public IContainer ApplicationContainer { get; private set; }
        
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            var appContext = new Services.AppContext();
            var dbService = new DataBaseService(appContext);

            services.AddSingleton<IAppContext>(appContext);
            services.AddSingleton<IDataBaseService>(dbService);
            services.AddSingleton<IBotService, BotService>();
            services.AddSingleton<IUpdateService, UpdateService>();
            
            
            
            
            var builder = new ContainerBuilder();
            builder.RegisterType<DataBaseService>().As<IDataBaseService>();
            ConfigureContainer(builder);
            
            
            ApplicationContainer = builder.Build();

            return new AutofacServiceProvider(ApplicationContainer);
        }
        public void ConfigureContainer(ContainerBuilder builder)
        {
           
        }
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                System.Console.WriteLine("Use Developer Exception Page");

            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
                System.Console.WriteLine("Use hsts");

            }

            app.UseMvc();

        }
    }
}
