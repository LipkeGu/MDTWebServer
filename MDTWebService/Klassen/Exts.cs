using System.Text;

namespace MDTWebService
{
	public static class Exts
	{
		public static string F(this string fmt, params object[] objs) => string.Format(fmt, objs);

		public static string EncodeTo(string data, Encoding src, Encoding target)
		{
			return target.GetString(src.GetBytes(data));
		}

		public static string EncodeTo(byte[] data, Encoding target)
		{
			return target.GetString(data);
		}
	}
}
