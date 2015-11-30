using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Murta.DatabaseGenerator.Console
{
    public class DatabaseProvider
    {
        protected Assembly assemblyProvider = null;
        
        public DatabaseProvider(string provider)
        {
            try
            {
                if (string.IsNullOrEmpty(provider))
                {
                    throw new Exception("Invalid provider.");
                }

                this.assemblyProvider = Assembly.Load(provider);
            }
            catch(Exception)
            {
                throw;
            }
        }       

        public DbConnection GetDatabaseConnectionInstance(ProviderInfo providerInfo)
        {
            object databaseConnectionInstance = null;

            try
            {
                var connectionStringBuilderClass = this.GetDerivatedType<DbConnectionStringBuilder>(providerInfo);

                var databaseConnectionStringInstance = Activator.CreateInstance(connectionStringBuilderClass);
                databaseConnectionStringInstance.GetType().GetProperty(providerInfo.ServerInstance.Key).SetValue(databaseConnectionStringInstance, providerInfo.ServerInstance.Value);
                databaseConnectionStringInstance.GetType().GetProperty(providerInfo.DatabaseName.Key).SetValue(databaseConnectionStringInstance, providerInfo.DatabaseName.Value);
                databaseConnectionStringInstance.GetType().GetProperty(providerInfo.Username.Key).SetValue(databaseConnectionStringInstance, providerInfo.Username.Value);
                databaseConnectionStringInstance.GetType().GetProperty(providerInfo.Password.Key).SetValue(databaseConnectionStringInstance, providerInfo.Password.Value);

                if (providerInfo.Port.Value != 0)
                {
                    databaseConnectionStringInstance.GetType().GetProperty(providerInfo.Port.Key).SetValue(databaseConnectionStringInstance, Convert.ToUInt32(providerInfo.Port.Value));   
                }

                var generatedConnectionString = databaseConnectionStringInstance.GetType().GetProperty("ConnectionString").GetValue(databaseConnectionStringInstance, null).ToString();

                var databaseConnectionInstanceClass = this.GetDerivatedType<DbConnection>(providerInfo);
                databaseConnectionInstance = Activator.CreateInstance(databaseConnectionInstanceClass);
                databaseConnectionInstance.GetType().GetProperty("ConnectionString").SetValue(databaseConnectionInstance, generatedConnectionString);
                databaseConnectionInstance.GetType().GetMethod("Open").Invoke(databaseConnectionInstance, null);
            }
            catch(Exception)
            {
                throw;
            }            

            return (DbConnection)databaseConnectionInstance;
        }
        protected Type GetDerivatedType<T>(ProviderInfo provider)
        {
            return this.assemblyProvider.GetTypes().Where(t => t.Namespace == provider.ProviderNamespace &&
                                                                    t.IsSubclassOf(typeof(T))).FirstOrDefault();
        }
    }
}
