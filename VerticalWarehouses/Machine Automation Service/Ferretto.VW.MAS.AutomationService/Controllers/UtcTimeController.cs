﻿using System;
using Ferretto.VW.MAS.TimeManagement;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UtcTimeController : ControllerBase
    {
        #region Fields

        private readonly ISystemTimeProvider systemTimeProvider;

        #endregion

        #region Constructors

        public UtcTimeController(ISystemTimeProvider systemTimeProvider)
        {
            this.systemTimeProvider = systemTimeProvider ?? throw new ArgumentNullException(nameof(systemTimeProvider));
        }

        #endregion

        #region Methods

        [HttpGet("wms-auto-sync/can-enable")]
        public ActionResult<bool> CanEnableWmsAutoSyncMode()
        {
            return this.Ok(this.systemTimeProvider.CanEnableWmsAutoSyncMode);
        }

        [HttpGet]
        public ActionResult<DateTimeOffset> Get()
        {
            return this.Ok(DateTimeOffset.Now);
        }

        [HttpGet("wms-auto-sync/enabled")]
        public ActionResult<bool> IsWmsAutoSyncEnabled()
        {
            return this.Ok(this.systemTimeProvider.IsWmsAutoSyncEnabled);
        }

        [HttpPost]
        public IActionResult Set(DateTimeOffset dateTime)
        {
            this.systemTimeProvider.SetUtcSystemTime(dateTime);
            return this.Ok();
        }

        [HttpPut("wms-auto-sync/enabled")]
        public IActionResult SetWmsAutoSync(bool isEnabled)
        {
            this.systemTimeProvider.IsWmsAutoSyncEnabled = isEnabled;
            return this.Ok();
        }

        #endregion
    }
}
