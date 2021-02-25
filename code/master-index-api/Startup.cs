using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using master_index_data_access;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace master_index_api
{
    public class Startup
    {


        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }


        public IConfiguration Configuration { get; }
        public static string databaseName { get; set; }
        public static string masterIndexContainerName { get; set; }
        public static string idRelationContainerName { get; set; }
        public static string account { get; set; }
        public static string key { get; set; }




        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "master_index_api", Version = "v1" });
            });

            var settings = Configuration.GetSection("AppSettings").Get<AppSettings>();


            services.AddSingleton<IDataStoreIntegration>(InitializeCosmosClientInstanceAsync(settings).GetAwaiter().GetResult());
        }


        private static async Task<CosmosMasterIndexIntegration> InitializeCosmosClientInstanceAsync(AppSettings settings)
        {

            databaseName = settings.CosmosDb.DatabaseName;
            masterIndexContainerName = settings.CosmosDb.MasterIndexContainerName;
            idRelationContainerName = settings.CosmosDb.IdRelationContainerName;
            
            account = settings.CosmosDb.Account;
            key = settings.CosmosDb.Key;


            CosmosClient client = new CosmosClient(account, key);

            CosmosMasterIndexIntegration cosmosDBService = new CosmosMasterIndexIntegration(client, databaseName, masterIndexContainerName, idRelationContainerName);

          
            Database database =  client.GetDatabase(databaseName);

            await database.CreateContainerIfNotExistsAsync(masterIndexContainerName, "/partitionKey");

            await database.CreateContainerIfNotExistsAsync(idRelationContainerName, "/partitionKey");

            return cosmosDBService;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "master_index_api v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
