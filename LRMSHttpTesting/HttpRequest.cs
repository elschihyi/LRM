using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace LRMSHttpTesting
{
	public delegate void WebCallResponds (MyWebResponse MyWebResponse);

	public class HttpRequest
	{
		public static string Request(string Url, string Method, string Payload)
		{
			HttpWebRequest MyWebRequest = (HttpWebRequest)WebRequest.Create(Url);
			MyWebRequest.ProtocolVersion = HttpVersion.Version10;
			MyWebRequest.Method = Method;
			MyWebRequest.ContentType = "application/x-www-form-urlencoded";
			MyWebRequest.Timeout = 15000;//15 sec

			if (Method == "POST" || Method == "PUT")
			{
				byte[] data = Encoding.ASCII.GetBytes(Payload);
				MyWebRequest.ContentLength = data.Length;
				using (var stream = MyWebRequest.GetRequestStream())
				{
					stream.Write(data, 0, data.Length);
				}
			}
			using (WebResponse response = MyWebRequest.GetResponse())
			{
				StringBuilder stringbuilder = new StringBuilder();
				using (StreamReader reader = new StreamReader(response.GetResponseStream()))
				{
					string line;
					while ((line = reader.ReadLine()) != null)
					{
						stringbuilder.Append(line);
					}
					return stringbuilder.ToString();
				}
			}
		}	

		public static void RequestAsync(string Url,string Method, string Payload, WebCallResponds WebRespond)
		{
			ThreadPool.QueueUserWorkItem((o) =>
			{
				try
				{
					MyWebResponse NewWebResponse = new MyWebResponse();
					NewWebResponse.Success = true;
					NewWebResponse.RespondData = Request(Url, Method, Payload);
					if (WebRespond != null)
					{
						WebRespond(NewWebResponse);
					}
				}
				catch(Exception e)
				{
					MyWebResponse NewWebResponse = new MyWebResponse();
					NewWebResponse.Success = false;
					NewWebResponse.ErrMsg = e.ToString();
					NewWebResponse.RespondData = "";
					if (WebRespond != null)
					{
						WebRespond(NewWebResponse);
					}
				}	
			});
		}
	}

	public class MyWebResponse
	{
		public bool Success = false;
		public string ErrMsg = "";
		public string RespondData = "";
	}	
}

