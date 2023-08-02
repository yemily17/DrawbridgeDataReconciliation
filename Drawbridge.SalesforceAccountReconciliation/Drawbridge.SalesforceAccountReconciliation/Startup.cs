// -----------------------------------------------------------------------
// <copyright file="Startup.cs" company="Drawbridge Partners, LLC">
// Copyright (c) Drawbridge Partners, LLC. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using Drawbridge.SalesforceAccountReconciliation.Configuration;
using Drawbridge.WebApi;
using Drawbridge.WebApi.Configuration;
using Drawbridge.WebApi.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Drawbridge.SalesforceAccountReconciliation
{
    /// <summary>
    /// The startup class used to configure the application.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="env">The environment.</param>
        public Startup(IWebHostEnvironment env)
        {
            Configuration = AppConfigurationBuilder.Build(env);
        }

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Configures the services for the application.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            var assembly = typeof(Startup).Assembly;
            services.AddDefaults(assembly, Configuration);
            services.AddRazorPages();
            services.Configure<SampleConfiguration>(Configuration.GetSection("Drawbridge:Sample"));
        }

        /// <summary>
        /// Configures the application.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <param name="env">The environment.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseDefaults(env, typeof(Startup).Assembly);
            app.UseStaticFiles();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }
    }
}
