using System.Collections.Generic;

namespace VotingWeb.Models
{
	public class LogicalVolume
	{
		public string Name { get; set; }
		public string Size { get; set; }
		public string Type { get; set; }
		public List<string> MountPoints { get; set; }
		public string FSType { get; set; }
		public string FSUsed { get; set; }
		public string Group { get; set; }
		public string SubSystems { get; set; }

		public string Ip { get; set; }
		public string Port { get; set; }
		public string DeviceId { get; set; }
		public string VgId { get; set; }
		public string LvId { get; set; }

		public LogicalVolume()
		{
			MountPoints = new List<string>();
			Name = string.Empty;
			Size = string.Empty;
			Type = string.Empty;
			FSType = string.Empty;
			FSUsed = string.Empty;
			Group = string.Empty;
			SubSystems = string.Empty;
			Ip = string.Empty;
			Port = string.Empty;
			DeviceId = string.Empty;
			VgId = string.Empty;
			LvId = string.Empty;
		}

		public string Print()
		{
			return "LogicalVolume{" +
					"name='" + Name + '\'' +
					", size='" + Size + '\'' +
					", type='" + Type + '\'' +
					", mountpoints=" + MountPoints +
					", fstype='" + FSType + '\'' +
					", fsused='" + FSUsed + '\'' +
					", group='" + Group + '\'' +
					", subsystems='" + SubSystems + '\'' +
					'}';
		}
	}

}