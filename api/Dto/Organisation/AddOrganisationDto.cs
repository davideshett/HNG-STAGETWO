using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dto.Organisation
{
    public class AddOrganisationDto
    {
        public string OrgId { get; set; }
        public string Name { get; set; } 
        public string Description { get; set; } 
    }
}