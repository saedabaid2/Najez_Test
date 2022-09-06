using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Almanea.Data;
using Almanea.Models;

namespace Almanea.BusinessLogic
{
	public class cls_Sms
	{
		private static string appSid = "wWIwjOQaSiqKSgyzmMrG7JCqwvZT";

		private static string SenderID = "NAJEZ";

		public static async Task<bool> SendSMS(string Recipient, string message)
		{
			_ = 1;
			try
			{
				db_Settings objSettings = new db_Settings();
				int IsProoduction = Convert.ToInt32((await objSettings.GetSetting()).IsProoduction);
				if (IsProoduction == 1)
				{
					string sendsmsservice = "https://ht.deewan.sa:8443/Send.aspx?UserName=Tarqeem&Password=b9Th5Z&MessageType=text&Recipients=" + Recipient + "&SenderName=Najez&MessageText=" + message;
					using (var client = new HttpClient { Timeout = TimeSpan.FromSeconds(3000) })
					{
						HttpRequestMessage request1 = new HttpRequestMessage(HttpMethod.Get, sendsmsservice);

						HttpResponseMessage response1 = new HttpResponseMessage();
						// HTTP GET  
						response1 = await client.SendAsync(request1).ConfigureAwait(false);
						// Verification  
						if (response1.IsSuccessStatusCode)
						{
							// Reading Response.  
							// string result = response.Content.ReadAsStringAsync().Result;
							//List<Item> items = JsonConvert.DeserializeObject<>(json);
							//var datas = result.Length..csv"];
							//  var dataObj = JObject.Parse(result);
							// var base64 = dataObj["csv"].ToString();
							//Base64Decode(base64);                          

						}
					}
				}
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		public static async Task managecapacity(int OrderId)
		{
			Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_UserGroupTypeId]);
			db_User objUser = new db_User();
			db_Settings objSettings = new db_Settings();
			AlmaneaDbEntities _context = new AlmaneaDbEntities();
			vm_Order order = await objSettings.GetOrderById(OrderId);
			if (order.Status != 1)
			{
				return;
			}
			int calcva = 0;
			new List<int>();
			List<int?> splist = objSettings.GetSpOrderDistributed(OrderId);
			if (splist.Count <= 0)
			{
				return;
			}
			foreach (int? itemspid in splist)
			{
				int dailcapacity = 0;
				List<tblAdminUser> labours = objUser.GetLaboursDopr(Convert.ToInt32(itemspid));
				tblSetting wokrhrs = objSettings.GetEorkinhHrsysettings(Convert.ToInt32(itemspid));
				if (labours.Count > 0 && labours != null && labours.Count > 0)
				{
					dailcapacity = Convert.ToInt32(labours.Count * Convert.ToInt32(wokrhrs.KeyValue));
				}
				List<tblOrderService> OrderServicssse = _context.tblOrderServices.Where((tblOrderService x) => x.OrderId == OrderId).ToList();
				foreach (tblOrderService item in OrderServicssse)
				{
					tblServiceMapper spassignedservice = objSettings.GetServicesmap2(Convert.ToInt32(itemspid), item.ServiceId);
					int spestimat = Convert.ToInt32(spassignedservice.Estimated);
					calcva += item.Quantity * spestimat / 60;
				}
				int idd = Convert.ToInt32(itemspid);
				DateTime orderdatatt = Convert.ToDateTime(order.InstallDate);
				tblTeamCapacityCalculation model2 = (from x in _context.tblTeamCapacityCalculations
													 where x.ServiceProviderId == (int?)idd && x.InstallDate == orderdatatt && x.OrderId == (int?)order.OrderId
													 orderby x.Id descending
													 select x).FirstOrDefault();
				if (model2 != null)
				{
					Convert.ToInt32(model2.CurrentCapacity);
					model2.Updatedate = DateTime.Now;
					model2.InstallDate = Convert.ToDateTime(order.InstallDate);
					model2.OrderId = order.OrderId;
					model2.ServiceProviderId = Convert.ToInt32(itemspid);
					model2.DailyCapacity = dailcapacity;
					model2.ConsumedCapacity = calcva;
					model2.CurrentCapacity -= calcva;
					double curavailabe2 = (double)model2.CurrentCapacity.Value / (double)model2.DailyCapacity.Value * 100.0;
					if (curavailabe2 < 0.0)
					{
						curavailabe2 = 0.0;
					}
					model2.CapcityPercentage = Convert.ToDecimal(curavailabe2);
					_context.Entry(model2).State = EntityState.Modified;
					_context.SaveChanges();
				}
				else
				{
					tblTeamCapacityCalculation model3 = (from x in _context.tblTeamCapacityCalculations
														 where x.ServiceProviderId == (int?)idd && x.InstallDate == orderdatatt
														 orderby x.Id descending
														 select x).FirstOrDefault();
					int curcap = ((model3 == null) ? dailcapacity : Convert.ToInt32(model3.CurrentCapacity));
					tblTeamCapacityCalculation capcal = new tblTeamCapacityCalculation();
					capcal.Updatedate = DateTime.Now;
					capcal.InstallDate = Convert.ToDateTime(order.InstallDate);
					capcal.OrderId = order.OrderId;
					capcal.ServiceProviderId = Convert.ToInt32(itemspid);
					capcal.DailyCapacity = dailcapacity;
					capcal.ConsumedCapacity = calcva;
					capcal.CurrentCapacity = curcap - calcva;
					double curavailabe = (double)capcal.CurrentCapacity.Value / (double)capcal.DailyCapacity.Value * 100.0;
					if (curavailabe < 0.0)
					{
						curavailabe = 0.0;
					}
					capcal.CapcityPercentage = Convert.ToDecimal(curavailabe);
					_context.tblTeamCapacityCalculations.Add(capcal);
					_context.SaveChanges();
				}
				calcva = 0;
			}
		}

