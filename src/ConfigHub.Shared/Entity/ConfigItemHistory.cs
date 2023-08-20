using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigHub.Shared.Entity
{
    using System;

    namespace ConfigHub.Domain.Entity
    {
        public class ConfigItemHistory
        {
            public Guid Id { get; set; }
            public string ItemId { get; set; }

            public string Key { get; set; }

            public OperationType OperationType { get; set; }
            public DateTime ChangeDate { get; set; }
            public List<string> ChangedProperties { get; set; }
            public string ChangedBy { get; set; }
            public string ApplicationName { get; set; }
            public string Component { get; set; }
        }

        public enum OperationType
        {
            Create = 0,
            Update = 1,
            Delete = 2
        }
    }

}
