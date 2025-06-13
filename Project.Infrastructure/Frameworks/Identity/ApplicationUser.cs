using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Project.Infrastructure.Frameworks.Identity
{
    public class ApplicationUser : IdentityUser<string>
    {
        public string Identification { get; set; } // Cedula
        public bool IsBlocked { get; set; }
        public DateTime? BlockedUntil { get; set; }
        public int FailedLoginAttempts { get; set; }
        // Additional properties can be added here as needed
    }
    
    
}
