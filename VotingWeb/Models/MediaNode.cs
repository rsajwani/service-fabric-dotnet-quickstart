using System.Collections.Generic;
using VotingWeb.Models;

namespace VotingWeb.Models
{
	public class MediaNode
	{
		public string NodeName { get; set; }
		public string Health { get; set; }
		public string ServiceUrl { get; set; }
		public List<BlockDevices> BlockDevices { get; set; }

		public MediaNode()
		{
			BlockDevices = new List<BlockDevices>();
			NodeName = string.Empty;
			Health = string.Empty;
			ServiceUrl = string.Empty;
		}
	}
}