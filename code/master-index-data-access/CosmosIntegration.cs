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

        private static string Enitiy = "PERSON";
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

        public async Task<DataStoreIntegrationResponse<string>> AddIdRelation(string id, string system, string systemId, string createdBy, string provider)
        {

            var synteticMasterId = "";
            if(provider!=null)
            {
                ItemResponse<MasterIndexRelation> relation  =
    await _idRelationContainer.ReadItemAsync<MasterIndexRelation>(id, new PartitionKey($"{provider.ToUpper()}.{Enitiy}.{id}"));
                synteticMasterId = relation.Resource.SyntheticMasterId;
            }
            else
            {
                synteticMasterId = id; 
            }


            ItemResponse<Master> response = await _masterIndexContainer.ReadItemAsync<Master>(synteticMasterId, new PartitionKey($"{Enitiy}.{synteticMasterId}"));
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
                PartitionKey = $"{system.ToUpper()}.{Enitiy.ToUpper()}.{systemId}"

            };

            var idRelationIndexReq = await _idRelationContainer.CreateItemAsync<MasterIndexRelation>(indexRelation);

            return new DataStoreIntegrationResponse<string> { QueryCost = response.RequestCharge + idRelationIndexReq .RequestCharge};


        }
        public async Task<DataStoreIntegrationResponse<string>> CreateMasterIndex(string masterId, string createdBy)
        {
            var syntheticMasterId = Guid.NewGuid().ToString();
            var masterIndexRelation = CreateMasterIndexRelation(masterId, createdBy, syntheticMasterId);
            var queryCost = 0.0; 
            try
            {
                var masterIndexRelationResponse = await _idRelationContainer.CreateItemAsync<MasterIndexRelation>(masterIndexRelation);
                queryCost += masterIndexRelationResponse.RequestCharge;
                Master masterIndex = CreateMasterIndex(masterId, createdBy, syntheticMasterId);
                masterIndex.MasterIndex.Add(new MasterIndexRecord
                {
                    System = "MASTER",
                    SystemId = masterId
                });
                try
                {
                    var masterIndexResponse = await _masterIndexContainer.CreateItemAsync<Master>(masterIndex);
                    queryCost += masterIndexResponse.RequestCharge;
                    return new DataStoreIntegrationResponse <string> { ResponseObject = masterIndex.Id , QueryCost = queryCost};
                }
                catch (Exception ex)
                {
                    //rollback
                    var response = await _idRelationContainer.DeleteItemAsync<MasterIndexRelation>(masterId, new PartitionKey(masterIndexRelation.PartitionKey));
                    throw new Exception($"Could not create master index for {masterId}", ex);
                }
            }
            catch (CosmosException ex)
            {
                if(ex.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    throw new ArgumentException($"MasterIndex for {masterId} exsits");
                }
                else
                {
                    throw ex;
                }
            }

        }

        private static Master CreateMasterIndex(string masterId, string createdBy, string syntheticMasterId)
        {
            return new Master
            {
                CreatedAt = DateTime.Now,
                CreatedBy = createdBy,
                Enitiy = Enitiy,
                MasterId = masterId,
                Id = syntheticMasterId,
                PartitionKey = $"{Enitiy}.{syntheticMasterId}",
                MasterIndex = new List<MasterIndexRecord>()
            };
        }

        private static MasterIndexRelation CreateMasterIndexRelation(string masterId, string createdBy, string syntheticMasterId)
        {
            return new MasterIndexRelation
            {
                CreatedAt = DateTime.Now,
                CreatedBy = createdBy,
                Id = masterId,
                SyntheticMasterId = syntheticMasterId,
                PartitionKey = $"MASTER.{Enitiy}.{masterId}"
            };
        }

        public async Task<DataStoreIntegrationResponse<string>> GetIdRelation(string synteticMasterId, string system)
        {
            try
            {
                ItemResponse<Master> response = await _masterIndexContainer.ReadItemAsync<Master>(synteticMasterId, new PartitionKey($"{Enitiy}.{synteticMasterId}"));
                return new DataStoreIntegrationResponse<string> { ResponseObject = response.Resource.MasterIndex.Find(x => x.System.Equals(system)).SystemId, QueryCost = response.RequestCharge };

            }catch(CosmosException exp)
            {
                if (exp.StatusCode == System.Net.HttpStatusCode.NotFound)
                    throw new ArgumentException($"Could not find system id for {system} using synteticMasterId {synteticMasterId}");
                throw exp;
            }
        }



        public async Task<DataStoreIntegrationResponse<string>> GetIdRelation(string fromSystemId, string toSystem, string idProvider = null)
        {

            try
            {
                if(idProvider == null)
                {
                    ItemResponse<Master> response = await _masterIndexContainer.ReadItemAsync<Master>(fromSystemId, new PartitionKey($"{Enitiy}.{fromSystemId}"));
                    return new DataStoreIntegrationResponse<string> { ResponseObject = response.Resource.MasterIndex.Find(x => x.System.Equals(toSystem)).SystemId, QueryCost = response.RequestCharge };
                }else
                {
                    ItemResponse<MasterIndexRelation> responseMaster =
               await _idRelationContainer.ReadItemAsync<MasterIndexRelation>(fromSystemId, new PartitionKey($"{idProvider.ToUpper()}.{Enitiy}.{fromSystemId}"));

                    var syntheticMasterId = responseMaster.Resource.SyntheticMasterId;
                    ItemResponse<Master> masterIndex = await _masterIndexContainer.ReadItemAsync<Master>(syntheticMasterId, new PartitionKey($"PERSON.{syntheticMasterId}"));
                    return new DataStoreIntegrationResponse<string> { QueryCost = masterIndex.RequestCharge+responseMaster.RequestCharge, ResponseObject = masterIndex.Resource.MasterIndex.Find(x => x.System.Equals(toSystem)).SystemId };
                }
               

            }
            catch (CosmosException exp)
            {
                if (exp.StatusCode == System.Net.HttpStatusCode.NotFound)
                    throw new ArgumentException($"Could not find system id for {toSystem} using id {fromSystemId}");
                throw exp;
            }

           
        }
    }
}
