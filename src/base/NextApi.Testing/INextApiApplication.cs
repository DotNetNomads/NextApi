using System;
using NextApi.Client;
using NextApi.Common.Abstractions;
using NextApi.Common.Serialization;
using Xunit.Abstractions;

namespace NextApi.Testing
{
    /// <summary>
    /// Base non-generic interface for NextApi Application
    /// </summary>
    public interface INextApiApplication
    {
        /// <summary>
        /// Provides access to all server's services
        /// </summary>
        IServiceProvider ServerServices { get; }
    }

    /// <summary>
    /// Base interface for NextApi Application
    /// </summary>
    /// <typeparam name="TClient">Type of NextApi client</typeparam>
    public interface INextApiApplication<out TClient> : IDisposable, INextApiApplication
        where TClient : class, INextApiClient
    {
        /// <summary>
        /// Resolve NextApi client
        /// </summary>
        /// <param name="token">Fake token (if required)</param>
        /// <param name="transport">Transport type</param>
        /// <param name="httpSerializationType">Serialization type for http transport type</param>
        /// <returns>Instance of NextApi client</returns>
        TClient ResolveClient(string token = null, NextApiTransport transport = NextApiTransport.Http, SerializationType httpSerializationType = SerializationType.Json);

        /// <summary>
        /// Resolve client-side NextApi service
        /// </summary>
        /// <param name="token">Fake token (if required)</param>
        /// <param name="transport">Transport type</param>
        /// <param name="httpSerializationType">Serialization type for http transport type</param>
        /// <typeparam name="TService">Service type</typeparam>
        /// <returns></returns>
        TService ResolveService<TService>(string token = null, NextApiTransport transport = NextApiTransport.Http, SerializationType httpSerializationType = SerializationType.Json)
            where TService : INextApiService;

        /// <summary>
        /// XUnit output
        /// </summary>
        ITestOutputHelper Output { get; set; }
    }
}
