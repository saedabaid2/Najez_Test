using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Almanea.BusinessLogic
{

	public static class cultureHelper
	{
		private static readonly List<string> _cultures = new List<string> { "en-US", "es", "ar" };

		public static string GetImplementedCulture(string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				return GetDefaultCulture();
			}
			if (_cultures.Where((string c) => c.Equals(name, StringComparison.InvariantCultureIgnoreCase)).Count() > 0)
			{
				return name;
			}
			string i = GetNeutralCulture(name);
			foreach (string c2 in _cultures)
			{
				if (c2.StartsWith(i))
				{
					return c2;
				}
			}
			return GetDefaultCulture();
		}

		public static string GetDefaultCulture()
		{
			return _cultures[0];
		}

		public static string GetCurrentCulture()
		{
			return Thread.CurrentThread.CurrentCulture.Name;
		}

		public static string GetCurrentNeutralCulture()
		{
			return GetNeutralCulture(Thread.CurrentThread.CurrentCulture.Name);
		}

		public static string GetNeutralCulture(string name)
		{
			if (!name.Contains("-"))
			{
				return name;
			}
			return name.Split('-')[0];
		}
	}
}