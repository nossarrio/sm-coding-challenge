using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using sm_coding_challenge.Models;
using sm_coding_challenge.Services.DataProvider;

namespace sm_coding_challenge.Controllers
{
    public class HomeController : Controller
    {
        string genericError = "Oops!. something went wrong.our engineers are working to fix it.";
        private IDataProvider _dataProvider;

        public HomeController(IDataProvider dataProvider)
        {
            _dataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Player(string id)
        {
            try
            {
                var player = await _dataProvider.GetPlayerById(id);
                if (player == null)
                {
                    return Ok("Player not found!");
                }
                else
                    return Json(await _dataProvider.GetPlayerById(id));
            }
            catch (Exception err) { }

            return Ok(genericError);
        }

        [HttpGet]
        public async Task<IActionResult> Players(string ids)
        {             
            try
            {
                var idList = ids.Split(',');
                List<Task<PlayerModel>> List = new List<Task<PlayerModel>>();

                foreach (var id in idList)
                {
                    List.Add(_dataProvider.GetPlayerById(id));
                }
                var GetPlayerResult = await Task.WhenAll(List);

                //remove duplicate returned Ids
                var UniquePlayerRecords = GetPlayerResult.GroupBy(id => id)
                   .Select(id => id.First())
                   .ToList();

                //remove null items returned
                UniquePlayerRecords.RemoveAll(item => item == null);

                //return friendly response if players with specified Ids were not found
                if (UniquePlayerRecords.Count == 0)
                {
                    return Ok("Players not found!");
                }
                else
                    return Json(UniquePlayerRecords);
            }
            catch (Exception err) { }

            return Ok(genericError);
        }

        [HttpGet]
        public async Task<IActionResult> LatestPlayers(string ids)
        { 
            try
            {
                var player = await _dataProvider.GetLasterPlayerById(ids);
                if (player == null)
                {
                    return Ok("Player not found!");
                }
                else
                    return Json(player);
            }
            catch (Exception err) { }

            return Ok(genericError);
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
