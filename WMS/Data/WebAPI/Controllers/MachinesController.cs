using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ferretto.Common.Utils.Expressions;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Ferretto.WMS.Data.WebAPI.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Ferretto.WMS.Data.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MachinesController :
        ControllerBase,
        IReadAllPagedController<Machine>,
        IReadSingleController<Machine, int>,
        IGetUniqueValuesController
    {
        #region Fields

        private readonly ILogger logger;

        private readonly IMachineProvider machineProvider;

        #endregion

        #region Constructors

        public MachinesController(
            ILogger<MachinesController> logger,
            IMachineProvider machineProvider)
        {
            this.logger = logger;
            this.machineProvider = machineProvider;
        }

        #endregion

        #region Methods

        [ProducesResponseType(200, Type = typeof(IEnumerable<Machine>))]
        [ProducesResponseType(400, Type = typeof(string))]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Machine>>> GetAllAsync(
            int skip = 0,
            int take = int.MaxValue,
            string where = null,
            string orderBy = null,
            string search = null)
        {
            try
            {
                var searchExpression = BuildSearchExpression(search);
                var whereExpression = where.AsIExpression();

                return this.Ok(
                    await this.machineProvider.GetAllAsync(
                        skip,
                        take,
                        orderBy,
                        whereExpression,
                        searchExpression));
            }
            catch (NotSupportedException e)
            {
                return this.BadRequest(e.Message);
            }
        }

        [ProducesResponseType(200, Type = typeof(int))]
        [ProducesResponseType(400, Type = typeof(string))]
        [ProducesResponseType(404)]
        [HttpGet("count")]
        public async Task<ActionResult<int>> GetAllCountAsync(
            string where = null,
            string search = null)
        {
            try
            {
                var searchExpression = BuildSearchExpression(search);
                var whereExpression = where.AsIExpression();

                return await this.machineProvider.GetAllCountAsync(
                           whereExpression,
                           searchExpression);
            }
            catch (NotSupportedException e)
            {
                return this.BadRequest(e.Message);
            }
        }

        [ProducesResponseType(200, Type = typeof(Machine))]
        [ProducesResponseType(404, Type = typeof(SerializableError))]
        [HttpGet("{id}")]
        public async Task<ActionResult<Machine>> GetByIdAsync(int id)
        {
            var result = await this.machineProvider.GetByIdAsync(id);
            if (result == null)
            {
                var message = $"No entity with the specified id={id} exists.";
                this.logger.LogWarning(message);
                return this.NotFound(message);
            }

            return this.Ok(result);
        }

        [ProducesResponseType(200, Type = typeof(IEnumerable<object>))]
        [HttpGet("unique/{propertyName}")]
        public async Task<ActionResult<object[]>> GetUniqueValuesAsync(
            string propertyName)
        {
            return this.Ok(await this.machineProvider.GetUniqueValuesAsync(propertyName));
        }

        private static Expression<Func<Machine, bool>> BuildSearchExpression(string search)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                return null;
            }

            return (m) =>
                m.AisleName.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                m.AreaName.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                m.MachineTypeDescription.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                m.Model.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                m.Nickname.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                m.RegistrationNumber.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                m.FillRate.ToString().Contains(search, StringComparison.InvariantCultureIgnoreCase);
        }

        #endregion
    }
}
