using System.Collections.Generic;

namespace VotingWeb.Models
{
	public class Partition
	{
		public List<Replica> Replicas{ get; set; }
		public string Id { get; set; }
		public string ApplicationName { get; set; }
		public string ServiceName { get; set; }


		public Partition()
		{
			Replicas = new List<Replica>();
			Id = string.Empty;
			ApplicationName = string.Empty;
			ServiceName = string.Empty;
		}
	}
}