using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using SurveyTalkService.Common.AppConfigurations.App.interfaces;
using SurveyTalkService.Common.AppConfigurations.FilePath.interfaces;
using SurveyTalkService.DataAccess.Data;
using SurveyTalkService.DataAccess.UOW;
using SurveyTalkService.DataAccess.Repositories.interfaces;
using SurveyTalkService.DataAccess.Entities;
using SurveyTalkService.BusinessLogic.Helpers.AuthHelpers;
using SurveyTalkService.BusinessLogic.Helpers.FileHelpers;
using SurveyTalkService.BusinessLogic.Services.CrossServiceServices;
using SurveyTalkService.BusinessLogic.Models.CrossService;
using System.Linq.Expressions;
using System.Reflection;

namespace SurveyTalkService.BusinessLogic.Services.CrossServiceServices.QueryServices
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

        public async Task<object> HandleQueryAsync(BatchQueryItem query)
        {
            // Get entity type by name
            var entityType = GetEntityTypeByName(query.EntityType);
            if (entityType == null)
                throw new NotSupportedException($"Entity type '{query.EntityType}' not found");

            // Use reflection to call generic method
            var method = GetType().GetMethod(nameof(HandleTypedQueryAsync), BindingFlags.NonPublic | BindingFlags.Instance);
            var genericMethod = method.MakeGenericMethod(entityType);

            var task = (Task<object>)genericMethod.Invoke(this, new object[] { query });
            return await task;
        }

        private async Task<object> HandleTypedQueryAsync<T>(BatchQueryItem query) where T : class
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

        private async Task<object> HandleFindByIdAsync<T>(IGenericRepository<T> repo, BatchQueryItem query) where T : class
        {
            if (!query.Parameters.TryGetValue("id", out var idObj))
                throw new ArgumentException("'id' parameter required");

            // Build includes as strings
            var includePaths = BuildDynamicIncludes<T>(query.Parameters);

            // Use string-based includes
            var entity = await repo.FindByIdWithPaths(idObj, includePaths);
            if (entity == null) return new { };

            return ApplyFieldSelection(entity, query.Fields);
        }

        private async Task<object> HandleFindAllAsync<T>(IGenericRepository<T> repo, BatchQueryItem query) where T : class
        {
            // Build predicate completely dynamically
            var predicate = BuildDynamicPredicate<T>(query.Parameters);

            // Build includes as strings
            var includePaths = BuildDynamicIncludes<T>(query.Parameters);

            // Use string-based includes
            var queryable = repo.FindAllWithPaths(predicate, includePaths);

            // Apply pagination dynamically
            queryable = ApplyPagination(queryable, query.Parameters);

            // Apply ordering dynamically
            queryable = ApplyOrdering<T>(queryable, query.Parameters);

            var entities = await queryable.ToListAsync();

            return query.Fields.Any()
                ? entities.Select(e => ApplyFieldSelection(e, query.Fields))
                : entities;
        }

        private async Task<object> HandleSearchAsync<T>(IGenericRepository<T> repo, BatchQueryItem query) where T : class
        {
            var searchTerm = query.Parameters.GetValueOrDefault("searchTerm", "").ToString() ?? "";
            var searchFields = GetSearchFields<T>(query.Parameters);

            if (string.IsNullOrEmpty(searchTerm) || !searchFields.Any())
                return new List<object>();

            // Build search predicate dynamically
            var searchPredicate = BuildDynamicSearchPredicate<T>(searchTerm, searchFields);

            var includePaths = BuildDynamicIncludes<T>(query.Parameters);
            var queryable = repo.FindAllWithPaths(searchPredicate, includePaths);

            // Apply pagination
            queryable = ApplyPagination(queryable, query.Parameters);

            var entities = await queryable.ToListAsync();

            return query.Fields.Any()
                ? entities.Select(e => ApplyFieldSelection(e, query.Fields))
                : entities;
        }

        private async Task<object> HandleCountAsync<T>(IGenericRepository<T> repo, BatchQueryItem query) where T : class
        {
            var predicate = BuildDynamicPredicate<T>(query.Parameters);
            var count = await repo.FindAll(predicate).CountAsync();
            return new { count };
        }

        // 🔨 COMPLETELY DYNAMIC PREDICATE BUILDING
        // private Expression<Func<T, bool>>? BuildDynamicPredicate<T>(Dictionary<string, object> parameters) where T : class
        // {
        //     var entityType = typeof(T);
        //     var parameter = Expression.Parameter(entityType, "x");
        //     Expression? combinedExpression = null;

        //     foreach (var param in parameters)
        //     {
        //         // Skip special parameters
        //         if (IsSpecialParameter(param.Key)) continue;

        //         // Check if property exists on entity
        //         var property = entityType.GetProperty(param.Key, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
        //         if (property == null) continue;

        //         try
        //         {
        //             var propertyExpression = Expression.Property(parameter, property);
        //             var valueExpression = CreateValueExpression(property.PropertyType, param.Value);
        //             var equalsExpression = Expression.Equal(propertyExpression, valueExpression);

        //             combinedExpression = combinedExpression == null
        //                 ? equalsExpression
        //                 : Expression.AndAlso(combinedExpression, equalsExpression);
        //         }
        //         catch
        //         {
        //             // Skip if conversion fails
        //             continue;
        //         }
        //     }

        //     return combinedExpression != null
        //         ? Expression.Lambda<Func<T, bool>>(combinedExpression, parameter)
        //         : null;
        // }

        private Expression<Func<T, bool>>? BuildDynamicPredicate<T>(Dictionary<string, object> parameters) where T : class
        {
            // Lấy where object (nếu có), hỗ trợ JObject (Newtonsoft)
            Dictionary<string, object> whereDict = null;
            if (parameters.TryGetValue("where", out var whereObj))
            {
                if (whereObj is Newtonsoft.Json.Linq.JObject jObj)
                {
                    whereDict = jObj.ToObject<Dictionary<string, object>>();
                }
                else if (whereObj is Dictionary<string, object> wd)
                {
                    whereDict = wd;
                }
            }
            if (whereDict == null)
            {
                whereDict = parameters.Where(kv => !IsSpecialParameter(kv.Key)).ToDictionary(kv => kv.Key, kv => kv.Value);
            }

            var entityType = typeof(T);
            var parameter = Expression.Parameter(entityType, "x");
            Expression? combinedExpression = null;

            foreach (var param in whereDict)
            {
                try
                {
                    // Hỗ trợ property lồng nhau: "Role.Name", "SurveyTakenResults.Status"
                    var propertyPath = param.Key.Split('.');
                    Expression propertyExpression = parameter;
                    Type currentType = entityType;

                    foreach (var propName in propertyPath)
                    {
                        var property = currentType.GetProperty(propName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                        if (property == null)
                        {
                            propertyExpression = null;
                            break;
                        }

                        propertyExpression = Expression.Property(propertyExpression, property);
                        currentType = property.PropertyType;
                    }

                    if (propertyExpression == null) continue;

                    // Nếu là collection, build Any
                    if (typeof(System.Collections.IEnumerable).IsAssignableFrom(currentType) && currentType != typeof(string))
                    {
                        // Chỉ hỗ trợ filter sâu 1 cấp cho collection: "SurveyTakenResults.Status"
                        // propertyPath[^1] là property của phần tử trong collection
                        var elementType = currentType.IsGenericType
                            ? currentType.GetGenericArguments()[0]
                            : currentType.GetElementType();

                        var innerParam = Expression.Parameter(elementType, "e");
                        var innerProp = elementType.GetProperty(propertyPath[^1], BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                        if (innerProp == null) continue;

                        var innerPropExpr = Expression.Property(innerParam, innerProp);
                        var valueExpr = CreateValueExpression(innerProp.PropertyType, param.Value);
                        var equalsExpr = Expression.Equal(innerPropExpr, valueExpr);

                        var anyLambda = Expression.Lambda(equalsExpr, innerParam);
                        var anyMethod = typeof(Enumerable).GetMethods()
                            .First(m => m.Name == "Any" && m.GetParameters().Length == 2)
                            .MakeGenericMethod(elementType);

                        var anyCall = Expression.Call(anyMethod, propertyExpression, anyLambda);

                        combinedExpression = combinedExpression == null
                            ? anyCall
                            : Expression.AndAlso(combinedExpression, anyCall);
                    }
                    else
                    {
                        // Reference hoặc value type
                        var valueExpression = CreateValueExpression(currentType, param.Value);
                        var equalsExpression = Expression.Equal(propertyExpression, valueExpression);

                        combinedExpression = combinedExpression == null
                            ? equalsExpression
                            : Expression.AndAlso(combinedExpression, equalsExpression);
                    }
                }
                catch
                {
                    continue;
                }
            }

            return combinedExpression != null
                ? Expression.Lambda<Func<T, bool>>(combinedExpression, parameter)
                : null;
        }

        // 🔨 DYNAMIC SEARCH PREDICATE  
        private Expression<Func<T, bool>>? BuildDynamicSearchPredicate<T>(string searchTerm, string[] searchFields) where T : class
        {
            var entityType = typeof(T);
            var parameter = Expression.Parameter(entityType, "x");
            var searchConstant = Expression.Constant(searchTerm);
            var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });

            Expression? combinedExpression = null;

            foreach (var fieldName in searchFields)
            {
                var property = entityType.GetProperty(fieldName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (property == null || property.PropertyType != typeof(string)) continue;

                try
                {
                    var propertyExpression = Expression.Property(parameter, property);
                    var containsExpression = Expression.Call(propertyExpression, containsMethod, searchConstant);

                    combinedExpression = combinedExpression == null
                        ? containsExpression
                        : Expression.OrElse(combinedExpression, containsExpression);
                }
                catch
                {
                    continue;
                }
            }

            return combinedExpression != null
                ? Expression.Lambda<Func<T, bool>>(combinedExpression, parameter)
                : null;
        }

        // 🔨 DYNAMIC INCLUDES - Updated to use string-based includes
        private string[] BuildDynamicIncludes<T>(Dictionary<string, object> parameters) where T : class
        {
            var includes = new List<string>();

            if (!parameters.TryGetValue("include", out var includeObj))
                return includes.ToArray();

            var includeProperties = includeObj.ToString()?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();

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

        // Add new method to validate include paths
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

        // Add helper methods for navigation property validation
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

        // 🔨 DYNAMIC PAGINATION
        private IQueryable<T> ApplyPagination<T>(IQueryable<T> queryable, Dictionary<string, object> parameters)
        {
            if (parameters.TryGetValue("offset", out var offsetObj))
            {
                if (int.TryParse(offsetObj.ToString(), out var offset))
                {
                    queryable = queryable.Skip(offset);
                }
            }

            if (parameters.TryGetValue("limit", out var limitObj))
            {
                if (int.TryParse(limitObj.ToString(), out var limit))
                {
                    queryable = queryable.Take(limit);
                }
            }

            return queryable;
        }

        // 🔨 DYNAMIC ORDERING
        private IQueryable<T> ApplyOrdering<T>(IQueryable<T> queryable, Dictionary<string, object> parameters)
        {
            if (!parameters.TryGetValue("orderBy", out var orderByObj))
                return queryable;

            var orderBy = orderByObj.ToString();
            if (string.IsNullOrEmpty(orderBy)) return queryable;

            var isDescending = parameters.TryGetValue("orderDesc", out var descObj) &&
                              bool.TryParse(descObj.ToString(), out var desc) && desc;

            return ApplyDynamicOrdering(queryable, orderBy, isDescending);
        }

        private IQueryable<T> ApplyDynamicOrdering<T>(IQueryable<T> queryable, string propertyName, bool descending)
        {
            try
            {
                var entityType = typeof(T);
                var property = entityType.GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                if (property == null) return queryable;

                var parameter = Expression.Parameter(entityType, "x");
                var propertyExpression = Expression.Property(parameter, property);
                var lambda = Expression.Lambda(propertyExpression, parameter);

                var methodName = descending ? "OrderByDescending" : "OrderBy";
                var method = typeof(Queryable).GetMethods()
                    .Where(m => m.Name == methodName && m.GetParameters().Length == 2)
                    .Single()
                    .MakeGenericMethod(entityType, property.PropertyType);

                return (IQueryable<T>)method.Invoke(null, new object[] { queryable, lambda });
            }
            catch
            {
                return queryable;
            }
        }

        // 🔧 UTILITY METHODS
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

        private string[] GetSearchFields<T>(Dictionary<string, object> parameters)
        {
            // Get from parameters if specified
            if (parameters.TryGetValue("searchFields", out var searchFieldsObj))
            {
                return searchFieldsObj.ToString()?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
            }

            // Auto-detect string properties
            return typeof(T).GetProperties()
                .Where(p => p.PropertyType == typeof(string) && p.CanRead)
                .Select(p => p.Name)
                .ToArray();
        }

        private bool IsSpecialParameter(string paramName)
        {
            var specialParams = new[] { "include", "limit", "offset", "orderBy", "orderDesc", "searchTerm", "searchFields" };
            return specialParams.Contains(paramName, StringComparer.OrdinalIgnoreCase);
        }

        private Expression CreateValueExpression(Type targetType, object value)
        {
            // Handle nullable types
            var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            // Convert value to target type
            var convertedValue = Convert.ChangeType(value, underlyingType);

            return Expression.Constant(convertedValue, targetType);
        }

        private object ApplyFieldSelection(object entity, string[] fields)
        {
            return fields.Any() ? _fieldSelector.SelectFields(entity, fields) : entity;
        }
    }
}