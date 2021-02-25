namespace master_index_api
{
    public class AppSettings
    {
        public CosmosDb CosmosDb { get; set; }
    }

    public class CosmosDb
    {
        public string Account { get; set; }

        public string Key { get; set; }

        public string DatabaseName { get; set; }

        public string MasterIndexContainerName { get; set; }

        public string IdRelationContainerName { get; set; }

  

    }
}