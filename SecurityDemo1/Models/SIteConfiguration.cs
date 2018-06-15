using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SecurityDemo1.Services
{
    public class SiteConfiguration
    {
        public string Name { get; set; } = "Security Demo 1";
        public string Logo { get; set; } = "Logo";
        public string BootstrapVersion { get; set; } = "";
        public SiteTheme Theme { get; set; }

        public class SiteTheme
        {
            public string Name { get; set; }
        }

    }
}
