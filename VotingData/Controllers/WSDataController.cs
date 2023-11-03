namespace VotingData.Controllers
{
	using System.Collections.Generic;
    using System.Fabric;
    using System.Threading;
    using System.Threading.Tasks;
    using Model;
    using Microsoft.AspNetCore.Mvc;
	using Microsoft.ServiceFabric.Data;
	using Microsoft.ServiceFabric.Data.Collections;
    using Microsoft.ServiceFabric.Services.Client;
    using Microsoft.ServiceFabric.Data.Collections;
    using System.Diagnostics;
    using System.Fabric.Description;
    using System.Fabric.Query;
    using Grpc.Net.Client;
    using GrpcClient1;
    using System;

    [Route("api/[controller]")]
	public class WSDataController : Controller
	{
		private readonly IReliableStateManager stateManager;
        private ServiceInfo serviceInfo;

		public WSDataController(IReliableStateManager stateManager)
		{
			this.stateManager = stateManager;
            this.serviceInfo = new ServiceInfo();
		}

		// GET api/WSData
		[HttpGet]
		public async Task<IActionResult> Get()
		{
			//List<List<KeyValuePair<string, string>>> listOfLists = new List<List<KeyValuePair<string, string>>>();
            // creat keyvaluepair list

            //List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
            //list.Add(new KeyValuePair<string, string>("test", "test"));
            //listOfLists.Add(ServiceInfo.toList());

            CancellationToken ct = new CancellationToken();

            IReliableDictionary<string, string> endPointDictionary = await this.stateManager.GetOrAddAsync<IReliableDictionary<string, string>>("endpoints");

            using (ITransaction tx = this.stateManager.CreateTransaction())
            {
                Microsoft.ServiceFabric.Data.IAsyncEnumerable<KeyValuePair<string, string>> list = await endPointDictionary.CreateEnumerableAsync(tx);

                Microsoft.ServiceFabric.Data.IAsyncEnumerator<KeyValuePair<string, string>> enumerator = list.GetAsyncEnumerator();

                List<KeyValuePair<string, string>> result = new List<KeyValuePair<string, string>>();

                while (await enumerator.MoveNextAsync(ct))
                {
                    //if (enumerator.Current.Key.Equals(this.serviceInfo.WALServiceEndpoint))
                    //{
                    //    continue;
                    //}
                    // add to result
                    result.Add(new KeyValuePair<string, string>(enumerator.Current.Key, enumerator.Current.Value));
                    //result.Add(enumerator.Current);
                }

                return this.Json(result);
            }

            //List<KeyValuePair<string, string>> result = new List<KeyValuePair<string, string>>();
            /*using var channel = GrpcChannel.ForAddress(this.serviceInfo.WALServiceEndpoint);
            var client = new Greeter.GreeterClient(channel);
            var reply = client.SayHello(new HelloRequest { Name = "GreeterClient" });
            if (!reply.Message.Equals(""))
            {
                result.Add(new KeyValuePair<string, string>("PingMessage", reply.Message));
                result.Add(new KeyValuePair<string, string>("PingTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")));
            }
            else
            {
                result.Add(new KeyValuePair<string, string>("Error", "No reply from WALService"));
            }*/

             //return this.Json(list);
        }


        private async Task<List<string>> findServiceEndpoint(string serviceName, string partitionKey)
        {
            // use fabricClient to find the endpoint of the service
            //string applicationName = "fabric:/BackEndService";
            //string serviceName = "fabric:/BackEndService/Guest1";

            ServicePartitionResolver resolver = new ServicePartitionResolver(() => new FabricClient());
            //ServicePartitionResolver resolver = ServicePartitionResolver.GetDefault();

            CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            CancellationToken cancellationToken = cts.Token;
            ServicePartitionKey servicePartitionKey = new ServicePartitionKey(0);

            ResolvedServicePartition partition =
                await resolver.ResolveAsync(new Uri(serviceName), servicePartitionKey, cancellationToken);

            List<string> endpoints = new List<string>();
            foreach (var endpoint in partition.Endpoints)
            {
                endpoints.Add(endpoint.Address);
                // You can access information about each endpoint here.
                // For example, you can check the endpoint.Role or endpoint.Address properties.
                // endpoint.Role will tell you if it's a primary, secondary, or other type of endpoint.
                // endpoint.Address will give you the actual endpoint address (e.g., IP and port).
            }

            return endpoints;

        }

        // PUT api/WSData/name
        [HttpPut("{name}")]
		public async Task<IActionResult> Put(string name)
		{
			IReliableDictionary<string, string> endPointDictionary = await this.stateManager.GetOrAddAsync<IReliableDictionary<string, string>>("endpoints");

            List<string> endpoints = await findServiceEndpoint("fabric:/OrionDP/GrpcService1", "");


            /*Task.Run(() => runExe(@"PackageRoot\Code\GrpcService1.exe"));

            using (ITransaction tx = this.stateManager.CreateTransaction())
			{
				await endPointDictionary.AddOrUpdateAsync(tx, name, "localhost:1001", (key, oldvalue) => oldvalue + 1);
				await tx.CommitAsync();
			}*/

            return new OkResult();
		}

        private async Task runExe(string exePath, string arguments)
        {
            try
            {
                // Start the grpc1.exe process
                var grpcProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = exePath,
                        Arguments = arguments,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                    }
                };

                grpcProcess.Start();

                // Optionally, capture the output and error streams asynchronously
                string output = await grpcProcess.StandardOutput.ReadToEndAsync();
                string error = await grpcProcess.StandardError.ReadToEndAsync();

                // Continue asynchronously without waiting for the process to exit
                grpcProcess.WaitForExit();

                // Optionally, handle the output and error as needed
                if (!string.IsNullOrEmpty(output))
                {
                    // Handle standard output
                }

                if (!string.IsNullOrEmpty(error))
                {
                    // Handle standard error
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions if the process fails to start
                //Console.WriteLine($"Error: {ex.Message}");
            }
        }

        [HttpPut("CreateWALService")]
        public async Task<IActionResult> CreateWALService([FromQuery] string sfServiceName, [FromQuery] string partitionId, [FromQuery] string endpoint)
        {
            // generate number between 1000 and 9999
            int port = new Random().Next(1000, 9999);
            string address = $"localhost {port}";
            Task.Run(() => runExe(@"PackageRoot\Code\GrpcService1.exe", address));
            IReliableDictionary<string, string> endPointDictionary = await this.stateManager.GetOrAddAsync<IReliableDictionary<string, string>>("endpoints");

           /*try {
                List<string> endpoints = await findServiceEndpoint(sfServiceName, partitionId);
            } catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }*/
            

            using (ITransaction tx = this.stateManager.CreateTransaction())
			{
                var key1 = $"{sfServiceName}&{partitionId}&{endpoint}";
                await endPointDictionary.AddOrUpdateAsync(tx, key1, $"http://localhost:{port}", (key, oldvalue) => $"http://localhost:{port}");
                //await endPointDictionary.AddOrUpdateAsync(tx, endpoint, $"http://localhost:{port}", (key, oldvalue) => oldvalue + 1);
				await tx.CommitAsync();
                this.serviceInfo.WALServiceEndpoint = $"http://localhost:{port}";
                this.serviceInfo.MyServiceEndpoint = endpoint;
                this.serviceInfo.GuestServiceName = sfServiceName;
                this.serviceInfo.PartitionId = partitionId;
			}
            return new OkResult();
        }

        // DELETE api/WSData/name
        [HttpDelete("{name}")]
		public async Task<IActionResult> Delete(string name)
		{
			IReliableDictionary<string, string> votesDictionary = await this.stateManager.GetOrAddAsync<IReliableDictionary<string, string>>("endpoints");

			using (ITransaction tx = this.stateManager.CreateTransaction())
			{
				if (await votesDictionary.ContainsKeyAsync(tx, name))
				{
					await votesDictionary.TryRemoveAsync(tx, name);
					await tx.CommitAsync();
					return new OkResult();
				}
				else
				{
					return new NotFoundResult();
				}
			}
		}
	}
}