using Murta.DatabaseGenerator.Annotations;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Murta.DatabaseGenerator
{
    public class Generator
    {
        protected IDbConnection connection = null;

        public Generator(IDbConnection connection)
        {
            if (connection == null) throw new Exception("Connection instance is null.");

            try
            {
                this.connection = connection;

                if (this.connection.State != ConnectionState.Open)
                {
                    this.connection.Open();
                }
            }
            catch (DataException)
            {
                throw;
            }
            finally
            {
                this.connection.Close();
            }
        }

        public void GenerateSchema(IEnumerable<Type> classesToTable)
        {
            var command = this.connection.CreateCommand();

            try
            {
                this.connection.Open();
                
                var databaseScript = new StringBuilder();
                var foreignKeysStatements = new StringBuilder();

                foreach (var classToTable in classesToTable)
                {
                    var tableAnnotation = (Table)System.Attribute.GetCustomAttributes(classToTable, typeof(Table))[0];
                    var tableName = tableAnnotation.Name;

                    databaseScript.Append(string.Format(" CREATE TABLE {0} (", tableName));

                    var properties = classToTable.GetProperties();
                    foreach (var property in properties)
                    {
                        databaseScript.Append(this.GenerateCollumn(property));
                    }
                     
                    var indexLastComma = databaseScript.ToString().LastIndexOf(',');
                    databaseScript.Remove(indexLastComma, 1);
                    databaseScript.Append("); ");

                    databaseScript.Append(this.GeneratePrimaryKey(properties, tableName));
                    foreignKeysStatements.Append(this.GenerateForeignKeys(properties, tableName));
                }

                databaseScript.Append(foreignKeysStatements);

                command.CommandText = databaseScript.ToString();

                command.ExecuteNonQuery();
            }
            catch (DataException)
            {
                throw;
            }
            finally
            {
                if (this.connection.State == ConnectionState.Open)
                {
                    this.connection.Close();    
                }

                command.Dispose();
            }
        }

        protected string GenerateCollumn(PropertyInfo property)
        {
            if (property == null)
            {
                throw new ArgumentException("Invalid arguments.");    
            }

            if (System.Attribute.GetCustomAttributes(property, typeof(Column)).Length != 0)
            {
                var propertyAnnotation = (Column)System.Attribute.GetCustomAttributes(property, typeof(Column))[0];
                var columnName = propertyAnnotation.Name;
                var columnType = propertyAnnotation.Type;

                return string.Format("{0} {1} {2} ", columnName, columnType.ToString(), ", ");    
            }
            else
            {
                return string.Empty;
            }            
        }

        protected string GeneratePrimaryKey(IEnumerable<PropertyInfo> properties, string tableName)
        {
            if (properties == null || properties.Count() == 0 || string.IsNullOrEmpty(tableName))
            {
                throw new ArgumentException("Invalid arguments");
            }

            var primaryKeyStatement = new StringBuilder();
            var primaryKeyColumns = new StringBuilder();
            int index = 0;
            foreach (var property in properties)
            {
                if (this.IsPrimaryKey(property))
                {
                    var propertyPrimaryKey = (PrimaryKey)System.Attribute.GetCustomAttributes(property, typeof(PrimaryKey))[0];

                    if (propertyPrimaryKey != null)
                    {
                        var propertyAnnotation = (Column)System.Attribute.GetCustomAttributes(property, typeof(Column))[0];
                        primaryKeyColumns.Append(string.Format(" {0} ", propertyAnnotation.Name, index == properties.Count() ? " " : ", "));
                        index++;
                    }
                }
            }

            primaryKeyStatement.Append(string.Format("ALTER TABLE {0} ADD PRIMARY KEY ( {1} );", tableName, primaryKeyColumns.ToString()));
            return primaryKeyStatement.ToString();
        }
    
        protected string GenerateForeignKeys(IEnumerable<PropertyInfo> properties, string tableName)
        {
            if (properties == null || properties.Count() == 0 || string.IsNullOrEmpty(tableName))
            {
                throw new ArgumentException("Invalid arguments");
            }

            var foreignKeyStatement = new StringBuilder();

            var referenceTypeProperties = properties.Where(p => p.PropertyType.IsClass && p.PropertyType != typeof(string));           

            foreach (var property in referenceTypeProperties)
            {
                var foreignKeyProperties = property.PropertyType.GetProperties();

                foreach (var foreignProperty in foreignKeyProperties)
                {                    
                    var foreignColumn = (Column)System.Attribute.GetCustomAttributes(foreignProperty, typeof(Column))[0];
                    var foreignPrimaryKey = property.PropertyType.GetProperties().Where(p => this.IsPrimaryKey(p)).FirstOrDefault();

                    var column = (Column)System.Attribute.GetCustomAttributes(foreignPrimaryKey, typeof(Column))[0];

                    if (this.IsManyToManyRelationship(foreignPrimaryKey))
                    {
                        //TODO: building...
                    }
                    else
                    {
                        if (column.Name == foreignColumn.Name && column.Type == foreignColumn.Type && this.IsPrimaryKey(foreignProperty))
                        {
                            var foreignType = (Table)System.Attribute.GetCustomAttributes(property.PropertyType, typeof(Table))[0];


                            foreignKeyStatement.Append(string.Format(" ALTER TABLE {0} ADD CONSTRAINT FK_{0}_{2}_{1} FOREIGN KEY ( {1} ) REFERENCES  {2} ({3});",
                                                        tableName,
                                                        column.Name,
                                                        foreignType.Name,
                                                        foreignColumn.Name));
                        }
                    }
                }
            }

            return foreignKeyStatement.ToString();
        }

        protected bool IsManyToManyRelationship(PropertyInfo property)
        {
            //TODO: building...
            return false;
        }

        protected bool IsPrimaryKey(PropertyInfo property)
        {
            return System.Attribute.GetCustomAttributes(property, typeof(PrimaryKey)).Length != 0;
        }
    }
}
