using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Abitech.NextApi.Server.Base
{
    /// <summary>
    /// useful utils for nextapi entity services
    /// </summary>
    public static class NextApiUtils
    {
        private static string FirstCharToUpper(string input)
        {
            switch (input)
            {
                case null: throw new ArgumentNullException(nameof(input));
                case "": throw new ArgumentException($@"{nameof(input)} cannot be empty", nameof(input));
                default: return input.First().ToString().ToUpper() + input.Substring(1);
            }
        }

        private static bool IsNullableType(Type type)
        {
            return type.IsGenericType
                   && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        /// <summary>
        /// Patches entity
        /// </summary>
        /// <param name="patch"></param>
        /// <param name="entity"></param>
        /// <exception cref="Exception"></exception>
        public static void PatchEntity(object patch, object entity)
        {
            if (patch == null)
            {
                throw new ArgumentNullException(nameof(patch));
            }

            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            foreach (var patchProperty in patch.GetType().GetProperties().Where(p => p.GetMethod.IsPublic))
            {
                var propNameAsIs = patchProperty.Name;
                var propNameUpper = FirstCharToUpper(propNameAsIs);
                var value = patchProperty.GetMethod.Invoke(patch, null);
                var entityProp = entity.GetType().GetProperty(propNameUpper) ??
                                 entity.GetType().GetProperty(propNameAsIs);
                if (entityProp == null)
                    throw new Exception($"Unknown property detected {propNameAsIs}");

                if (entityProp.GetGetMethod().IsVirtual)
                {
                    continue;
                }

                try
                {
                    #region Type mappings

                    switch (value)
                    {
                        case DateTime time when entityProp.PropertyType == typeof(DateTimeOffset):
                        {
                            var offsetValue = new DateTimeOffset(time);
                            entityProp.SetValue(entity, offsetValue);
                            continue;
                        }
                        case DateTime time when entityProp.PropertyType == typeof(DateTimeOffset?):
                        {
                            var offsetValue = new DateTimeOffset?(new DateTimeOffset(time));
                            entityProp.SetValue(entity, offsetValue);
                            continue;
                        }
                        case string input when entityProp.PropertyType == typeof(Guid):
                        {
                            var guid = Guid.Parse(input);
                            entityProp.SetValue(entity, guid);
                            continue;
                        }
                    }

                    #endregion

                    // handle int? vs int distinction
                    if (value != null)
                    {
                        var targetType = IsNullableType(entityProp.PropertyType)
                            ? Nullable.GetUnderlyingType(entityProp.PropertyType)
                            : entityProp.PropertyType;
                        var convertedValue = Convert.ChangeType(value, targetType);
                        entityProp.SetValue(entity, convertedValue, null);
                    }
                    else
                    {
                        entityProp.SetValue(entity, null);
                    }

                    //entityProp.SetValue(entity, Convert.ChangeType(value, entityProp.PropertyType));
                }
                catch
                {
                    throw new Exception($"Entity patching error {propNameAsIs}: {value}");
                }
            }
        }

        /// <summary>
        /// Expand entity from expand string (aka Include)
        /// </summary>
        /// <param name="source">Entity source</param>
        /// <param name="expand">Expand string array</param>
        /// <typeparam name="T">Entity type</typeparam>
        public static IQueryable<T> Expand<T>(this IQueryable<T> source, string[] expand) where T : class
        {
            return expand == null
                ? source
                : expand.Aggregate(source, (current, expandNode) => current.Include(expandNode));
        }
    }
}
