using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers
    {
    [Route("api/[controller]")]
    [ApiController]

    public class GetController : ControllerBase
    {
        // GET: api/<controller>
        [HttpGet]
        public String Get()
        {
            return "get normal";
        }

        // GET: api/<controller>/asd
        [HttpGet("asd")]
        public JsonResult GetValueJson()
        {
            return new JsonResult("sadasd");
        }
    }

}
