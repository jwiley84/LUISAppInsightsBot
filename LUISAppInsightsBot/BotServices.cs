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
        }

        public LuisRecognizer Luis { get; private set; }
        
        private LuisRecognizer ReadLuisRecognizer(IConfiguration configuration)
        {
            try
            {
                var services = configuration.GetSection("BotServices");
                var application = new LuisApplication(services.GetValue<string>("LuisAppId"),
                    services.GetValue<string>("LuisAuthoringKey"),
                    "https://" + services.GetValue<string>("LuisHostName"));
                var predictions = new LuisPredictionOptions 
                { 
                    IncludeAllIntents = true,
                    IncludeInstanceData = true 
                };
                var recognizer = new LuisRecognizer(application, predictions, true);
                return recognizer;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
