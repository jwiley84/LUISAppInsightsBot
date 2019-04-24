// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Configuration;

namespace Microsoft.BotBuilderSamples
{
    public interface IBotServices
    {
        LuisRecognizer Luis { get; }
    }
}
