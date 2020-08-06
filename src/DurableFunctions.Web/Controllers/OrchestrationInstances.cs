using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DurableFunctionsBrowser.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace DurableFunctionsBrowser.Controllers
{
    public class OrchestrationInstances : Controller
    {
        private AzureStorageRepository repository;

        public OrchestrationInstances(AzureStorageRepository repository)
        {
            this.repository = repository;
        }
        public async Task<IActionResult> Instance(string id)
        {
            var @events = await repository.GetAllEventsForInstanceIdAsync(id);
            ViewData["Title"] = $"Instance {id}";
            return View(@events);
        }
    }
}
