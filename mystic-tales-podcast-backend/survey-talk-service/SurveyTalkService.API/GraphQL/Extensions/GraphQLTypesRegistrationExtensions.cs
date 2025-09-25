using HotChocolate.Execution.Configuration;
using System.Reflection;

namespace SurveyTalkService.API.GraphQL.Extensions
{
    public static class GraphQLTypesRegistrationExtensions
    {
        public static IRequestExecutorBuilder AddGraphQLTypesFromAssembly(
        this IRequestExecutorBuilder builder,
        Assembly assembly)
        {
            Console.WriteLine($"\n\n\n[!] Searching for GraphQL types in assembly {assembly.FullName}...");
            var graphQLTypes = assembly
                .GetTypes()
                .Where(t =>
                    !t.IsAbstract &&
                    !t.IsGenericType &&
                    (typeof(IType).IsAssignableFrom(t) || t.GetInterfaces().Contains(typeof(IType))))

                .ToList();
            Console.WriteLine($"\n\n[*] Found {graphQLTypes.Count} GraphQL types in assembly {assembly.FullName}");
            foreach (var type in graphQLTypes)
            {
                Console.WriteLine($"\n[#] Adding GraphQL type {type.Name} to executor builder - {type.Namespace}");
                builder = builder.AddType(type);
            }

            return builder;
        }

        public static IRequestExecutorBuilder AddGraphQLTypesFromAssemblies(
            this IRequestExecutorBuilder builder,
            params Assembly[] assemblies)
        {
            foreach (var asm in assemblies)
            {
                builder = builder.AddGraphQLTypesFromAssembly(asm);
            }

            return builder;
        }

        

        public static IRequestExecutorBuilder AddGraphQLExtensionsFromAssembly(
        this IRequestExecutorBuilder builder,
        Assembly assembly)
        {
            Console.WriteLine($"\n\n\n[!] Searching for GraphQL extension types in assembly {assembly.FullName}...");
            var extensionTypes = assembly
                .GetTypes()
                .Where(t =>
                    !t.IsAbstract &&
                    !t.IsGenericType &&
                    (
                        t.GetCustomAttributes(typeof(HotChocolate.Types.ExtendObjectTypeAttribute), true).Length > 0 ||

                        IsSubclassOfRawGeneric(t, typeof(ObjectTypeExtension<>)) ||
                        IsSubclassOfRawGeneric(t, typeof(ObjectTypeExtension))


                    )
                ).ToList();

            Console.WriteLine($"\n\n[*] Found {extensionTypes.Count} [ExtendObjectType] types in assembly: {assembly.FullName}");
            foreach (var type in extensionTypes)
            {
                Console.WriteLine($"\n[#] Registering extension type: {type.FullName}");
                builder = builder.AddTypeExtension(type);
            }

            return builder;
        }

        public static IRequestExecutorBuilder AddGraphQLExtensionsFromAssemblies(
            this IRequestExecutorBuilder builder,
            params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                builder = builder.AddGraphQLExtensionsFromAssembly(assembly);
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
