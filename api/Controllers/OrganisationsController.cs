using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dto.Organisation;
using api.Repo.Interface;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [ApiController]
    [Route("api/organisations")]
    public class OrganisationsController : ControllerBase
    {
        private readonly IOrganisationRepository organisationRepository;
        private readonly IMapper mapper;

        public OrganisationsController(IOrganisationRepository organisationRepository, IMapper mapper)
        {
            this.organisationRepository = organisationRepository;
            this.mapper = mapper;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddOrganisations(AddOrganisationDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    status = "Bad Request",
                    message = "Client error",
                    statusCode = 400
                });
            }

            var dataFromRepo = await organisationRepository.AddOrganisation(model);

            if (dataFromRepo == null)
            {
                return BadRequest(new
                {
                    Status = "Error",
                    Message = "Duplicate Ids not allowed",
                });
            }
            return Created("", new
            {
                Status = "success",
                Message = "Organisation created successfully",
                Data = mapper.Map<OrganisationForReturn>(dataFromRepo)
            });
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetOrganisations()
        {
            var dataFromRepo = await organisationRepository.GetAll();
            return Ok(new
            {
                Status = "success",
                Message = "Returned data for all user organisations",
                Data = mapper.Map<ICollection<OrganisationForReturn>>(dataFromRepo)
            });
        }

        [Authorize]
        [HttpGet("{orgId}")]
        public async Task<IActionResult> GetOrganisations(string orgId)
        {
            var dataFromRepo = await organisationRepository.GetOrganisation(orgId);

            if (dataFromRepo == null)
            {
                return Unauthorized(new
                {
                    Status = "Error",
                    Message = "You are not authorised to access this resource or resource doesn't exist",
                    StatusCode = 401
                });
            }
            return Ok(new
            {
                Status = "success",
                Message = $"Returned data for {dataFromRepo.Name}",
                Data = mapper.Map<OrganisationForReturn>(dataFromRepo)
            });
        }

        [Authorize]
        [HttpPost("{orgId}/users")]
        public async Task<IActionResult> AddUserToOrganisation(string orgId, [FromBody] UserIdDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    status = "Bad Request",
                    message = "Client error",
                    statusCode = 400
                });
            }

            var dataFromRepo = await organisationRepository.AddUserToOrganisation(orgId, model);

            if (dataFromRepo == true)
            {
                return Ok(new
                {
                    Status = "Success",
                    Message = "User added to organisation successfully"
                });
            }
            return BadRequest(new
            {
                Status = "Error",
                Message = "Something went wrong whilst adding user"
            });
        }
    }
}
