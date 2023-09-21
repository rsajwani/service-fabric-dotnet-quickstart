// See https://aka.ms/new-console-template for more information
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;

namespace VotingData.Controllers
{
    public class PSHelper
    {

        /*var processInfo = new System.Diagnostics.ProcessStartInfo
        {
            Verb = "runas /trustlevel:0x20000",
            LoadUserProfile = true,
            FileName = "powershell.exe",
            Arguments = "Start-VM -name \"win11-Lite\"",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };*/
        /*
            var processInfo = new System.Diagnostics.ProcessStartInfo
            {
                Verb = "runas",
                LoadUserProfile = true,
                FileName = "powershell.exe",
                Arguments = "Start-sleep -seconds 10",
                RedirectStandardOutput = false,
                UseShellExecute = true,
                CreateNoWindow = true
            };

            var p = System.Diagnostics.Process.Start(processInfo);
         */

        public static (string, string) getDeviceNamespace(string deviceID)
        {
            // Define the path to your PowerShell script
            //string scriptPath = psFilePath;
            string script = "Get-PhysicalDisk | Where-Object MediaType -eq \"SSD\" | ForEach-Object {\r\n    $physicalDisk = $_\r\n    Write-Host \"Physical Disk: $($physicalDisk.DeviceID)\"\r\n    Write-Host \"Physical Disk: $($physicalDisk)\"\r\n    Get-StorageReliabilityCounter -PhysicalDisk $physicalDisk | ForEach-Object {\r\n        $namespaceInfo = $_\r\n        Write-Host \"  Namespace ID: $($namespaceInfo.NamespaceID)\"\r\n        Write-Host \"  Namespace Size in Bytes: $($namespaceInfo.Size)\"\r\n        Write-Host \"  Namespace Operational Status: $($namespaceInfo.OperationalStatus)\"\r\n        Write-Host \"  Namespace Operational Status Description: $($namespaceInfo.OperationalStatusDescription)\"\r\n        Write-Host \"--------------------------------------------\"\r\n    }\r\n}";
            // Create a new process to run PowerShell
            // run it as admin
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                Arguments = $"-ExecutionPolicy Bypass \"{script}\"",
                // run it as admin
                Verb = "runas",
                LoadUserProfile = true,
                //Arguments = $"Get-PhysicalDisk"
            };

            using (Process process = new Process { StartInfo = psi })
            {
                // Start the PowerShell process
                process.Start();

                // Read and display the output
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                Console.WriteLine("Output:");
                Console.WriteLine(output);

                Console.WriteLine("Error:");
                Console.WriteLine(error);

                return (output, error);

                //process.WaitForExit();
            }

            // this didn't work.
            /*try
            {
                // Define the query to retrieve NVMe namespaces
                string query = "SELECT * FROM MSFT_NvmeNamespace";

                // Create a new ManagementObjectSearcher
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\Microsoft\\Windows\\Storage", query))
                {
                    // Execute the query and get the collection of results
                    ManagementObjectCollection results = searcher.Get();

                    // Iterate through the results
                    foreach (ManagementObject nvmeNamespace in results)
                    {
                        Console.WriteLine("Namespace ID: " + nvmeNamespace["NamespaceID"]);
                        Console.WriteLine("Namespace Size (Bytes): " + nvmeNamespace["Size"]);
                        Console.WriteLine("Namespace Capacity (Bytes): " + nvmeNamespace["Capacity"]);
                        Console.WriteLine("Namespace Formatted Capacity (Bytes): " + nvmeNamespace["FormattedNamespaceCapacity"]);
                        Console.WriteLine("-------------------------------------------------");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }*/

            /*try // this requires higher version of powershell
            {
                // Create a PowerShell runspace and pipeline
                using (Runspace runspace = RunspaceFactory.CreateRunspace())
                {
                    runspace.Open();
                    using (Pipeline pipeline = runspace.CreatePipeline())
                    {
                        // Add the Get-StorageReliabilityCounter cmdlet to the pipeline
                        pipeline.Commands.AddScript("Get-StorageReliabilityCounter");

                        // Execute the pipeline and retrieve the results
                        Collection<PSObject> results = pipeline.Invoke();

                        // Process and display the results
                        foreach (PSObject result in results)
                        {
                            Console.WriteLine(result.ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }*/

