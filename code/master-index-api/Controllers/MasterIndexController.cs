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

        /// <summary>
        /// Creates a new master index
        /// </summary>
        /// <remarks>
        /// This service will allocate a new master index and insure uniqueness within the scope of the provided entity.
        /// </remarks>
        /// <param name="entity"> The master index ids are unique per entity. A "entity" is business objects as 'customer' or 'employee'   </param>
        /// <param name="masterId"> The unique business identifier for a given  entity.  
        /// This is probably something that should have minumum exposure within the organization.  </param>  
        /// <response code="201"></response>
        /// <response code="400"></response>  
        [Route("enitiy/{entity}/{masterId}")]
        [HttpPost]
        public async Task<IActionResult> CreateMasterIndex(string entity,string masterId)
        {
            try
            {
                return await Task.FromResult(StatusCode((int)HttpStatusCode.Created, await _dataStoreIntegration.CreateMasterIndex(masterId, "magnarw")));
            }
            catch(ArgumentException e)
            {
                return await Task.FromResult(StatusCode((int)HttpStatusCode.Conflict, e.Message));
            }
        }


        /// <summary>
        /// Creates a new id relation associated with the master id 
        /// </summary>
        /// <remarks>
        /// Default id provider is synthetic master id for the provided entity.
        /// This default behaviour can be overridden by setting an other id provder.    
        /// </remarks>
        /// <param name="entity"> The master index ids are unique per entity. A "entity" is business objects as 'customer' or 'employee'   </param>
        /// <param name="id"> Identifter in master index</param>
        /// <param name="system"> The name of the system you want to associate. This could be values such as "payroll" or "accouting" </param>
        /// <param name="systemId">The entity's identifter in the provided system</param>
        /// <param name="idProvider">Provider to use when finding correct entry in master index. 
        /// The default is synthetic master id, but any known id relation id can be used. 
        /// </param>
        /// <response code="201"></response>
        /// <response code="400"></response>    
        [Route("enitiy/{entity}/{id}/system/{system}/{systemId}")]
        [HttpPut]
        public async Task<IActionResult> AddIdRelation(string entity, 
            string id,
            string system, 
            string systemId,
            [FromQuery] string idProvider
            )
        {
                var resposne = await _dataStoreIntegration.AddIdRelation(id, system, systemId, "magnarw", idProvider);
                return await Task.FromResult(StatusCode((int)HttpStatusCode.Accepted, resposne));
        }

        /// <summary>
        /// Allow clients to look a system spesific id from master index. 
        /// </summary>
        /// <remarks>
        /// Default id provider is synthetic master id for the provided entity.
        /// This default behaviour can be overridden by setting an other id provder   
        /// </remarks>
        /// <param name="entity"> The master index ids are unique per entity. A "entity" is business objects as 'customer' or 'employee'   </param>
        /// <param name="id"> Identifter in master index</param>
        /// <param name="system"> The name of the system you want to associate. This could be values such as "payroll" or "accouting" </param>
        /// <param name="idProvider">Provider to use when finding correct entry in master index. 
        /// The default is synthetic master id, but any known id relation id can be used. 
        /// </param>
        /// <response code="200"></response>
        /// <response code="400"></response>   
        [Route("enitiy/{entity}/{id}/system/{systemName}")]
        [HttpGet]
        public async Task<IActionResult> GetSystemId(string entity, 
            string id, 
            string systemName,
            [FromQuery] string idProvider
            )
        {
            try
            {
                return await Task.FromResult(StatusCode((int)HttpStatusCode.OK, await _dataStoreIntegration.GetIdRelation(id, systemName, idProvider)));
            }
            catch (ArgumentException e)
            {
                return await Task.FromResult(StatusCode((int)HttpStatusCode.NotFound, e.Message));
            }

        }


    }
}
