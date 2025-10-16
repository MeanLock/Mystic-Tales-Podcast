// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.DependencyInjection;
// using UserService.DataAccess.Data;
// using UserService.DataAccess.Repositories.interfaces;
// using UserService.BusinessLogic.Models.CrossService;
// using System.Linq.Expressions;
// using System.Reflection;
// using Newtonsoft.Json.Linq;
// using Newtonsoft.Json;

// namespace UserService.BusinessLogic.Services.CrossServiceServices.QueryServices
// {
//     public class GenericQueryService
//     {
//         private readonly AppDbContext _appDbContext;
//         private readonly IServiceProvider _serviceProvider;
//         private readonly FieldSelector _fieldSelector;

//         public GenericQueryService(
//             AppDbContext context,
//             IServiceProvider serviceProvider,
//             FieldSelector fieldSelector)
//         {
//             _appDbContext = context;
//             _serviceProvider = serviceProvider;
//             _fieldSelector = fieldSelector;
//         }

//         public async Task<JToken> HandleQueryAsync(BatchQueryItem query)
//         {
//             // Get entity type by name
//             var entityType = GetEntityTypeByName(query.EntityType);
//             if (entityType == null)
//                 throw new NotSupportedException($"Entity type '{query.EntityType}' not found");

//             // Use reflection to call generic method
//             var method = GetType().GetMethod(nameof(HandleTypedQueryAsync), BindingFlags.NonPublic | BindingFlags.Instance);
//             var genericMethod = method.MakeGenericMethod(entityType);

//             // var task = (Task<object>)genericMethod.Invoke(this, new object[] { query });
//             var task = (Task<JToken>)genericMethod.Invoke(this, new object[] { query });
//             return await task;
//         }

//         private async Task<JToken> HandleTypedQueryAsync<T>(BatchQueryItem query) where T : class
//         {
//             var repo = _serviceProvider.GetRequiredService<IGenericRepository<T>>();

//             return query.QueryType.ToLower() switch
//             {
//                 "findbyid" => await HandleFindByIdAsync(repo, query),
//                 "findall" => await HandleFindAllAsync(repo, query),
//                 "search" => await HandleSearchAsync<T>(repo, query),
//                 "count" => await HandleCountAsync(repo, query),
//                 _ => throw new NotSupportedException($"Query type '{query.QueryType}' not supported")
//             };
//         }

//         private async Task<JToken> HandleFindByIdAsync<T>(IGenericRepository<T> repo, BatchQueryItem query) where T : class
//         {
//             // Extract 'id' parameter from JObject
//             var idToken = query.Parameters["id"];
//             if (idToken == null)
//                 throw new ArgumentException("'id' parameter required");

//             // Convert JToken to appropriate object type
//             var idObj = idToken.Type switch
//             {
//                 JTokenType.String => idToken.Value<string>(),
//                 JTokenType.Integer => idToken.Value<int>(),
//                 JTokenType.Guid => idToken.Value<Guid>(),
//                 JTokenType.Float => idToken.Value<decimal>(),
//                 _ => idToken.ToObject<object>()
//             };

//             var includePaths = BuildDynamicIncludes<T>(query.Parameters);

//             var entity = await repo.FindByIdWithPaths(idObj, includePaths);
//             if (entity == null)
//                 return JObject.FromObject(new { }); // Empty JObject for not found

//             var selectedData = ApplyFieldSelection(entity, query.Fields);
//             return ConvertToJson(selectedData);
//         }



//         private async Task<JToken> HandleFindAllAsync<T>(IGenericRepository<T> repo, BatchQueryItem query) where T : class
//         {
//             var predicate = BuildDynamicPredicate<T>(query.Parameters);

//             var includePaths = BuildDynamicIncludes<T>(query.Parameters);

//             var queryable = repo.FindAllWithPaths(predicate, includePaths);

//             queryable = ApplyPagination(queryable, query.Parameters);

//             queryable = ApplyOrdering<T>(queryable, query.Parameters);

//             var entities = await queryable.ToListAsync();

