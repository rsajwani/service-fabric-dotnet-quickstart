// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace VotingWeb.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Dao;
    using Models;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Fabric;
    using System.Fabric.Query;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using Partition = Models.Partition;

    public class HomeController : Controller
    {
        /*public async Task<IActionResult> Index()
        {
            
            List<NodeModel> nodes = await NodeModel.GetNodesFromServiceFabric();

            return this.View(nodes);
            //return this.View();
        }*/

        public IActionResult Index()
        {
            Cluster cluster = Helper.GetNodesFromServiceFabric().Result;
            return View(cluster);
        }

        [HttpGet("CreatePartition")]
        public IActionResult CreatePartition()
        {
            //Cluster cluster = Helper.GetNodesFromServiceFabric().Result;
            Partition partition = new Partition();
            partition.ApplicationName = "fabric:/Voting";
            partition.ServiceName = "fabric:/Voting/VotingData";
            return View(partition);
        }

        public IActionResult SavePartition()
        {
            bool result = Helper.CreatePartition("fabric:/Voting", "fabric:/Voting/VotingData").Result;
            // redirect to error page
            if (result == true)
                return new RedirectResult("/Home/Index");

            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Error()
        {
            return this.View();
        }
    }
}