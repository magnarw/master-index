using System;
using master_index_data_access.datamodel;
using System.Threading.Tasks;

namespace master_index_data_access
{
    public interface IDataStoreIntegration
    {
        Task<string> CreateMasterIndex(String masterId, String createdBy);
        Task AddIdRelation(String synteticMasterId, String system, String systemId, String createdBy);
        Task<String> GetIdRelation(String synteticMasterId, String system);
        Task<String> GetIdRelation(String fromSystem,String fromSystemId, String toSystem);
    }
    
}