//             var result = query.Fields.Any()
//                 ? entities.Select(e => ApplyFieldSelection(e, query.Fields))
//                 : entities;

//             return ConvertToJson(result);
//         }

//         private async Task<JToken> HandleSearchAsync<T>(IGenericRepository<T> repo, BatchQueryItem query) where T : class
//         {
//             // Extract 'searchTerm' parameter from JObject
//             var searchTermToken = query.Parameters["searchTerm"];
//             var searchTerm = searchTermToken?.Type switch
//             {
//                 JTokenType.String => searchTermToken.Value<string>(),
//                 JTokenType.Null => string.Empty,
//                 null => string.Empty,
//                 _ => searchTermToken.ToString()
//             } ?? "";

//             var searchFields = GetSearchFields<T>(query.Parameters);

//             if (string.IsNullOrEmpty(searchTerm) || !searchFields.Any())
//                 return JArray.FromObject(new List<object>());

//             // Build search predicate dynamically
//             var searchPredicate = BuildDynamicSearchPredicate<T>(searchTerm, searchFields);

//             var includePaths = BuildDynamicIncludes<T>(query.Parameters);
//             var queryable = repo.FindAllWithPaths(searchPredicate, includePaths);

//             // Apply pagination
//             queryable = ApplyPagination(queryable, query.Parameters);

//             var entities = await queryable.ToListAsync();

//             var result = query.Fields.Any()
//                 ? entities.Select(e => ApplyFieldSelection(e, query.Fields))
//                 : entities;

//             return ConvertToJson(result);
//         }

//         private async Task<JToken> HandleCountAsync<T>(IGenericRepository<T> repo, BatchQueryItem query) where T : class
//         {
//             var predicate = BuildDynamicPredicate<T>(query.Parameters);
//             var count = await repo.FindAll(predicate).CountAsync();
//             return JObject.FromObject(new { count });
//         }



//         private Expression<Func<T, bool>>? BuildDynamicPredicate<T>(JObject parameters) where T : class
//         {
//             // Lấy where object (nếu có) từ JObject
//             JObject whereObj = null;
//             var whereToken = parameters["where"];
//             if (whereToken != null && whereToken.Type == JTokenType.Object)
//             {
//                 whereObj = (JObject)whereToken;
//             }
//             else
//             {
//                 // Nếu không có 'where', sử dụng tất cả parameters trừ special parameters
//                 whereObj = new JObject();
//                 foreach (var prop in parameters.Properties())
//                 {
//                     if (!IsSpecialParameter(prop.Name))
//                     {
//                         whereObj[prop.Name] = prop.Value;
//                     }
//                 }
//             }

//             if (!whereObj.HasValues)
//                 return null;

//             var entityType = typeof(T);
//             var parameter = Expression.Parameter(entityType, "x");
//             Expression? combinedExpression = null;

//             foreach (var prop in whereObj.Properties())
//             {
//                 try
//                 {
//                     // Hỗ trợ property lồng nhau: "Role.Name", "SurveyTakenResults.Status"
//                     var propertyPath = prop.Name.Split('.');
//                     Expression propertyExpression = parameter;
//                     Type currentType = entityType;

//                     foreach (var propName in propertyPath)
//                     {
//                         var property = currentType.GetProperty(propName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
//                         if (property == null)
//                         {
//                             propertyExpression = null;
//                             break;
//                         }

//                         propertyExpression = Expression.Property(propertyExpression, property);
//                         currentType = property.PropertyType;
//                     }

//                     if (propertyExpression == null) continue;

//                     // Convert JToken value to appropriate object type
//                     object paramValue = ConvertJTokenToValue(prop.Value, currentType);
//                     if (paramValue == null) continue;

//                     // Nếu là collection, build Any
//                     if (typeof(System.Collections.IEnumerable).IsAssignableFrom(currentType) && currentType != typeof(string))
//                     {
//                         // Chỉ hỗ trợ filter sâu 1 cấp cho collection: "SurveyTakenResults.Status"
//                         // propertyPath[^1] là property của phần tử trong collection
//                         var elementType = currentType.IsGenericType
//                             ? currentType.GetGenericArguments()[0]
//                             : currentType.GetElementType();

