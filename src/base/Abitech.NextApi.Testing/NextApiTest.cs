using System;
using Abitech.NextApi.Client;
using Abitech.NextApi.Testing.Data;
using MartinCostello.Logging.XUnit;
using Xunit.Abstractions;

namespace Abitech.NextApi.Testing
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

        /// <param name="output"></param>
        /// <inheritdoc />
        protected NextApiTest(ITestOutputHelper output)
        {
            App = new TNextApiApplication {Output = output};
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
