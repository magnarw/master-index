using System;
using master_index_data_access.datamodel;
using System.Threading.Tasks;

namespace master_index_data_access
{
    public interface IDataStoreIntegration
    {
        Task<DataStoreIntegrationResponse<string>> CreateMasterIndex(String masterId, String createdBy);
        Task<DataStoreIntegrationResponse<String>> AddIdRelation(String id, String system, String systemId, String createdBy, String idprovider = null);
        Task<DataStoreIntegrationResponse<String>> GetIdRelation(String id, String system, String idProvider = null);
    }
    
}
