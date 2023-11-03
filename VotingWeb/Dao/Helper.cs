namespace VotingWeb.Dao
{
	using System.Collections.Generic;
	using System;
	using System.Fabric;
	using System.Net.Http;
	using System.Threading.Tasks;
	using System.Linq;
	using static System.Net.WebRequestMethods;
	using Microsoft.AspNetCore.Mvc;
	using Models;
	using System.Fabric.Description;
	using System.Fabric.Query;
	using ModelReplica = Models.Replica;
	using ModelPartition = Models.Partition;
	using ModelServiceEndpoint = Models.ServiceEndpoint;
	using System.Xml.Linq;
	using Newtonsoft.Json.Linq;
    using System.Collections.Concurrent;

    public class Helper
	{
		public static async Task<Cluster> GetNodesFromServiceFabric()
		{
			Cluster cluster = new Cluster();
			cluster.Name = "Default";
			cluster.Region = "";
			cluster.MediaNodes = GetAllMediaNodesForCluster().Result;
			cluster.NodeCount = cluster.MediaNodes.Count;
			string applicationName = "fabric:/Voting";
			string serviceName = "fabric:/Voting/VotingData";
			cluster.Partitions = GetAllPartitionsForGivenService(applicationName, serviceName).Result;
			cluster.PartitionCount = cluster.Partitions.Count;
			
			return cluster;
		}

		private static async Task<List<MediaNode>> GetAllMediaNodesForCluster()
		{
			List<MediaNode> mediaNodes = new List<MediaNode>();
			try
			{
				// string endpoint = "https://sf-demo2.westus.cloudapp.azure.com:19080";
				using (var fabricClient = new FabricClient())
				{
					var nodes = await fabricClient.QueryManager.GetNodeListAsync();
					foreach (var node in nodes)
					{
						MediaNode mediaNode = new MediaNode();
						mediaNode.NodeName = node.NodeName;

						var baseUrl = $"http://{node.IpAddressOrFQDN}/";
						//var listUrl = $"{baseUrl}Voting/VotingData/api/VoteData";
						//var listUrl = $"{baseUrl}/api/WSData";
						mediaNode.ServiceUrl = baseUrl;

						// Make an HTTP call to the List() methodtry{
						/*try
						{
							using (var httpClient = new HttpClient())
							{
								var response = await httpClient.GetAsync(listUrl);
								if (response.IsSuccessStatusCode)
								{
									var jsonString = await response.Content.ReadAsStringAsync();
									mediaNode.Health = jsonString;
								}
								else
								{
									mediaNode.Health = response.StatusCode.ToString();
								}
							}
						}
						catch (Exception ex)
						{
							mediaNode.Health = ex.Message;
						}*/

						mediaNode.BlockDevices = new List<BlockDevices>();
						mediaNodes.Add(mediaNode);
					}
				}
			}
			catch (Exception ex)
			{
				MediaNode n = new MediaNode();
				n.NodeName = ex.Message;
				mediaNodes.Add(n);
			}

			return mediaNodes;
		}

		private static async Task<List<ModelPartition>> GetAllPartitionsForGivenService(string applicationName, string serviceName)
		{
			List<ModelPartition> allPartitions = new List<ModelPartition>();
			try
			{
				using (var fabricClient = new FabricClient())
				{
					// Service Fabric application and service names
					//string applicationName = "fabric:/OrionWSDemoDP";
					//string serviceName = "fabric:/OrionWSDemoDP/OrionDPFacade";


					// List all the services of the application where service name starts with "fabric:/OrionWSDemoDP"
					var services = await fabricClient.QueryManager.GetServiceListAsync(new Uri(applicationName));
					foreach (var service in services)
					{
						// List all partitions of the stateful service
						var partitions = await fabricClient.QueryManager.GetPartitionListAsync(service.ServiceName);

						foreach (var partition in partitions)
						{
							ModelPartition p = new ModelPartition();
							p.Id = partition.PartitionInformation.Id.ToString();
							p.ApplicationName = applicationName;
							p.ServiceName = service.ServiceName.ToString();

							// List replicas for this partition
							var replicas = await fabricClient.QueryManager.GetReplicaListAsync(partition.PartitionInformation.Id);
							List<ModelReplica> allReplicas = new List<ModelReplica>();
							foreach (var replica in replicas)
							{
								ModelReplica r = new ModelReplica();
								r.Id = replica.Id.ToString();
								r.NodeName = replica.NodeName;
								r.Role = replica.ReplicaStatus.ToString();
								r.Health = replica.HealthState.ToString();
								// Check if this replica is the primary replica
								//if (replica.ReplicaRole == ReplicaRole.Primary)
								//{
								//	Console.WriteLine("This is the primary replica.");
								//}

								// List service endpoints exposed by this replica
								var serviceEndpoints = replica.ReplicaAddress.Split(';');
								List<ModelServiceEndpoint> allServiceEndpoints = new List<ModelServiceEndpoint>();
								foreach (var endpoint in serviceEndpoints)
								{
									ModelServiceEndpoint se = new ModelServiceEndpoint();

									// Parse the JSON string
									JObject jsonObject = JObject.Parse(endpoint);
									if (jsonObject != null && jsonObject["Endpoints"] != null)
									{
										// Extract the endpoint details
										string ePoint = jsonObject["Endpoints"][""].ToString();
										se.Url = ePoint;
										var listUrl = $"{ePoint}/api/WSData";

										// Make an HTTP call to the List() methodtry{
										try
										{
											using (var httpClient = new HttpClient())
											{
												var response = await httpClient.GetAsync(listUrl);
												if (response.IsSuccessStatusCode)
												{
													var jsonString = await response.Content.ReadAsStringAsync();
													r.WebAPI = jsonString;
												}
												else
												{
													r.WebAPI = response.StatusCode.ToString();
												}
											}
										}
										catch (Exception ex)
										{
											r.WebAPI = ex.Message;
										}

									}

									allServiceEndpoints.Add(se);
								}
								r.Endpoints = allServiceEndpoints;

								// Get health status of this replica
								//var replicaHealth = await fabricClient.HealthManager.GetReplicaHealthAsync(partition.PartitionInformation.Id, replica.Id);
								//r.Health = replicaHealth.AggregatedHealthState.ToString();
								allReplicas.Add(r);
							}

							p.Replicas = allReplicas;
							allPartitions.Add(p);
						}
					}
				}
			}
			catch (Exception ex)
			{
				ModelPartition err = new ModelPartition();
				err.Id = ex.Message;
				allPartitions.Add(err);
			}
			return allPartitions;
		}

        static async Task<bool> ProvisionApplication(string sfapPath)
        {
            // Specify your cluster connection information
            string clusterEndpoint = "http://localhost:19080"; // Replace with your cluster endpoint
            FabricClient fabricClient = new FabricClient(clusterEndpoint);

            // Specify the path to your SFAP (Service Fabric Application Package) ZIP file
            //string sfapPath = @"C:\Path\To\Your\ApplicationPackage.zip";

            try
            {
                // Provision the application
                //await fabricClient.ApplicationManager.ProvisionApplicationAsync(
                    //new ApplicationDescription(sfapPath));

                Console.WriteLine("Application provisioned successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
				return false;
            }

			return true;
        }

		public static async Task<bool> isServiceReady(Uri serviceName, int size)
		{
            FabricClient fabricClient = new FabricClient();
            var partitions = await fabricClient.QueryManager.GetPartitionListAsync(serviceName);
			int count = 0;
            foreach (var partition in partitions)
			{
				// list all the replicas for a given service instance
				var replicas = await fabricClient.QueryManager.GetReplicaListAsync(partition.PartitionInformation.Id);
				foreach (var replica in replicas)
				{
					// call the web api for each replica
					var serviceEndpoints = replica.ReplicaAddress.Split(';');
					foreach (var endpoint in serviceEndpoints)
					{
						// if endpoint is not empty
						if (endpoint.Length > 0)
						{
							count++;
						}
					}
				}
			}
			return count == size;
		}

        public static async Task<bool> CreatePartition(string applicationName, string serviceName)
        {
            //string applicationName = "fabric:/OrionWSDemoDP";
            //string serviceName = "fabric:/OrionWSDemoDP/OrionDPFacade";

            // Create a FabricClient
            FabricClient fabricClient = new FabricClient();

            // Define the partition scheme
            // Configure the HTTP request pipeline.
            //PartitionSchemeDescription partitionScheme = new UniformInt64RangePartitionSchemeDescription(3, 0, 2); // (1,0, 1)
            /*if (!app.Environment.IsDevelopment())
            {
                partitionScheme = new UniformInt64RangePartitionSchemeDescription(3, 0, 2);
            } else
			{
                partitionScheme = new UniformInt64RangePartitionSchemeDescription(3, 0, 2);
            }*/
            // Generate a unique partition key (can be based on a timestamp or other unique identifier)
            string partitionKey  = Guid.NewGuid().ToString();

            try
            {
                // Create a service description
                StatefulServiceDescription serviceDescription = new StatefulServiceDescription
                {
                    ApplicationName = new Uri(applicationName),
                    ServiceName = new Uri(serviceName + "/" + partitionKey), // Append the partition key
                    ServiceTypeName = "VotingDataType",
                    PartitionSchemeDescription = new UniformInt64RangePartitionSchemeDescription(1, 0, 1), // (1,0, 1)
                    HasPersistedState = true,
                    MinReplicaSetSize = 3, //1
                    TargetReplicaSetSize = 3, //1
                };

			

                // Create the service
                await fabricClient.ServiceManager.CreateServiceAsync(serviceDescription);


				while (! await isServiceReady(serviceDescription.ServiceName, serviceDescription.TargetReplicaSetSize))
				{
                    // sleep for 10 seconds
                    System.Threading.Thread.Sleep(10000);
                }
				
                var partitions = await fabricClient.QueryManager.GetPartitionListAsync(serviceDescription.ServiceName);

                foreach (var partition in partitions)
                {
                    // list all the replicas for a given service instance
                    var replicas = await fabricClient.QueryManager.GetReplicaListAsync(partition.PartitionInformation.Id);
                    foreach (var replica in replicas)
					{ 
                        // call the web api for each replica
                        var serviceEndpoints = replica.ReplicaAddress.Split(';');
                        foreach (var endpoint in serviceEndpoints)
                        {
							// if endpoint is not empty
							if (endpoint.Length > 0)
							{
								// Parse the JSON string
								JObject jsonObject = JObject.Parse(endpoint);
								if (jsonObject != null && jsonObject["Endpoints"] != null)
								{
									// Extract the endpoint details
									string ePoint = jsonObject["Endpoints"][""].ToString();
									var listUrl = $"{ePoint}/api/WSData/CreateWALService?sfServiceName=" + serviceDescription.ServiceName + "&partitionId=" + partition.PartitionInformation.Id + "&endpoint=" + ePoint;

									// Make an HTTP call to the List() methodtry{
									try
									{
										using (var httpClient = new HttpClient())
										{
											// call put method
											var response = await httpClient.PutAsync(listUrl, null);
											//var response = await httpClient.PutAsync(listUrl);
											if (response.IsSuccessStatusCode)
											{
												var jsonString = await response.Content.ReadAsStringAsync();
												Console.WriteLine(jsonString);
											}
											else
											{
												Console.WriteLine(response.StatusCode.ToString());
											}
										}
									}
									catch (Exception ex)
									{
										Console.WriteLine(ex.Message);
									}
								}
							} else
							{

							}
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            return true;
        }
    }
}