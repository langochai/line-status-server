using System;
using System.Configuration;
using System.Data;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Security.Cryptography;

namespace LineStatusServer.Common
{
	/// <summary>
	/// Summary description for DBUtils.
	/// </summary>
	public class DBUtils
	{
		public DBUtils()
		{
		}
        public static bool GetInforConnectionString(ref string[] GetInfor, ref string err)
        {
            try
            {
                string[] Infor = new string[4];//0:server,1:Database;2:User,3:Pass

                //string strPath = Application.StartupPath.ToString() + @"\default.ini";
                //string strPath = Application.StartupPath.ToString() + @"\" + Global.DefaultFileName;
                //FileStream file = new FileStream(strPath, FileMode.OpenOrCreate, FileAccess.Read);
                //StreamReader sr = new StreamReader(file);
                //int i = 0;
                //while (i < 4)
                //{
                //    Infor[i] = sr.ReadLine();
                //    i = i + 1;
                //}
                //sr.Close();
                //file.Close();
                //GetInfor = Infor;

                //var arr = MD5.DecodeChecksum(File.ReadAllText(strPath)).Split(';');
                return true;
            }
            catch (Exception ex)
            {
                err = ex.Message;
                return false;
            }
        }

        #region MH - GM
        public static string passPhrase = "Pas5pr@se";        // can be any string
        public static string saltValue = "s@1tValue";        // can be any string
        public static string hashAlgorithm = "SHA1";             // can be "MD5"
        public static int passwordIterations = 2;                  // can be any number
        public static string initVector = "@CSS@CSS@CSS@CSS"; // must be 16 bytes
        public static int keySize = 256;
        public static string Encrypt(string plainText,
                                     string passPhrase,
                                     string saltValue,
                                     string hashAlgorithm,
                                     int passwordIterations,
                                     string initVector,
                                     int keySize)
        {
            byte[] initVectorBytes = Encoding.ASCII.GetBytes(initVector);
            byte[] saltValueBytes = Encoding.ASCII.GetBytes(saltValue);

            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);

            PasswordDeriveBytes password = new PasswordDeriveBytes(
                                                            passPhrase,
                                                            saltValueBytes,
                                                            hashAlgorithm,
                                                            passwordIterations);

            byte[] keyBytes = password.GetBytes(keySize / 8);
            RijndaelManaged symmetricKey = new RijndaelManaged();

            symmetricKey.Mode = CipherMode.CBC;
            ICryptoTransform encryptor = symmetricKey.CreateEncryptor(
                                                             keyBytes,
                                                             initVectorBytes);

            MemoryStream memoryStream = new MemoryStream();
            CryptoStream cryptoStream = new CryptoStream(memoryStream,
                                                         encryptor,
                                                         CryptoStreamMode.Write);
            cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
            cryptoStream.FlushFinalBlock();

            byte[] cipherTextBytes = memoryStream.ToArray();

            memoryStream.Close();
            cryptoStream.Close();
            string cipherText = Convert.ToBase64String(cipherTextBytes);

            return cipherText;
        }


