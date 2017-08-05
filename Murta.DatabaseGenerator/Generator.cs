using Murta.DatabaseGenerator.Annotations;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Murta.DatabaseGenerator.Utils;

namespace Murta.DatabaseGenerator
{
    public class Generator
    {
        protected IDbConnection connection = null;
        protected IEnumerable<Type> classesToTable = null;

        public Generator(IDbConnection connection, IEnumerable<Type> classesToTable)
        {
            if (connection == null)
            {
                throw new ArgumentException("Connection instance is null.");
            }

            if (classesToTable == null)
            {
                throw new ArgumentException("List of types is not defined.");
            }

            this.classesToTable = classesToTable;
            this.connection = connection;            
        }

        public void GenerateSchema()
        {            
            try
            {
                using (var command = this.connection.CreateCommand())
                {
                    if (this.connection.State != ConnectionState.Open)
                    {
                        this.connection.Open();
                    }

                    var databaseScript = new StringBuilder();
                    var foreignKeysStatements = new StringBuilder();

                    foreach (var classToTable in this.classesToTable)
                    {
                        var tableAnnotation = (Table)System.Attribute.GetCustomAttributes(classToTable, typeof(Table))[0];
                        var tableName = tableAnnotation.Name;

                        databaseScript.Append(string.Format(" CREATE TABLE {0} (", tableName));

                        var properties = classToTable.GetProperties();
                        foreach (var property in properties)
                        {
                            databaseScript.Append(this.GenerateCollumn(property, false));
                        }

                        var indexLastComma = databaseScript.ToString().LastIndexOf(',');
                        databaseScript.Remove(indexLastComma, 1);
                        databaseScript.Append("); ");

                        databaseScript.Append(this.GeneratePrimaryKey(properties, tableName));
                        foreignKeysStatements.Append(this.GenerateForeignKeys(properties, tableName));
                    }

                    databaseScript.Append(foreignKeysStatements);
                    databaseScript = databaseScript.DeleteLastComma();
                    command.CommandText = databaseScript.ToString();

                    command.ExecuteNonQuery();
                }
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
            }
        }

        protected string GenerateCollumn(PropertyInfo property, bool isForeignKey)
        {
            if (property == null)
            {
                throw new ArgumentException("Invalid arguments.");    
            }

            if (System.Attribute.GetCustomAttributes(property, typeof(Column)).Length != 0)
            {
                var propertyAnnotation = (Column)System.Attribute.GetCustomAttributes(property, typeof(Column))[0];
                var columnType = propertyAnnotation.Type;
                var columnName = string.Empty;

                if (isForeignKey)
                {
                    columnName = property.DeclaringType.Name.ToUpper() + "_" + propertyAnnotation.Name;
                }
                else
                {
                    columnName = propertyAnnotation.Name;
                }

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
                    if (this.IsManyToManyRelationship(foreignProperty))
                    {
                        var firstPrimaryKey = property.PropertyType.GetProperties().Where(p => this.IsPrimaryKey(p)).FirstOrDefault();
                        var secondPrimaryKey = property.DeclaringType.GetProperties().Where(p => this.IsPrimaryKey(p)).FirstOrDefault();

                        foreignKeyStatement.AppendFormat("CREATE TABLE {0} ( ", 
                                                            firstPrimaryKey.DeclaringType.Name.Substring(0,3).ToUpper() + "_" +
                                                            secondPrimaryKey.DeclaringType.Name.Substring(0, 3).ToUpper());

                        foreignKeyStatement.Append(this.GenerateCollumn(firstPrimaryKey, true));
                        foreignKeyStatement.Append(this.GenerateCollumn(secondPrimaryKey, true));
                        foreignKeyStatement = foreignKeyStatement.DeleteLastComma();
                        foreignKeyStatement.Append(" ) ");
                    }
                    else
                    {
                        var foreignColumn = (Column)System.Attribute.GetCustomAttributes(foreignProperty, typeof(Column))[0];
                        var foreignPrimaryKey = property.PropertyType.GetProperties().Where(p => this.IsPrimaryKey(p)).FirstOrDefault();

                        var column = (Column)System.Attribute.GetCustomAttributes(foreignPrimaryKey, typeof(Column))[0];

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
            var isForeignMappedOnList = this.classesToTable.Where(t => t.Equals(property.PropertyType)).Any();
            var isDeclaringType = this.classesToTable.Where(t => t.Equals(property.DeclaringType)).Any();

            return isForeignMappedOnList && isDeclaringType;
        }

        protected bool IsPrimaryKey(PropertyInfo property)
        {
            return System.Attribute.GetCustomAttributes(property, typeof(PrimaryKey)).Length != 0;
        }
    }
}
