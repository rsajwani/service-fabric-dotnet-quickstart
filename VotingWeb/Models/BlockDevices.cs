using System.Collections.Generic;

namespace VotingWeb.Models
{
	public class BlockDevices
	{
		public string Name { get; set; }
		public string Size { get; set; }
		public string Type { get; set; }
		public List<string> MountPoints { get; set; }
		public string FSType { get; set; }
		public string FSUsed { get; set; }
		public string Group { get; set; }
		public string SubSystems { get; set; }
		public List<LogicalVolume> Volumes { get; set; }

		public BlockDevices()
		{
			MountPoints = new List<string>();
			Volumes = new List<LogicalVolume>();
			Name = string.Empty;
			Size = string.Empty;
			Type = string.Empty;
			FSType = string.Empty;
			FSUsed = string.Empty;
			Group = string.Empty;
			SubSystems = string.Empty;
		}
	}
}