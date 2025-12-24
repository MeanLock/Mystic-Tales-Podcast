using Microsoft.Extensions.Logging;
using BookingManagementService.BusinessLogic.DTOs.Cache.ListesnSessionProcedure;
using BookingManagementService.Infrastructure.Services.Redis;

namespace BookingManagementService.BusinessLogic.Services.DbServices.CachingServices
{
    public class CustomerListenSessionProcedureCachingService
    {
        private readonly RedisSharedCacheService _redisSharedCacheService;
        private readonly ILogger<CustomerListenSessionProcedureCachingService> _logger;

        public CustomerListenSessionProcedureCachingService(
            RedisSharedCacheService redisSharedCacheService,
            ILogger<CustomerListenSessionProcedureCachingService> logger)
        {
            _redisSharedCacheService = redisSharedCacheService;
            _logger = logger;
        }

        #region Private Helpers

        /// <summary>
        /// Build Redis key for a specific procedure
        /// Format: account:customer_listen_session:{customerId}:procedure:{procedureId}
        /// </summary>
        private string BuildProcedureKey(int customerId, Guid procedureId)
        {
            return $"account:customer_listen_session:{customerId}:procedure:{procedureId}";
        }

        /// <summary>
        /// Build Redis key prefix for all procedures of a customer
        /// Format: account:customer_listen_session:{customerId}:procedure:
        /// </summary>
        private string BuildCustomerProcedurePrefix(int customerId)
        {
            return $"account:customer_listen_session:{customerId}:procedure:";
        }

        #endregion

        #region Read Operations

        /// <summary>
        /// Get all procedures (active and completed) for a specific customer
        /// </summary>
        /// <param name="customerId">Customer ID</param>
        /// <returns>Dictionary with procedureId as key and procedure data as value. Empty dictionary if none found.</returns>
        public async Task<Dictionary<Guid, CustomerListenSessionProcedure>> GetAllProceduresByCustomerIdAsync(int customerId)
        {
            try
            {
                var prefix = BuildCustomerProcedurePrefix(customerId);
                var allProcedures = await _redisSharedCacheService.GetStringKeyValuesByPrefixAsync<CustomerListenSessionProcedure>(prefix);

                // Parse procedureId from keys and convert to Guid-keyed dictionary
                var result = new Dictionary<Guid, CustomerListenSessionProcedure>();
                foreach (var kvp in allProcedures)
                {
                    // Extract procedureId from key pattern: account:customer_listen_session:{customerId}:procedure:{procedureId}
                    var parts = kvp.Key.Split(':');
                    if (parts.Length >= 4 && Guid.TryParse(parts[^1], out var procedureId))
                    {
                        result[procedureId] = kvp.Value;
                    }
                }

                _logger.LogInformation($"Retrieved {result.Count} procedures for customer {customerId}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting all procedures for customer {customerId}");
                return new Dictionary<Guid, CustomerListenSessionProcedure>();
            }
        }

        /// <summary>
        /// Get the active (IsCompleted=false) procedure for a customer
        /// Business rule: Only ONE active procedure is allowed per customer
        /// </summary>
        /// <param name="customerId">Customer ID</param>
        /// <returns>Active procedure if found, null if none exists</returns>
        /// <exception cref="InvalidOperationException">Thrown when multiple active procedures are found (data integrity violation)</exception>
        public async Task<CustomerListenSessionProcedure?> GetActiveProcedureByCustomerIdAsync(int customerId)
        {
            try
            {
                var allProcedures = await GetAllProceduresByCustomerIdAsync(customerId);
                var activeProcedures = allProcedures.Values.Where(p => !p.IsCompleted).ToList();

                if (activeProcedures.Count > 1)
                {
                    _logger.LogError($"Data integrity violation: Customer {customerId} has {activeProcedures.Count} active procedures");
                    throw new InvalidOperationException($"Customer {customerId} has multiple active procedures. Only one is allowed.");
                }

                var activeProcedure = activeProcedures.FirstOrDefault();
                if (activeProcedure != null)
                {
                    _logger.LogInformation($"Found active procedure {activeProcedure.Id} for customer {customerId}");
                }
                else
                {
                    _logger.LogInformation($"No active procedure found for customer {customerId}");
                }

                return activeProcedure;
            }
            catch (Exception ex) when (ex is not InvalidOperationException)
            {
                _logger.LogError(ex, $"Error getting active procedure for customer {customerId}");
                return null;
            }
        }

        /// <summary>
        /// Get a specific procedure by customerId and procedureId
        /// </summary>
        /// <param name="customerId">Customer ID</param>
        /// <param name="procedureId">Procedure ID</param>
        /// <returns>Procedure data if found, null otherwise</returns>
        public async Task<CustomerListenSessionProcedure?> GetProcedureAsync(int customerId, Guid procedureId)
        {
            try
            {
                var key = BuildProcedureKey(customerId, procedureId);
                var procedure = await _redisSharedCacheService.KeyGetAsync<CustomerListenSessionProcedure>(key);
                
                if (procedure != null)
                {
                    _logger.LogInformation($"Retrieved procedure {procedureId} for customer {customerId}");
                }
                else
                {
                    _logger.LogInformation($"Procedure {procedureId} not found for customer {customerId}");
                }

                return procedure;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting procedure {procedureId} for customer {customerId}");
                return null;
            }
        }

        #endregion

        #region Write Operations

