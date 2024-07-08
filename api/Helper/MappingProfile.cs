using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dto.Organisation;
using api.Models;
using AutoMapper;

namespace api.Helper
{
    public class MappingProfile: Profile
    {
         public MappingProfile()
        {
            CreateMap<Organisation, OrganisationForReturn>().ReverseMap();
        }
    }
}