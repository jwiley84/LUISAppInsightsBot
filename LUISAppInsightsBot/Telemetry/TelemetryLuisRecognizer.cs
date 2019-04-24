// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json.Linq;

namespace Microsoft.BotBuilderSamples.Telemetry
{
    
    public class TelemetryLuisRecognizer : LuisRecognizer
    {
        
        public TelemetryLuisRecognizer(LuisApplication application, LuisPredictionOptions predictionOptions = null, bool includeApiResults = false, bool logOriginalMessage = false, bool logUserName = false)
            : base(application, predictionOptions, includeApiResults)
        {
            LogOriginalMessage = logOriginalMessage;
            LogUsername = logUserName;
        }

        
        public bool LogOriginalMessage { get; }

        
        public bool LogUsername { get; }

        public async Task<T> RecognizeAsync<T>(ITurnContext turnContext, bool logOriginalMessage, CancellationToken cancellationToken = default(CancellationToken))
            where T : IRecognizerConvert, new()
        {
            var result = new T();
            result.Convert(await RecognizeAsync(turnContext, logOriginalMessage, cancellationToken).ConfigureAwait(false));
            return result;
        }

        
        public async Task<RecognizerResult> RecognizeAsync(DialogContext dialogContext, bool logOriginalMessage, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (dialogContext == null)
            {
                throw new ArgumentNullException(nameof(dialogContext));
            }

            return await RecognizeInternalAsync(dialogContext.Context, logOriginalMessage, dialogContext.ActiveDialog.Id, cancellationToken);
        }

       
        public async Task<RecognizerResult> RecognizeAsync(ITurnContext context, bool logOriginalMessage, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await RecognizeInternalAsync(context, logOriginalMessage, null, cancellationToken);
        }

        private async Task<RecognizerResult> RecognizeInternalAsync(ITurnContext context, bool logOriginalMessage, string dialogId = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // Call Luis Recognizer
            var recognizerResult = await RecognizeAsync(context, cancellationToken);

            // Find the Telemetry Client
            if (context.TurnState.TryGetValue(TelemetryLoggerMiddleware.AppInsightsServiceKey, out var telemetryClient) && recognizerResult != null)
            {
                var topLuisIntent = recognizerResult.GetTopScoringIntent();
                var intentScore = topLuisIntent.score.ToString("N2");

                // Add the intent score and conversation id properties
                var telemetryProperties = new Dictionary<string, string>()
                {
                    { LuisTelemetryConstants.IntentProperty, topLuisIntent.intent },
                    { LuisTelemetryConstants.IntentScoreProperty, intentScore },
                };

                if (dialogId != null)
                {
                    telemetryProperties.Add(LuisTelemetryConstants.DialogId, dialogId);
                }

                if (recognizerResult.Properties.TryGetValue("sentiment", out var sentiment) && sentiment is JObject)
                {
                    if (((JObject)sentiment).TryGetValue("label", out var label))
                    {
                        telemetryProperties.Add(LuisTelemetryConstants.SentimentLabelProperty, label.Value<string>());
                    }

                    if (((JObject)sentiment).TryGetValue("score", out var score))
                    {
                        telemetryProperties.Add(LuisTelemetryConstants.SentimentScoreProperty, score.Value<string>());
                    }
                }

                // Add Luis Entitites
                var entities = new List<string>();
                foreach (var entity in recognizerResult.Entities)
                {
                    if (!entity.Key.ToString().Equals("$instance"))
                    {
                        entities.Add($"{entity.Key}: {entity.Value.First}");
                    }
                }

                // For some customers, logging user name within Application Insights might be an issue so have provided a config setting to disable this feature
                if (logOriginalMessage && !string.IsNullOrEmpty(context.Activity.Text))
                {
                    telemetryProperties.Add(LuisTelemetryConstants.QuestionProperty, context.Activity.Text);
                }

                // Track the event
                ((TelemetryClient)telemetryClient).TrackEvent($"{LuisTelemetryConstants.IntentPrefix}.{topLuisIntent.intent}", telemetryProperties);
            }

            return recognizerResult;
        }
    }
}
