using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace DurableFunctionsBrowser.Controllers
{
    public class ApplicationController : Controller
    {
        public async Task<IActionResult> Index()
        {            
            return View();
        }
    }
}
