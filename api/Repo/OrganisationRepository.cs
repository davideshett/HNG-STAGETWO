using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using api.Data;
using api.Dto.Organisation;
using api.Models;
using api.Repo.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace api.Repo
{
    public class OrganisationRepository : IOrganisationRepository
    {
        private readonly DataContext dataContext;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly UserManager<User> userManager;

        public OrganisationRepository(DataContext dataContext, IHttpContextAccessor httpContextAccessor,
        UserManager<User> userManager)
        {
            this.dataContext = dataContext;
            this.httpContextAccessor = httpContextAccessor;
            this.userManager = userManager;
        }
        public async Task<Organisation> AddOrganisation(AddOrganisationDto model)
        {
            var userId = httpContextAccessor.HttpContext.User.FindFirstValue("userId");
            var user = await userManager.Users
            .Include(x=> x.Organisations).FirstOrDefaultAsync(x=> x.UserId.Equals(userId));

            if(await OrganisationExists(model.OrgId))
            {
                return null;
            }

            var organisation = new Organisation
            {
                Name = model.Name,
                Description = model.Description,
                OrgId = model.OrgId
            };

            user.Organisations.Add(organisation);
            await userManager.UpdateAsync(user);

            return organisation;
        }

        public async Task<bool> AddUserToOrganisation(string OrgId, UserIdDto model)
        {
            var organisation = await dataContext.Organisations
            .Include(x=> x.Users)
            .FirstOrDefaultAsync(x=> x.OrgId.Equals(OrgId));
            if (organisation == null)
            {
                return false;
            }

            var user = await userManager.FindByIdAsync(model.UserId);
            
            if (user == null)
            {
                return false;
            }

            organisation.Users.Add(user);
            await dataContext.SaveChangesAsync();
            return true;

        }

        public async Task<IEnumerable<Organisation>> GetAll()
        {
            var userId = httpContextAccessor.HttpContext.User.FindFirstValue("userId");
            var user = await userManager.Users
            .Include(x=> x.Organisations)
            .FirstOrDefaultAsync(x=> x.UserId.Equals(userId));
            
            return user.Organisations;
        }

        public async Task<Organisation> GetOrganisation(string orgId)
        {
            var userId = httpContextAccessor.HttpContext.User.FindFirstValue("userId");
            var user = await userManager.Users
            .Include(x=> x.Organisations)
            .FirstOrDefaultAsync(x=> x.UserId.Equals(userId));

            if(GetUsersOrgIs(user).Contains(orgId))
            {
                return await dataContext.Organisations
                .FirstOrDefaultAsync(x=> x.OrgId.Equals(orgId));
            }

            return null;
            
        }

        public List<string> GetUsersOrgIs(User loggedInUser)
        {
            var orgIds = new List<string>();
            foreach (var organisation in loggedInUser.Organisations)
            {
                orgIds.Add(organisation.OrgId);
            }
            return orgIds;
        }

        public async Task<bool> OrganisationExists(string orgId)
        {
            var data = await dataContext.Organisations.FirstOrDefaultAsync(x=> x.OrgId.Equals(orgId));
            if(data == null)
            {
                return false;
            }
            return true;
        }
    }
}