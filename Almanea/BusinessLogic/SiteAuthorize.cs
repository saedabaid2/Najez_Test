using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Almanea.BusinessLogic
{ 

public class SiteAuthorize : AuthorizeAttribute
{
	private readonly string[] allowedroles;

	public SiteAuthorize(params string[] roles)
	{
		allowedroles = roles;
	}

	public override void OnAuthorization(AuthorizationContext filterContext)
	{
		if (filterContext.HttpContext.Request.IsAjaxRequest())
		{
			base.OnAuthorization(filterContext);
		}
		else if (!filterContext.HttpContext.Request.IsAjaxRequest())
		{
			if (string.IsNullOrEmpty(Convert.ToString(filterContext.HttpContext.Session[cls_Defaults.Session_UserId])))
			{
				filterContext.Result = new RedirectResult("/Home/Index");
			}
			else
			{
				base.OnAuthorization(filterContext);
			}
		}
	}

	protected override bool AuthorizeCore(HttpContextBase httpContextBase)
	{
		bool authorize = false;
		if (string.IsNullOrEmpty(Convert.ToString(httpContextBase.Session[cls_Defaults.Session_UserId])))
		{
			return authorize;
		}
		int UserGroupId = Convert.ToInt32(httpContextBase.Session[cls_Defaults.Session_UserGroupTypeId]);
		string userRole = cls_DropDowns.UserGroupName(UserGroupId);
		string[] array = allowedroles;
		foreach (string role in array)
		{
			if (role == userRole)
			{
				return true;
			}
		}
		return authorize;
	}

	protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
	{
		filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary
		{
			{ "controller", "Home" },
			{ "action", "UnAuthorized" }
		});
	}
}
}
