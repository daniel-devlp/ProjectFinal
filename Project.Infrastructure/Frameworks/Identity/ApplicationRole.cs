using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Project.Infrastructure.Frameworks.Identity
{
    public class ApplicationRole : IdentityRole
    {
        // Constructor sin parámetros
        public ApplicationRole() : base() { }

        // Constructor con nombre del rol
        public ApplicationRole(string roleName) : base(roleName) { }
        public string Description { get; set; } 
        public bool IsActive { get; set; } = true;


    }
}