//                         var innerParam = Expression.Parameter(elementType, "e");
//                         var innerProp = elementType.GetProperty(propertyPath[^1], BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
//                         if (innerProp == null) continue;

//                         var innerPropExpr = Expression.Property(innerParam, innerProp);
//                         var valueExpr = CreateValueExpression(innerProp.PropertyType, paramValue);
//                         var equalsExpr = Expression.Equal(innerPropExpr, valueExpr);

//                         var anyLambda = Expression.Lambda(equalsExpr, innerParam);
//                         var anyMethod = typeof(Enumerable).GetMethods()
//                             .First(m => m.Name == "Any" && m.GetParameters().Length == 2)
//                             .MakeGenericMethod(elementType);

//                         var anyCall = Expression.Call(anyMethod, propertyExpression, anyLambda);

//                         combinedExpression = combinedExpression == null
//                             ? anyCall
//                             : Expression.AndAlso(combinedExpression, anyCall);
//                     }
//                     else
//                     {
//                         // Reference hoặc value type
//                         var valueExpression = CreateValueExpression(currentType, paramValue);
//                         var equalsExpression = Expression.Equal(propertyExpression, valueExpression);

//                         combinedExpression = combinedExpression == null
//                             ? equalsExpression
//                             : Expression.AndAlso(combinedExpression, equalsExpression);
//                     }
//                 }
//                 catch
//                 {
//                     continue;
//                 }
//             }

//             return combinedExpression != null
//                 ? Expression.Lambda<Func<T, bool>>(combinedExpression, parameter)
//                 : null;
//         }
//         private Expression<Func<T, bool>>? BuildDynamicSearchPredicate<T>(string searchTerm, string[] searchFields) where T : class
//         {
//             var entityType = typeof(T);
//             var parameter = Expression.Parameter(entityType, "x");
//             var searchConstant = Expression.Constant(searchTerm);
//             var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });

//             Expression? combinedExpression = null;

//             foreach (var fieldName in searchFields)
//             {
//                 var property = entityType.GetProperty(fieldName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
//                 if (property == null || property.PropertyType != typeof(string)) continue;

//                 try
//                 {
//                     var propertyExpression = Expression.Property(parameter, property);
//                     var containsExpression = Expression.Call(propertyExpression, containsMethod, searchConstant);

//                     combinedExpression = combinedExpression == null
//                         ? containsExpression
//                         : Expression.OrElse(combinedExpression, containsExpression);
//                 }
//                 catch
//                 {
//                     continue;
//                 }
//             }

//             return combinedExpression != null
//                 ? Expression.Lambda<Func<T, bool>>(combinedExpression, parameter)
//                 : null;
//         }



//         private string[] BuildDynamicIncludes<T>(JObject parameters) where T : class
//         {
//             var includes = new List<string>();

//             // Extract 'include' parameter from JObject
//             var includeToken = parameters["include"];
//             if (includeToken == null)
//                 return includes.ToArray();

//             // Handle different JToken types for include parameter
//             string[] includeProperties = includeToken.Type switch
//             {
//                 JTokenType.String => includeToken.Value<string>()?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>(),
//                 JTokenType.Array => includeToken.ToObject<string[]>() ?? Array.Empty<string>(),
//                 _ => new[] { includeToken.ToString() }
//             };

//             foreach (var includePath in includeProperties)
//             {
//                 var trimmedPath = includePath.Trim();
//                 if (IsValidIncludePath<T>(trimmedPath))
//                 {
//                     includes.Add(trimmedPath);
//                 }
//             }

//             return includes.ToArray();
//         }

//         // Add new method to validate include paths
//         private bool IsValidIncludePath<T>(string propertyPath) where T : class
//         {
//             try
//             {
//                 var entityType = typeof(T);
//                 var propertyNames = propertyPath.Split('.');
//                 var currentType = entityType;

//                 foreach (var propertyName in propertyNames)
//                 {
//                     var property = currentType.GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
//                     if (property == null) return false;

//                     // Check if it's a navigation property (reference or collection)
//                     if (!IsNavigationProperty(property)) return false;

