using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookingManagementService.BusinessLogic.Services.IdentityServerServices
{
    public class IdentityServerConfigurationService
    {
        private readonly ConfigurationDbContext _configurationDbContext;

        public IdentityServerConfigurationService(ConfigurationDbContext configurationDbContext)
        {
            _configurationDbContext = configurationDbContext;
        }

        public async Task<IEnumerable<Client>> GetAllClientsAsync()
        {
            return await _configurationDbContext.Clients
                .Include(c => c.AllowedGrantTypes)
                .Include(c => c.AllowedScopes)
                .Include(c => c.RedirectUris)
                .Include(c => c.PostLogoutRedirectUris)
                .Include(c => c.AllowedCorsOrigins)
                .Include(c => c.ClientSecrets)
                .ToListAsync();
        }

        public async Task<Client> GetClientByIdAsync(int id)
        {
            return await _configurationDbContext.Clients
                .Include(c => c.AllowedGrantTypes)
                .Include(c => c.AllowedScopes)
                .Include(c => c.RedirectUris)
                .Include(c => c.PostLogoutRedirectUris)
                .Include(c => c.AllowedCorsOrigins)
                .Include(c => c.ClientSecrets)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Client> GetClientByClientIdAsync(string clientId)
        {
            return await _configurationDbContext.Clients
                .Include(c => c.AllowedGrantTypes)
                .Include(c => c.AllowedScopes)
                .Include(c => c.RedirectUris)
                .Include(c => c.PostLogoutRedirectUris)
                .Include(c => c.AllowedCorsOrigins)
                .Include(c => c.ClientSecrets)
                .FirstOrDefaultAsync(c => c.ClientId == clientId);
        }

        public async Task<Client> CreateClientAsync(Client client)
        {
            client.Created = DateTime.UtcNow;
            await _configurationDbContext.Clients.AddAsync(client);
            await _configurationDbContext.SaveChangesAsync();
            return client;
        }

        public async Task<Client> UpdateClientAsync(dynamic client)
        {
            int clientId = int.Parse(client.Id.ToString());
            var existingClient = await _configurationDbContext.Clients
                .Include(c => c.AllowedGrantTypes)
                .Include(c => c.AllowedScopes)
                .Include(c => c.RedirectUris)
                .Include(c => c.PostLogoutRedirectUris)
                .Include(c => c.AllowedCorsOrigins)
                .Include(c => c.ClientSecrets)
                .FirstOrDefaultAsync(c => c.Id == clientId);

            if (existingClient == null)
                throw new KeyNotFoundException($"Client with ID {client.Id} not found");

            // Update basic properties
            // existingClient.ClientId = client.ClientId;
            // existingClient.ClientName = client.ClientName;
            // existingClient.Description = client.Description;
            // existingClient.ClientUri = client.ClientUri;
            // existingClient.LogoUri = client.LogoUri;
            // existingClient.Enabled = client.Enabled;
            // existingClient.RequireClientSecret = client.RequireClientSecret;
            // existingClient.RequireConsent = client.RequireConsent;
            // existingClient.AllowRememberConsent = client.AllowRememberConsent;
            // existingClient.AlwaysIncludeUserClaimsInIdToken = client.AlwaysIncludeUserClaimsInIdToken;
            // existingClient.RequirePkce = client.RequirePkce;
            // existingClient.AllowPlainTextPkce = client.AllowPlainTextPkce;
            // existingClient.RequireRequestObject = client.RequireRequestObject;
            // existingClient.AllowAccessTokensViaBrowser = client.AllowAccessTokensViaBrowser;
            // existingClient.Updated = DateTime.UtcNow;
            existingClient.ClientId = client.ContainsKey("ClientId") ? client.ClientId : existingClient.ClientId;
            existingClient.ClientName = client.ContainsKey("ClientName") ? client.ClientName : existingClient.ClientName;
            existingClient.Description = client.ContainsKey("Description") ? client.Description : existingClient.Description;
            existingClient.ClientUri = client.ContainsKey("ClientUri") ? client.ClientUri : existingClient.ClientUri;
            existingClient.LogoUri = client.ContainsKey("LogoUri") ? client.LogoUri : existingClient.LogoUri;
            existingClient.Enabled = client.ContainsKey("Enabled") ? client.Enabled : existingClient.Enabled;
            existingClient.RequireClientSecret = client.ContainsKey("RequireClientSecret") ? client.RequireClientSecret : existingClient.RequireClientSecret;
            existingClient.RequireConsent = client.ContainsKey("RequireConsent") ? client.RequireConsent : existingClient.RequireConsent;
            existingClient.AllowRememberConsent = client.ContainsKey("AllowRememberConsent") ? client.AllowRememberConsent : existingClient.AllowRememberConsent;
            existingClient.AlwaysIncludeUserClaimsInIdToken = client.ContainsKey("AlwaysIncludeUserClaimsInIdToken") ? client.AlwaysIncludeUserClaimsInIdToken : existingClient.AlwaysIncludeUserClaimsInIdToken;
            existingClient.RequirePkce = client.ContainsKey("RequirePkce") ? client.RequirePkce : existingClient.RequirePkce;
            existingClient.AllowPlainTextPkce = client.ContainsKey("AllowPlainTextPkce") ? client.AllowPlainTextPkce : existingClient.AllowPlainTextPkce;
            existingClient.RequireRequestObject = client.ContainsKey("RequireRequestObject") ? client.RequireRequestObject : existingClient.RequireRequestObject;
            existingClient.AllowAccessTokensViaBrowser = client.ContainsKey("AllowAccessTokensViaBrowser") ? client.AllowAccessTokensViaBrowser : existingClient.AllowAccessTokensViaBrowser;
            existingClient.Claims = client.ContainsKey("Claims") ? client.Claims.ToObject<List<ClientClaim>>() : existingClient.Claims;
            existingClient.RedirectUris = client.ContainsKey("RedirectUris") ? client.RedirectUris : existingClient.RedirectUris;
            existingClient.PostLogoutRedirectUris = client.ContainsKey("PostLogoutRedirectUris") ? client.PostLogoutRedirectUris : existingClient.PostLogoutRedirectUris;
            existingClient.AllowedCorsOrigins = client.ContainsKey("AllowedCorsOrigins") ? client.AllowedCorsOrigins : existingClient.AllowedCorsOrigins;
            existingClient.ClientSecrets = client.ContainsKey("ClientSecrets") ? client.ClientSecrets.ToObject<List<ClientSecret>>() : existingClient.ClientSecrets;
            existingClient.IdentityTokenLifetime = client.ContainsKey("IdentityTokenLifetime") ? client.IdentityTokenLifetime : existingClient.IdentityTokenLifetime;
            existingClient.AccessTokenLifetime = client.ContainsKey("AccessTokenLifetime") ? client.AccessTokenLifetime : existingClient.AccessTokenLifetime;
            existingClient.AbsoluteRefreshTokenLifetime = client.ContainsKey("AbsoluteRefreshTokenLifetime") ? client.AbsoluteRefreshTokenLifetime : existingClient.AbsoluteRefreshTokenLifetime;
            existingClient.AllowOfflineAccess = client.ContainsKey("AllowOfflineAccess") ? client.AllowOfflineAccess : existingClient.AllowOfflineAccess;
            existingClient.RefreshTokenUsage = client.ContainsKey("RefreshTokenUsage") ? client.RefreshTokenUsage : existingClient.RefreshTokenUsage;

            // Update related collections dynamically
            if (client.ContainsKey("AllowedGrantTypes"))
            {
                _configurationDbContext.Set<ClientGrantType>().RemoveRange(existingClient.AllowedGrantTypes);
                existingClient.AllowedGrantTypes = client.AllowedGrantTypes.ToObject<List<ClientGrantType>>();
            }

            if (client.ContainsKey("AllowedScopes"))
            {
                _configurationDbContext.Set<ClientScope>().RemoveRange(existingClient.AllowedScopes);
                existingClient.AllowedScopes = client.AllowedScopes;
            }

            if (client.ContainsKey("RedirectUris"))
            {
                _configurationDbContext.Set<ClientRedirectUri>().RemoveRange(existingClient.RedirectUris);
                existingClient.RedirectUris = client.RedirectUris;
            }

            if (client.ContainsKey("PostLogoutRedirectUris"))
            {
                _configurationDbContext.Set<ClientPostLogoutRedirectUri>().RemoveRange(existingClient.PostLogoutRedirectUris);
                existingClient.PostLogoutRedirectUris = client.PostLogoutRedirectUris;
            }

            if (client.ContainsKey("AllowedCorsOrigins"))
            {
                _configurationDbContext.Set<ClientCorsOrigin>().RemoveRange(existingClient.AllowedCorsOrigins);
                existingClient.AllowedCorsOrigins = client.AllowedCorsOrigins;
            }

            if (client.ContainsKey("ClientSecrets"))
            {
                _configurationDbContext.Set<ClientSecret>().RemoveRange(existingClient.ClientSecrets);
                existingClient.ClientSecrets = client.ClientSecrets;
            }

            await _configurationDbContext.SaveChangesAsync();
            return existingClient;
        }

        public async Task DeleteClientAsync(int id)
        {
            var client = await _configurationDbContext.Clients.FindAsync(id);
            if (client == null)
                throw new KeyNotFoundException($"Client with ID {id} not found");

            _configurationDbContext.Clients.Remove(client);
            await _configurationDbContext.SaveChangesAsync();
        }

        public async Task<bool> ClientExistsAsync(string clientId)
        {
            return await _configurationDbContext.Clients.AnyAsync(c => c.ClientId == clientId);
        }
    }
}