        public static string Decrypt(string cipherText,
                                 string passPhrase,
                                 string saltValue,
                                 string hashAlgorithm,
                                 int passwordIterations,
                                 string initVector,
                                 int keySize)
        {
            byte[] initVectorBytes = Encoding.ASCII.GetBytes(initVector);
            byte[] saltValueBytes = Encoding.ASCII.GetBytes(saltValue);

            // Convert our ciphertext into a byte array.
            byte[] cipherTextBytes = Convert.FromBase64String(cipherText);

            PasswordDeriveBytes password = new PasswordDeriveBytes(
                                                            passPhrase,
                                                            saltValueBytes,
                                                            hashAlgorithm,
                                                            passwordIterations);

            byte[] keyBytes = password.GetBytes(keySize / 8);

            RijndaelManaged symmetricKey = new RijndaelManaged();
            symmetricKey.Mode = CipherMode.CBC;

            ICryptoTransform decryptor = symmetricKey.CreateDecryptor(
                                                             keyBytes,
                                                             initVectorBytes);
            MemoryStream memoryStream = new MemoryStream(cipherTextBytes);
            CryptoStream cryptoStream = new CryptoStream(memoryStream,
                                                          decryptor,
                                                          CryptoStreamMode.Read);

            byte[] plainTextBytes = new byte[cipherTextBytes.Length];
            int decryptedByteCount = cryptoStream.Read(plainTextBytes,
                                                       0,
                                                       plainTextBytes.Length);
            memoryStream.Close();
            cryptoStream.Close();
            string plainText = Encoding.UTF8.GetString(plainTextBytes,
                                                       0,
                                                       decryptedByteCount);

            // Return decrypted string.   
            return plainText;
        }

        #endregion
		
