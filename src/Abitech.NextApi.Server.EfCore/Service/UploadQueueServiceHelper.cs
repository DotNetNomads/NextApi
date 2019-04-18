using System;
using System.Linq.Expressions;

namespace Abitech.NextApi.Server.EfCore.Service
{
    public static class UploadQueueServiceHelper
    {
        /// <summary>
        /// Determines whether this type can be assigned to the supplied generic type
        /// </summary>
        /// <param name="givenType">Implementation type</param>
        /// <param name="genericType">Generic type</param>
        /// <returns>Boolean</returns>
        public static bool IsAssignableToGenericType(this Type givenType, Type genericType)
        {
            var interfaceTypes = givenType.GetInterfaces();

            foreach (var it in interfaceTypes)
            {
                if (it.IsGenericType && it.GetGenericTypeDefinition() == genericType)
                    return true;
            }

            if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
                return true;

            var baseType = givenType.BaseType;
            return baseType != null && IsAssignableToGenericType(baseType, genericType);
        }

        /// <summary>
        /// Resolves "equals" lambda expression
        /// </summary>
        /// <param name="type">Parameter type</param>
        /// <param name="compareFieldName">Name of property to compare</param>
        /// <param name="compareTo">Compare to this object</param>
        /// <returns>LambdaExpression as object</returns>
        public static object GetEqualsExpression(
            Type type, 
            string compareFieldName, 
            object compareTo)
        {
            var compareConstant = Expression.Constant(compareTo);
            var param = Expression.Parameter(type);
            var property = Expression.PropertyOrField(param, compareFieldName);
            var body = Expression.Equal(property, compareConstant);
            
            var funcType = typeof(Func<,>).MakeGenericType(type, typeof(bool));
            
            var lambda = Expression.Lambda(funcType, body, param);
            return lambda;
        }
    }
}
