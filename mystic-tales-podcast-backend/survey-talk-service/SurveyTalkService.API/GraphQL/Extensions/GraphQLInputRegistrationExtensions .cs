using HotChocolate.Execution.Configuration;
using System.Reflection;

namespace SurveyTalkService.API.GraphQL.Extensions
{
    public static class GraphQLInputRegistrationExtensions
    {
        public static IRequestExecutorBuilder AddGraphQLInputTypesFromAssembly(
    this IRequestExecutorBuilder builder,
    Assembly assembly)
        {
            Console.WriteLine($"\n\n\n[!] Searching for GraphQL input types in assembly {assembly.FullName}...");

            var inputTypes = assembly
                .GetTypes()
                .Where(t =>
                    !t.IsAbstract &&
                    !t.IsGenericType &&
                    IsSubclassOfRawGeneric(t, typeof(InputObjectType<>))
                ).ToList();

            Console.WriteLine($"\n\n[*] Found {inputTypes.Count} InputObjectType types in assembly: {assembly.FullName}");

            foreach (var type in inputTypes)
            {
                Console.WriteLine($"\n[#] Registering input type: {type.FullName}");
                builder = builder.AddType(type);
            }

            return builder;
        }

        public static IRequestExecutorBuilder AddGraphQLInputTypesFromAssemblies(
    this IRequestExecutorBuilder builder,
    params Assembly[] assemblies)
        {
            foreach (var asm in assemblies)
            {
                builder = builder.AddGraphQLInputTypesFromAssembly(asm);
            }

            return builder;
        }

        private static bool IsSubclassOfRawGeneric(Type type, Type generic)
        {
            while (type != null && type != typeof(object))
            {
                var current = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
                if (current == generic)
                    return true;

                type = type.BaseType;
            }
            return false;
        }

    }
}
