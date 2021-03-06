﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Security;
using TeamSpark.AzureDay.WebSite.App;
using TeamSpark.AzureDay.WebSite.App.Entity;
using TeamSpark.AzureDay.WebSite.App.Service;
using TeamSpark.AzureDay.WebSite.Data.Enum;
using TeamSpark.AzureDay.WebSite.Host.Filter;
using TeamSpark.AzureDay.WebSite.Host.Models.Home;

namespace TeamSpark.AzureDay.WebSite.Host.Controllers
{
	public class HomeController : Controller
	{
		public async Task<ActionResult> Index()
		{
			var model = new IndexModel
			{
				Speakers = new SpeakersModel
				{
					SpeakersCollections = new List<List<Speaker>>()
				},
				Partners = new PartnersModel
				{
					PartnersCollection = new Dictionary<PartnerType, List<Partner>>()
				}
			};

			var speakers = new SpeakerService().GetSpeakers();
			var i = 0;
			foreach (var speaker in speakers)
			{
				List<Speaker> list;
				if (i == 0)
				{
					list = new List<Speaker>();
					model.Speakers.SpeakersCollections.Add(list);
				}
				else
				{
					list = model.Speakers.SpeakersCollections.Last();
				}

				list.Add(speaker);

				if (i == 3)
				{
					i = 0;
				}
				else
				{
					i++;
				}
			}

			var partners = new PartnerService().GetPartners();
			model.Partners.PartnersCollection = partners
				.GroupBy(p => p.PartnerType)
				.ToDictionary(p => p.Key, group => group.OrderBy(p => p.OrderN).ToList());

			return View(model);
		}

		public async Task<ActionResult> Schedule()
		{
			var model = new ScheduleModel();

			model.Rooms = new RoomService()
				.GetRooms()
				.Where(x => x.RoomType == RoomType.LectureRoom)
				.ToList();

			model.Timetables = new TimetableService().GetTimetable()
				.GroupBy(
					t => t.TimeStart,
					(key, timetables) => timetables.OrderBy(t => t.Room.ColorNumber).ToList())
				.ToList();

			return View(model);
		}

        public async Task<ActionResult> Workshops()
		{
			var model = new WorkshopsModel
			{
				Workshops = new List<WorkshopEntityModel>()
			};

			var workshops = new WorkshopService().GetWorkshops().ToList();
			var workshopTickets = await AppFactory.TicketService.Value.GetWorkshopsTicketsAsync();

			foreach (var workshop in workshops)
			{
				var ticketsLeft = workshop.MaxTickets - workshopTickets.Count(x => x.WorkshopId == workshop.Id);
				if (ticketsLeft < 0)
				{
					ticketsLeft = 0;
				}

				model.Workshops.Add(new WorkshopEntityModel
				{
					Workshop = workshop,
					TicketsLeft = ticketsLeft,
					ShowSpeakerInfo = true
				});
			}

			return View(model);
		}

		public async Task<ActionResult> WorkshopEntity(int id)
		{
			var model = new WorkshopEntityModel
			{
				ShowSpeakerInfo = true
			};

			model.Workshop = new WorkshopService()
				.GetWorkshop(id);

			if (model.Workshop == null)
			{
				return RedirectToAction("Workshops");
			}

			model.TicketsLeft = model.Workshop.MaxTickets - (await AppFactory.TicketService.Value.GetWorkshopTicketsAsync(id)).Count;
			if (model.TicketsLeft < 0)
			{
				model.TicketsLeft = 0;
			}

			return View(model);
		}

		public async Task<ActionResult> Speakers()
		{
			var model = new SpeakersModel
			{
				SpeakersCollections = new List<List<Speaker>>()
			};

			var speakers = new SpeakerService().GetSpeakers();
			var i = 0;
			foreach (var speaker in speakers)
			{
				List<Speaker> list;
				if (i == 0)
				{
					list = new List<Speaker>();
					model.SpeakersCollections.Add(list);
				}
				else
				{
					list = model.SpeakersCollections.Last();
				}

				list.Add(speaker);

				if (i == 3)
				{
					i = 0;
				}
				else
				{
					i++;
				}
			}

			return View(model);
		}

		public async Task<ActionResult> SpeakerEntity(string id)
		{
			var model = new SpeakerEntityModel();

			model.Speaker = new SpeakerService().GetSpeaker(id);

			if (model.Speaker == null)
			{
				return RedirectToAction("Index");
			}

			var workshops = new WorkshopService()
				.GetWorkshops()
				.Where(x => x.Speaker.Id.Equals(model.Speaker.Id, StringComparison.InvariantCultureIgnoreCase))
				.ToList();

			model.Workshops = new List<WorkshopEntityModel>();

			foreach (var workshop in workshops)
			{
				var ticketsLeft = workshop.MaxTickets - (await AppFactory.TicketService.Value.GetWorkshopTicketsAsync(workshop.Id)).Count;
				if (ticketsLeft < 0)
				{
					ticketsLeft = 0;
				}

				model.Workshops.Add(new WorkshopEntityModel
				{
					Workshop = workshop,
					TicketsLeft = ticketsLeft,
					ShowSpeakerInfo = false
				});
			}

			model.Topics = new TopicService()
				.GetTopics()
				.Where(x => x.Speaker.Id != null) // ???
				.Where(x => x.Speaker.Id.Equals(model.Speaker.Id, StringComparison.InvariantCultureIgnoreCase))
				.OrderBy(x => x.Timetable.TimeStart)
				.ToList();

			return View(model);
		}

		public async Task<ActionResult> Partners()
		{
			var model = new PartnersModel();

			model.PartnersCollection = new PartnerService().GetPartners()
				.GroupBy(p => p.PartnerType)
				.ToDictionary(p => p.Key, group => group.OrderBy(p => p.OrderN).ToList());

			return View(model);
		}

		[NonAuthorize]
		public async Task<ActionResult> Redirect([FromUri] string quickAuthToken, [FromUri] string redirectUrl)
		{
			var authToken = await AppFactory.QuickAuthTokenService.Value.GetQuickAuthTokenByValueAsync(quickAuthToken, false);

			if (authToken != null)
			{
				var attendee = await AppFactory.AttendeeService.Value.GetAttendeeByEmailAsync(authToken.Email);
				if (attendee != null && attendee.IsConfirmed)
				{
					FormsAuthentication.SetAuthCookie(attendee.EMail, true);
					await AppFactory.QuickAuthTokenService.Value.ExpireTokenByValueAsync(quickAuthToken);
				}
			}

			return Redirect(redirectUrl);
		}
	}
}