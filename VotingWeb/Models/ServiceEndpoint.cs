using System;

namespace VotingWeb.Models
{
	public class ServiceEndpoint
	{
		public string Dns { get; set; }
		public int Port { get; set; }
		public string Protocol { get; set; }

		private string _url;
		// create property to set url
		public string Url
		{
			get
			{
				return _url;
			}
			set
			{
				_url = value;
				// parse url to get dns, port, and protocol
				if (!string.IsNullOrEmpty(_url))
				{
					var uri = new Uri(_url);
					Dns = uri.Host;
					Port = uri.Port;
					Protocol = uri.Scheme;
				}
			}
		}

		public ServiceEndpoint()
		{
			_url = string.Empty;
			Dns = string.Empty;
			Port = 0;
			Protocol = string.Empty;

		}
	}
}