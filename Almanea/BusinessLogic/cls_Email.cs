using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Almanea.Data;
using Almanea.Models;

namespace Almanea.BusinessLogic
{

	public class cls_Email
	{
		private static string username = "shahein.tasaly@gmail.com";

		private static string password = "tasaly@321";

		private static string from = "no-reply@tasaly.net";

		private static string smtp = "smtp.gmail.com";

		private static async Task SendEmail(string ToEmail, string Subject, string Body)
		{
			MailMessage mailMessage = new MailMessage(new MailAddress(username), new MailAddress(ToEmail));
			NetworkCredential SmtpUser = new NetworkCredential(username, password);
			mailMessage.Subject = Subject;
			mailMessage.Body = Body;
			mailMessage.IsBodyHtml = true;
			SmtpClient smtpMail = new SmtpClient();
			smtpMail.Host = smtp;
			smtpMail.Port = 587;
			smtpMail.DeliveryMethod = SmtpDeliveryMethod.Network;
			smtpMail.UseDefaultCredentials = false;
			smtpMail.Credentials = SmtpUser;
			await smtpMail.SendMailAsync(mailMessage);
		}

		public static async Task FinishOrder(int OrderId)
		{
			db_Settings objSettings = new db_Settings();
			vm_Email template = await objSettings.getEmailById(1);
			db_User objUser = new db_User();
			tblOrder order = await objUser.GetOrderById(OrderId);
			int addedBy = order.AddedBy;
			vm_User user = await objUser.GetUserById(addedBy);
			Dictionary<string, string> dct = new Dictionary<string, string>();
			dct.Add("{SUPPLIERNAME}", user.FirstName + " " + user.LastName);
			dct.Add("{ORDERNO}", order.OrderId.ToString());
			dct.Add("{ORDERDATE}", order.AddedDate.ToString("dd/MM/yyyy hh:mm:ss tt", cls_Defaults.DateTimeCulture));
			dct.Add("{CUSTOMERNAME}", order.CustomerName);
			dct.Add("{CUSTOMERCONTACT}", order.CustomerContact);
			dct.Add("{AREA}", order.tblLocation.LocationNameEN);
			string strEmailBody = template.EmailTextEN;
			foreach (KeyValuePair<string, string> item in dct)
			{
				strEmailBody = strEmailBody.Replace(item.Key, item.Value);
			}
			await SendEmail(user.Email, template.SubjectEN, strEmailBody);
		}
	}
}