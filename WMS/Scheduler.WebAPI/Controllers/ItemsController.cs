using Microsoft.AspNetCore.Mvc;

namespace Ferretto.WMS.Scheduler.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemsController : ControllerBase
    {
        #region Methods

        // GET api/items
        [HttpGet]
        public ActionResult Get(int id, int quantity)
        {
            return this.Ok();
        }

        // POST api/items/withdraw
        [HttpPost]
        public ActionResult Withdraw(int id, int quantity)
        {
            return this.Ok($"Item {id} withdrawn with quantity {quantity}");
        }

        #endregion Methods
    }
}
