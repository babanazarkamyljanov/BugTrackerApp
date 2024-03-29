﻿namespace BugTracker.Helpers;

public static class NavigationIndicatorHelper
{
    public static string MakeActiveClass(this IUrlHelper urlHelper, string controller, string action)
    {
        try
        {
            string result = "nav-active";
            var controllerName = urlHelper?.ActionContext?.RouteData?.Values["controller"]?.ToString();
            var methodName = urlHelper?.ActionContext?.RouteData?.Values["action"]?.ToString();

            if (string.IsNullOrEmpty(controllerName))
                return "";
            if (controllerName.Equals(controller, StringComparison.OrdinalIgnoreCase))
            {
                if (methodName != null && methodName.Equals(action, StringComparison.OrdinalIgnoreCase))
                {
                    return result;
                }
            }
            return "";
        }
        catch (Exception)
        {
            return "";
        }
    }
}
