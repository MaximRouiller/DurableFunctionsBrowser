using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DurableFunctionsBrowser.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace DurableFunctionsBrowser.Controllers
{
    public class ApplicationController : Controller
    {
        private readonly AzureStorageRepository repository;

        public ApplicationController(AzureStorageRepository repository)
        {
            this.repository = repository;
        }
        public async Task<IActionResult> Index()
        {
            var instances = await repository.GetAllDurableInstancesAsync();
            return View(instances);
        }
    }
}
