// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.WebJobs.Extensions.Kafka
{
    public interface ICommitStrategyFactory
    {
        ICommitStrategy<TKey, TValue> Create<TKey, TValue>(string strategy, IConsumer<TKey, TValue> consumer, ILogger logger);
    }
}
