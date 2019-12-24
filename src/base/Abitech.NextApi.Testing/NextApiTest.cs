using System;
using Abitech.NextApi.Client;
using Abitech.NextApi.Testing;

namespace Abitech.NextApi.Server.Tests
{
    /// <summary>
    /// Base class for all NextApi tests
    /// </summary>
    /// <typeparam name="TNextApiApplication">Type for NextApi Application</typeparam>
    /// <typeparam name="TClient">Type of NextApiClient</typeparam>
    public abstract class NextApiTest<TNextApiApplication, TClient> : IDisposable
        where TNextApiApplication : INextApiApplication<TClient>, new()
        where TClient : class, INextApiClient
    {
        /// <summary>
        /// NextApi application instance
        /// </summary>
        protected INextApiApplication<TClient> App { get; }

        /// <inheritdoc />
        protected NextApiTest()
        {
            App = new TNextApiApplication();
        }

        /// <inheritdoc />
        public void Dispose() => App.Dispose();
    }
}
