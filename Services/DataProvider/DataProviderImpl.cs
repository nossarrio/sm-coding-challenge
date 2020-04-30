using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using EchelonWebAPI.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using sm_coding_challenge.Models;

namespace sm_coding_challenge.Services.DataProvider
{
    public class DataProviderImpl : IDataProvider
    {
        public static TimeSpan Timeout = TimeSpan.FromSeconds(30);
        private static int cacheTimeOutInMins = 10080;//refresh every 10080 hours (1 week)
        private readonly IMemoryCache cache;
        private IConfiguration Configuration;
       
        
        //Create an in Memory Cache to store player information which will last in memory for a week since the data set is not updated very frequently (once a week)
        public DataProviderImpl(IMemoryCache cache, IConfiguration _configuration)
        {
            this.cache = cache;
            Configuration = _configuration;
        }


        // Fetches the data and saves it in a cache. subsequent fetches within one week period are pulled from the cache.
        private async Task<DataResponseModel> GetPlayerData()
        {
            DataResponseModel dataResponseModel = new DataResponseModel();
            dataResponseModel = cache.Get<DataResponseModel>(CacheEnum.PLAYERS.ToString());

            if (dataResponseModel == null)
            {
                var handler = new HttpClientHandler()
                {
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                };

                using (var client = new HttpClient(handler))
                {
                    client.Timeout = Timeout; 
                    var response = client.GetAsync(Configuration.GetSection("AppSettings")["ProviderUrl"]).Result;
                    var stringData = response.Content.ReadAsStringAsync().Result;
                    dataResponseModel = JsonConvert.DeserializeObject<DataResponseModel>(stringData, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });

                    if (dataResponseModel != null)
                    {
                        cache.Set<DataResponseModel>(CacheEnum.PLAYERS.ToString(), dataResponseModel, DateTime.Now.AddMinutes(cacheTimeOutInMins));
                        return dataResponseModel;
                    }
                }
            }

            return dataResponseModel;

        }


        //Search for player by id in all categories i.e. Rushing,Passing,Receiving and Kicking
        public async Task<PlayerModel> GetPlayerById(string id)
        {
            DataResponseModel dataResponse = new DataResponseModel();
            dataResponse = await GetPlayerData();

            // Usimg Linq
            var player = dataResponse.Rushing.SingleOrDefault(x => x.Id == id);
            if (player != null)
                return player;

            player = dataResponse.Passing.SingleOrDefault(x => x.Id == id);
            if (player != null)
                return player;

            player = dataResponse.Receiving.SingleOrDefault(x => x.Id == id);
            if (player != null)
                return player;

            player = dataResponse.Kicking.SingleOrDefault(x => x.Id == id);
            if (player != null)
                return player;

            return null;
        }
        
     
        public async Task<LatestPlayerDataModel> GetLasterPlayerById(string id)
        {
            DataResponseModel dataResponse = await GetPlayerData();
            LatestPlayerDataModel latestPlayerDataModel = new LatestPlayerDataModel();
           
            var playerReceiving = dataResponse.Receiving.SingleOrDefault(x => x.Id == id);
            if (playerReceiving != null)
            {
                latestPlayerDataModel.Receiving = new Receiving[1];
                latestPlayerDataModel.Receiving[0] = new Receiving();
                latestPlayerDataModel.Receiving[0].Player =  playerReceiving;
            }

            var playerRushing = dataResponse.Rushing.SingleOrDefault(x => x.Id == id);
            if (playerRushing != null)
            {
                latestPlayerDataModel.Rushing = new Rushing[1];
                latestPlayerDataModel.Rushing[0] = new Rushing();
                latestPlayerDataModel.Rushing[0].Player = playerRushing;
            }

            //var playerKicking = dataResponse.Kicking.SingleOrDefault(x => x.Id == id);
            //if (playerKicking != null)
            //{
            //    latestPlayerDataModel.Kicking = new Kicking[1];
            //    latestPlayerDataModel.Kicking[0] = new Kicking();
            //    latestPlayerDataModel.Kicking[0].Player = playerKicking;
            //}

            //var playerPassing = dataResponse.Passing.SingleOrDefault(x => x.Id == id);
            //if (playerPassing != null)
            //{
            //    latestPlayerDataModel.Passing = new Rushing[1];
            //    latestPlayerDataModel.Passing[0] = new Rushing();
            //    latestPlayerDataModel.Passing[0].Player = playerPassing;
            //}
            return latestPlayerDataModel;
        }

    }
}
