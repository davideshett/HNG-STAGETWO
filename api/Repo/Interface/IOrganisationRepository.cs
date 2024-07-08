using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dto.Organisation;
using api.Models;

namespace api.Repo.Interface
{
    public interface IOrganisationRepository
    {
        Task<IEnumerable<Organisation>> GetAll ();
        Task<Organisation> AddOrganisation (AddOrganisationDto model);
        Task<Organisation> GetOrganisation (string orgId);
        Task<bool> AddUserToOrganisation (string OrgId, UserIdDto model);
        Task<bool> OrganisationExists(string orgId);
    }
}