        public static string SQLInsertTemp(string tableName, DataTable Source)
        {
            string Insert = "insert into " + tableName + " (";

            for (int i = 0; i < Source.Columns.Count; i++)
            {
                Insert = Insert + Source.Columns[i].ColumnName;
                Insert = Insert + ",";
            }
            Insert = Insert.Substring(0, Insert.Length - 1);
            Insert = Insert + ") values (";
            for (int i = 0; i < Source.Columns.Count; i++)
            {
                Insert = Insert + "@";
                Insert = Insert + Source.Columns[i].ColumnName;
                Insert = Insert + ",";
            }
            Insert = Insert.Substring(0, Insert.Length - 1);
            Insert = Insert + ")";
            return Insert;
        }
        public static string SQLSelect(string tableName, string field, object value)
		{
			return SQLSelect(tableName, new Expression(field, value));
		}
		public static string SQLSelect(string tableName, string field, object value, string op)
		{
			return SQLSelect(tableName, new Expression(field, value, op));
		}
		public static string SQLSelect(string tableName, Expression exp)
		{
            string sql = "SELECT * FROM " + tableName + " with (nolock) ";
			if (exp != null)
				sql += " WHERE " + exp.ToString();
			return sql;
		}
		public static string SQLSelectMax(string tableName, string field, string field1, object value)
		{
			return SQLSelectMax(tableName , field, new Expression(field1, value));
		}
		public static string SQLSelectMaxRoot(string tableName, string field)
		{
			return SQLSelectMax(tableName , field);
		}
		public static string SQLSelectMax(string tableName, string field)
		{
			string sql = "SELECT Max(" + field + ") FROM " + tableName;
			return sql;
		}
        public static string SQLSelectMinRoot(string tableName, string field)
        {
            return SQLSelectMin(tableName, field);
        }
        public static string SQLSelectMin(string tableName, string field)
        {
            string sql = "SELECT Min(" + field + ") FROM " + tableName;
            return sql;
        }
		public static string SQLSelectMax(string tableName, string field, Expression exp)
		{
			string sql = "SELECT Max(" + field + ") FROM " + tableName;
			if (exp != null)
				sql += " WHERE " + exp.ToString();
			return sql;
		}
		public static string SQLSelectCount(string tableName, string field, Expression exp)
		{
			string sql = "SELECT count(" + field + ") FROM " + tableName;
			if (exp != null)
				sql += " WHERE " + exp.ToString();
			return sql;
		}
		public static string SQLDelete(string tableName, Expression exp)
		{
			string sql = "DELETE FROM " + tableName;
			if (exp != null)
				sql += " WHERE " + exp.ToString();
			return sql;
		}
		public static string SQLDelete(string tableName, Int64 IDvalue)
		{
			string Delete = "DELETE FROM " + tableName + " WHERE ID=" + IDvalue;
			return Delete;
		}
		public static string SQLDelete(string tableName, string field, string value)
		{
			string Delete = "delete FROM " + tableName + " WHERE " + field + "='" + value + "'";
			return Delete;
		}
		public static string SQLDelete(string tableName, string field, Int64 value)
		{
			string Delete = "delete FROM " + tableName + " WHERE " + field + "=" + value;
			return Delete;
		}
		public static SqlDbType ConvertToSQLType(Type type)
		{
			if (type == typeof (string))
			{
				return SqlDbType.NVarChar;
			}
			if (type == typeof (int))
			{
				return SqlDbType.Int;
			}
			if (type == typeof (Int16))
			{
				return SqlDbType.TinyInt;
			}
			if (type == typeof (Int64))
			{
				return SqlDbType.BigInt;
			}
			if (type == typeof (DateTime))
			{
				return SqlDbType.DateTime;
			}
            if (type == typeof(DateTime?))
            {
                return SqlDbType.DateTime;
            }
            if(type == typeof(TimeSpan?))
            {
                return SqlDbType.Time;
            }    
			if (type == typeof (long))
			{
				return SqlDbType.BigInt;
			}
			if (type == typeof (Decimal))
			{
				return SqlDbType.Decimal;
			}
            if (type == typeof(Byte[]))
            {
                return SqlDbType.Image;
            }
            if (type == typeof(Guid))
            {
                return SqlDbType.UniqueIdentifier;
            }
			return SqlDbType.NVarChar;
		}
        //Insert vào bảng eventlog 
        private static string GetDescription(string strTableName, ActionType Action)
        {
            return ("Table: " + strTableName + " Action: " + Action.ToString() + " ");
        }
        public static string GetDescription(string strTableName, ActionType Action, string Code)
        {
            return (GetDescription(strTableName, Action) + "With Code: " + Code + " Event: OK");
        }
        public static string GetDescription(string strTableName, ActionType Action, int ID)
        {
            return (GetDescription(strTableName, Action) + "With iD: " + ID.ToString() + " Event: OK");
        }
        public static string GetDescription(string strTableName, ActionType Action, string Code, string strError)
        {
            return (GetDescription(strTableName, Action) + "With Code: " + Code + " Event: " + strError);
        }
        public static string GetDescription(string strTableName, ActionType Action, int ID, string strError)
        {
            return (GetDescription(strTableName, Action) + "With iD: " + ID.ToString() + " Event: " + strError);
        }
        //Sử dụng một trường tùy ý
        public static string GetDescription(string strTableName, ActionType Action, string strColumnName, int value)
        {
            return (GetDescription(strTableName, Action) + "With " + strColumnName + ": " + value.ToString() + " Event: OK");
        }
        public static string GetDescription(string strTableName, ActionType Action, string strColumnName, int value, string strError)
        {
            return (GetDescription(strTableName, Action) + "With " + strColumnName + ": " + value.ToString() + " Event: " + strError);
        }
        public static string GetDescription(string strTableName, ActionType Action, string strColumnName, string value, string strError)
        {
            return (GetDescription(strTableName, Action) + "With " + strColumnName + ": " + value + " Event: " + strError);
        }
        public enum ActionType
        {
            DELETE,
            INSERT,
            UPDATE,
            ERROR
        }
        public static bool GetInforConnectionString(ref string[] GetInfor, ref string err, string filename)
        {
            try
            {
                string[] Infor = new string[4];//0:server,1:Database;2:User,3:Pass

                string strPath = Application.StartupPath + @"\" + filename;
                FileStream file = new FileStream(strPath, FileMode.OpenOrCreate, FileAccess.Read);
                StreamReader sr = new StreamReader(file);
                int i = 0;
                while (i < 4)
                {
                    Infor[i] = sr.ReadLine();
                    i = i + 1;
                }
                sr.Close();
                file.Close();
                GetInfor = Infor;
                return true;
            }
            catch (Exception ex)
            {
                err = ex.Message;
                return false;
            }
        }
	}
}
