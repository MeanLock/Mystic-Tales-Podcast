using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using SystemConfigurationService.Common.AppConfigurations.App.interfaces;
using SystemConfigurationService.Common.AppConfigurations.FilePath.interfaces;
using SystemConfigurationService.DataAccess.Data;
using SystemConfigurationService.DataAccess.UOW;
using SystemConfigurationService.DataAccess.Repositories.interfaces;
using SystemConfigurationService.DataAccess.Entities;
using SystemConfigurationService.BusinessLogic.Helpers.AuthHelpers;
using SystemConfigurationService.BusinessLogic.Helpers.FileHelpers;
using SystemConfigurationService.BusinessLogic.Services.CrossServiceServices;
using SystemConfigurationService.BusinessLogic.Models.CrossService;
using System.Linq.Expressions;
using System.Reflection;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace SystemConfigurationService.BusinessLogic.Services.CrossServiceServices.QueryServices
{
    public class GenericQueryService
    {
        private readonly AppDbContext _appDbContext;
        private readonly IServiceProvider _serviceProvider;
        private readonly FieldSelector _fieldSelector;

        public GenericQueryService(
            AppDbContext context,
            IServiceProvider serviceProvider,
            FieldSelector fieldSelector)
        {
            _appDbContext = context;
            _serviceProvider = serviceProvider;
            _fieldSelector = fieldSelector;
        }

        public async Task<JToken> HandleQueryAsync(BatchQueryItem query)
        {
            // Get entity type by name
            var entityType = GetEntityTypeByName(query.EntityType);
            if (entityType == null)
                throw new NotSupportedException($"Entity type '{query.EntityType}' not found");

            // Use reflection to call generic method
            var method = GetType().GetMethod(nameof(HandleTypedQueryAsync), BindingFlags.NonPublic | BindingFlags.Instance);
            var genericMethod = method.MakeGenericMethod(entityType);

            // var task = (Task<object>)genericMethod.Invoke(this, new object[] { query });
            var task = (Task<JToken>)genericMethod.Invoke(this, new object[] { query });
            return await task;
        }

        private async Task<JToken> HandleTypedQueryAsync<T>(BatchQueryItem query) where T : class
        {
            var repo = _serviceProvider.GetRequiredService<IGenericRepository<T>>();

            return query.QueryType.ToLower() switch
            {
                "findbyid" => await HandleFindByIdAsync(repo, query),
                "findall" => await HandleFindAllAsync(repo, query),
                "search" => await HandleSearchAsync<T>(repo, query),
                "count" => await HandleCountAsync(repo, query),
                _ => throw new NotSupportedException($"Query type '{query.QueryType}' not supported")
            };
        }

        private async Task<JToken> HandleFindByIdAsync<T>(IGenericRepository<T> repo, BatchQueryItem query) where T : class
        {
            // Extract 'id' parameter from JObject
            var idToken = query.Parameters["id"];
            if (idToken == null)
                throw new ArgumentException("'id' parameter required");

            // Convert JToken to appropriate object type
            var idObj = idToken.Type switch
            {
                JTokenType.String => idToken.Value<string>(),
                JTokenType.Integer => idToken.Value<int>(),
                JTokenType.Guid => idToken.Value<Guid>(),
                JTokenType.Float => idToken.Value<decimal>(),
                _ => idToken.ToObject<object>()
            };

            var includePaths = BuildDynamicIncludes<T>(query.Parameters);

            var entity = await repo.FindByIdWithPaths(idObj, includePaths);
            if (entity == null)
                return JObject.FromObject(new { }); // Empty JObject for not found

            var selectedData = ApplyFieldSelection(entity, query.Fields);
            return ConvertToJson(selectedData);
        }

        private async Task<JToken> HandleFindAllAsync<T>(IGenericRepository<T> repo, BatchQueryItem query) where T : class
        {
            var predicate = BuildDynamicPredicate<T>(query.Parameters);

            var includePaths = BuildDynamicIncludes<T>(query.Parameters);

            var queryable = repo.FindAllWithPaths(predicate, includePaths);

            queryable = ApplyPagination(queryable, query.Parameters);

            queryable = ApplyOrdering<T>(queryable, query.Parameters);

            var entities = await queryable.ToListAsync();

            var result = query.Fields.Any()
                ? entities.Select(e => ApplyFieldSelection(e, query.Fields))
                : entities;

            return ConvertToJson(result);
        }

        private async Task<JToken> HandleSearchAsync<T>(IGenericRepository<T> repo, BatchQueryItem query) where T : class
        {
            // Extract 'searchTerm' parameter from JObject
            var searchTermToken = query.Parameters["searchTerm"];
            var searchTerm = searchTermToken?.Type switch
            {
                JTokenType.String => searchTermToken.Value<string>(),
                JTokenType.Null => string.Empty,
                null => string.Empty,
                _ => searchTermToken.ToString()
            } ?? "";

            var searchFields = GetSearchFields<T>(query.Parameters);

            if (string.IsNullOrEmpty(searchTerm) || !searchFields.Any())
                return JArray.FromObject(new List<object>());

            // Build search predicate dynamically
            var searchPredicate = BuildDynamicSearchPredicate<T>(searchTerm, searchFields);

            var includePaths = BuildDynamicIncludes<T>(query.Parameters);
            var queryable = repo.FindAllWithPaths(searchPredicate, includePaths);

            // Apply pagination
            queryable = ApplyPagination(queryable, query.Parameters);

            var entities = await queryable.ToListAsync();

            var result = query.Fields.Any()
                ? entities.Select(e => ApplyFieldSelection(e, query.Fields))
                : entities;

            return ConvertToJson(result);
        }

        private async Task<JToken> HandleCountAsync<T>(IGenericRepository<T> repo, BatchQueryItem query) where T : class
        {
            var predicate = BuildDynamicPredicate<T>(query.Parameters);
            var count = await repo.FindAll(predicate).CountAsync();
            return JObject.FromObject(new { count });
        }

        private Expression<Func<T, bool>>? BuildDynamicPredicate<T>(JObject parameters) where T : class
        {
            Console.WriteLine($"[BuildDynamicPredicate] Starting for entity type: {typeof(T).Name}");

            // Lấy where object (nếu có) từ JObject
            JObject whereObj = null;
            var whereToken = parameters["where"];

            if (whereToken != null && whereToken.Type == JTokenType.Object)
            {
                Console.WriteLine("[BuildDynamicPredicate] Found 'where' object in parameters");
                whereObj = (JObject)whereToken;
            }
            else
            {
                Console.WriteLine("[BuildDynamicPredicate] No 'where' object found, using all non-special parameters");
                whereObj = new JObject();
                foreach (var prop in parameters.Properties())
                {
                    if (!IsSpecialParameter(prop.Name))
                    {
                        Console.WriteLine($"[BuildDynamicPredicate] Adding filter for property: {prop.Name}, Value: {prop.Value}");
                        whereObj[prop.Name] = prop.Value;
                    }
                }
            }

            if (!whereObj.HasValues)
            {
                Console.WriteLine("[BuildDynamicPredicate] No filter conditions found, returning null predicate");
                return null;
            }

            Console.WriteLine($"[BuildDynamicPredicate] Processing {whereObj.Count} filter conditions");

            var entityType = typeof(T);
            var parameter = Expression.Parameter(entityType, "x");
            Expression? combinedExpression = null;

            foreach (var prop in whereObj.Properties())
            {
                Console.WriteLine($"\n[BuildDynamicPredicate] ===== Processing Property: {prop.Name} =====");
                Console.WriteLine($"[BuildDynamicPredicate] JToken Type: {prop.Value.Type}");
                Console.WriteLine($"[BuildDynamicPredicate] Raw Value: {prop.Value}");

                try
                {
                    // Hỗ trợ property lồng nhau: "Role.Name", "SurveyTakenResults.Status"
                    var propertyPath = prop.Name.Split('.');
                    Expression propertyExpression = parameter;
                    Type currentType = entityType;

                    Console.WriteLine($"[BuildDynamicPredicate] Property path parts: [{string.Join(", ", propertyPath)}]");

                    // Duyệt qua từng phần của property path
                    foreach (var propName in propertyPath)
                    {
                        Console.WriteLine($"[BuildDynamicPredicate] Looking for property '{propName}' in type '{currentType.Name}'");

                        var property = currentType.GetProperty(propName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                        if (property == null)
                        {
                            Console.WriteLine($"[BuildDynamicPredicate] ❌ Property '{propName}' not found in type '{currentType.Name}'");
                            propertyExpression = null;
                            break;
                        }

                        Console.WriteLine($"[BuildDynamicPredicate] ✅ Found property '{propName}' with type '{property.PropertyType.Name}'");
                        propertyExpression = Expression.Property(propertyExpression, property);
                        currentType = property.PropertyType;
                    }

                    if (propertyExpression == null)
                    {
                        Console.WriteLine($"[BuildDynamicPredicate] ⚠️ Skipping property '{prop.Name}' - property path not found");
                        continue;
                    }

                    // Convert JToken value to appropriate object type
                    object paramValue = ConvertJTokenToValueEnhanced(prop.Value, currentType);

                    Console.WriteLine($"[BuildDynamicPredicate] Conversion result:");
                    Console.WriteLine($"  - Original: {prop.Value}");
                    Console.WriteLine($"  - Converted: {paramValue ?? "NULL"}");
                    Console.WriteLine($"  - Target Type: {currentType.Name}");
                    Console.WriteLine($"  - Is Nullable: {IsNullableType(currentType)}");

                    // Tạo điều kiện expression
                    Expression conditionExpression = null;

                    // ✅ THAY ĐỔI: Kiểm tra null values với logic mới
                    if (paramValue == null || prop.Value.Type == JTokenType.Null)
                    {
                        Console.WriteLine($"[BuildDynamicPredicate] 🔍 Creating NULL condition for property: {prop.Name}");

                        // ✅ Kiểm tra nếu cột không allow null
                        if (!IsNullableType(currentType) && currentType.IsValueType)
                        {
                            Console.WriteLine($"[BuildDynamicPredicate] ⚠️ Property '{prop.Name}' does not allow null, IGNORING this condition (treating as always true)");
                            // SKIP điều kiện này hoàn toàn - không thêm vào combined expression
                            continue;
                        }
                        else
                        {
                            // Đối với nullable types, so sánh trực tiếp với null
                            conditionExpression = Expression.Equal(propertyExpression, Expression.Constant(null, currentType));
                            Console.WriteLine($"[BuildDynamicPredicate] ✅ Created IS NULL condition");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"[BuildDynamicPredicate] 🔍 Creating equality condition for property: {prop.Name}, Value: {paramValue}");

                        // Xử lý collection properties (Any condition)
                        if (typeof(System.Collections.IEnumerable).IsAssignableFrom(currentType) && currentType != typeof(string))
                        {
                            Console.WriteLine($"[BuildDynamicPredicate] 📋 Handling collection property");

                            var elementType = currentType.IsGenericType
                                ? currentType.GetGenericArguments()[0]
                                : currentType.GetElementType();

                            if (elementType != null && propertyPath.Length > 1)
                            {
                                var innerParam = Expression.Parameter(elementType, "e");
                                var lastPropertyName = propertyPath[^1];
                                var innerProp = elementType.GetProperty(lastPropertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                                if (innerProp == null)
                                {
                                    Console.WriteLine($"[BuildDynamicPredicate] ❌ Inner property '{lastPropertyName}' not found in collection element type '{elementType.Name}'");
                                    continue;
                                }

                                var innerPropExpr = Expression.Property(innerParam, innerProp);
                                var valueExpr = CreateValueExpressionEnhanced(innerProp.PropertyType, paramValue);
                                var equalsExpr = Expression.Equal(innerPropExpr, valueExpr);

                                var anyLambda = Expression.Lambda(equalsExpr, innerParam);
                                var anyMethod = typeof(Enumerable).GetMethods()
                                    .First(m => m.Name == "Any" && m.GetParameters().Length == 2)
                                    .MakeGenericMethod(elementType);

                                conditionExpression = Expression.Call(anyMethod, propertyExpression, anyLambda);
                                Console.WriteLine($"[BuildDynamicPredicate] ✅ Created Any() condition for collection");
                            }
                        }
                        else
                        {
                            // Reference hoặc value type - equality condition
                            var valueExpression = CreateValueExpressionEnhanced(currentType, paramValue);
                            conditionExpression = Expression.Equal(propertyExpression, valueExpression);
                            Console.WriteLine($"[BuildDynamicPredicate] ✅ Created equality condition");
                        }
                    }

                    // Kết hợp với expression tổng thể
                    if (conditionExpression != null)
                    {
                        combinedExpression = combinedExpression == null
                            ? conditionExpression
                            : Expression.AndAlso(combinedExpression, conditionExpression);

                        Console.WriteLine($"[BuildDynamicPredicate] ✅ Added condition to combined expression");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[BuildDynamicPredicate] ❌ Exception processing property '{prop.Name}': {ex.Message}");
                    Console.WriteLine($"[BuildDynamicPredicate] Stack trace: {ex.StackTrace}");
                    continue;
                }
            }

            var result = combinedExpression != null
                ? Expression.Lambda<Func<T, bool>>(combinedExpression, parameter)
                : null;

            Console.WriteLine($"[BuildDynamicPredicate] Final result: {(result != null ? "Predicate created" : "No predicate (null)")}");

            if (result != null)
            {
                Console.WriteLine($"[BuildDynamicPredicate] Generated expression: {result}");
            }

            return result;
        }

        private bool IsNullableType(Type type)
        {
            return !type.IsValueType || (Nullable.GetUnderlyingType(type) != null);
        }

        private object ConvertJTokenToValueEnhanced(JToken token, Type targetType)
        {
            Console.WriteLine($"[ConvertJTokenToValueEnhanced] Converting JToken type '{token.Type}' to target type '{targetType.Name}'");

            try
            {
                // Handle null values
                if (token.Type == JTokenType.Null)
                {
                    Console.WriteLine($"[ConvertJTokenToValueEnhanced] Input is null");
                    return null;
                }

                var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;
                Console.WriteLine($"[ConvertJTokenToValueEnhanced] Underlying type: {underlyingType.Name}");

                var result = underlyingType.Name switch
                {
                    nameof(String) => token.Value<string>(),
                    nameof(Int32) => token.Value<int>(),
                    nameof(Int64) => token.Value<long>(),
                    nameof(Int16) => token.Value<short>(),
                    nameof(Byte) => token.Value<byte>(),
                    nameof(Boolean) => token.Value<bool>(),
                    nameof(Decimal) => token.Value<decimal>(),
                    nameof(Double) => token.Value<double>(),
                    nameof(Single) => token.Value<float>(),
                    nameof(DateTime) => ConvertToDateTime(token),
                    nameof(DateTimeOffset) => ConvertToDateTimeOffset(token),
                    nameof(Guid) => ConvertToGuid(token),
                    _ => token.ToObject(underlyingType)
                };

                Console.WriteLine($"[ConvertJTokenToValueEnhanced] Conversion successful: {result}");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ConvertJTokenToValueEnhanced] ❌ Conversion failed: {ex.Message}");
                return null;
            }
        }

        private Guid? ConvertToGuid(JToken token)
        {
            Console.WriteLine($"[ConvertToGuid] Converting token '{token}' to Guid");

            try
            {
                return token.Type switch
                {
                    JTokenType.String => Guid.TryParse(token.Value<string>(), out var guid) ? guid : null,
                    JTokenType.Guid => token.Value<Guid>(),
                    _ => null
                };
            }
            catch
            {
                Console.WriteLine($"[ConvertToGuid] ❌ Failed to convert to Guid");
                return null;
            }
        }

        private DateTime? ConvertToDateTime(JToken token)
        {
            Console.WriteLine($"[ConvertToDateTime] Converting token '{token}' to DateTime");

            try
            {
                return token.Type switch
                {
                    JTokenType.String => DateTime.TryParse(token.Value<string>(), out var dt) ? dt : null,
                    JTokenType.Date => token.Value<DateTime>(),
                    JTokenType.Null => null,
                    _ => null
                };
            }
            catch
            {
                Console.WriteLine($"[ConvertToDateTime] ❌ Failed to convert to DateTime");
                return null;
            }
        }

        private DateTimeOffset? ConvertToDateTimeOffset(JToken token)
        {
            Console.WriteLine($"[ConvertToDateTimeOffset] Converting token '{token}' to DateTimeOffset");

            try
            {
                return token.Type switch
                {
                    JTokenType.String => DateTimeOffset.TryParse(token.Value<string>(), out var dto) ? dto : null,
                    JTokenType.Date => token.Value<DateTimeOffset>(),
                    JTokenType.Null => null,
                    _ => null
                };
            }
            catch
            {
                Console.WriteLine($"[ConvertToDateTimeOffset] ❌ Failed to convert to DateTimeOffset");
                return null;
            }
        }

        private Expression CreateValueExpressionEnhanced(Type targetType, object value)
        {
            Console.WriteLine($"[CreateValueExpressionEnhanced] Creating expression for type '{targetType.Name}' with value '{value}'");

            try
            {
                // Handle null values
                if (value == null)
                {
                    return Expression.Constant(null, targetType);
                }

                // Handle nullable types
                var underlyingType = Nullable.GetUnderlyingType(targetType);
                if (underlyingType != null)
                {
                    // Convert to underlying type first, then wrap in nullable
                    var convertedValue = Convert.ChangeType(value, underlyingType);
                    return Expression.Constant(convertedValue, targetType);
                }

                // Handle regular types
                var finalValue = Convert.ChangeType(value, targetType);
                return Expression.Constant(finalValue, targetType);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CreateValueExpressionEnhanced] ❌ Failed to create expression: {ex.Message}");
                throw;
            }
        }


        private Expression<Func<T, bool>>? BuildDynamicSearchPredicate<T>(string searchTerm, string[] searchFields) where T : class
        {
            Console.WriteLine($"[BuildDynamicSearchPredicate] Building search predicate for term: '{searchTerm}'");
            Console.WriteLine($"[BuildDynamicSearchPredicate] Search fields: [{string.Join(", ", searchFields)}]");

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                Console.WriteLine("[BuildDynamicSearchPredicate] Search term is empty");
                return null;
            }

            if (!searchFields.Any())
            {
                Console.WriteLine("[BuildDynamicSearchPredicate] No search fields provided");
                return null;
            }

            // ✅ Sanitize search term để tránh injection
            var sanitizedSearchTerm = SanitizeSearchTerm(searchTerm);
            Console.WriteLine($"[BuildDynamicSearchPredicate] Sanitized search term: '{sanitizedSearchTerm}'");

            var entityType = typeof(T);
            var parameter = Expression.Parameter(entityType, "x");
            var searchConstant = Expression.Constant(sanitizedSearchTerm);
            var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });

            Expression? combinedExpression = null;
            int validFieldsProcessed = 0;

            foreach (var fieldName in searchFields)
            {
                Console.WriteLine($"[BuildDynamicSearchPredicate] Processing field: {fieldName}");

                // ✅ Double-check field validity
                if (!IsValidSearchField<T>(fieldName))
                {
                    Console.WriteLine($"[BuildDynamicSearchPredicate] ⚠️ Skipping invalid field: {fieldName}");
                    continue;
                }

                var property = entityType.GetProperty(fieldName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (property == null)
                {
                    Console.WriteLine($"[BuildDynamicSearchPredicate] ⚠️ Property not found: {fieldName}");
                    continue;
                }

                try
                {
                    var propertyExpression = Expression.Property(parameter, property);

                    // ✅ Add null check for string properties
                    var notNullCheck = Expression.NotEqual(propertyExpression, Expression.Constant(null, typeof(string)));
                    var containsExpression = Expression.Call(propertyExpression, containsMethod, searchConstant);
                    var safeContainsExpression = Expression.AndAlso(notNullCheck, containsExpression);

                    combinedExpression = combinedExpression == null
                        ? safeContainsExpression
                        : Expression.OrElse(combinedExpression, safeContainsExpression);

                    validFieldsProcessed++;
                    Console.WriteLine($"[BuildDynamicSearchPredicate] ✅ Added search condition for field: {fieldName}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[BuildDynamicSearchPredicate] ❌ Exception processing field '{fieldName}': {ex.Message}");
                    continue;
                }
            }

            Console.WriteLine($"[BuildDynamicSearchPredicate] Successfully processed {validFieldsProcessed} fields");

            return combinedExpression != null
                ? Expression.Lambda<Func<T, bool>>(combinedExpression, parameter)
                : null;
        }

        private string SanitizeSearchTerm(string searchTerm)
        {
            Console.WriteLine($"[SanitizeSearchTerm] Sanitizing search term: '{searchTerm}'");

            if (string.IsNullOrEmpty(searchTerm)) return searchTerm;

            // ✅ Remove hoặc escape các ký tự đặc biệt có thể gây injection
            var sanitized = searchTerm
                .Replace("%", "\\%")     // Escape SQL LIKE wildcard
                .Replace("_", "\\_")     // Escape SQL LIKE single char wildcard
                .Replace("'", "''")      // Escape single quotes
                .Replace("--", "")       // Remove SQL comment markers
                .Replace(";", "")        // Remove statement terminators
                .Replace("/*", "")       // Remove SQL comment start
                .Replace("*/", "")       // Remove SQL comment end
                .Replace("\0", "");      // Remove null bytes

            // ✅ Limit length để tránh DoS
            const int MAX_SEARCH_TERM_LENGTH = 100;
            if (sanitized.Length > MAX_SEARCH_TERM_LENGTH)
            {
                sanitized = sanitized.Substring(0, MAX_SEARCH_TERM_LENGTH);
                Console.WriteLine($"[SanitizeSearchTerm] Search term truncated to {MAX_SEARCH_TERM_LENGTH} characters");
            }

            Console.WriteLine($"[SanitizeSearchTerm] Result: '{sanitized}'");
            return sanitized;
        }

        private string[] BuildDynamicIncludes<T>(JObject parameters) where T : class
        {
            var includes = new List<string>();

            // Extract 'include' parameter from JObject
            var includeToken = parameters["include"];
            if (includeToken == null)
                return includes.ToArray();

            // Handle different JToken types for include parameter
            string[] includeProperties = includeToken.Type switch
            {
                JTokenType.String => includeToken.Value<string>()?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>(),
                JTokenType.Array => includeToken.ToObject<string[]>() ?? Array.Empty<string>(),
                _ => new[] { includeToken.ToString() }
            };

            foreach (var includePath in includeProperties)
            {
                var trimmedPath = includePath.Trim();
                if (IsValidIncludePath<T>(trimmedPath))
                {
                    includes.Add(trimmedPath);
                }
            }

            return includes.ToArray();
        }

        private bool IsValidIncludePath<T>(string propertyPath) where T : class
        {
            try
            {
                var entityType = typeof(T);
                var propertyNames = propertyPath.Split('.');
                var currentType = entityType;

                foreach (var propertyName in propertyNames)
                {
                    var property = currentType.GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                    if (property == null) return false;

                    // Check if it's a navigation property (reference or collection)
                    if (!IsNavigationProperty(property)) return false;

                    currentType = GetNavigationTargetType(property);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool IsNavigationProperty(PropertyInfo property)
        {
            // Check if it's a reference navigation (class type)
            if (property.PropertyType.IsClass && property.PropertyType != typeof(string))
                return true;

            // Check if it's a collection navigation (ICollection<T>, IEnumerable<T>, etc.)
            if (property.PropertyType.IsGenericType)
            {
                var genericTypeDefinition = property.PropertyType.GetGenericTypeDefinition();
                return genericTypeDefinition == typeof(ICollection<>) ||
                       genericTypeDefinition == typeof(IEnumerable<>) ||
                       genericTypeDefinition == typeof(List<>);
            }

            return false;
        }

        private Type GetNavigationTargetType(PropertyInfo property)
        {
            if (property.PropertyType.IsGenericType)
            {
                // For collections, get the element type
                return property.PropertyType.GetGenericArguments()[0];
            }

            // For reference navigations, return the property type
            return property.PropertyType;
        }

        private IQueryable<T> ApplyPagination<T>(IQueryable<T> queryable, JObject parameters)
        {
            // Extract 'offset' parameter from JObject
            var offsetToken = parameters["offset"];
            if (offsetToken != null)
            {
                var offset = offsetToken.Type switch
                {
                    JTokenType.Integer => offsetToken.Value<int>(),
                    JTokenType.String when int.TryParse(offsetToken.Value<string>(), out var parsed) => parsed,
                    _ => (int?)null
                };

                if (offset.HasValue && offset.Value >= 0)
                {
                    queryable = queryable.Skip(offset.Value);
                }
            }

            // Extract 'limit' parameter from JObject
            var limitToken = parameters["limit"];
            if (limitToken != null)
            {
                var limit = limitToken.Type switch
                {
                    JTokenType.Integer => limitToken.Value<int>(),
                    JTokenType.String when int.TryParse(limitToken.Value<string>(), out var parsed) => parsed,
                    _ => (int?)null
                };

                if (limit.HasValue && limit.Value > 0)
                {
                    queryable = queryable.Take(limit.Value);
                }
            }

            return queryable;
        }

        private IQueryable<T> ApplyOrdering<T>(IQueryable<T> queryable, JObject parameters) where T : class
        {
            Console.WriteLine("[ApplyOrdering] Starting ordering validation and application");

            // Extract 'orderBy' parameter from JObject
            var orderByToken = parameters["orderBy"];
            if (orderByToken == null)
            {
                Console.WriteLine("[ApplyOrdering] No orderBy parameter found");
                return queryable;
            }

            var orderBy = orderByToken.Type switch
            {
                JTokenType.String => orderByToken.Value<string>(),
                _ => orderByToken.ToString()
            };

            if (string.IsNullOrEmpty(orderBy))
            {
                Console.WriteLine("[ApplyOrdering] OrderBy parameter is empty");
                return queryable;
            }

            Console.WriteLine($"[ApplyOrdering] Requested orderBy field: {orderBy}");

            // ✅ VALIDATION: Check if property exists and is sortable
            if (!IsValidOrderByProperty<T>(orderBy))
            {
                Console.WriteLine($"[ApplyOrdering] ❌ Invalid orderBy property: {orderBy}");
                throw new ArgumentException($"Invalid orderBy property: '{orderBy}'. Property does not exist or is not sortable.");
            }

            // Extract 'orderDesc' parameter from JObject
            var orderDescToken = parameters["orderDesc"];
            var isDescending = false;

            if (orderDescToken != null)
            {
                isDescending = orderDescToken.Type switch
                {
                    JTokenType.Boolean => orderDescToken.Value<bool>(),
                    JTokenType.String when bool.TryParse(orderDescToken.Value<string>(), out var parsed) => parsed,
                    JTokenType.Integer => orderDescToken.Value<int>() != 0, // 0 = false, non-zero = true
                    _ => false
                };
            }

            Console.WriteLine($"[ApplyOrdering] Applying order by '{orderBy}', descending: {isDescending}");
            return ApplyDynamicOrderingSafe<T>(queryable, orderBy, isDescending);
        }
        private bool IsValidOrderByProperty<T>(string propertyName) where T : class
        {
            Console.WriteLine($"[IsValidOrderByProperty] Validating orderBy property: {propertyName}");

            try
            {
                var entityType = typeof(T);
                var property = entityType.GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                if (property == null)
                {
                    Console.WriteLine($"[IsValidOrderByProperty] Property '{propertyName}' not found");
                    return false;
                }

                var allowedSortTypes = new[]
                {
            typeof(string),
            typeof(int), typeof(int?),
            typeof(long), typeof(long?),
            typeof(short), typeof(short?),
            typeof(byte), typeof(byte?),
            typeof(decimal), typeof(decimal?),
            typeof(double), typeof(double?),
            typeof(float), typeof(float?),
            typeof(DateTime), typeof(DateTime?),
            typeof(DateTimeOffset), typeof(DateTimeOffset?),
            typeof(Guid), typeof(Guid?),
            typeof(bool), typeof(bool?),
            typeof(char), typeof(char?)
        };

                var isSortable = allowedSortTypes.Contains(property.PropertyType);

                if (!isSortable)
                {
                    Console.WriteLine($"[IsValidOrderByProperty] Property '{propertyName}' type '{property.PropertyType.Name}' is not sortable");
                    return false;
                }

                Console.WriteLine($"[IsValidOrderByProperty] ✅ Property '{propertyName}' is valid for sorting");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[IsValidOrderByProperty] ❌ Exception validating property '{propertyName}': {ex.Message}");
                return false;
            }
        }
        

        // 🔧 UTILITY METHODS

        private IQueryable<T> ApplyDynamicOrderingSafe<T>(IQueryable<T> queryable, string propertyName, bool descending) where T : class
        {
            Console.WriteLine($"[ApplyDynamicOrderingSafe] Applying dynamic ordering for property: {propertyName}");

            try
            {
                var entityType = typeof(T);
                var property = entityType.GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                if (property == null)
                {
                    Console.WriteLine($"[ApplyDynamicOrderingSafe] ❌ Property not found: {propertyName}");
                    throw new ArgumentException($"Property '{propertyName}' not found");
                }

                var parameter = Expression.Parameter(entityType, "x");
                var propertyExpression = Expression.Property(parameter, property);
                var lambda = Expression.Lambda(propertyExpression, parameter);

                var methodName = descending ? "OrderByDescending" : "OrderBy";
                var method = typeof(Queryable).GetMethods()
                    .Where(m => m.Name == methodName && m.GetParameters().Length == 2)
                    .Single()
                    .MakeGenericMethod(entityType, property.PropertyType);

                var result = (IQueryable<T>)method.Invoke(null, new object[] { queryable, lambda });
                Console.WriteLine($"[ApplyDynamicOrderingSafe] ✅ Successfully applied ordering");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ApplyDynamicOrderingSafe] ❌ Exception applying ordering: {ex.Message}");
                throw new InvalidOperationException($"Failed to apply ordering for property '{propertyName}': {ex.Message}", ex);
            }
        }

        private Type? GetEntityTypeByName(string entityTypeName)
        {
            // Get all entity types from DbContext
            var dbSetProperties = _appDbContext.GetType().GetProperties()
                .Where(p => p.PropertyType.IsGenericType &&
                           p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
                .ToList();

            foreach (var property in dbSetProperties)
            {
                var entityType = property.PropertyType.GetGenericArguments()[0];
                if (entityType.Name.Equals(entityTypeName, StringComparison.OrdinalIgnoreCase))
                {
                    return entityType;
                }
            }

            return null;
        }

        private string[] GetSearchFields<T>(JObject parameters) where T : class
        {
            Console.WriteLine("[GetSearchFields] Starting search fields validation");

            // Extract 'searchFields' parameter from JObject
            var searchFieldsToken = parameters["searchFields"];

            string[] requestedFields = null;

            if (searchFieldsToken != null)
            {
                Console.WriteLine("[GetSearchFields] Found searchFields parameter");

                // Handle different JToken types for searchFields parameter
                requestedFields = searchFieldsToken.Type switch
                {
                    JTokenType.String => searchFieldsToken.Value<string>()?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>(),
                    JTokenType.Array => searchFieldsToken.ToObject<string[]>() ?? Array.Empty<string>(),
                    _ => new[] { searchFieldsToken.ToString() }
                };

                Console.WriteLine($"[GetSearchFields] Requested fields: [{string.Join(", ", requestedFields)}]");

                // ✅ VALIDATION: Validate each requested search field
                var validatedFields = new List<string>();

                foreach (var field in requestedFields)
                {
                    var trimmedField = field?.Trim();
                    if (string.IsNullOrEmpty(trimmedField)) continue;

                    if (IsValidSearchField<T>(trimmedField))
                    {
                        validatedFields.Add(trimmedField);
                        Console.WriteLine($"[GetSearchFields] ✅ Valid search field: {trimmedField}");
                    }
                    else
                    {
                        Console.WriteLine($"[GetSearchFields] ❌ Invalid search field: {trimmedField}");
                        throw new ArgumentException($"Invalid search field: '{trimmedField}'. Field does not exist or is not searchable.");
                    }
                }

                if (validatedFields.Any())
                {
                    Console.WriteLine($"[GetSearchFields] Returning {validatedFields.Count} validated fields");
                    return validatedFields.ToArray();
                }
            }

            // ✅ IMPROVED: Auto-detect với whitelist an toàn
            Console.WriteLine("[GetSearchFields] No valid searchFields specified, auto-detecting safe string properties");
            var autoDetectedFields = GetSafeAutoSearchFields<T>();
            Console.WriteLine($"[GetSearchFields] Auto-detected {autoDetectedFields.Length} safe search fields");
            return autoDetectedFields;
        }

        private bool IsValidSearchField<T>(string fieldName) where T : class
        {
            Console.WriteLine($"[IsValidSearchField] Validating search field: {fieldName}");

            try
            {
                var entityType = typeof(T);
                var property = entityType.GetProperty(fieldName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                if (property == null)
                {
                    Console.WriteLine($"[IsValidSearchField] Property '{fieldName}' not found");
                    return false;
                }

                // ✅ Chỉ cho phép search trên string fields
                if (property.PropertyType != typeof(string))
                {
                    Console.WriteLine($"[IsValidSearchField] Property '{fieldName}' is not string type: {property.PropertyType.Name}");
                    return false;
                }

                // ✅ Kiểm tra property có readable không
                if (!property.CanRead)
                {
                    Console.WriteLine($"[IsValidSearchField] Property '{fieldName}' is not readable");
                    return false;
                }

                // ✅ Blacklist các sensitive fields (có thể customize)
                var blacklistedFields = new[] { "Password", "Token", "Secret", "Key", "Hash", "Salt", "ApiKey" };
                var isBlacklisted = blacklistedFields.Any(bf =>
                    fieldName.Contains(bf, StringComparison.OrdinalIgnoreCase));

                if (isBlacklisted)
                {
                    Console.WriteLine($"[IsValidSearchField] Property '{fieldName}' is blacklisted for security");
                    return false;
                }

                Console.WriteLine($"[IsValidSearchField] ✅ Property '{fieldName}' is valid for search");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[IsValidSearchField] ❌ Exception validating field '{fieldName}': {ex.Message}");
                return false;
            }
        }

        private string[] GetSafeAutoSearchFields<T>() where T : class
        {
            Console.WriteLine("[GetSafeAutoSearchFields] Auto-detecting safe search fields");

            try
            {
                var safeFields = typeof(T).GetProperties()
                    .Where(p => p.PropertyType == typeof(string) && p.CanRead)
                    .Select(p => p.Name)
                    .Where(name => IsValidSearchField<T>(name))
                    .ToArray();

                Console.WriteLine($"[GetSafeAutoSearchFields] Found {safeFields.Length} safe fields: [{string.Join(", ", safeFields)}]");
                return safeFields;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetSafeAutoSearchFields] ❌ Exception auto-detecting fields: {ex.Message}");
                return Array.Empty<string>();
            }
        }

        private bool IsSpecialParameter(string paramName)
        {
            var specialParams = new[] { "include", "limit", "offset", "orderBy", "orderDesc", "searchTerm", "searchFields" };
            return specialParams.Contains(paramName, StringComparer.OrdinalIgnoreCase);
        }

        private object ApplyFieldSelection(object entity, string[] fields)
        {
            return fields.Any() ? _fieldSelector.SelectFields(entity, fields) : entity;
        }

        private JToken ConvertToJson(object data)
        {
            if (data == null) return JValue.CreateNull();

            var jsonString = JsonConvert.SerializeObject(data, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                NullValueHandling = NullValueHandling.Include,
                DateFormatHandling = DateFormatHandling.IsoDateFormat
            });

            return JToken.Parse(jsonString);
        }
    }


}