//                     currentType = GetNavigationTargetType(property);
//                 }

//                 return true;
//             }
//             catch
//             {
//                 return false;
//             }
//         }

//         // Add helper methods for navigation property validation
//         private bool IsNavigationProperty(PropertyInfo property)
//         {
//             // Check if it's a reference navigation (class type)
//             if (property.PropertyType.IsClass && property.PropertyType != typeof(string))
//                 return true;

//             // Check if it's a collection navigation (ICollection<T>, IEnumerable<T>, etc.)
//             if (property.PropertyType.IsGenericType)
//             {
//                 var genericTypeDefinition = property.PropertyType.GetGenericTypeDefinition();
//                 return genericTypeDefinition == typeof(ICollection<>) ||
//                        genericTypeDefinition == typeof(IEnumerable<>) ||
//                        genericTypeDefinition == typeof(List<>);
//             }

//             return false;
//         }

//         private Type GetNavigationTargetType(PropertyInfo property)
//         {
//             if (property.PropertyType.IsGenericType)
//             {
//                 // For collections, get the element type
//                 return property.PropertyType.GetGenericArguments()[0];
//             }

//             // For reference navigations, return the property type
//             return property.PropertyType;
//         }


//         private IQueryable<T> ApplyPagination<T>(IQueryable<T> queryable, JObject parameters)
//         {
//             // Extract 'offset' parameter from JObject
//             var offsetToken = parameters["offset"];
//             if (offsetToken != null)
//             {
//                 var offset = offsetToken.Type switch
//                 {
//                     JTokenType.Integer => offsetToken.Value<int>(),
//                     JTokenType.String when int.TryParse(offsetToken.Value<string>(), out var parsed) => parsed,
//                     _ => (int?)null
//                 };

//                 if (offset.HasValue && offset.Value >= 0)
//                 {
//                     queryable = queryable.Skip(offset.Value);
//                 }
//             }

//             // Extract 'limit' parameter from JObject
//             var limitToken = parameters["limit"];
//             if (limitToken != null)
//             {
//                 var limit = limitToken.Type switch
//                 {
//                     JTokenType.Integer => limitToken.Value<int>(),
//                     JTokenType.String when int.TryParse(limitToken.Value<string>(), out var parsed) => parsed,
//                     _ => (int?)null
//                 };

//                 if (limit.HasValue && limit.Value > 0)
//                 {
//                     queryable = queryable.Take(limit.Value);
//                 }
//             }

//             return queryable;
//         }


//         private IQueryable<T> ApplyOrdering<T>(IQueryable<T> queryable, JObject parameters)
//         {
//             // Extract 'orderBy' parameter from JObject
//             var orderByToken = parameters["orderBy"];
//             if (orderByToken == null)
//                 return queryable;

//             var orderBy = orderByToken.Type switch
//             {
//                 JTokenType.String => orderByToken.Value<string>(),
//                 _ => orderByToken.ToString()
//             };

//             if (string.IsNullOrEmpty(orderBy))
//                 return queryable;

//             // Extract 'orderDesc' parameter from JObject
//             var orderDescToken = parameters["orderDesc"];
//             var isDescending = false;

//             if (orderDescToken != null)
//             {
//                 isDescending = orderDescToken.Type switch
//                 {
//                     JTokenType.Boolean => orderDescToken.Value<bool>(),
//                     JTokenType.String when bool.TryParse(orderDescToken.Value<string>(), out var parsed) => parsed,
//                     JTokenType.Integer => orderDescToken.Value<int>() != 0, // 0 = false, non-zero = true
//                     _ => false
//                 };
//             }

//             return ApplyDynamicOrdering(queryable, orderBy, isDescending);
//         }

//         private IQueryable<T> ApplyDynamicOrdering<T>(IQueryable<T> queryable, string propertyName, bool descending)
//         {
//             try
//             {
//                 var entityType = typeof(T);
//                 var property = entityType.GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

//                 if (property == null) return queryable;

//                 var parameter = Expression.Parameter(entityType, "x");
//                 var propertyExpression = Expression.Property(parameter, property);
//                 var lambda = Expression.Lambda(propertyExpression, parameter);

