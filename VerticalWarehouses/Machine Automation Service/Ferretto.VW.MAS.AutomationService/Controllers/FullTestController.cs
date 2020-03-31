﻿using System;
using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.MachineManager;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FullTestController : ControllerBase, IRequestingBayController
    {
        #region Fields

        private readonly IMachineModeProvider machineModeProvider;

        private readonly ISetupProceduresDataProvider setupProceduresDataProvider;

        #endregion

        #region Constructors

        public FullTestController(
            IMachineModeProvider machineModeProvider,
            ISetupProceduresDataProvider setupProceduresDataProvider)
        {
            this.machineModeProvider = machineModeProvider ?? throw new ArgumentNullException(nameof(machineModeProvider));
            this.setupProceduresDataProvider = setupProceduresDataProvider ?? throw new ArgumentNullException(nameof(setupProceduresDataProvider));
        }

        #endregion

        #region Properties

        public BayNumber BayNumber { get; set; }

        #endregion

        //[HttpGet("parameters")]
        //public ActionResult<RepeatedTestProcedure> GetParameters()
        //{
        //    return this.Ok(this.setupProceduresDataProvider.GetFullTest());
        //}

        #region Methods

        [HttpPost("start")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public IActionResult Start(List<int> loadunits, int cycles)
        {
            this.machineModeProvider.RequestChange(CommonUtils.Messages.MachineMode.FullTest, this.BayNumber, loadunits, cycles);
            return this.Accepted();
        }

        [HttpPost("stop")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult Stop()
        {
            this.machineModeProvider.RequestChange(CommonUtils.Messages.MachineMode.Manual);
            return this.Accepted();
        }

        #endregion
    }
}
