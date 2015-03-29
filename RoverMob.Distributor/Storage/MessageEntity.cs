using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoverMob.Distributor.Storage
{
    public class MessageEntity : TableEntity
    {
        public MessageEntity(string topic, string timestamp)
        {
            this.PartitionKey = topic;
            this.RowKey = timestamp;
        }

        public MessageEntity() { }

        public string Hash { get; set; }
        public string MessageType { get; set; }
        public string Predecessors { get; set; }
        public Guid ObjectId { get; set; }
        public string Body { get; set; }
    }
}
