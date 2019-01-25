using System;
using System.Collections.Generic;
using System.Linq;
using Ferretto.WMS.Data.WebAPI.Interfaces;
using Ferretto.WMS.Data.WebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Ferretto.WMS.Data.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MissionsController : ControllerBase,
        IReadAllController<Models.Mission>,
        IReadSingleController<Models.ItemList>
    {
        #region Fields

        private readonly ILogger logger;

        private readonly IWarehouse warehouse;

        #endregion Fields

        #region Constructors

        public MissionsController(
            ILogger<ItemsController> logger,
            Models.IWarehouse warehouse)
        {
            this.logger = logger;
            this.warehouse = warehouse;
        }

        #endregion Constructors

        #region Methods

        [HttpGet]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Mission>))]
        public ActionResult<IEnumerable<Models.Mission>> GetAll()
        {
            return this.Ok(this.warehouse.Missions);
        }

        [ProducesResponseType(200, Type = typeof(Mission))]
        [ProducesResponseType(400, Type = typeof(SerializableError))]
        [ProducesResponseType(404, Type = typeof(SerializableError))]
        [HttpGet("{id}")]
        public ActionResult<Models.ItemList> GetById(int id)
        {
            try
            {
                var result = this.warehouse.Missions.SingleOrDefault(m => m.Id == id);
                if (result == null)
                {
                    var message = $"No entity with the specified id={id} exists.";
                    this.logger.LogWarning(message);
                    return this.NotFound(message);
                }

                return this.Ok(result);
            }
            catch (Exception ex)
            {
                var message = $"An error occurred while retrieving the requested entity with id={id}.";
                this.logger.LogError(ex, message);
                return this.BadRequest(message);
            }
        }

        #endregion Methods
    }
}