		public static async Task managecapacityAPI(int OrderId, int UserGroupTypeId)
		{
			_ = UserGroupTypeId;
			db_User objUser = new db_User();
			db_Settings objSettings = new db_Settings();
			AlmaneaDbEntities _context = new AlmaneaDbEntities();
			vm_Order order = await objSettings.GetOrderById(OrderId);
			if (order.Status != 1)
			{
				return;
			}
			int calcva = 0;
			new List<int>();
			List<int?> splist = objSettings.GetSpOrderDistributed(OrderId);
			if (splist.Count <= 0)
			{
				return;
			}
			foreach (int? itemspid in splist)
			{
				int dailcapacity = 0;
				List<tblAdminUser> labours = objUser.GetLaboursDopr(Convert.ToInt32(itemspid));
				tblSetting wokrhrs = objSettings.GetEorkinhHrsysettings(Convert.ToInt32(itemspid));
				if (labours.Count > 0 && labours != null && labours.Count > 0)
				{
					dailcapacity = Convert.ToInt32(labours.Count * Convert.ToInt32(wokrhrs.KeyValue));
				}
				List<tblOrderService> OrderServicssse = _context.tblOrderServices.Where((tblOrderService x) => x.OrderId == OrderId).ToList();
				foreach (tblOrderService item in OrderServicssse)
				{
					tblServiceMapper spassignedservice = objSettings.GetServicesmap2(Convert.ToInt32(itemspid), item.ServiceId);
					int spestimat = Convert.ToInt32(spassignedservice.Estimated);
					calcva += item.Quantity * spestimat / 60;
				}
				int idd = Convert.ToInt32(itemspid);
				DateTime orderdatatt = Convert.ToDateTime(order.InstallDate);
				tblTeamCapacityCalculation model2 = (from x in _context.tblTeamCapacityCalculations
													 where x.ServiceProviderId == (int?)idd && x.InstallDate == orderdatatt && x.OrderId == (int?)order.OrderId
													 orderby x.Id descending
													 select x).FirstOrDefault();
				if (model2 != null)
				{
					Convert.ToInt32(model2.CurrentCapacity);
					model2.Updatedate = DateTime.Now;
					model2.InstallDate = Convert.ToDateTime(order.InstallDate);
					model2.OrderId = order.OrderId;
					model2.ServiceProviderId = Convert.ToInt32(itemspid);
					model2.DailyCapacity = dailcapacity;
					model2.ConsumedCapacity = calcva;
					model2.CurrentCapacity -= calcva;
					double curavailabe2 = (double)model2.CurrentCapacity.Value / (double)model2.DailyCapacity.Value * 100.0;
					if (curavailabe2 < 0.0)
					{
						curavailabe2 = 0.0;
					}
					model2.CapcityPercentage = Convert.ToDecimal(curavailabe2);
					_context.Entry(model2).State = EntityState.Modified;
					_context.SaveChanges();
				}
				else
				{
					tblTeamCapacityCalculation model3 = (from x in _context.tblTeamCapacityCalculations
														 where x.ServiceProviderId == (int?)idd && x.InstallDate == orderdatatt
														 orderby x.Id descending
														 select x).FirstOrDefault();
					int curcap = ((model3 == null) ? dailcapacity : Convert.ToInt32(model3.CurrentCapacity));
					tblTeamCapacityCalculation capcal = new tblTeamCapacityCalculation();
					capcal.Updatedate = DateTime.Now;
					capcal.InstallDate = Convert.ToDateTime(order.InstallDate);
					capcal.OrderId = order.OrderId;
					capcal.ServiceProviderId = Convert.ToInt32(itemspid);
					capcal.DailyCapacity = dailcapacity;
					capcal.ConsumedCapacity = calcva;
					capcal.CurrentCapacity = curcap - calcva;
					double curavailabe = (double)capcal.CurrentCapacity.Value / (double)capcal.DailyCapacity.Value * 100.0;
					if (curavailabe < 0.0)
					{
						curavailabe = 0.0;
					}
					capcal.CapcityPercentage = Convert.ToDecimal(curavailabe);
					_context.tblTeamCapacityCalculations.Add(capcal);
					_context.SaveChanges();
				}
				calcva = 0;
			}
		}