        /// <summary>
        /// Create a new procedure in Redis cache (persistent, no expiry)
        /// </summary>
        /// <param name="customerId">Customer ID</param>
        /// <param name="procedureId">Procedure ID</param>
        /// <param name="procedure">Procedure data to cache</param>
        /// <returns>True if created successfully, false otherwise</returns>
        public async Task<bool> CreateProcedureAsync(int customerId, Guid procedureId, CustomerListenSessionProcedure procedure)
        {
            try
            {
                var key = BuildProcedureKey(customerId, procedureId);
                var result = await _redisSharedCacheService.KeySetWithoutExpiryAsync(key, procedure);
                
                if (result)
                {
                    _logger.LogInformation($"Created procedure {procedureId} for customer {customerId}");
                }
                else
                {
                    _logger.LogWarning($"Failed to create procedure {procedureId} for customer {customerId}");
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating procedure {procedureId} for customer {customerId}");
                return false;
            }
        }

        /// <summary>
        /// Update an existing procedure in Redis cache
        /// </summary>
        /// <param name="customerId">Customer ID</param>
        /// <param name="procedureId">Procedure ID</param>
        /// <param name="procedure">Updated procedure data</param>
        /// <returns>True if updated successfully, false otherwise</returns>
        public async Task<bool> UpdateProcedureAsync(int customerId, Guid procedureId, CustomerListenSessionProcedure procedure)
        {
            try
            {
                var key = BuildProcedureKey(customerId, procedureId);
                var result = await _redisSharedCacheService.KeySetWithoutExpiryAsync(key, procedure);
                
                if (result)
                {
                    _logger.LogInformation($"Updated procedure {procedureId} for customer {customerId}");
                }
                else
                {
                    _logger.LogWarning($"Failed to update procedure {procedureId} for customer {customerId}");
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating procedure {procedureId} for customer {customerId}");
                return false;
            }
        }

        /// <summary>
        /// Mark a specific procedure as completed or active by updating IsCompleted flag
        /// </summary>
        /// <param name="customerId">Customer ID</param>
        /// <param name="procedureId">Procedure ID</param>
        /// <param name="isCompleted">True to mark as completed, false to mark as active</param>
        /// <returns>True if updated successfully, false if procedure not found or update failed</returns>
        public async Task<bool> MarkProcedureCompletedAsync(int customerId, Guid procedureId, bool isCompleted)
        {
            try
            {
                var procedure = await GetProcedureAsync(customerId, procedureId);
                if (procedure == null)
                {
                    _logger.LogWarning($"Cannot mark procedure {procedureId} as completed: not found for customer {customerId}");
                    return false;
                }

                procedure.IsCompleted = isCompleted;
                var result = await UpdateProcedureAsync(customerId, procedureId, procedure);
                
                if (result)
                {
                    _logger.LogInformation($"Marked procedure {procedureId} as {(isCompleted ? "completed" : "active")} for customer {customerId}");
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error marking procedure {procedureId} as completed for customer {customerId}");
                return false;
            }
        }

        /// <summary>
        /// Mark all procedures of a customer as completed (IsCompleted=true)
        /// </summary>
        /// <param name="customerId">Customer ID</param>
        /// <returns>Number of procedures marked as completed</returns>
        public async Task<int> MarkAllProceduresCompletedAsync(int customerId)
        {
            try
            {
                var allProcedures = await GetAllProceduresByCustomerIdAsync(customerId);
                var count = 0;

                foreach (var kvp in allProcedures)
                {
                    if (!kvp.Value.IsCompleted)
                    {
                        kvp.Value.IsCompleted = true;
                        var result = await UpdateProcedureAsync(customerId, kvp.Key, kvp.Value);
                        if (result)
                        {
                            count++;
                        }
                    }
                }

                _logger.LogInformation($"Marked {count} procedures as completed for customer {customerId}");
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error marking all procedures as completed for customer {customerId}");
                return 0;
            }
        }

        #endregion

        #region Delete Operations

        /// <summary>
        /// Delete a specific procedure from Redis cache
        /// </summary>
        /// <param name="customerId">Customer ID</param>
        /// <param name="procedureId">Procedure ID</param>
        /// <returns>True if deleted successfully, false otherwise</returns>
        public async Task<bool> DeleteProcedureAsync(int customerId, Guid procedureId)
        {
            try
            {
                var key = BuildProcedureKey(customerId, procedureId);
                var result = await _redisSharedCacheService.KeyDeleteAsync(key);
                
                if (result)
                {
                    _logger.LogInformation($"Deleted procedure {procedureId} for customer {customerId}");
                }
                else
                {
                    _logger.LogWarning($"Failed to delete procedure {procedureId} for customer {customerId} (may not exist)");
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting procedure {procedureId} for customer {customerId}");
                return false;
            }
        }

        /// <summary>
        /// Delete all procedures (active and completed) for a specific customer
        /// </summary>
        /// <param name="customerId">Customer ID</param>
        /// <returns>Number of procedures deleted</returns>
        public async Task<int> DeleteAllProceduresAsync(int customerId)
        {
            try
            {
                var allProcedures = await GetAllProceduresByCustomerIdAsync(customerId);
                var count = 0;

                foreach (var procedureId in allProcedures.Keys)
                {
                    var result = await DeleteProcedureAsync(customerId, procedureId);
                    if (result)
                    {
                        count++;
                    }
                }

                _logger.LogInformation($"Deleted {count} procedures for customer {customerId}");
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting all procedures for customer {customerId}");
                return 0;
            }
        }

        #endregion
    }
}