            /*string cmdlet = "Get-StorageReliabilityCounter";
            string script = $"powershell -Command {cmdlet}";

            ProcessStartInfo psi = new ProcessStartInfo("pwsh.exe")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                Arguments = script
            };

            using (Process process = new Process() { StartInfo = psi })
            {
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                Console.WriteLine(output);
            }*/

        }
    }

    public class WmiHelper
    {
        public static List<List<KeyValuePair<string, string>>> getAllSSDs()
        {
            List<List<KeyValuePair<string, string>>> listOfLists = new List<List<KeyValuePair<string, string>>>();
            try
            {
                string query = "SELECT FriendlyName, DeviceId, MediaType, BusType, Size, AllocatedSize, HealthStatus, PhysicalSectorSize, LogicalSectorSize FROM MSFT_PhysicalDisk";

                // Create a new ManagementObjectSearcher
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\Microsoft\\Windows\\Storage", query))
                {
                    // Execute the query and get the collection of results
                    ManagementObjectCollection results = searcher.Get();

                    foreach (ManagementObject disk in results)
                    {
                        if (Convert.ToInt32(disk["MediaType"]) == 4)
                        {
                            // Create and populate the inner lists
                            List<KeyValuePair<string, string>> innerList = new List<KeyValuePair<string, string>>();
                            innerList.Add(new KeyValuePair<string, string>("Friendly Name", "" + disk["FriendlyName"]));
                            innerList.Add(new KeyValuePair<string, string>("Device Number", "" + disk["DeviceId"]));
                            innerList.Add(new KeyValuePair<string, string>("Media Type", toMediaTypeString(Convert.ToInt32(disk["MediaType"]))));
                            innerList.Add(new KeyValuePair<string, string>("Bus Type", "" + toBusTypeString(Convert.ToInt32(disk["BusType"]))));
                            innerList.Add(new KeyValuePair<string, string>("Size", "" + disk["Size"]));
                            innerList.Add(new KeyValuePair<string, string>("Allocated Size", "" + disk["AllocatedSize"]));
                            innerList.Add(new KeyValuePair<string, string>("Health Status", "" + disk["HealthStatus"]));
                            innerList.Add(new KeyValuePair<string, string>("Physical Sector Size", "" + disk["PhysicalSectorSize"]));
                            innerList.Add(new KeyValuePair<string, string>("Logical Sector Size", "" + disk["LogicalSectorSize"]));
                            var deviceID = getDeviceID(disk["DeviceId"].ToString(), disk["FriendlyName"].ToString());                            
                            innerList.Add(new KeyValuePair<string, string>("Device ID", deviceID));
                            // call powershell to get namespace
                            var namespaceInfo = PSHelper.getDeviceNamespace(disk["DeviceId"].ToString());
                            innerList.Add(new KeyValuePair<string, string>("Namespaces Meta", namespaceInfo.Item1 + namespaceInfo.Item2));
                            listOfLists.Add(innerList);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                List<KeyValuePair<string, string>> innerList1 = new List<KeyValuePair<string, string>>();
                innerList1.Add(new KeyValuePair<string, string>("error", ex.Message));
            }

            return listOfLists;
            // This method didn't work for SSD. Then I read that win32 does not gurantee to have updated status of SSD
            /*string query = "SELECT * FROM Win32_DiskDrive WHERE MediaType='SSD'";
           // Iterate through the results
           using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
           {
               foreach (ManagementObject disk in results)
               {
                   result.Add(new KeyValuePair<string, string>("Device ID", "" + disk["DeviceID"]));
                   result.Add(new KeyValuePair<string, string>("Model", "" + disk["Model"]));
                   result.Add(new KeyValuePair<string, string>("Size (Bytes)", "" + disk["Size"]));
                   result.Add(new KeyValuePair<string, string>("Media Type", "" + disk["MediaType"]));
                   result.Add(new KeyValuePair<string, string>("Interface Type", "" + disk["InterfaceType"]));
                   result.Add(new KeyValuePair<string, string>("=============", "==========="));
               }
           }*/

            // Then I tried this but it does not gave me desired result.
            //Get-WmiObject -Class MSFT_PhysicalDisk -Namespace root\Microsoft\Windows\Storage
            /*string query = "SELECT * FROM Win32_PhysicalMedia";
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
            {
                foreach (ManagementObject media in results)
                    {
                        result.Add(new KeyValuePair<string, string>("Device ID", "" + media["SerialNumber"]));
                        result.Add(new KeyValuePair<string, string>("Model", "" + media["Model"]));
                        //result.Add(new KeyValuePair<string, string>("Size (Bytes)", "" + media["Size"]));
                        result.Add(new KeyValuePair<string, string>("Media Type", "" + media["MediaType"]));
                        result.Add(new KeyValuePair<string, string>("=============", "==========="));
                    }
            }*/

        }

        public static string getDeviceID(string number, string name)
        {
            try
            {
                var tmplDeviceID = "\\\\.\\PHYSICALDRIVE" + number;
                //string query = "SELECT DeviceID FROM Win32_DiskDrive where Model=" + name +" and DeviceID=" + tmplDeviceID;
                string query = "SELECT DeviceID, Model FROM Win32_DiskDrive";

                // Create a new ManagementObjectSearcher
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
                {
                    // Execute the query and get the collection of results
                    ManagementObjectCollection results = searcher.Get();

                    foreach (ManagementObject disk in results)
                    {
                        if ((disk["DeviceID"].ToString() == tmplDeviceID) && (disk["Model"].ToString() == name))
                        {
                            return disk["DeviceID"].ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return "";
        }

        private static string toMediaTypeString(int mediaType)
        {
            switch (mediaType)
            {
                case 0: return "Unspecified";
                case 3: return "HDD";
                case 4: return "SSD";
                case 5: return "SCM";
                default: return "UNKNOWN";
            }
        }

        private static string toBusTypeString(int mediaType)
        {
            switch (mediaType)
            {
                case 0: return "Unknown";
                case 8: return "RAID";
                case 9: return "iSCSI";
                case 17: return "NVMe";
                default: return "Other Types";
            }
        }
    }
}

