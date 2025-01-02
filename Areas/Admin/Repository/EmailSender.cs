using System.Net;
using System.Net.Mail;

namespace Shopping_Tutorial.Areas.Admin.Repository
{
	public class EmailSender : IEmailSender
	{
		public Task SendEmailAsync(string email, string subject, string message)
		{
			var client = new SmtpClient("smtp.gmail.com", 587)
			{
				EnableSsl = true, //bật bảo mật
				UseDefaultCredentials = false,
				Credentials = new NetworkCredential("2024802010305@student.tdmu.edu.vn", "czsf svfe ggkv ykvc")
			};

			return client.SendMailAsync(
				new MailMessage(from: "2024802010305@student.tdmu.edu.vn",
								to: email,
								subject,
								message
								));
		}
	}
}
