using System;
using System.Collections.Generic;
using System.Linq;
using NextApi.Common.Entity;

namespace Abitech.NextApi.UploadQueue.Common.UploadQueue
{
    /// <summary>
    /// UploadQueue helper methods to be used at both server and client sides
    /// </summary>
    public static class UploadQueueActions
    {
        /// <summary>
        /// Apply UploadQueue update operations on top of an entity instance
        /// </summary>
        /// <param name="entity">Entity instance</param>
        /// <param name="modifications">List of update operations</param>
        /// <typeparam name="T">Type of the entity instance</typeparam>
        /// <returns>Dictionary of rejected operations with operation Id and an exception</returns>
        /// <exception cref="Exception">Throws if RowGuid property is unable to be resolved</exception>
        public static Dictionary<Guid, Exception> ApplyModifications<T>(this T entity, IEnumerable<UploadQueueDto> modifications)
        where T: class, IEntity<Guid>
        {
            var entityType = typeof(T);
            var sort = modifications
                .Where(m => m.EntityRowGuid == entity.Id && m.EntityName == entityType.Name && m.OperationType == OperationType.Update)
                .OrderBy(m => m.OccuredAt);

            var rejectedModifications = new Dictionary<Guid, Exception>(); // modification Id and reason
            var guidType = typeof(Guid);
            var nullableGuidType = typeof(Guid?);
            foreach (var modification in sort)
            {
                try
                {
                    var prop = entityType.GetProperty(modification.ColumnName);
                    if (prop == null)
                        continue;

                    var propType = prop.PropertyType;
                    if ((propType == guidType || propType == nullableGuidType)
                        && modification.NewValue != null
                        && modification.NewValue is string value)
                        modification.NewValue = new Guid(value);

                    var underlyingType = Nullable.GetUnderlyingType(propType);
                    if (underlyingType == null)
                    {
                        // underlying type is null, which means it is not a nullable type
                        prop.SetValue(entity, Convert.ChangeType(modification.NewValue, prop.PropertyType));
                    }
                    else
                    {
                        // it is a nullable type
                        if (modification.NewValue == null)
                            prop.SetValue(entity, null);
                        else
                            prop.SetValue(entity, Convert.ChangeType(modification.NewValue, underlyingType));
                    }
                    
                }
                catch (Exception e)
                {
                    rejectedModifications.Add(modification.Id, e);
                }
            }

            return rejectedModifications;
        }
        
        /// <summary>
        /// Resolve property value of an object instance by property name
        /// </summary>
        /// <param name="entityInstance">Object</param>
        /// <param name="propertyName">Property name</param>
        /// <typeparam name="T">Expected type</typeparam>
        /// <returns>Value of the property</returns>
        /// <exception cref="Exception"></exception>
        public static T ResolveProperty<T>(object entityInstance, string propertyName)
        {
            var property = entityInstance.GetType().GetProperty(propertyName);
            if (property == null)
                throw new Exception($"Could not resolve property {propertyName}");
            
            if (property.PropertyType != typeof(T))
                throw new Exception($"Property actual type: {property.PropertyType.Name}, expected: {typeof(T).Name}!");
            
            var propValue = property.GetValue(entityInstance);
            return (T) propValue;
        }
    }
}
