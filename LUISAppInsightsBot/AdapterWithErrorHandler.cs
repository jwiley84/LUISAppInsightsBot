// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.BotBuilderSamples.Telemetry;

namespace Microsoft.BotBuilderSamples
{
    public class AdapterWithErrorHandler : BotFrameworkHttpAdapter
    {
        public AdapterWithErrorHandler(ICredentialProvider credentialProvider, ILogger<BotFrameworkHttpAdapter> logger, TelemetryLoggerMiddleware telemetryLoggerMiddleware)
            : base(credentialProvider)
        {
            if (credentialProvider == null)
            {
                throw new NullReferenceException(nameof(credentialProvider));
            }

            if (logger == null)
            {
                throw new NullReferenceException(nameof(logger));
            }

            if (telemetryLoggerMiddleware == null)
            {
                throw new NullReferenceException(nameof(telemetryLoggerMiddleware));
            }

            // Add telemetryLogger Middleware to the adapter's middleware pipeline
            Use(telemetryLoggerMiddleware);

            // Enable logging at the adapter level using OnTurnError.
            OnTurnError = async (turnContext, exception) =>
            {
                logger.LogError($"Exception caught : {exception}");
                await turnContext.SendActivityAsync("Sorry, it looks like something went wrong.");
                await turnContext.SendActivityAsync("To run this sample make sure you have the LUIS model deployed and App Insights telemtry configured.");
            };
        }
    }
}
