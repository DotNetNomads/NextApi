using System;

namespace Abitech.NextApi.Server.Attributes
{
    /// <inheritdoc />
    /// <summary>
    /// Attribute enables NextApi service only for anonymous users.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class NextApiAnonymousAttribute : Attribute
    {
        /// <inheritdoc />
        /// <summary>
        /// Attribute enables NextApi service only for anonymous users.
        /// </summary>
        public NextApiAnonymousAttribute()
        {
        }
    }
}