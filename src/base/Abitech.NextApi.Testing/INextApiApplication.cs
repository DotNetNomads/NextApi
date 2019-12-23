using System;
using Abitech.NextApi.Client;
using Abitech.NextApi.Common.Abstractions;

namespace Abitech.NextApi.Server.Tests
{
    public interface INextApiApplication<out TClient>: IDisposable
        where TClient : class, INextApiClient
    {
        TClient ResolveClient(string token = null);
        TService ResolveService<TService>(string token = null) where TService : INextApiService;
        IServiceProvider ServerServices { get; }
        
    }
}
