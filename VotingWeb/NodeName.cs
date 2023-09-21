namespace VotingWeb.Controllers
{
    using System.Collections.Generic;
    using System;
    using System.Fabric;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Linq;
    using Microsoft.ServiceFabric.Services.Remoting.Client;
    using Microsoft.ServiceFabric.Services.Remoting;
    using static System.Net.WebRequestMethods;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;

    public class NodeModel
    {
        public string NodeName { get; set; }
        public List<AppDetails> Apps { get; set; }

        public static async Task<List<NodeModel>> GetNodesFromServiceFabric()
        {
            try
            {
                using (var fabricClient = new FabricClient())
                {
                    var nodes = await fabricClient.QueryManager.GetNodeListAsync();

                    var nodeModels = new List<NodeModel>();
                    // Map nodes to NodeModel
                    //var nodeModels = nodes.Select(node => new NodeModel
                    //{
                    //    NodeName = node.NodeName,
                    //}).ToList();


                    foreach (var n in nodes)
                    {
                        var nodeModel = new NodeModel();
                        nodeModel.NodeName = n.NodeName;

                        nodeModel.Apps = new List<AppDetails>();

                        var deployedApps = await fabricClient.QueryManager.GetDeployedApplicationListAsync(n.NodeName);

                        foreach (var app in deployedApps)
                        {
                            var appDetail = new AppDetails();
                            appDetail.CodePkg = new List<CodePkgDetails>();
                            appDetail.AppName = app.ApplicationName;

                            var deployedCode = await fabricClient.QueryManager.GetDeployedCodePackageListAsync(n.NodeName, app.ApplicationName);

                            foreach (var code in deployedCode)
                            {
                                //var voteDataApplication = deployedCode.FirstOrDefault(cc => cc.ServiceManifestName == "VotingDataPkg");

                                if (code.ServiceManifestName == "VotingDataPkg")
                                {
                                    var c = new CodePkgDetails();
                                    c.Disks = new List<DiskDetails>();
                                    c.CodeName = code.ServiceManifestName;
                                    // Construct the URL to call the List() method
                                    var baseUrl = $"http://{n.IpAddressOrFQDN}:83/";
                                    //var listUrl = $"{baseUrl}Voting/VotingData/api/VoteData";
                                    var listUrl = $"{baseUrl}api/VoteData";
                                    c.ServiceUrl = listUrl;
                                    //var uri2 = "http://localhost:8081/Voting/VotingData/api/VoteData?PartitionKey=0&PartitionKind=Int64Range";
                                    //Uri serviceUri = new Uri("fabric:/MyApplication/MyService");

                                    // Create a proxy to the service
                                    //IMyService serviceProxy = ServiceProxy.Create<IMyService>(serviceUri);

                                    // Make an HTTP call to the List() method
                                    using (var httpClient = new HttpClient())
                                    {
                                        var response = await httpClient.GetAsync(listUrl);
                                        if (response.IsSuccessStatusCode)
                                        {
                                            List<List<KeyValuePair<string, string>>> result = new List<List<KeyValuePair<string, string>>>();
                                            var jsonString = await response.Content.ReadAsStringAsync();
                                            result = JsonConvert.DeserializeObject<List<List<KeyValuePair<string, string>>>>(jsonString);

                                            foreach (List<KeyValuePair<string, string>> innerList in result)
                                            {
                                                DiskDetails diskDetail = ConvertToDisk(innerList);
                                                c.Disks.Add(diskDetail);
                                            }
                                        }
                                        else
                                        {
                                            DiskDetails diskDetail = new DiskDetails();
                                            c.Disks.Add(diskDetail);
                                        }
                                    }
                                    appDetail.CodePkg.Add(c);
                                }
                            }
                            nodeModel.Apps.Add(appDetail);
                        }
                        nodeModels.Add(nodeModel);
                    }

                    return nodeModels;
                }
            }
            catch (Exception ex)
            {   
                Console.WriteLine(ex.Message);
                // Handle exceptions as needed
                var tmp = new List<NodeModel>();
                tmp.Add(CreateEmptyNodeModel(ex.Message));
                return tmp;
            }
        }

        private static NodeModel CreateEmptyNodeModel(string name)
        {
            var nodeModel = new NodeModel();
            nodeModel.NodeName = name;
            nodeModel.Apps = new List<AppDetails>();
            AppDetails appDetail = new AppDetails();
            appDetail.CodePkg = new List<CodePkgDetails>();
            appDetail.AppName = new Uri("http://test.com");
            var c = new CodePkgDetails();
            c.Disks = new List<DiskDetails>();
            c.CodeName = "";
            appDetail.CodePkg.Add(c);
            nodeModel.Apps.Add(appDetail);
            return nodeModel;
        }

        private static DiskDetails ConvertToDisk(List<KeyValuePair<string,string>> keyValuePairs)
        {
            DiskDetails diskDetails = new DiskDetails
            {
                DiskName = GetValueByKey(keyValuePairs, "Friendly Name"),
                DeviceID = GetValueByKey(keyValuePairs, "Device ID"),
                DeviceNumber = GetValueByKey(keyValuePairs, "Device Number"),
                MediaType = GetValueByKey(keyValuePairs, "Media Type"),
                BusType = GetValueByKey(keyValuePairs, "Bus Type"),
                Size = GetValueByKey(keyValuePairs, "Size"),
                AllocatedSize = GetValueByKey(keyValuePairs, "Allocated Size"),
                HealthStatus = GetValueByKey(keyValuePairs, "Health Status"),
                PhysicalSectorSize = GetValueByKey(keyValuePairs, "Physical Sector Size"),
                LogicalSectorSize = GetValueByKey(keyValuePairs, "Logical Sector Size"),
                NamespaceMeta = GetValueByKey(keyValuePairs, "Namespaces Meta")
            };

            return diskDetails;
        }

        // Helper method to retrieve a value by key from the list of KeyValuePairs
        static string GetValueByKey(List<KeyValuePair<string, string>> keyValuePairs, string key)
        {
            var pair = keyValuePairs.FirstOrDefault(kv => kv.Key == key);
            return pair.Value;
        }
    }


    public class DiskDetails
    {
        public string DiskName { get; set; }
        public string DeviceID { get; set; }
        public string DeviceNumber { get; set; }
        public string MediaType { get; set; }
        public string BusType { get; set; }
        public string Size { get; set; }
        public string AllocatedSize { get; set; }
        public string HealthStatus { get; set; }
        public string PhysicalSectorSize { get; set; }
        public string LogicalSectorSize { get; set; }
        public string NamespaceMeta { get; set; }
    }

    public class Namespace
    {
        public string NamespaceID { get; set; }
        public List<Namespace> namespaces { get; set; }
    }

    public class AppDetails
    {
        public Uri AppName { get; set; }
        public List<CodePkgDetails> CodePkg { get; set; }
        // Add additional properties as needed
    }

    public class CodePkgDetails
    {
        public string CodeName { get; set; }
        public string ServiceUrl { get; set; }
        public List<DiskDetails> Disks { get; set; }
    }
}
