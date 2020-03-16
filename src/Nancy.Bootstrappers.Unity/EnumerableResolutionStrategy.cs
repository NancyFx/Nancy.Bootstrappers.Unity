using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity;
using Unity.Builder;
using Unity.Strategies;

namespace Nancy.Bootstrappers.Unity
{
    /// <summary>
    /// This strategy implements the logic that will return all instances
    /// when an <see cref="IEnumerable{T}"/> parameter is detected.
    /// </summary>
    /// <remarks>
    /// Nicked from https://piotr-wlodek-code-gallery.googlecode.com/svn-history/r40/trunk/Unity.Extensions/Unity.Extensions/EnumerableResolutionStrategy.cs
    /// </remarks>
    public class EnumerableResolutionStrategy : BuilderStrategy
    {
        private delegate object Resolver(BuilderContext context);

        private static readonly MethodInfo GenericResolveEnumerableMethod =
            typeof(EnumerableResolutionStrategy).GetMethod("ResolveEnumerable",
                BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly);

        private static readonly MethodInfo GenericResolveLazyEnumerableMethod =
            typeof(EnumerableResolutionStrategy).GetMethod("ResolveLazyEnumerable",
                BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly);

        /// <summary>
        /// Do the PreBuildUp stage of construction. This is where the actual work is performed.
        /// </summary>
        /// <param name="context">Current build context.</param>
        public override void PreBuildUp(ref BuilderContext context)
        {
            if (!IsResolvingIEnumerable(context.RegistrationType))
            {
                return;
            }

            MethodInfo resolverMethod;
            var typeToBuild = GetTypeToBuild(context.RegistrationType);

            if (IsResolvingLazy(typeToBuild))
            {
                typeToBuild = GetTypeToBuild(typeToBuild);
                resolverMethod = GenericResolveLazyEnumerableMethod.MakeGenericMethod(typeToBuild);
            }
            else
            {
                resolverMethod = GenericResolveEnumerableMethod.MakeGenericMethod(typeToBuild);
            }

            var resolver = (Resolver) Delegate.CreateDelegate(typeof(Resolver), resolverMethod);
            context.Existing = resolver(context);
            context.BuildComplete = true;
        }

        private static Type GetTypeToBuild(Type type)
        {
            return type.GetGenericArguments()[0];
        }

        private static bool IsResolvingIEnumerable(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>);
        }

        private static bool IsResolvingLazy(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Lazy<>);
        }

        private static object ResolveLazyEnumerable<T>(BuilderContext context)
        {
            var typeToBuild = typeof(T);
            var typeWrapper = typeof(Lazy<T>);

            return ResolveAll(context, typeToBuild, typeWrapper).OfType<Lazy<T>>().ToList();
        }

        private static object ResolveEnumerable<T>(BuilderContext context)
        {
            var typeToBuild = typeof(T);

            return ResolveAll(context, typeToBuild, typeToBuild).OfType<T>().ToList();
        }

        private static IEnumerable<object> ResolveAll(BuilderContext context, Type type, Type typeWrapper)
        {
            var names = GetRegisteredNames(context, type);

            if (type.IsGenericType)
            {
                names = names.Concat(GetRegisteredNames(context, type.GetGenericTypeDefinition()));
            }

            return names.Distinct()
                .Select(t => t.Name)
                .Select(name => context.Resolve(typeWrapper, name));
        }

        private static IEnumerable<IContainerRegistration> GetRegisteredNames(BuilderContext context, Type type)
        {
            return context.Container.Registrations.Where(t => t.RegisteredType == type);
        }
    }
}

