using System;
using System.IO;
using System.Data.SQLite;
using System.Collections.Specialized;
using System.Collections.Generic;

namespace MDTWebService.Klassen
{
	public class SQLDatabase
	{
		string dataBase = string.Empty;
		int version = 3;
		SQLiteConnection sqlConn;

		public SQLDatabase(string database = "database.sqlite", int version = 3)
		{
			this.dataBase = Path.Combine(Environment.CurrentDirectory, database);
			if (File.Exists(this.dataBase))
			{
				this.version = version;

				this.sqlConn = new SQLiteConnection("Data Source={0};Version={1};".F(this.dataBase, this.version));
				this.sqlConn.Open();
			}
			else
			{
				Console.WriteLine("Cant find Database file: {0}", this.dataBase);
			}
		}

		public int Count(string table, string condition, string value)
		{
			var cmd = new SQLiteCommand("SELECT Count({0}) FROM {1} WHERE {0} LIKE '{2}'".F(condition,table,value), this.sqlConn);
			cmd.CommandType = System.Data.CommandType.Text;

			return Convert.ToInt32(cmd.ExecuteScalar());
		}

		public Dictionary<uint, NameValueCollection> SQLQuery(string sql)
		{
			var cmd = new SQLiteCommand(sql, this.sqlConn);
			cmd.CommandType = System.Data.CommandType.Text;
			cmd.ExecuteNonQuery(System.Data.CommandBehavior.Default);

			var result = new Dictionary<uint, NameValueCollection>();
			var reader = cmd.ExecuteReader();
			var i = uint.MinValue;

			while (reader.Read())
				if (!result.ContainsKey(i))
				{
					result.Add(i, reader.GetValues());
					i++;
				}

			reader.Close();

			return result;
		}

		public string SQLQuery(string sql, string key)
		{
			var cmd = new SQLiteCommand(sql, this.sqlConn);
			cmd.CommandType = System.Data.CommandType.Text;
			cmd.ExecuteNonQuery(System.Data.CommandBehavior.SingleResult);

			var result = string.Empty;
			var reader = cmd.ExecuteReader();

			while (reader.Read())
				result = "{0}".F(reader[key]);

			reader.Close();

			return result;
		}

		public void Close()
		{
			this.sqlConn.Close();
		}
	}
}
