using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;

namespace Almanea.BusinessLogic
{

	public static class cls_Cast
	{
		public static T Cast<T>(this FormCollection data)
		{
			Type objectType = data.GetType();
			Type target = typeof(T);
			object x = Activator.CreateInstance(target, nonPublic: false);
			IEnumerable<MemberInfo> dest = from source in target.GetMembers().ToList()
										   where source.MemberType == MemberTypes.Property
										   select source;
			List<MemberInfo> members = dest.Where((MemberInfo memberInfo) => dest.Select((MemberInfo c) => c.Name).ToList().Contains(memberInfo.Name)).ToList();
			foreach (MemberInfo memberInfo2 in members)
			{
				PropertyInfo propertyInfo = typeof(T).GetProperty(memberInfo2.Name);
				if (data.AllKeys.Contains(memberInfo2.Name) && !string.IsNullOrEmpty(data[memberInfo2.Name]))
				{
					string temp = data[memberInfo2.Name].ToString();
					object value = ((propertyInfo.PropertyType.Name.ToLower() == "int32") ? ((object)Convert.ToInt32(data[memberInfo2.Name])) : ((propertyInfo.PropertyType.Name.ToLower() == "boolean") ? ((object)Convert.ToBoolean(data[memberInfo2.Name])) : ((propertyInfo.PropertyType.Name.ToLower() == "datetime") ? ((object)DateTime.ParseExact(data[memberInfo2.Name], "dd/MM/yyyy", CultureInfo.InvariantCulture)) : ((propertyInfo.PropertyType.Name.ToLower() == "byte") ? ((object)Convert.ToByte(data[memberInfo2.Name])) : ((!(propertyInfo.PropertyType.Name.ToLower() == "decimal")) ? data[memberInfo2.Name] : ((object)Convert.ToDecimal(data[memberInfo2.Name])))))));
					propertyInfo.SetValue(x, value, null);
				}
			}
			return (T)x;
		}

		public static DataTable ToDataTable<T>(this List<T> items)
		{
			DataTable tb = new DataTable(typeof(T).Name);
			PropertyInfo[] props = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public);
			PropertyInfo[] array = props;
			foreach (PropertyInfo prop in array)
			{
				tb.Columns.Add(prop.Name, prop.PropertyType);
			}
			foreach (T item in items)
			{
				object[] values = new object[props.Length];
				for (int i = 0; i < props.Length; i++)
				{
					values[i] = props[i].GetValue(item, null);
				}
				tb.Rows.Add(values);
			}
			return tb;
		}
	}
}