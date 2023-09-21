// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace VotingWeb.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.Collections.Generic;
    using System.Fabric;
    using System.Fabric.Query;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Xml.Linq;

    public class HomeController : Controller
    {
        public async Task<IActionResult> Index()
        {
            
            List<NodeModel> nodes = await NodeModel.GetNodesFromServiceFabric();

            return this.View(nodes);
            //return this.View();
        }

        public IActionResult Error()
        {
            return this.View();
        }
    }
}