using System.Collections.Generic;

namespace VotingWeb.Models
{
	public class Cluster
	{
		public List<MediaNode> MediaNodes{ get; set; }
		public List<Partition> Partitions { get; set; }
		public string Name { get; set; }
		public string Region { get; set; }
		public int NodeCount { get; set; }
		public int PartitionCount { get; set; }


		public Cluster()
		{
			MediaNodes = new List<MediaNode>();
			Partitions = new List<Partition>();
			Name = string.Empty;
			Region = string.Empty;
			NodeCount = 0;
			PartitionCount = 0;
		}
	}
}