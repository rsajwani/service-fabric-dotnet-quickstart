using System.Collections.Generic;

namespace VotingWeb.Models
{
	public class Replica
	{
		public List<ServiceEndpoint> Endpoints{ get; set; }
		public string Id { get; set; }
		public string NodeName { get; set; }
		public string Role { get; set; }
		public string Health { get; set; }
		public string WebAPI { get; set; }


		public Replica()
		{
			Endpoints = new List<ServiceEndpoint>();
			Id = string.Empty;
			NodeName = string.Empty;
			Role = string.Empty;
			Health = string.Empty;
			WebAPI = string.Empty;
		}
	}
}