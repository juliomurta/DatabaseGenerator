using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Collections.Generic;
using Murta.DatabaseGenerator.Test.Models;
using System.Reflection;
using MySql.Data.MySqlClient;

namespace Murta.DatabaseGenerator.Test
{
    [TestClass]
    public class GeracaoSchemaQuery
    {
        [TestMethod]
        public void GenerateSchemaSQL()
        {
            var connection = GenerateConnection();

            var tipos = new List<Type>();
            tipos.AddRange(MappingClasses("Murta.DatabaseGenerator.Test.Models"));

            var databaseGenerator = new Generator(connection);
            databaseGenerator.GenerateSchema(tipos);
        }

        private static MySqlConnection GenerateConnection()
        {
            var connectionString = new MySqlConnectionStringBuilder();
            connectionString.Database = "TEST_GENERATOR";
            connectionString.UserID = "root";
            connectionString.Password = "123456";
            connectionString.Server = "localhost";
            connectionString.Port = 3306;

            var connection = new MySqlConnection();
            connection.ConnectionString = connectionString.ConnectionString;
            return connection;
        }

        public IEnumerable<Type> MappingClasses(string nameSpace)
        {
            return Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsClass && t.Namespace == nameSpace).AsEnumerable<Type>();
        }
    }
}
