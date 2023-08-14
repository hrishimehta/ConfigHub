using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigHub.Shared.Entity
{
    public class AppInfo
    {
        public string ApplicationName { get; set; }

        public List<string> Components { get; set; }
    }
}
