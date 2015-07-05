using RoverMob.Protocol;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace RoverMob.Distributor.Storage
{
    public class AzureStorageProvider
    {
        private readonly string _storageConnectionString;

        public AzureStorageProvider(string storageConnectionString)
        {
            _storageConnectionString = storageConnectionString;
        }

        public void WriteMessage(string topic, MessageMemento message)
        {
            var messageTable = OpenTable("Message");

            var timestamp = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmssfffffff");
            var entity = new MessageEntity(topic, timestamp);
            entity.Hash = message.Hash;
            entity.MessageType = message.MessageType;
            entity.Predecessors = JsonConvert.SerializeObject(message.Predecessors);
            entity.ObjectId = message.ObjectId;
            entity.Body = JsonConvert.SerializeObject(message.Body);

            var insert = TableOperation.Insert(entity);
            messageTable.Execute(insert);
        }

        public PageMemento ReadMessages(string topic, string bookmark)
        {
            var messageTable = OpenTable("Message");

            var query = new TableQuery<MessageEntity>().Where(
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition(
                        "PartitionKey", QueryComparisons.Equal, topic),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition(
                        "RowKey", QueryComparisons.GreaterThan, bookmark)));

            var messages = messageTable.ExecuteQuery(query);

            return new PageMemento
            {
                Bookmark = messages.Select(m => m.RowKey).Max() ??
                    bookmark,
                Messages = messages.Select(m => new MessageMemento
                {
                    Hash = m.Hash,
                    Topics = new List<string> { topic },
                    MessageType = m.MessageType,
                    Predecessors = JsonConvert.DeserializeObject<List<PredecessorMemento>>(
                        m.Predecessors),
                    ObjectId = m.ObjectId,
                    Body = JsonConvert.DeserializeObject<ExpandoObject>(m.Body)
                }).ToList()
            };
        }

        public Guid GetUserIdentifier(string role, string userId)
        {
            var userIdentifierTable = OpenTable("UserIdentifier");

            // The row key must not contain special characters.
            var encodedUserId = Uri.EscapeDataString(userId);

            // First try to retrieve an existing entity.
            var retrieve = TableOperation.Retrieve<UserIdentifierEntity>(
                role, encodedUserId);
            var retrieveResult = userIdentifierTable.Execute(retrieve);
            if (retrieveResult.Result != null)
                return ((UserIdentifierEntity)retrieveResult.Result).Identifier;

            // Then create a new one if the existing one is not present.
            var entity = new UserIdentifierEntity(role, encodedUserId)
            {
                Identifier = Guid.NewGuid()
            };
            var insert = TableOperation.Insert(entity);
            userIdentifierTable.Execute(insert);
            return entity.Identifier;
        }

        private CloudTable OpenTable(string tableName)
        {
            var storageAccount = CloudStorageAccount.Parse(_storageConnectionString);

            var tableClient = storageAccount.CreateCloudTableClient();
            var messageTable = tableClient.GetTableReference(tableName);
            messageTable.CreateIfNotExists();

            return messageTable;
        }
    }
}
