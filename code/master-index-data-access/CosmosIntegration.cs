using master_index_data_access.datamodel;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace master_index_data_access
{

    /*
        TODO : Validation 
    */
    public class CosmosMasterIndexIntegration : IDataStoreIntegration
    {

        private static Container _masterIndexContainer;
        private static Container _idRelationContainer;

        public CosmosMasterIndexIntegration(CosmosClient dbClient,
            string databaseName,
            string masterIndexContainerName,
            string idRelationContainerName)
        {
            _masterIndexContainer = dbClient.GetContainer(databaseName, masterIndexContainerName);
            _idRelationContainer = dbClient.GetContainer(databaseName, idRelationContainerName);
        }

        public async Task AddIdRelation(string synteticMasterId, string system, string systemId, string createdBy)
        {
            ItemResponse<Master> response = await _masterIndexContainer.ReadItemAsync<Master>(synteticMasterId, new PartitionKey($"PERSON.{synteticMasterId}"));
            var masterIndex = response.Resource;

            ItemRequestOptions requestOptions = new ItemRequestOptions { IfMatchEtag = response.ETag };

            if (masterIndex.MasterIndex == null)
                masterIndex.MasterIndex = new List<MasterIndexRecord>();
            masterIndex.MasterIndex.Add(new MasterIndexRecord
            {
                System = system.ToUpper(),
                SystemId = systemId
            });

            response = await _masterIndexContainer.UpsertItemAsync(response.Resource, requestOptions: requestOptions);

            var indexRelation = new MasterIndexRelation
            {
                CreatedAt = DateTime.Now,
                CreatedBy = createdBy,
                Id = systemId,
                SyntheticMasterId = response.Resource.Id,
                ParititionKey = $"{system.ToUpper()}.{systemId}"

            };

            var idRelationIndexReq = await _idRelationContainer.CreateItemAsync<MasterIndexRelation>(indexRelation);


        }

        public async Task<string> CreateMasterIndex(string masterId, string createdBy)
        {
            var syntheticMasterId = new Guid().ToString();
            var masterIndex = new Master
            {
                CreatedAt = DateTime.Now,
                CreatedBy = createdBy,
                Enitiy = "PERSON",
                MasterId = masterId,
                Id = syntheticMasterId,
                ParititionKey = $"PERSON.{syntheticMasterId}"
            };

            try
            {
                var masterIndexReq = await _masterIndexContainer.CreateItemAsync<Master>(masterIndex);

                if (masterIndexReq.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var indexRelation = new MasterIndexRelation
                    {
                        CreatedAt = DateTime.Now,
                        CreatedBy = createdBy,
                        Id = masterId,
                        SyntheticMasterId = syntheticMasterId,
                        ParititionKey = $"MASTER.{masterId}"

                    };
                    var idRelationIndexReq = await _idRelationContainer.CreateItemAsync<MasterIndexRelation>(indexRelation);
                    if (idRelationIndexReq.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        return masterIndex.Id;
                    }
               
                }

                throw new Exception($"Could not create index due to status code {masterIndexReq.StatusCode}");

            }
            catch (CosmosException ex)
            {

                throw ex;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async Task<string> GetIdRelation(string synteticMasterId, string system)
        {
            ItemResponse<Master> response = await _masterIndexContainer.ReadItemAsync<Master>(synteticMasterId, new PartitionKey($"PERSON.{synteticMasterId}"));
            return response.Resource.MasterIndex.Find(x=>x.System.Equals(system)).SystemId;
        }

        public async Task<string> GetIdRelation(string fromSystem, string fromSystemId, string toSystem)
        {
            ItemResponse<MasterIndexRelation> response =
                await _idRelationContainer.ReadItemAsync<MasterIndexRelation>(fromSystemId, new PartitionKey($"{fromSystem.ToUpper()}.{fromSystem}"));

            var syntheticMasterId = response.Resource.SyntheticMasterId;
            ItemResponse<Master> masterIndex = await _masterIndexContainer.ReadItemAsync<Master>(syntheticMasterId, new PartitionKey($"PERSON.{syntheticMasterId}"));
            return masterIndex.Resource.MasterIndex.Find(x => x.System.Equals(toSystem)).SystemId;
        }
    }
}
