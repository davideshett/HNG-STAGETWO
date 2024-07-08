using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace api.Data.Seeder
{
    public class Seed
    {
         public static async Task SeedOrganisation(DataContext dataContext)
        {
            if (await dataContext.Organisations.AnyAsync()) return;

            var dataFronJsonFile = await System.IO.File.ReadAllTextAsync("Data/Seeder/OrganisationTableSeeder.json");
            var organisations = JsonConvert.DeserializeObject<List<Organisation>>(dataFronJsonFile);
            if (organisations == null) return;

            foreach (var organisation in organisations)
            {
                var orgId = Guid.NewGuid().ToString();
                dataContext.Organisations.Add(new Organisation
               {
                    Name = organisation.Name,
                    OrgId = orgId,
                    Description = organisation.Description,
               });
            }

            await dataContext.SaveChangesAsync();
        }
    }
}