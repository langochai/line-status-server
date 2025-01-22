using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace LineDowntime.Common
{
    public static class SQLHelper<T> where T : class, new()
    {
        private static readonly string connectionString = Settings.connectionString;

        public static T ProcedureToModel(string procedureName, string[] paramName, object[] paramValue)
        {
            T model = new T();
            SqlConnection mySqlConnection = new SqlConnection(connectionString);
            SqlParameter sqlParam;
            mySqlConnection.Open();

            try
            {
                SqlCommand mySqlCommand = new SqlCommand(procedureName, mySqlConnection);
                mySqlCommand.CommandType = CommandType.StoredProcedure;
                if (paramName != null)
                {
                    for (int i = 0; i < paramName.Length; i++)
                    {
                        sqlParam = new SqlParameter(paramName[i], paramValue[i]);
                        mySqlCommand.Parameters.Add(sqlParam);
                    }
                }
                SqlDataReader reader = mySqlCommand.ExecuteReader();
                model = reader.MapToSingle<T>();
            }
            catch (SqlException e)
            {
                throw new Exception(e.ToString());
            }
            finally
            {
                mySqlConnection.Close();
            }

            return model;
        }

        public static List<T> ProcedureToList(string procedureName, string[] paramName, object[] paramValue)
        {
            List<T> lst = new List<T>();
            SqlConnection mySqlConnection = new SqlConnection(connectionString);
            SqlParameter sqlParam;
            mySqlConnection.Open();

            try
            {
                SqlCommand mySqlCommand = new SqlCommand(procedureName, mySqlConnection);
                mySqlCommand.CommandType = CommandType.StoredProcedure;
                if (paramName != null)
                {
                    for (int i = 0; i < paramName.Length; i++)
                    {
                        sqlParam = new SqlParameter(paramName[i], paramValue[i]);
                        mySqlCommand.Parameters.Add(sqlParam);
                    }
                }
                SqlDataReader reader = mySqlCommand.ExecuteReader();
                lst = reader.MapToList<T>();
            }
            catch (SqlException e)
            {
                throw new Exception(e.ToString());
            }
            finally
            {
                mySqlConnection.Close();
            }

            return lst;
        }

        public static T SqlToModel(string sql)
        {
            T model = new T();
            SqlConnection mySqlConnection = new SqlConnection(connectionString);
            mySqlConnection.Open();
            try
            {
                SqlCommand mySqlCommand = new SqlCommand(sql, mySqlConnection);
                mySqlCommand.CommandType = CommandType.Text;
                SqlDataReader reader = mySqlCommand.ExecuteReader();
                model = reader.MapToSingle<T>();
            }
            catch (SqlException e)
            {
                throw new Exception(e.ToString());
            }
            finally
            {
                mySqlConnection.Close();
            }

            return model;
        }

        public static List<T> SqlToList(string sql)
        {
            List<T> lst = new List<T>();
            SqlConnection mySqlConnection = new SqlConnection(connectionString);
            mySqlConnection.Open();
            try
            {
                SqlCommand mySqlCommand = new SqlCommand(sql, mySqlConnection);
                mySqlCommand.CommandType = CommandType.Text;
                SqlDataReader reader = mySqlCommand.ExecuteReader();
                lst = reader.MapToList<T>();
            }
            catch (SqlException e)
            {
                throw new Exception(e.ToString());
            }
            finally
            {
                mySqlConnection.Close();
            }

            return lst;
        }

        public static string SQLInsert(T model)
        {
            Type type = model.GetType();
            string tableName = type.Name.Contains("Model") ? type.Name : type.Name.Replace("Model", "");

            string Insert = "insert into " + tableName + " (";
            PropertyInfo[] pis = type.GetProperties();

            for (int i = 0; i < pis.Length; i++)
            {
                if (!pis[i].Name.Equals("ID"))
                {
                    Insert = Insert + pis[i].Name;
                    Insert = Insert + ",";
                }
            }
            Insert = Insert.Substring(0, Insert.Length - 1);
            Insert = Insert + ") values (";
            for (int i = 0; i < pis.Length; i++)
            {
                if (!pis[i].Name.Equals("ID"))
                {
                    Insert = Insert + "@";
                    Insert = Insert + pis[i].Name;
                    Insert = Insert + ",";
                }
            }
            Insert = Insert.Substring(0, Insert.Length - 1);
            Insert = Insert + ") Select Scope_Identity()";
            return Insert;
        }

        public static string SQLUpdate(T model)
        {
            Type type = model.GetType();
            string tableName = type.Name.Contains("Model") ? type.Name : type.Name.Replace("Model", "");
            string Update = "UPDATE " + tableName + " SET ";
            PropertyInfo[] pis = type.GetProperties();

            for (int i = 0; i < pis.Length; i++)
            {
                if (!pis[i].Name.Equals("ID"))
                {
                    Update = Update + pis[i].Name;
                    Update = Update + "=@" + pis[i].Name;
                    Update = Update + ",";
                }
            }
            Update = Update.Substring(0, Update.Length - 1);
            Update = Update + " WHERE ID=" + type.GetProperty("ID").GetValue(model, null).ToString();

            return Update;
        }

        public static void Insert(T model, string userName = "")
        {
            Type type = model.GetType();
            SqlConnection conn = new SqlConnection(connectionString);
            try
            {
                string sql = SQLInsert(model);
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandTimeout = 2000;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = sql;

                PropertyInfo[] propertiesName = type.GetProperties();
                for (int i = 0; i < propertiesName.Length; i++)
                {
                    object value = propertiesName[i].GetValue(model, null);

                    if (!propertiesName[i].Name.Equals("ID") && !propertiesName[i].Name.Equals("iD"))
                    {
                        if (propertiesName[i].Name.ToLower().Equals("createdby") || propertiesName[i].Name.ToLower().Equals("updatedby"))
                        {
                            cmd.Parameters.Add("@" + propertiesName[i].Name, SqlDbType.NVarChar).Value = userName;
                        }
                        else if (propertiesName[i].Name.ToLower().Equals("createddate") || propertiesName[i].Name.ToLower().Equals("updateddate"))
                        {
                            cmd.Parameters.Add("@" + propertiesName[i].Name, SqlDbType.DateTime).Value = DateTime.Now;
                        }
                        else if (value != null)
                        {
                            if (propertiesName[i].PropertyType.Equals(typeof(DateTime)))
                            {
                                if ((DateTime)value == DateTime.MinValue)
                                    value = new DateTime(1900, 01, 01);
                            }
                            if (propertiesName[i].PropertyType.Name.Equals("Byte[]"))
                            {
                                cmd.Parameters.Add("@" + propertiesName[i].Name, SqlDbType.Image).Value = value;
                            }
                            else
                            {
                                cmd.Parameters.Add("@" + propertiesName[i].Name, ConvertToSQLType(propertiesName[i].PropertyType)).Value = value;
                            }
                        }
                        else
                        {
                            if (propertiesName[i].PropertyType.Equals(typeof(DateTime?)))
                            {
                                cmd.Parameters.Add("@" + propertiesName[i].Name, ConvertToSQLType(propertiesName[i].PropertyType)).Value = DBNull.Value;
                            }
                            else
                            {
                                cmd.Parameters.Add("@" + propertiesName[i].Name, ConvertToSQLType(propertiesName[i].PropertyType)).Value = "";
                            }
                        }
                    }
                }

                conn.Open();
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
            catch
            {
                throw;
            }
            finally
            {
                if (conn.State != ConnectionState.Closed) conn.Close();
                conn.Dispose();
            }
        }

        public static void Update(T model, string userName = "")
        {
            Type type = model.GetType();
            SqlConnection conn = new SqlConnection(connectionString);
            try
            {
                string sql = SQLUpdate(model);
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandTimeout = 2000;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = sql;

                PropertyInfo[] propertiesName = type.GetProperties();
                for (int i = 0; i < propertiesName.Length; i++)
                {
                    SqlDbType dbType = ConvertToSQLType(propertiesName[i].PropertyType);
                    object value = propertiesName[i].GetValue(model, null);

                    if (propertiesName[i].Name.ToLower().Equals("updatedby"))
                    {
                        cmd.Parameters.Add("@" + propertiesName[i].Name, SqlDbType.NVarChar).Value = userName;
                    }
                    else if (propertiesName[i].Name.ToLower().Equals("updateddate"))
                    {
                        cmd.Parameters.Add("@" + propertiesName[i].Name, SqlDbType.DateTime).Value = DateTime.Now;
                    }
                    else if (value != null)
                    {
                        if (propertiesName[i].PropertyType.Equals(typeof(DateTime)))
                        {
                            if ((DateTime)value == DateTime.MinValue)
                                value = new DateTime(1900, 01, 01);
                        }
                        if (propertiesName[i].PropertyType.Name.Equals("Byte[]"))
                        {
                            cmd.Parameters.Add("@" + propertiesName[i].Name, SqlDbType.Image).Value = value;
                        }
                        else
                        {
                            cmd.Parameters.Add("@" + propertiesName[i].Name, dbType).Value = value;
                        }
                    }
                    else
                    {
                        if (propertiesName[i].PropertyType.Equals(typeof(DateTime?)))
                        {
                            cmd.Parameters.Add("@" + propertiesName[i].Name, dbType).Value = DBNull.Value;
                        }
                        else
                            cmd.Parameters.Add("@" + propertiesName[i].Name, dbType).Value = "";
                    }
                }
                conn.Open();
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
            catch
            {
                throw;
            }
            finally
            {
                if (conn.State != ConnectionState.Closed) conn.Close();
                conn.Dispose();
            }
        }

        public static SqlDbType ConvertToSQLType(Type type)
        {
            if (type == typeof(string))
            {
                return SqlDbType.NVarChar;
            }
            if (type == typeof(int))
            {
                return SqlDbType.Int;
            }
            if (type == typeof(short))
            {
                return SqlDbType.TinyInt;
            }
            if (type == typeof(long))
            {
                return SqlDbType.BigInt;
            }
            if (type == typeof(DateTime))
            {
                return SqlDbType.DateTime;
            }
            if (type == typeof(DateTime?))
            {
                return SqlDbType.DateTime;
            }
            if (type == typeof(long))
            {
                return SqlDbType.BigInt;
            }
            if (type == typeof(decimal))
            {
                return SqlDbType.Decimal;
            }
            if (type == typeof(byte[]))
            {
                return SqlDbType.Image;
            }
            if (type == typeof(Guid))
            {
                return SqlDbType.UniqueIdentifier;
            }
            if (type == typeof(byte[]))
            {
                return SqlDbType.VarBinary;
            }
            return SqlDbType.NVarChar;
        }
    }
}