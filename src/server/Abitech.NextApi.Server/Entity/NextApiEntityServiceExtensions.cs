using System;
using System.Linq.Expressions;
using Abitech.NextApi.Model.Filtering;
using AutoMapper;

namespace Abitech.NextApi.Server.Entity
{
    /// <summary>
    /// Useful extensions for NextApiEntityService
    /// </summary>
    public static class NextApiEntityServiceExtensions
    {
        /// <summary>
        /// Create two way map for types (automapper)
        /// </summary>
        /// <param name="profile"></param>
        /// <typeparam name="TFrom"></typeparam>
        /// <typeparam name="TTo"></typeparam>
        public static void CreateTwoWayMap<TFrom, TTo>(this Profile profile)
        {
            profile.CreateMap<TFrom, TTo>(MemberList.None);
            profile.CreateMap<TTo, TFrom>(MemberList.None);
        }
    }
}
