using System;
using System.Text;

namespace MDTWebService.Klassen
{
	public static class Functions
	{
		public static string EncodeTo(string data, Encoding src, Encoding target)
		{
			return target.GetString(src.GetBytes(data));
		}

		public static string GetNestedEntry(string id, string table, string key, ref SQLDatabase db)
		{
			var result = string.Empty;

			if (db.Count(table, "id", id) != 0)
			{
				var x = db.SQLQuery("SELECT {2} FROM {1} WHERE id LIKE {0}".F(id, table, key), key);
				if (!string.IsNullOrEmpty(x))
					result = x;
			}

			return result;
		}

		public static string GetAssociatedEntry(string table, string table2, string key, string key2, string uuid, ref SQLDatabase db)
		{
			var result = string.Empty;
			var id = db.SQLQuery("SELECT {0} FROM {1} WHERE UUID LIKE '{2}'".F(key, table, uuid), key);

			if (!string.IsNullOrEmpty(id))
				result = GetNestedEntry(id, table2, key2, ref db);

			return result;
		}

		public static string GetComputerEntry(string table, string uuid, string key, string param, ref SQLDatabase db)
		{
			var result = string.Empty;

			if (db.Count("Computer", "UUID", uuid) != 0)
			{
				if (param != "--kms" && param != "--wsus")
					result = db.SQLQuery("SELECT {0} FROM Computer WHERE UUID LIKE '{1}'".F(key, uuid), key);

				if (param == "--wsus")
					result = GetAssociatedEntry("Computer", "WUServers", "WUServer", "Address", uuid, ref db);

				if (param == "--kms")
					result = GetAssociatedEntry("Computer", "SLServers", "SLServer", "Address", uuid, ref db);

				if (param == "--user")
					result = GetAssociatedEntry("Computer", "Usernames", "Username", "Name", uuid, ref db);

				if (param == "--owner")
					result = GetAssociatedEntry("Computer", "Owners", "Owner", "Name", uuid, ref db);

				if (param == "--wgroup")
					result = GetAssociatedEntry("Computer", "WorkGroups", "WorkGroup", "Name", uuid, ref db);
			}

			if (string.IsNullOrEmpty(result))
			{
				Console.WriteLine("Cant find requested entries client: {0}", uuid);
				result = string.Empty;
			}

			return result;
		}
	}
}
