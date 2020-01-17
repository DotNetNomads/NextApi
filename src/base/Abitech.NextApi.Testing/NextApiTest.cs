using System;
using System.Threading.Tasks;
using Abitech.NextApi.Client;
using Abitech.NextApi.Testing;
using Abitech.NextApi.Testing.Data;

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
        private readonly ITestApplicationStatesHandler _applicationStatesHandler;

        /// <summary>
        /// NextApi application instance
        /// </summary>
        protected INextApiApplication<TClient> App { get; }

        /// <inheritdoc />
        protected NextApiTest()
        {
            App = new TNextApiApplication();
            try
            {
                _applicationStatesHandler =
                    (ITestApplicationStatesHandler)App.ServerServices.GetService(typeof(ITestApplicationStatesHandler));
            }
            catch
            {
                // its allright, just ignore. looks like this tests dont uses AddFakeMySqlDbContext
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _applicationStatesHandler?.Shutdown();
            App.Dispose();
        }
    }
}
