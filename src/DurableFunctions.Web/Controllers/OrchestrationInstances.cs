using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace DurableFunctionsBrowser.Controllers
{
    public class OrchestrationInstances : Controller
    {
        public async Task<IActionResult> Instance(string id)
        {
            ViewData["Title"] = $"Instance {id}";
            return View();
        }
    }
}
