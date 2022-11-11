﻿using System.Data.SqlClient;
using System.Security.Principal;
using System.Text;
using System.Reflection;

namespace MyORM
{
    public class MyORM
    {
        readonly string connectionString;

        public MyORM(string dataSource, string initialCatalog, bool integratedSecurity)
        {
            connectionString = $"Data Source={dataSource};Initial Catalog={initialCatalog};Integrated Security={integratedSecurity}";
        }

        void ExecuteNonReturnCommand(string sqlExpression)
        {
            using SqlConnection connection = new(connectionString);
            connection.Open();
            SqlCommand command = new(sqlExpression, connection);
            command.ExecuteNonQuery();
        }

        List<T> ExecuteReadCommand<T>(string sqlExpression) where T : class
        {
            var genericType = typeof(T);
            var list = new List<T>();
            var properties = GetDBFieldProperties(genericType);
            var genericConstructor = genericType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, properties.Select(x => x.PropertyType).ToArray());

            if (genericConstructor == null)
                throw new NotImplementedException("Not found constructor for DB table model");

            var propertiesLength = properties.Length;
            var values = new object[properties.Length];

            using (SqlConnection connection = new(connectionString))
            {
                connection.Open();
                SqlCommand command = new(sqlExpression, connection);
                SqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        for (int i = 0; i < propertiesLength; i++)
                            values[i] = reader.GetValue(i);
                        var constrParams = genericConstructor.GetParameters()
                                                            .Select((p, i) => Convert.ChangeType(values[i], p.ParameterType))
                                                            .ToArray();
                        if (genericConstructor.Invoke(constrParams) is T T_object)
                            list.Add(T_object);
                    }
                }
                reader.Close();
            }
            return list;
        }

        public void Insert<T>(T data) where T : class
        {
            ValidateClassHasTableAttribute(typeof(T));

            var sqlExpression = new StringBuilder($"INSERT INTO {typeof(T).Name}");
            var insertProperties = GetDBFieldProperties(data.GetType()).Where(x => !Attribute.IsDefined(x, typeof(DB_Identity)));

            sqlExpression.Append(SqlExpressionBuilder.PackInParentheses(insertProperties.Select(x => x.Name), ","));
            sqlExpression.Append(" VALUES");
            sqlExpression.Append(SqlExpressionBuilder.PackInParentheses(insertProperties.Select(x => $"'{Conversions.ConvertToSqlFormat(x.GetValue(data))}'"), ","));

            ExecuteNonReturnCommand(sqlExpression.ToString());
        }

        public void Update<T>(int id, string changeField, string newValue) where T : class
        {
            var genericType = typeof(T);

            ValidateClassHasTableAttribute(genericType);

            var identityProperty = GetDBIdentityProperty(genericType);
            var sqlExpression = new StringBuilder();
            var property = GetDBFieldProperty(genericType, changeField);

            if (property != null)
                if (property.Name == identityProperty.Name)
                    throw new ChangeOfImmutableAttemptException();
                else
                {
                    sqlExpression.Append($"UPDATE {genericType.Name} SET {changeField} = '{newValue}' WHERE {identityProperty.Name} = {id}");
                    ExecuteNonReturnCommand(sqlExpression.ToString());
                }
        }

        public void Delete<T>(int id) where T : class
        {
            var genericType = typeof(T);

            ValidateClassHasTableAttribute(genericType);

            var idField = GetDBIdentityProperty(genericType);
            var sqlExpression = new StringBuilder();

            sqlExpression.Append($"DELETE FROM {genericType.Name} WHERE {idField.Name} = {id};");

            ExecuteNonReturnCommand(sqlExpression.ToString());
        }

        public List<T> Select<T>() where T : class
        {
            var genericType = typeof(T);

            ValidateClassHasTableAttribute(genericType);

            var result = ExecuteReadCommand<T>($"SELECT * FROM {genericType.Name}");
            return result;
        }

        static void ValidateClassHasTableAttribute(Type type)
        {
            if (!Attribute.IsDefined(type, typeof(DB_Table)))
                throw new TableAttributeException();
        }

        static PropertyInfo GetDBIdentityProperty(Type type)
        {
            var identityProperty = type.GetProperties().Where(x => Attribute.IsDefined(x, typeof(DB_Identity)));
            var count = identityProperty.Count();
            if (count > 1)
                throw new IdentityAttributeException("Multiple identity attributes. Leave only one");
            if (count == 0)
                throw new IdentityAttributeException();
            return identityProperty.First();
        }

        static PropertyInfo[] GetDBFieldProperties(Type type) => type.GetProperties().Where(x => Attribute.IsDefined(x, typeof(DB_Field))).ToArray();

        static PropertyInfo? GetDBFieldProperty(Type type, string name) => GetDBFieldProperties(type).Where(x => x.Name == name).FirstOrDefault();
    }
}