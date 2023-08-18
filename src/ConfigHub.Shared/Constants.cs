using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigHub.Shared
{
    public static class Constants
    {
        public const string ApplicationNameHeader = "X-ApplicationId"; 

        public const string ClientCertificateHeader = "X-Client-Cert";

        public const string TotalCountResponseHeader = "X-TotalCount";

        public const int DefaultPagingSize = 100;

        public const int MinimumSearchLength = 3;

        public const string PagingTakeAttributeName = "take";

        public const string PagingSkipAttributeName = "skip";

    }
}
