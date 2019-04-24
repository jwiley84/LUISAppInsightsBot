// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License

using System;
using System.Collections.Generic;
using Microsoft.ApplicationInsights;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.BotBuilderSamples.Telemetry;

namespace Microsoft.BotBuilderSamples
{
    public class BotServices : IBotServices
    {
        public BotServices(IConfiguration configuration) 
        {
            // Read the settings for the cognitive services from the appsettings.json
            Luis = ReadLuisRecognizer(configuration);
            //AppInsights = ReadAppInsights(configuration);
        }

        public LuisRecognizer Luis { get; private set; }

        //public AppInsightsService AppInsights { get; private set; }

        private LuisRecognizer ReadLuisRecognizer(IConfiguration configuration)
        {
            try
            {
                var services = configuration.GetSection("BotServices");
                var luisService = new LuisService
                {
                    AppId = services.GetValue<string>("LuisAppId"),
                    AuthoringKey = services.GetValue<string>("LuisAuthoringKey"),
                    Region = services.GetValue<string>("LuisRegion")
                };
                var application = new LuisApplication(luisService.AppId,
                    luisService.AuthoringKey,
                    luisService.GetEndpoint());
                var predictions = new LuisPredictionOptions 
                { 
                    IncludeAllIntents = true,
                    IncludeInstanceData = true 
                };
                var recognizer = new LuisRecognizer(application, predictions, true);
                var telemetryRecognizer = new TelemetryLuisRecognizer(application, predictions, false, false, false);
                return telemetryRecognizer;
               // return recognizer;
            }
            catch (Exception)
            {
                return null;
            }
        }
        /*
        private AppInsightsService ReadAppInsights(IConfiguration configuration)
        {
            try
            {
                var services = configuration.GetSection("BotServices");
                var appInsightsService = new AppInsightsService
                {
                    InstrumentationKey = services.GetValue<string>("AppInsightsInstrumentationKey"),
                    //AuthoringKey = services.GetValue<string>("LuisAuthoringKey"),
                    //Region = services.GetValue<string>("LuisRegion")
                };
                
                if (appInsightsService == null)
                {
                    throw new InvalidOperationException("The Application Insights is not configured correctly in your appsettings.");
                }

                if (string.IsNullOrWhiteSpace(appInsightsService.InstrumentationKey))
                {
                    throw new InvalidOperationException("The Application Insights Instrumentation Key ('instrumentationKey') is required to run this sample.  Please update your '.bot' file.");
                }

                return appInsightsService;
            }
            catch (Exception)
            {
                return null;
            }
        }*/
    }
}
