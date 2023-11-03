using System.Collections.Generic;

namespace VotingData.Model
{
	public class ServiceInfo
	{
		public string GuestServiceName{ get; set; }
		public string WALServiceEndpoint{ get; set; }
        public string MyServiceEndpoint { get; set; }
        public string PartitionId { get; set; }
        public string LastPingTime { get; set; }
        public string LastPingMessage { get; set; }


		public List<KeyValuePair<string,string>> toList()
		{
            List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
			list.Add(new KeyValuePair<string, string>("GuestServiceName", GuestServiceName));
			list.Add(new KeyValuePair<string, string>("WALServiceEndpoint", WALServiceEndpoint));
            list.Add(new KeyValuePair<string, string>("MyServiceEndpoint", MyServiceEndpoint ));
            list.Add(new KeyValuePair<string, string>("PartitionId", PartitionId));
            list.Add(new KeyValuePair<string, string>("LastPingTime", LastPingTime));
            list.Add(new KeyValuePair<string, string>("LastPingMessage", LastPingMessage));
            return list;
        }
	}
}