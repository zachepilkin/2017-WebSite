﻿using System.Web.Mvc;
using System.Web.Routing;

namespace TeamSpark.AzureDay.WebSite.Host
{
	public class RouteConfig
	{
		public static void RegisterRoutes(RouteCollection routes)
		{
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

			routes.MapRoute(
				name: "Home",
				url: "{action}",
				defaults: new { controller = "Home", action = "Index" }
			);

			routes.MapRoute(
				name: "Token",
				url: "{controller}/{action}/{token}"
			);

			routes.MapRoute(
				name: "WorkshopEntity",
				url: "workshop/{id}",
				defaults: new { controller = "Home", action = "WorkshopEntity" }
			);

			routes.MapRoute(
				name: "SpeakerEntity",
				url: "speaker/{id}",
				defaults: new { controller = "Home", action = "SpeakerEntity" }
			);

			routes.MapRoute(
				name: "Language",
				url: "language/{culture}",
				defaults: new { controller = "Language", action = "Index" }
			);

			routes.MapRoute(
				name: "Default",
				url: "{controller}/{action}"
			);
		}
	}
}
