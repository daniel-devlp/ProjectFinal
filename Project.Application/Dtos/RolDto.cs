using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Application.Dtos
{
    public class RolDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
    public class RoleCreateDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
    public class RoleUpdateDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
