using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoverMob.Distributor.Storage
{
    public class UserIdentifierEntity : TableEntity
    {
        public UserIdentifierEntity(string role, string userId)
        {
            this.PartitionKey = role;
            this.RowKey = userId;
        }

        public UserIdentifierEntity() { }

        public Guid Identifier { get; set; }
    }
}
