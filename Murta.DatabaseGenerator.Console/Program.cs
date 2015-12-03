using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Murta.DatabaseGenerator.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            DbConnection databaseConnection = null;

            try
            {
                var databaseName = GetConsoleValue("Input the database name - REQUIRED: (propertyname-value)", true);
                var user = GetConsoleValue("Input the database user name - REQUIRED: (propertyname-value)", true);
                var password = GetConsoleValue("Input the database password - REQUIRED: (propertyname-value)", true);
                var serverInstance = GetConsoleValue("Input the server instance name - REQUIRED: (propertyname-value)", true);
                var port = GetConsoleValue("Input the database port: (propertyname-value)", false);
                var databaseProviderPath = GetConsoleValue("Input the database provider dll path - REQUIRED: ", true);
                var databaseProviderNamespace = GetConsoleValue("Input the database provider namespace - REQUIRED: ", true);

                var providerInfo = new ProviderInfo();

                providerInfo.DatabaseName = new KeyValuePair<string, string>(databaseName.Split('-')[0], databaseName.Split('-')[1]);
                providerInfo.ServerInstance = new KeyValuePair<string, string>(serverInstance.Split('-')[0], serverInstance.Split('-')[1]);
                providerInfo.Username = new KeyValuePair<string, string>(user.Split('-')[0], user.Split('-')[1]);
                providerInfo.Password = new KeyValuePair<string, string>(password.Split('-')[0], password.Split('-')[1]);
                providerInfo.ProviderNamespace = databaseProviderNamespace;

                if (!string.IsNullOrEmpty(port))
                {
                    providerInfo.Port = new KeyValuePair<string, int>(port.Split('-')[0], Convert.ToInt32(port.Split('-')[1]));
                }

                var provider = new DatabaseProvider(databaseProviderPath);

                databaseConnection = provider.GetDatabaseConnectionInstance(providerInfo);

                System.Console.WriteLine("Database Connected Succefully!!!");

                var types = new List<Type>();
                types.AddRange(MappingClasses("Murta.DatabaseGenerator.Console.Models"));

                var databaseGenerator = new Generator(databaseConnection, types);
                databaseGenerator.GenerateSchema();

                System.Console.WriteLine("Schema generated succefully!!");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("An error ocurred while generating the database schema: " + ex.Message);
            }
            finally
            {
                if (databaseConnection != null)                
                {
                    if (databaseConnection.State != System.Data.ConnectionState.Closed)
                    {
                        databaseConnection.Close();
                        databaseConnection.Dispose();
                    }
                }
            }

            System.Console.ReadKey();
        }

        static string GetConsoleValue(string consoleMessage, bool required)
        {
            if (string.IsNullOrEmpty(consoleMessage))
            {
                throw new Exception("You need define an console message.");
            }

            var consoleValue = string.Empty;

            do
            {
                System.Console.WriteLine(consoleMessage);
                consoleValue = System.Console.ReadLine();

                if (string.IsNullOrEmpty(consoleValue) && required)
                {
                    System.Console.WriteLine("Please write something.");
                }

            } while (string.IsNullOrEmpty(consoleValue) && required);

            return consoleValue;
        }
        static IEnumerable<Type> MappingClasses(string nameSpace)
        {
            return Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsClass && t.Namespace == nameSpace).AsEnumerable<Type>();
        }
    }
}