		public static async Task NewOrderAPI(int GroupTypeId, int UserGroupType, int OrderId, string InvoiceNo, int?[] basic, List<vm_OrderServices> services)
		{
			_ = 6;
			try
			{
				new[]
				{
				new
				{
					dates = ""
				}
			}.ToList();
				AlmaneaDbEntities db = new AlmaneaDbEntities();
				db_User objUser = new db_User();
				db_Settings objSettings = new db_Settings();
				tblOrder ordernbyidorderdata = await objSettings.GetOrder(OrderId);
				string msg = (await objSettings.GetSMS("NEWORDER")).SMSTextEN.Replace("{ORDERNO}", InvoiceNo);
				int calcva = 0;
				new List<int>();
				List<SelectListItem> splist = objSettings.GetProviderSetting(GroupTypeId);
				var records = new[]
				{
				new
				{
					ID = "",
					Value = ""
				}
			}.ToList();
				if (splist.Count > 0)
				{
					foreach (SelectListItem sp in splist)
					{
						tblAdminUser activesp = objUser.getactievsp(Convert.ToInt32(sp.Text));
						if (!activesp.Status)
						{
							break;
						}
						int Index = 0;
						foreach (vm_OrderServices item in services)
						{
							tblSetting getsettingper2 = objSettings.GetBlockeddate(Convert.ToInt32(sp.Text));
							if (getsettingper2 != null && getsettingper2.KeyValue != null)
							{
								List<DateTime> bdate = getsettingper2.KeyValue.Split(',').Select(Convert.ToDateTime).ToList();
								foreach (DateTime itemblovkdate in bdate)
								{
									DateTime value = itemblovkdate;
									DateTime? installDate = ordernbyidorderdata.InstallDate;
									if (value == installDate)
									{
										Index = 1;
										break;
									}
								}
							}
							tblAdminUser activespa = objUser.getactievsp(Convert.ToInt32(sp.Text));
							if (!activespa.Status)
							{
								Index = 1;
								break;
							}
							tblSetting wokrhrs = objSettings.GetEorkinhHrsysettings(Convert.ToInt32(sp.Text));
							if (wokrhrs == null || wokrhrs.KeyValue == null)
							{
								Index = 1;
								break;
							}
							int qty = item.Quantity;
							tblServiceMapper spassignedservice2 = objSettings.GetServicesmap2(Convert.ToInt32(sp.Text), item.ServiceId);
							if (spassignedservice2 == null || spassignedservice2.Estimated == null)
							{
								Index = 1;
								break;
							}
							int spestimat = Convert.ToInt32(spassignedservice2.Estimated);
							if (!spassignedservice2.IsWorking)
							{
								Index = 1;
								break;
							}
							calcva += qty * spestimat / 60;
						}
						if (Index == 0)
						{
							records.Add(new
							{
								ID = sp.Text,
								Value = calcva.ToString()
							});
						}
						calcva = 0;
						Convert.ToInt32(sp.Text);
					}
					records = records.Skip(1).ToList();
				}
				foreach (var itemsp in records)
				{
					if (string.IsNullOrEmpty(itemsp.ID))
					{
						continue;
					}
					tblOrder ordernbyid = await objSettings.GetOrder(OrderId);
					tblTeamCapacityCalculation teamcap = objSettings.GetSPCapacitynotinadded(Convert.ToInt32(itemsp.ID), ordernbyid.InstallDate, ordernbyid.OrderId);
					objSettings.GetEorkinhHrsysettings(Convert.ToInt32(itemsp.ID));
					_ = ordernbyid.InstallDate;
					if (teamcap.DailyCapacity >= Convert.ToInt32(itemsp.Value) && teamcap.CurrentCapacity >= Convert.ToInt32(itemsp.Value))
					{
						List<tblAdminUser> spAndAgent3 = objUser.GetServiceProvider(Convert.ToInt32(itemsp.ID));
						if (spAndAgent3.Count > 0 && spAndAgent3 != null && spAndAgent3.Count > 0)
						{
							foreach (tblAdminUser p3 in spAndAgent3)
							{
								await SendSMS(p3.MobileNo, msg);
								objUser.SaveOrderDiplay(OrderId, p3.UserId);
							}
						}
					}
					if (!(teamcap.DailyCapacity <= Convert.ToInt32(itemsp.Value)))
					{
						continue;
					}
					int spidd = Convert.ToInt32(itemsp.ID);
					List<IGrouping<DateTime?, tblTeamCapacityCalculation>> model = (from x in db.tblTeamCapacityCalculations
																					where x.ServiceProviderId == (int?)spidd && x.InstallDate == ordernbyid.InstallDate
																					select x into c
																					group c by c.InstallDate).ToList();
					if (model.Count > 0)
					{
						new vm_installdateblock();
						foreach (IGrouping<DateTime?, tblTeamCapacityCalculation> iteminstaldate in model)
						{
							tblTeamCapacityCalculation totaloccperct = iteminstaldate.OrderByDescending((tblTeamCapacityCalculation x) => x.Id).FirstOrDefault();
							iteminstaldate.Count();
							tblSetting getsettingper = objSettings.Getpercensetting(Convert.ToInt32(itemsp.ID));
							if (getsettingper == null)
							{
								continue;
							}
							decimal? capcityPercentage = totaloccperct.CapcityPercentage;
							decimal num = Convert.ToInt32(getsettingper.KeyValue);
							_ = (capcityPercentage.GetValueOrDefault() < num) & capcityPercentage.HasValue;
							capcityPercentage = totaloccperct.CapcityPercentage;
							num = Convert.ToInt32(getsettingper.KeyValue);
							if (!((capcityPercentage.GetValueOrDefault() >= num) & capcityPercentage.HasValue))
							{
								continue;
							}
							if (!(ordernbyid.InstallDate.ToString() == totaloccperct.InstallDate.ToString()))
							{
								capcityPercentage = totaloccperct.CapcityPercentage;
								num = 100;
								if (!((capcityPercentage.GetValueOrDefault() == num) & capcityPercentage.HasValue))
								{
									continue;
								}
							}
							List<tblAdminUser> spAndAgent2 = objUser.GetServiceProvider(Convert.ToInt32(itemsp.ID));
							if (spAndAgent2.Count <= 0 || spAndAgent2 == null || spAndAgent2.Count <= 0)
							{
								continue;
							}
							foreach (tblAdminUser p3 in spAndAgent2)
							{
								await SendSMS(p3.MobileNo, msg);
								objUser.SaveOrderDiplay(OrderId, p3.UserId);
							}
						}
						continue;
					}
					List<tblAdminUser> spAndAgent = objUser.GetServiceProvider(Convert.ToInt32(itemsp.ID));
					if (spAndAgent.Count <= 0 || spAndAgent == null || spAndAgent.Count <= 0)
					{
						continue;
					}
					foreach (tblAdminUser p3 in spAndAgent)
					{
						await SendSMS(p3.MobileNo, msg);
						objUser.SaveOrderDiplay(OrderId, p3.UserId);
					}
				}
				List<int?> splistdis = objSettings.GetSpOrderDistributed(OrderId);
				if (splistdis.Count > 0)
				{
					await managecapacityAPI(OrderId, GroupTypeId);
				}
			}
			catch (Exception)
			{
				throw;
			}
		}

