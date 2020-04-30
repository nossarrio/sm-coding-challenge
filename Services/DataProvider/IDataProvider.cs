using sm_coding_challenge.Models;
using System.Threading.Tasks;

namespace sm_coding_challenge.Services.DataProvider
{
    public interface IDataProvider
    {
        Task<PlayerModel> GetPlayerById(string id);
        Task<LatestPlayerDataModel> GetLasterPlayerById(string id);
    }
}
