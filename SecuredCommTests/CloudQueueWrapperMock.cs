﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using SecuredCommunication;

namespace UnitTests.Mocks
{
    public class CloudQueueWrapperMock : ICloudQueueWrapper
    {
        public List<CloudQueueMessage> QueueList;

        public Task AddMessageAsync(CloudQueueMessage message)
        {
            QueueList.Add(message);
            return Task.FromResult("result");
        }

        public Task CreateIfNotExistsAsync()
        {
            QueueList = new List<CloudQueueMessage>();
            return Task.FromResult("result");
        }

        public Task<CloudQueueMessage> GetMessageAsync(TimeSpan? visibilityTimeout, QueueRequestOptions options, OperationContext operationContext)
        {
            if (QueueList.Count == 0) {
                return Task.FromResult<CloudQueueMessage>(null);
            }
            var message = QueueList[QueueList.Count - 1];
            

            return Task.FromResult(message);
        }

        public Task DeleteMessageAsync(CloudQueueMessage message)
        {
            QueueList.RemoveAt(QueueList.Count - 1);
            return Task.FromResult("result");
        }
    }
}
