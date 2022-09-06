using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using Almanea.Data;

namespace Almanea.BusinessLogic
{

	public class cls_PushNotification
	{
		private static string ServerKey = "AAAA6w2hc30:APA91bHU3A5mVDtcgdyHXp4Dnubm15TyjfaS9FkuynhMeOnreqbjMJLn3sM9p6m2lInShjQlWoX6KwVyhTEPCGeNJtDbADJ2W4YXvOcjahr0yN_BY8hYhEWNz8qCyErHsmcVxz8O1lgt";

		private static string SenderID = "1009545999229";

		public static void PushNotification(string DeviceId, string Title, string Body)
		{
			try
			{
				dynamic data = new
				{
					to = DeviceId,
					notification = new
					{
						title = Title,
						body = Body
					}
				};
				JavaScriptSerializer serializer = new JavaScriptSerializer();
				dynamic json = serializer.Serialize(data);
				byte[] byteArray = Encoding.UTF8.GetBytes(json);
				string SERVER_API_KEY = ServerKey;
				string SENDER_ID = SenderID;
				WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send");
				tRequest.Method = "post";
				tRequest.ContentType = "application/json";
				tRequest.Headers.Add($"Authorization: key={SERVER_API_KEY}");
				tRequest.Headers.Add($"Sender: id={SENDER_ID}");
				tRequest.ContentLength = byteArray.Length;
				Stream dataStream = tRequest.GetRequestStream();
				dataStream.Write(byteArray, 0, byteArray.Length);
				dataStream.Close();
				WebResponse tResponse = tRequest.GetResponse();
				dataStream = tResponse.GetResponseStream();
				StreamReader tReader = new StreamReader(dataStream);
				string sResponseFromServer = tReader.ReadToEnd();
				tReader.Close();
				dataStream.Close();
				tResponse.Close();
			}
			catch (Exception)
			{
				throw;
			}
		}

		public static async Task PushNotificationToDriver(int OrderId)
		{
			_ = 3;
			try
			{
				db_User objUser = new db_User();
				db_Settings objSettings = new db_Settings();
				tblOrder order = await objUser.GetOrderById(OrderId);
				List<OrdersAssigned> assigned = await objUser.GetAssignedOrderById(OrderId);
				tblPushNotification pushNotification = await objSettings.GetPushNotification("AssignDriver");
				if (order == null || !(order.DriverId > 0))
				{
					return;
				}
				List<string> add_list = new List<string>();
				foreach (OrdersAssigned item in assigned.ToList())
				{
					add_list.Add((await objUser.GetUsertbl(item.LabourId.Value)).DeviceToken);
				}
				List<string> users = new List<string>();
				users.AddRange(add_list.Distinct());
				if (add_list == null)
				{
					return;
				}
				foreach (string user in users)
				{
					string template = pushNotification.PushNotificationTextEN.Replace("{OrderNo}", order.OrderId.ToString());
					template = template.Replace("{AppointmentDate}", Convert.ToDateTime(order.InstallDate).ToString("dd/MM/yyyy"));
					PushNotification(user, "Appointment Confirm", template.ToString());
				}
			}
			catch (Exception)
			{
				throw;
			}
		}

		public static async Task PushNotificationToLabour(int OrderId)
		{
			_ = 3;
			try
			{
				db_User objUser = new db_User();
				db_Settings objSettings = new db_Settings();
				tblOrder order = await objUser.GetOrderById(OrderId);
				List<OrdersAssigned> assigned = await objUser.GetAssignedOrderById(OrderId);
				tblPushNotification pushNotification = await objSettings.GetPushNotification("AssignLabour");
				if (order == null || order.LabourId <= 0)
				{
					return;
				}
				foreach (OrdersAssigned item in assigned.ToList())
				{
					tblAdminUser user = await objUser.GetUsertbl(item.LabourId.Value);
					if (user != null && user.DeviceToken != null)
					{
						string template = pushNotification.PushNotificationTextEN.Replace("{OrderNo}", order.OrderId.ToString());
						PushNotification(user.DeviceToken, "Received from Warehouse", template.ToString());
					}
				}
			}
			catch (Exception)
			{
				throw;
			}
		}
	}
}