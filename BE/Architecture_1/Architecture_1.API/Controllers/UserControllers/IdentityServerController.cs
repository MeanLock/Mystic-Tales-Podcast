using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Architecture_1.API.Filters.ExceptionFilters;
using Architecture_1.BusinessLogic.Services.IdentityServerServices;
using Duende.IdentityServer.EntityFramework.Entities;

namespace Architecture_1.API.Controllers.UserControllers
{
    [Route("api/User/clients")]
    [ApiController]
    [TypeFilter(typeof(HttpExceptionFilter))]
    public class IdentityServerController : ControllerBase
    {
        private readonly IdentityServerConfigurationService _identityServerConfigurationService;

        public IdentityServerController(IdentityServerConfigurationService identityServerConfigurationService)
        {
            _identityServerConfigurationService = identityServerConfigurationService;
        }

        // Get all clients
        [HttpGet]
        public async Task<IActionResult> GetAllClients()
        {
            var clients = await _identityServerConfigurationService.GetAllClientsAsync();
            return Ok(clients);
        }

        // Get client by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetClientById(int id)
        {
            var client = await _identityServerConfigurationService.GetClientByIdAsync(id);
            if (client == null)
            {
                return NotFound();
            }
            return Ok(client);
        }

        // Create a new client
        [HttpPost]
        public async Task<IActionResult> CreateClient([FromBody] Client client)
        {
            if (client == null)
            {
                return BadRequest("Client data is required.");
            }

            var createdClient = await _identityServerConfigurationService.CreateClientAsync(client);
            return CreatedAtAction(nameof(GetClientById), new { id = createdClient.Id }, createdClient);
        }

        // Update an existing client
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateClient(int id, [FromBody] JToken updatedClient)
        {
            dynamic updatedClientDynamic = updatedClient.ToObject<dynamic>();
            updatedClientDynamic.Id = id;
            if (updatedClientDynamic == null)
            {

                return BadRequest("Invalid client data.");
            }


            try
            {
                var updated = await _identityServerConfigurationService.UpdateClientAsync(updatedClientDynamic);
                return Ok(updated);
            }

            catch (Exception ex)
            {
                Console.WriteLine("\n\n\nError updating client: " + ex.ToString());
                return StatusCode(500, "Internal server error: " + ex.Message);
            }

        }

        // Delete a client
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClient(int id)
        {
            try
            {
                await _identityServerConfigurationService.DeleteClientAsync(id);
                return NoContent();
            }
            catch (System.Collections.Generic.KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}

