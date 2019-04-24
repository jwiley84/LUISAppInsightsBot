// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Configuration;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;

namespace Microsoft.BotBuilderSamples
{
    
    public class LuisAppInsightsBot : ActivityHandler
    {
        private ILogger<LuisAppInsightsBot> _logger;
        private IBotServices _botServices;

        public LuisAppInsightsBot(IBotServices botServices, ILogger<LuisAppInsightsBot> logger)
        {
            _logger = logger;
            _botServices = botServices;
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var recognizerResult = await _botServices.Luis.RecognizeAsync(turnContext, cancellationToken);

            if (recognizerResult != null)
            {
                await ProcessLuisAsync(turnContext, recognizerResult, cancellationToken);
            } 
            else
            {
                var msg = @"No LUIS intents were found.
                            This sample is about identifying two user intents:
                            'Calendar.Add'
                            'Calendar.Find'
                            Try typing 'Add Event' or 'Show me tomorrow'.";
                await turnContext.SendActivityAsync(msg);
            }

        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            const string WelcomeText = @"This sample is about identifying two user intents: 
                            'Calendar.Add'
                            'Calendar.Find'
                            Try typing 'Add Event' or 'Show me tomorrow'.";

            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Welcome to Luis with Application Insights Bot{ member.Name }. { WelcomeText }"), cancellationToken);
                }
            }
        }

        private async Task ProcessLuisAsync(ITurnContext<IMessageActivity> turnContext, RecognizerResult recognizerResult, CancellationToken cancellationToken)
        {
            _logger.LogInformation("ProcessLuisAsync");

            var result = recognizerResult;
            var topIntent = result.GetTopScoringIntent();

            await turnContext.SendActivityAsync(MessageFactory.Text($"Top intent: { topIntent.intent }, Score: { topIntent.score }"), cancellationToken);
            await turnContext.SendActivityAsync(MessageFactory.Text($"Intents detected: \n\n {string.Join("\n\n", result.Intents.Select(i => i.Key))}"), cancellationToken);            
        }
    }
}
