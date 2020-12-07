// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.WebJobs.Extensions.Kafka
{
    public class DefaultCommitStrategyFactory : ICommitStrategyFactory
    {
        public virtual ICommitStrategy<TKey, TValue> Create<TKey, TValue>(string strategy, IConsumer<TKey, TValue> consumer, ILogger logger)
        {
            if (strategy is null)
            {
                throw new ArgumentNullException(nameof(strategy));
            }

            switch (strategy.ToLower())
            {
                case CommitStrategies.Async:
                    return new AsyncCommitStrategy<TKey, TValue>(consumer, logger);
                case CommitStrategies.Sync:
                    return new SyncCommitStrategy<TKey, TValue>(consumer, logger);
            }
            throw new ArgumentOutOfRangeException(nameof(strategy), "Unknown commit strategy: " + strategy);
        }
    }
}