		public static async Task NewOrder(int OrderId, string InvoiceNo, int?[] basic, List<vm_OrderServices> services)
		{
			_ = 6;
			try
			{
				new[]
				{
				new
				{
					dates = ""
				}
			}.ToList();
				AlmaneaDbEntities db = new AlmaneaDbEntities();
				int userGroupTypeId = Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_UserGroupId]);
				db_User objUser = new db_User();
				db_Settings objSettings = new db_Settings();
				tblOrder ordernbyidorderdata = await objSettings.GetOrder(OrderId);
				string msg = (await objSettings.GetSMS("NEWORDER")).SMSTextEN.Replace("{ORDERNO}", InvoiceNo);
				int calcva = 0;
				new List<int>();
				List<SelectListItem> splist = objSettings.GetProviderSetting(userGroupTypeId);
				var records = new[]
				{
				new
				{
					ID = "",
					Value = ""
				}
			}.ToList();
				if (splist.Count > 0)
				{
					foreach (SelectListItem sp in splist)
					{
						tblAdminUser activesp = objUser.getactievsp(Convert.ToInt32(sp.Text));
						if (!activesp.Status)
						{
							break;
						}
						int Index = 0;
						foreach (vm_OrderServices item in services)
						{
							tblSetting getsettingper2 = objSettings.GetBlockeddate(Convert.ToInt32(sp.Text));
							if (getsettingper2 != null && getsettingper2.KeyValue != null)
							{
								List<DateTime> bdate = getsettingper2.KeyValue.Split(',').Select(Convert.ToDateTime).ToList();
								foreach (DateTime itemblovkdate in bdate)
								{
									DateTime value = itemblovkdate;
									DateTime? installDate = ordernbyidorderdata.InstallDate;
									if (value == installDate)
									{
										Index = 1;
										break;
									}
								}
							}
							tblAdminUser activespa = objUser.getactievsp(Convert.ToInt32(sp.Text));
							if (!activespa.Status)
							{
								Index = 1;
								break;
							}
							tblSetting wokrhrs = objSettings.GetEorkinhHrsysettings(Convert.ToInt32(sp.Text));
							if (wokrhrs == null || wokrhrs.KeyValue == null)
							{
								Index = 1;
								break;
							}
							int qty = item.Quantity;
							tblServiceMapper spassignedservice2 = objSettings.GetServicesmap2(Convert.ToInt32(sp.Text), item.ServiceId);
							if (spassignedservice2 == null || spassignedservice2.Estimated == null)
							{
								Index = 1;
								break;
							}
							int spestimat = Convert.ToInt32(spassignedservice2.Estimated);
							if (!spassignedservice2.IsWorking)
							{
								Index = 1;
								break;
							}
							calcva += qty * spestimat / 60;
						}
						if (Index == 0)
						{
							records.Add(new
							{
								ID = sp.Text,
								Value = calcva.ToString()
							});
						}
						calcva = 0;
						Convert.ToInt32(sp.Text);
					}
					records = records.Skip(1).ToList();
				}
				foreach (var itemsp in records)
				{
					if (string.IsNullOrEmpty(itemsp.ID))
					{
						continue;
					}
					tblOrder ordernbyid = await objSettings.GetOrder(OrderId);
					tblTeamCapacityCalculation teamcap = objSettings.GetSPCapacitynotinadded(Convert.ToInt32(itemsp.ID), ordernbyid.InstallDate, ordernbyid.OrderId);
					objSettings.GetEorkinhHrsysettings(Convert.ToInt32(itemsp.ID));
					_ = ordernbyid.InstallDate;
					if (teamcap.DailyCapacity >= Convert.ToInt32(itemsp.Value) && teamcap.CurrentCapacity >= Convert.ToInt32(itemsp.Value))
					{
						List<tblAdminUser> spAndAgent3 = objUser.GetServiceProvider(Convert.ToInt32(itemsp.ID));
						if (spAndAgent3.Count > 0 && spAndAgent3 != null && spAndAgent3.Count > 0)
						{
							foreach (tblAdminUser p3 in spAndAgent3)
							{
								await SendSMS(p3.MobileNo, msg);
								objUser.SaveOrderDiplay(OrderId, p3.UserId);
							}
						}
					}
					if (!(teamcap.DailyCapacity <= Convert.ToInt32(itemsp.Value)))
					{
						continue;
					}
					int spidd = Convert.ToInt32(itemsp.ID);
					List<IGrouping<DateTime?, tblTeamCapacityCalculation>> model = (from x in db.tblTeamCapacityCalculations
																					where x.ServiceProviderId == (int?)spidd && x.InstallDate == ordernbyid.InstallDate
																					select x into c
																					group c by c.InstallDate).ToList();
					if (model.Count > 0)
					{
						new vm_installdateblock();
						foreach (IGrouping<DateTime?, tblTeamCapacityCalculation> iteminstaldate in model)
						{
							tblTeamCapacityCalculation totaloccperct = iteminstaldate.OrderByDescending((tblTeamCapacityCalculation x) => x.Id).FirstOrDefault();
							iteminstaldate.Count();
							tblSetting getsettingper = objSettings.Getpercensetting(Convert.ToInt32(itemsp.ID));
							if (getsettingper == null)
							{
								continue;
							}
							decimal? capcityPercentage = totaloccperct.CapcityPercentage;
							decimal num = Convert.ToInt32(getsettingper.KeyValue);
							_ = (capcityPercentage.GetValueOrDefault() < num) & capcityPercentage.HasValue;
							capcityPercentage = totaloccperct.CapcityPercentage;
							num = Convert.ToInt32(getsettingper.KeyValue);
							if (!((capcityPercentage.GetValueOrDefault() >= num) & capcityPercentage.HasValue))
							{
								continue;
							}
							if (!(ordernbyid.InstallDate.ToString() == totaloccperct.InstallDate.ToString()))
							{
								capcityPercentage = totaloccperct.CapcityPercentage;
								num = 100;
								if (!((capcityPercentage.GetValueOrDefault() == num) & capcityPercentage.HasValue))
								{
									continue;
								}
							}
							List<tblAdminUser> spAndAgent2 = objUser.GetServiceProvider(Convert.ToInt32(itemsp.ID));
							if (spAndAgent2.Count <= 0 || spAndAgent2 == null || spAndAgent2.Count <= 0)
							{
								continue;
							}
							foreach (tblAdminUser p3 in spAndAgent2)
							{
								await SendSMS(p3.MobileNo, msg);
								objUser.SaveOrderDiplay(OrderId, p3.UserId);
							}
						}
						continue;
					}
					List<tblAdminUser> spAndAgent = objUser.GetServiceProvider(Convert.ToInt32(itemsp.ID));
					if (spAndAgent.Count <= 0 || spAndAgent == null || spAndAgent.Count <= 0)
					{
						continue;
					}
					foreach (tblAdminUser p3 in spAndAgent)
					{
						await SendSMS(p3.MobileNo, msg);
						objUser.SaveOrderDiplay(OrderId, p3.UserId);
					}
				}
				List<int?> splistdis = objSettings.GetSpOrderDistributed(OrderId);
				if (splistdis.Count > 0)
				{
					await managecapacity(OrderId);
				}
			}
			catch (Exception)
			{
				throw;
			}
		}

		public static async Task ConfirmOrder(int OrderId)
		{
			db_Settings objSettings = new db_Settings();
			new db_User();
			tblSM sms = await objSettings.GetSMS("CUSTOMERCONFIRM");
			vm_Order order = await objSettings.GetOrderById(OrderId);
			_ = order.InstallDate;
			tblProviderTimeSlot modeltimeslot = objSettings.GetOrderProviderTimeSlot(OrderId, DateTime.Parse(order.InstallDate));
			if (order != null)
			{
				string AMPM = "";
				bool lang = order.SmsInArabic;
				string hour = (lang ? "الساعة " : "at ");
				if (modeltimeslot.StartHour <= 12)
				{
					AMPM = (lang ? "ص" : "pm");
				}
				if (modeltimeslot.StartHour > 12)
				{
					AMPM = (lang ? "م" : "am");
				}
				Dictionary<string, string> dct = new Dictionary<string, string>();
				dct.Add("{INSTALLDATE}", order.InstallDate.Substring(0, 10) + " " + hour + " " + modeltimeslot.StartHour + " " + AMPM);
				dct.Add("{CODE}", order.CustomerCode.ToString());
				string EncryptOrderId = objSettings.EncryptString(OrderId.ToString(), useHashing: false);
				string link = "https://map.najez.app:8091/pointing-location?order_id=" + EncryptOrderId.ToString();
				string shortUrl = cls_Defaults.ShrinkURLz(link);
				dct.Add("{LINK}", shortUrl);
				string msg = cls_Defaults.StringReplace(lang ? sms.SMSTextAR : sms.SMSTextEN, dct);
				SendSMS(order.CustomerContact, msg);
			}
		}

		public static async Task ExpireOrder(string OrderNo)
		{
			db_Settings objSettings = new db_Settings();
			tblSM sms = await objSettings.GetSMS("NOACTION");
			Dictionary<string, string> dct = new Dictionary<string, string>();
			dct.Add("{ORDERNO}", OrderNo);
			string msg = cls_Defaults.StringReplace(sms.SMSTextEN, dct);
			db_User objUser = new db_User();
			List<tblAdminUser> admin = await objUser.GetUserByTypeId(3);
			if (admin == null || admin.Count <= 0)
			{
				return;
			}
			int IsProoduction = Convert.ToInt32((await objSettings.GetSetting()).IsProoduction);
			if (IsProoduction != 1)
			{
				return;
			}
			foreach (tblAdminUser a in admin)
			{
				SendSMS(a.MobileNo, msg);
			}
		}

		public static async Task CompleteOrder(int OrderId)
		{
			db_Settings objSettings = new db_Settings();
			string strLinkId = EncryptDecrypt.Encrypt((await objSettings.Add_OrderUserLink(OrderId)).ToString());
			string url = cls_Defaults.GetBaseUrl() + "/Complain/Customer/" + strLinkId;
			string shortUrl = await cls_Defaults.ShrinkURL(url);
			tblSM sms = await objSettings.GetSMS("COMPLETEORDER");
			vm_Order order = await objSettings.GetOrderById(OrderId);
			if (order != null)
			{
				bool lang = order.SmsInArabic;
				Dictionary<string, string> dct = new Dictionary<string, string>();
				dct.Add("{ORDERNO}", order.OrderNo);
				dct.Add("{LINK}", shortUrl);
				string msg = cls_Defaults.StringReplace(lang ? sms.SMSTextAR : sms.SMSTextEN, dct);
				int IsProoduction = Convert.ToInt32((await objSettings.GetSetting()).IsProoduction);
				if (IsProoduction == 1)
				{
					await SendSMS(order.CustomerContact, msg);
				}
				else
				{
					await SendSMS(order.CustomerContact, msg);
				}
			}
		}

		public static async Task NewUser(int UserID, string UserName, string Password)
		{
			_ = 2;
			try
			{
				db_User objUser = new db_User();
				db_Settings objSettings = new db_Settings();
				string msg = (await objSettings.GetSMS("NEWUSER")).SMSTextEN.Replace("{UserName}", UserName);
				await SendSMS(message: msg.Replace("{Password}", Password), Recipient: (await objUser.GetUserById(UserID)).MobileNo);
			}
			catch (Exception)
			{
				throw;
			}
		}

		public static async Task ForgotPassword(string Recipient, string Password)
		{
			db_Settings objSettings = new db_Settings();
			tblSM sms = await objSettings.GetSMS("FORGOT");
			Dictionary<string, string> dct = new Dictionary<string, string>();
			dct.Add("{PASSWORD}", Password);
			string msg = cls_Defaults.StringReplace(sms.SMSTextEN, dct);
			SendSMS(Recipient, msg);
		}
	}
}