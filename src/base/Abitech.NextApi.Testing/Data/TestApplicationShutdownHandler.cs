using System;
using Abitech.NextApi.Server.EfCore.DAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;

namespace Abitech.NextApi.Testing.Data
{
    /// <summary>
    /// Handles moment when the test application goes start or shutdown
    /// </summary>
    public class TestApplicationStatesHandler<TDbContext> : ITestApplicationStatesHandler
        where TDbContext : class, INextApiDbContext
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly string _dbHost;
        private readonly string _dbPort;
        private readonly string _dbUser;
        private readonly string _dbPassword;
        private readonly string _dbName;
        private readonly string _dbAdditional;
        private bool _isInitializedBefore = false;

        /// <inheritdoc />
        public TestApplicationStatesHandler(IServiceProvider serviceProvider, string dbHost, string dbPort,
            string dbUser, string dbPassword, string dbName, string dbAdditional)
        {
            _serviceProvider = serviceProvider;
            _dbHost = dbHost;
            _dbPort = dbPort;
            _dbUser = dbUser;
            _dbPassword = dbPassword;
            _dbName = dbName;
            _dbAdditional = dbAdditional;
        }

        /// <inheritdoc />
        public void Start()
        {
            if (_isInitializedBefore)
                return;
            using var scope = _serviceProvider.CreateScope();
            if (scope.ServiceProvider.GetService<TDbContext>() is DbContext dbContext)
            {
                dbContext.Database.EnsureCreated();
                _isInitializedBefore = true;
            }
            else throw new InvalidOperationException("Only EF Core DbContexts are supported");
        }

        /// <inheritdoc />
        public void Shutdown()
        {
            if (!_isInitializedBefore)
                return;
            var connection =
                new MySqlConnection(
                    $"Server={_dbHost};Port={_dbPort};User={_dbUser};Database=mysql;Password={_dbPassword};{_dbAdditional}");
            try
            {
                ExecuteInMysql(connection, $"DROP DATABASE `{_dbName}`");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }
        }

        private static void ExecuteInMysql(MySqlConnection connection, string query)
        {
            var command = connection.CreateCommand();
            command.CommandText = query;
            command.ExecuteNonQuery();
        }
    }

    /// <summary>
    /// Handles moment when the test application goes start or shutdown
    /// </summary>
    public interface ITestApplicationStatesHandler
    {
        /// <summary>
        /// Application is going to start state
        /// </summary>
        /// <returns></returns>
        void Start();

        /// <summary>
        /// Application is going to shutdown state
        /// </summary>
        /// <returns></returns>
        void Shutdown();
    }
}
