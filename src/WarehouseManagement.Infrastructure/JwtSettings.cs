using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarehouseManagement.Infrastructure
{
    public static class JwtSettings
    {
        public const string Key =
            "WMS2026$Secure$Jwt$Key#Warehouse$Management#UltraSecret#12345";

        public const string Issuer = "WarehouseManagement.API";

        public const string Audience = "WarehouseManagement.Client";

        public const int ExpiryHours = 1;
    }
}