//                 var methodName = descending ? "OrderByDescending" : "OrderBy";
//                 var method = typeof(Queryable).GetMethods()
//                     .Where(m => m.Name == methodName && m.GetParameters().Length == 2)
//                     .Single()
//                     .MakeGenericMethod(entityType, property.PropertyType);

//                 return (IQueryable<T>)method.Invoke(null, new object[] { queryable, lambda });
//             }
//             catch
//             {
//                 return queryable;
//             }
//         }

//         // 🔧 UTILITY METHODS
//         private Type? GetEntityTypeByName(string entityTypeName)
//         {
//             // Get all entity types from DbContext
//             var dbSetProperties = _appDbContext.GetType().GetProperties()
//                 .Where(p => p.PropertyType.IsGenericType &&
//                            p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
//                 .ToList();

//             foreach (var property in dbSetProperties)
//             {
//                 var entityType = property.PropertyType.GetGenericArguments()[0];
//                 if (entityType.Name.Equals(entityTypeName, StringComparison.OrdinalIgnoreCase))
//                 {
//                     return entityType;
//                 }
//             }

//             return null;
//         }


//         private string[] GetSearchFields<T>(JObject parameters)
//         {
//             // Extract 'searchFields' parameter from JObject
//             var searchFieldsToken = parameters["searchFields"];
//             if (searchFieldsToken != null)
//             {
//                 // Handle different JToken types for searchFields parameter
//                 return searchFieldsToken.Type switch
//                 {
//                     JTokenType.String => searchFieldsToken.Value<string>()?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>(),
//                     JTokenType.Array => searchFieldsToken.ToObject<string[]>() ?? Array.Empty<string>(),
//                     _ => new[] { searchFieldsToken.ToString() }
//                 };
//             }

//             // Auto-detect string properties if no searchFields specified
//             return typeof(T).GetProperties()
//                 .Where(p => p.PropertyType == typeof(string) && p.CanRead)
//                 .Select(p => p.Name)
//                 .ToArray();
//         }

//         private bool IsSpecialParameter(string paramName)
//         {
//             var specialParams = new[] { "include", "limit", "offset", "orderBy", "orderDesc", "searchTerm", "searchFields" };
//             return specialParams.Contains(paramName, StringComparer.OrdinalIgnoreCase);
//         }

//         private Expression CreateValueExpression(Type targetType, object value)
//         {
//             // Handle nullable types
//             var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

//             // Convert value to target type
//             var convertedValue = Convert.ChangeType(value, underlyingType);

//             return Expression.Constant(convertedValue, targetType);
//         }

//         private object ApplyFieldSelection(object entity, string[] fields)
//         {
//             return fields.Any() ? _fieldSelector.SelectFields(entity, fields) : entity;
//         }


//         private object ConvertJTokenToValue(JToken token, Type targetType)
//         {
//             try
//             {
//                 var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

//                 return underlyingType.Name switch
//                 {
//                     nameof(String) => token.Value<string>(),
//                     nameof(Int32) => token.Value<int>(),
//                     nameof(Int64) => token.Value<long>(),
//                     nameof(Guid) => token.Value<Guid>(),
//                     nameof(DateTime) => token.Value<DateTime>(),
//                     nameof(Boolean) => token.Value<bool>(),
//                     nameof(Decimal) => token.Value<decimal>(),
//                     nameof(Double) => token.Value<double>(),
//                     nameof(Single) => token.Value<float>(),
//                     _ => token.ToObject(underlyingType)
//                 };
//             }
//             catch
//             {
//                 return null;
//             }
//         }

//         private JToken ConvertToJson(object data)
//         {
//             if (data == null) return JValue.CreateNull();

//             var jsonString = JsonConvert.SerializeObject(data, new JsonSerializerSettings
//             {
//                 ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
//                 NullValueHandling = NullValueHandling.Include,
//                 DateFormatHandling = DateFormatHandling.IsoDateFormat
//             });

//             return JToken.Parse(jsonString);
//         }
//     }

// }