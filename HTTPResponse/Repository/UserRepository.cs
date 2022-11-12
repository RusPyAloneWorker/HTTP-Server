using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using HTTPResponse.Attributes;
using System.Linq;
using HTTPResponse.Controllers;
using System.Reflection;

namespace HTTPResponse.Repository
{
    internal class UserRepository : IRepository<User>
    {
        private string connectionString;
        public UserRepository(string con)
        {
            connectionString = con;
        }
        public void Insert(User entity)
        {
            try
            {
                string expr2 = $" VALUES (";
                string expr1 = $"INSERT INTO [{typeof(User).Name}] (";
                var propNames = typeof(User).GetProperties();
                for (var i = 0; i < propNames.Length; i++)
                {
                    if (propNames[i].IsDefined(typeof(Key)))
                        continue;
                    expr1 += propNames[i].Name + ((i == propNames.Length - 1) ? ")" : ", ");

                    string valueStr = Convert.ToString(propNames[i].GetValue(entity));
                    if (propNames[i].GetValue(entity) is DateTime || propNames[i].GetValue(entity) is String)
                        valueStr = "'" + valueStr + "'";
                    expr2 += valueStr + ((i == propNames.Length - 1) ? ")" : ", ");
                }
                string expr = expr1 + expr2;
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand(expr, connection);
                    int number = command.ExecuteNonQuery();
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine(e);
            }
        }
        public void Update(User entity)
        {
            try
            {
                var propNames = typeof(User).GetProperties().ToList();
                var propNamesPK = propNames.Where(p => p.IsDefined(typeof(Key))).ToList();
                var propNamesNonPK = propNames.Except(propNamesPK).ToList();
                string expr;
                expr = $"UPDATE {typeof(User).Name} SET";
                for (var i = 0; i < propNamesNonPK.Count(); i++)
                {
                    var value = Convert.ToString(propNamesNonPK[i].GetValue(entity));
                    if (value is DateTime || value is String)
                        value = "'" + value + "'" + ((i == propNamesNonPK.Count() - 1) ? " " : " , ");
                    expr += propNamesNonPK[i].Name + "=" + value;
                }
                expr += "WHERE ";
                for (var i = 0; i < propNamesPK.Count(); i++)
                {
                    var value = Convert.ToString(propNamesPK[i].GetValue(entity));
                    if (value is DateTime || value is String)
                        value = "'" + value + "'" + ((i == propNamesPK.Count() - 1) ? " " : " , ");
                    expr += propNamesNonPK[i].Name + "=" + value;
                }
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand(expr, connection);
                    int number = command.ExecuteNonQuery();
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine(e);
            }
        }
        public void Delete(User entity)
        {
            try
            {
                string expr;
                expr = $"DELETE FROM [{typeof(User).Name}] WHERE ";
                var propNames = typeof(User).GetProperties();
                for (int i = 0; i < propNames.Length; i++)
                {
                    var value = Convert.ToString(propNames[i].GetValue(entity));
                    if (value is DateTime || value is String)
                        value = "'" + value + "'";
                    expr += Convert.ToString(propNames[i].Name) + "=" + value
                         + ((i == propNames.Length - 1) ? "" : " AND ");
                    /// Или через первичные ключи удалять
                }
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand(expr, connection);
                    int number = command.ExecuteNonQuery();
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine(e);
            }
        }
        public User FindById(int id)
        {
            string expr;
            expr = $"select * from [{ typeof(User).Name }] where user_id = {id}";
            List<User> result = new List<User>();
            Type t = typeof(User);

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(expr, connection);
                connection.Open();
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    User obj = (User)Activator.CreateInstance(t);
                    t.GetProperties().ToList().ForEach(p =>
                    {
                        p.SetValue(obj, reader[p.Name]);
                    });
                    result.Add(obj);
                }
                connection.Close();
            }
            return result.FirstOrDefault();
        }
        public List<User> FindAll()
        {
            string expr;
            expr = $"select * from [{ typeof(User).Name }]";
            List<User> result = new List<User>();
            Type t = typeof(User);

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(expr, connection);
                connection.Open();
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    User obj = (User)Activator.CreateInstance(t);
                    t.GetProperties().ToList().ForEach(p =>
                    {
                        p.SetValue(obj, reader[p.Name]);
                    });
                    result.Add(obj);
                }
                connection.Close();
            }
            return result;
        }
    }
}
