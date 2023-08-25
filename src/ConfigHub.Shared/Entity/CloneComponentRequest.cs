using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigHub.Shared.Entity
{
    public class CloneComponentRequest
    {
        public string SourceComponentName { get; set; }
        public string TargetComponentName { get; set; }
        public bool CopyValuesFromSource { get; set; }
        public string DefaultValue { get; set; }
    }

}
