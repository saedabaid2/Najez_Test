using System;
using System.Web.Mvc;
using System.Web.Mvc.Filters;
using System.Web.Routing;

namespace Almanea.BusinessLogic
{
	public class CustomAuthenticationFilter : ActionFilterAttribute, IAuthenticationFilter
	{
		public void OnAuthentication(AuthenticationContext filterContext)
		{
			if (string.IsNullOrEmpty(Convert.ToString(filterContext.HttpContext.Session[cls_Defaults.Session_UserId])))
			{
				filterContext.Result = new HttpUnauthorizedResult();
			}
		}

		public void OnAuthenticationChallenge(AuthenticationChallengeContext filterContext)
		{
			if (filterContext.Result == null || filterContext.Result is HttpUnauthorizedResult)
			{
				filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary
			{
				{ "controller", "Home" },
				{ "action", "Index" }
			});
			}
		}
	}
}