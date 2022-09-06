using System;
using System.Web;
using Hangfire.Dashboard;

namespace Almanea.BusinessLogic { 

public class MyAuthorizationFilter : IDashboardAuthorizationFilter
{
	public bool Authorize(DashboardContext context)
	{
		int UserGroupId = Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_UserGroupTypeId]);
		string userRole = cls_DropDowns.UserGroupName(UserGroupId);
		if (userRole.Equals("Admin"))
		{
			return true;
		}
		return false;
	}
}
}
