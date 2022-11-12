using System;
using System.Collections.Generic;
using HTTPResponse.Controllers;
using System.Reflection;
using System.Data;
using System.Data.SqlClient;
using HTTPResponse.Attributes;
using System.Linq;

namespace HTTPResponse.DAO
{
    internal class UserDAO : DAO<User, int>
    {
        private string connectionString;
        public UserDAO(string conStr)
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
        public bool Create(User obj)
        {
            try
            {
                string expr2 = $" VALUES (";
                string expr1 = $"INSERT INTO [{typeof(User).Name}] (";
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
                ExecuteNonQuery<User>(expr);
                return true;
            }
            catch (SqlException e)
            {
                Console.WriteLine(e);
                return false;
            }
        }
        public List<User> GetAll()
        {
            try
            {
                string expr;
                expr = $"select * from [{ typeof(User).Name }]";
                return ExecuteReader<User>(expr);
            }
            catch (SqlException e)
            {
                Console.WriteLine(e);
                return null;
            }
        }
        public bool Delete(int id)
        {
            try
            {
                string expr;
                expr = $"DELETE FROM [User] WHERE user_id = { id }";
                ExecuteNonQuery<User>(expr);
                return true;
            }
            catch (SqlException e)
            {
                Console.WriteLine(e);
                return false;
            }
        }
        public User Update(User obj)
        {
            try
            {
                var propNames = obj.GetType().GetProperties().ToList();
                var propNamesPK = propNames.Where(p => p.IsDefined(typeof(Key))).ToList();
                var propNamesNonPK = propNames.Except(propNamesPK).ToList();
                string expr;
                expr = $"UPDATE {typeof(User).Name} SET";
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
                ExecuteNonQuery<User>(expr);
                return obj;
            }
            catch (SqlException e)
            {
                Console.WriteLine(e);
                return null;
            }
        }
        public User GetEntityById(int id)
        {
            string expr = $"SELECT * FROM [ User ] WHERE user_id = { id }";
            return ExecuteReader<User>(expr).FirstOrDefault();
        }
    }
}
