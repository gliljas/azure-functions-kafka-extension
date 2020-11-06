// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Confluent.Kafka;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Microsoft.Azure.WebJobs.Extensions.Kafka
{
    public class KafkaEventData<TKey, TValue> : IKafkaEventData, IKafkaEventDataWithHeaders
    {
        public TKey Key { get; set; }

        protected IKafkaEventDataHeaders headers;
        
        public long Offset { get; set; }
        public int Partition { get; set; }
        public string Topic { get; set; }
        public DateTime Timestamp { get; set; }
        public TValue Value { get; set; }

        object IKafkaEventData.Value => this.Value;

        object IKafkaEventData.Key => Key;

        public IKafkaEventDataHeaders Headers => LazyInitializer.EnsureInitialized(ref headers, () => new HeadersProxy(this));

        public KafkaEventData()
        {
        }

        public KafkaEventData(TKey key, TValue value)
        {
            this.Key = key;
        }

        public KafkaEventData(ConsumeResult<TKey, TValue> consumeResult)
        {
            this.Key = consumeResult.Key;
            this.Value = consumeResult.Value;
            this.Offset = consumeResult.Offset;
            this.Partition = consumeResult.Partition;
            this.Timestamp = consumeResult.Message.Timestamp.UtcDateTime;
            this.Topic = consumeResult.Topic;
            if (consumeResult.Message.Headers?.Count > 0)
            {
                this.headers = new KafkaEventDataHeaders(consumeResult.Message.Headers);
            }
        }

        #region IKafkaEventDataHeaders

        private class HeadersProxy : IKafkaEventDataHeaders
        {
            private static readonly IEnumerable<IKafkaEventDataHeader> EmptyHeaders = new IKafkaEventDataHeader[] { };
            private readonly KafkaEventData<TKey, TValue> eventData;
            private bool headersCreated = false;

            public HeadersProxy(KafkaEventData<TKey, TValue> eventData)
            {
                this.eventData = eventData;
            }
            private IKafkaEventDataHeaders EnsureHeadersCreated()
            {
                if (!headersCreated)
                {
                    Interlocked.CompareExchange(ref eventData.headers, new KafkaEventDataHeaders(), this);
                    headersCreated = true;
                }
                return eventData.headers;
            }

            int IKafkaEventDataHeaders.Count => 0;

            void IKafkaEventDataHeaders.Add(string key, byte[] value) => EnsureHeadersCreated().Add(key, value);

            IEnumerator<IKafkaEventDataHeader> IEnumerable<IKafkaEventDataHeader>.GetEnumerator() => EmptyHeaders.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => EmptyHeaders.GetEnumerator();
        }
        #endregion
    }

    public class KafkaEventData<TValue> : KafkaEventData<object, TValue>
    {
        public KafkaEventData() : base()
        {
        }

        public KafkaEventData(TValue value) : base(null, value)
        {
        }

        internal static KafkaEventData<TValue> CreateFrom<TKey>(ConsumeResult<TKey, TValue> consumeResult)
        {
            var result = new KafkaEventData<TValue>
            {
                Value = consumeResult.Value,
                Offset = consumeResult.Offset,
                Partition = consumeResult.Partition,
                Timestamp = consumeResult.Timestamp.UtcDateTime,
                Topic = consumeResult.Topic
            };

            if (consumeResult.Message?.Headers?.Count > 0)
            {
                result.headers = new KafkaEventDataHeaders(consumeResult.Message.Headers);
            }
            return result;
        }

        public KafkaEventData(IKafkaEventData src)
        {
            this.Value = (TValue)src.Value;
            this.Offset = src.Offset;
            this.Partition = src.Partition;
            this.Timestamp = src.Timestamp;
            this.Topic = src.Topic;
            if (src is IKafkaEventDataWithHeaders srcWithHeaders)
            {
                this.headers = new KafkaEventDataHeaders(srcWithHeaders.Headers.Select(x => new KafkaEventDataHeader(x.Key, x.Value)));
            }
        }
    }
}