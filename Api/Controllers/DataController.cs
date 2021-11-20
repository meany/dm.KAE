using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dm.KAE.Data;
using dm.KAE.Data.Models;
using dm.KAE.Data.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace dm.KAE.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class DataController : ControllerBase
    {
        private AppDbContext db;

        public DataController(AppDbContext db)
        {
            this.db = db;
        }

        // GET data
        [HttpGet]
        public async Task<ActionResult<AllInfo>> GetAllInfo()
        {
            return await Data.Common.GetAllInfo(db);
        }

        // GET data/circulation
        [HttpGet]
        [Route("circulation")]
        public async Task<ActionResult<decimal>> GetCirculation()
        {
            var item = await Data.Common.GetStats(db);
            return item.Circulation;
        }

        // GET data/supply
        [HttpGet]
        [Route("supply")]
        public async Task<ActionResult<decimal>> GetSupply()
        {
            var item = await Data.Common.GetStats(db);
            return item.Supply;
        }
    }
}
