// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.ApplicationInsights.Core;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.BotBuilderSamples.Telemetry;
using Microsoft.Extensions.Configuration;
using Microsoft.Bot.Builder.ApplicationInsights;

namespace Microsoft.BotBuilderSamples
{
    public class Startup
    {
        //public IConfiguration configuration { get; set; }
        //public int pause = 1;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            
            // Create the credential provider to be used with the Bot Framework Adapter.
            services.AddSingleton<ICredentialProvider, ConfigurationCredentialProvider>();

            // Create the Bot Framework Adapter with error handling enabled. 
            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

            // Create the bot services (LUIS, QnA) as a singleton.
            services.AddSingleton<IBotServices, BotServices>();

            // Create the storage we'll be using for the User and Conversation State. (Memory is great for testing purposes)
            services.AddSingleton<IStorage, MemoryStorage>();

            // Create Telemetry Client
            services.AddSingleton<TelemetryLuisRecognizer>();

            // Create the bot as a transient.
            services.AddTransient<IBot, LuisAppInsightsBot>();
            
            #region Option_1
            
            // The following line enables Application Insights telemetry collection.
            services.AddBotApplicationInsights();

            #endregion

            #region Option_2

            //services.AddApplicationInsightsTelemetry();

            //services.AddSingleton<IBotTelemetryClient>(options => new BotTelemetryClient(new TelemetryClient()));
            #endregion

            // Create the Telemetry Middleware that will be added to the middlware pipeline in the AdapterWithErrorHandler.
            services.AddSingleton<TelemetryLoggerMiddleware>();
            
            
            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();

            //app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
