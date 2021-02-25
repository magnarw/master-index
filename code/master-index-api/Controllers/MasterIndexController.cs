using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using master_index_data_access;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace master_index_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MasterIndexController : ControllerBase
    {

        private IDataStoreIntegration _dataStoreIntegration;

        public MasterIndexController(IDataStoreIntegration dataStoreIntegration)
        {
            _dataStoreIntegration = dataStoreIntegration;
        }

        [Route("enitiy/{enitiy}/{masterId}")]
        [HttpPost]
        public async Task<IActionResult> CreateMasterIndex(string masterId)
        {
            return await Task.FromResult(StatusCode((int)HttpStatusCode.Created, await _dataStoreIntegration.CreateMasterIndex(masterId,"magnarw")));
        }
    }
}
