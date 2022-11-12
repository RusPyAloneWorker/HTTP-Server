using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Data;
using System.Data.SqlClient;
using HTTPResponse.Attributes;

namespace HTTPResponse.MyORM
{
    public class ORM
    {
        private string connectionString;
        public ORM(string conStr)
        {
            connectionString = conStr;
        }
        private List<T> ExecuteReader<T>(string expr)
        {
            //string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=ProjectORISDB;Integrated Security=True";
            List<T> result = new List<T>();
            Type t = typeof(T);

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(expr, connection);
                connection.Open();
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    T obj = (T)Activator.CreateInstance(t);
                    t.GetProperties().ToList().ForEach(p =>
                    {
                        p.SetValue(obj, reader[p.Name]);
                    });
                    result.Add(obj);
                }
                connection.Close();
                return result;
            }
        }
        private void ExecuteNonQuery<T>(string expr)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(expr, connection);
                int number = command.ExecuteNonQuery();
            }
        }
        private T ExecuteScalar<T>(string expr)
        {
            T result = default(T);
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(expr, connection);
                result = (T)command.ExecuteScalar();

            }
            return result;
        }
        internal bool Insert<T>(T obj)
        {
            try
            {
                string expr2 = $" VALUES (";
                string expr1 = $"INSERT INTO [{typeof(T).Name}] (";
                var propNames = obj.GetType().GetProperties(); 
                for (var i = 0; i < propNames.Length; i++)
                {
                    if (propNames[i].IsDefined(typeof(Key)))
                        continue;
                    expr1 += propNames[i].Name + ((i == propNames.Length - 1) ? ")" : ", ");
                    
                    string valueStr = Convert.ToString(propNames[i].GetValue(obj));
                    if (propNames[i].GetValue(obj) is DateTime || propNames[i].GetValue(obj) is String)
                        valueStr = "'" + valueStr + "'";
                    expr2 += valueStr + ((i == propNames.Length - 1) ? ")" : ", ");
                }
                string expr = expr1 + expr2; 
                ExecuteNonQuery<T>(expr);
                return true;
            }
            catch (SqlException e)
            {
                Console.WriteLine(e);
                return false;
            }
        }
        internal List<T> Select<T>()
        {
            string expr;
            expr = $"select * from [{ typeof(T).Name }]";
            return ExecuteReader<T>(expr);
        }
        internal bool Delete<T>(T obj)
        {
            try
            {
                string expr;
                expr = $"DELETE FROM [{typeof(T).Name}] WHERE ";
                var propNames = obj.GetType().GetProperties();
                for (int i = 0; i < propNames.Length; i++)
                {
                    var value = Convert.ToString( propNames[i].GetValue(obj) );
                    if (value is DateTime || value is String)
                        value = "'" + value + "'";
                    expr += Convert.ToString(propNames[i].Name) + "=" + value
                         + ((i == propNames.Length - 1) ? "" : " AND ");
                    /// Или через первичные ключи удалять
                }
                ExecuteNonQuery<T>(expr);
                return true;
            }
            catch (SqlException e)
            {
                Console.WriteLine(e);
                return false;
            }
        }
        internal bool Update<T>(T obj)
        {
            try
            {
                var propNames = obj.GetType().GetProperties().ToList();
                var propNamesPK = propNames.Where(p => p.IsDefined(typeof(Key))).ToList();
                var propNamesNonPK = propNames.Except(propNamesPK).ToList();
                string expr;
                expr = $"UPDATE {typeof(T).Name} SET";
                for (var i = 0; i < propNamesNonPK.Count(); i++)
                {
                    var value = Convert.ToString(propNamesNonPK[i].GetValue(obj));
                    if (value is DateTime || value is String)
                        value = "'" + value + "'" + ((i == propNamesNonPK.Count() - 1) ? " " : " , ");
                    expr += propNamesNonPK[i].Name + "=" + value;
                }
                expr += "WHERE ";
                for (var i = 0; i < propNamesPK.Count(); i++)
                {
                    var value = Convert.ToString(propNamesPK[i].GetValue(obj));
                    if (value is DateTime || value is String)
                        value = "'" + value + "'" + ((i == propNamesPK.Count() - 1) ? " " : " , ");
                    expr += propNamesNonPK[i].Name + "=" + value;
                }
                ExecuteNonQuery<T>(expr);
                return true;
            }
            catch (SqlException e)
            {
                Console.WriteLine(e);
                return false;
            }
        }
    }
}
