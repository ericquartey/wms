﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.MAS.AutomationService.Models;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IdentityController : ControllerBase
    {
        #region Fields

        private readonly ILoadingUnitsDataProvider loadingUnitStatisticsProvider;

        private readonly IMachineProvider machineProvider;

        private readonly IMachineVolatileDataProvider machineVolatileDataProvider;

        private readonly IRotationClassScheduleProvider rotationClassScheduleProvider;

        private readonly IServicingProvider servicingProvider;

        private readonly IWmsSettingsProvider wmsSettingsProvider;

        #endregion

        #region Constructors

        public IdentityController(
            ILoadingUnitsDataProvider loadingUnitStatisticsProvider,
            IServicingProvider servicingProvider,
            IRotationClassScheduleProvider rotationClassScheduleProvider,
            IMachineProvider machineProvider,
            IMachineVolatileDataProvider machineVolatileDataProvider,
            IWmsSettingsProvider wmsSettingsProvider)
        {
            this.loadingUnitStatisticsProvider = loadingUnitStatisticsProvider ?? throw new System.ArgumentNullException(nameof(loadingUnitStatisticsProvider));
            this.servicingProvider = servicingProvider ?? throw new System.ArgumentNullException(nameof(servicingProvider));
            this.machineProvider = machineProvider ?? throw new System.ArgumentNullException(nameof(machineProvider));
            this.machineVolatileDataProvider = machineVolatileDataProvider ?? throw new System.ArgumentNullException(nameof(machineVolatileDataProvider));
            this.wmsSettingsProvider = wmsSettingsProvider ?? throw new System.ArgumentNullException(nameof(wmsSettingsProvider));
            this.rotationClassScheduleProvider = rotationClassScheduleProvider ?? throw new System.ArgumentNullException(nameof(rotationClassScheduleProvider));
        }

        #endregion

        #region Methods

        [HttpPost("add-or-modify-RotationClassSchedule")]
        public async Task<IActionResult> AddOrModifyRotationClassSchedule(RotationClassSchedule newRotationClassSchedule)
        {
            this.rotationClassScheduleProvider.AddOrModifyRotationClassSchedule(newRotationClassSchedule);
            return this.Ok();
        }

        [HttpGet]
        public async Task<ActionResult<MachineIdentity>> Get([FromServices] IMachinesWmsWebService machinesWebService)
        {
            if (machinesWebService is null)
            {
                throw new System.ArgumentNullException(nameof(machinesWebService));
            }

            var servicingInfo = this.servicingProvider.GetLastValid();
            var installationInfo = this.servicingProvider.GetInstallationInfo();
            if (servicingInfo is null)
            {
                servicingInfo = installationInfo;
            }
            else
            {
                servicingInfo.InstallationDate = installationInfo.InstallationDate;
            }

            var loadingUnits = this.loadingUnitStatisticsProvider.GetWeightStatistics();

            var machine = this.machineProvider.GetMinMaxHeight();

            int? areaId = null;
            if (this.wmsSettingsProvider.IsEnabled && this.wmsSettingsProvider.IsConnected)
            {
                try
                {
                    var area = await machinesWebService.GetAreaByIdAsync(machine.Id);
                    areaId = area.Id;
                }
                catch
                {
                    this.NotFound();
                }
            }

            var machineInfo = new MachineIdentity
            {
                AreaId = areaId,
                Id = machine.Id,
                ModelName = machine.ModelName,
                SerialNumber = machine.SerialNumber,
                TrayCount = loadingUnits.Count(),
                MaxGrossWeight = machine.MaxGrossWeight,
                InstallationDate = servicingInfo?.InstallationDate,
                NextServiceDate = servicingInfo?.NextServiceDate,
                LastServiceDate = servicingInfo?.LastServiceDate,
                IsOneTonMachine = this.machineVolatileDataProvider.IsOneTonMachine.Value,
                LoadingUnitDepth = machine.LoadUnitDepth,
                LoadingUnitWidth = machine.LoadUnitWidth,
            };

            return this.Ok(machineInfo);
        }

        [HttpPost("get/Aggregate/List")]
        public ActionResult<bool> GetAggregateList()
        {
            return this.Ok(this.machineProvider.GetAggregateList());
        }

        [HttpGet("get-all-RotationClassSchedule")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public ActionResult<IEnumerable<RotationClassSchedule>> GetAllRotationClassSchedule()
        {
            var RotationClassSchedules = this.rotationClassScheduleProvider.GetAllRotationClassSchedule();

            return this.Ok(RotationClassSchedules);
        }

        [HttpPost("get/box/enable")]
        public ActionResult<bool> GetBoxEnable()
        {
            return this.Ok(this.machineProvider.GetBox());
        }

        [HttpPost("get/can/user/enable/wms")]
        public ActionResult<bool> GetCanUserEnableWms()
        {
            return this.Ok(this.machineProvider.IsCanUserEnableWmsEnabled());
        }

        [HttpPost("get/firealarm/enable")]
        public ActionResult<bool> GetFireAlarmEnable()
        {
            return this.Ok(this.machineProvider.IsFireAlarmActive());
        }

        [HttpPost("get/HeightAlarm/enable")]
        public ActionResult<bool> GetHeightAlarmEnable()
        {
            return this.Ok(this.machineProvider.IsHeightAlarmActive());
        }

        [HttpPost("get/InverterResponseTimeout")]
        public ActionResult<int> GetInverterResponseTimeout()
        {
            return this.Ok(this.machineProvider.GetInverterResponseTimeout());
        }

        [HttpPost("get/IsLoadUnitFixed")]
        public ActionResult<bool> GetIsLoadUnitFixed()
        {
            return this.Ok(this.machineProvider.GetIsLoadUnitFixed());
        }

        [HttpPost("get/IsOstec/enable")]
        public ActionResult<bool> GetIsOstecEnable()
        {
            return this.Ok(this.machineProvider.IsOstecActive());
        }

        [HttpPost("get/is/rotation/class")]
        public ActionResult<bool> GetIsRotationClass()
        {
            return this.Ok(this.machineProvider.IsRotationClassEnabled());
        }

        [HttpPost("get/IsSpea/enable")]
        public ActionResult<bool> GetIsSpeaEnable()
        {
            return this.Ok(this.machineProvider.IsSpeaActive());
        }

        [HttpGet("get/MissionOperationSkipable")]
        public ActionResult<bool> GetMissionOperationSkipable()
        {
            return this.Ok(this.machineProvider.GetMissionOperationSkipable());
        }

        [HttpPost("get/ItemUniqueIdLength")]
        public ActionResult<int> GetItemUniqueIdLength()
        {
            return this.Ok(this.machineProvider.GetItemUniqueIdLength());
        }

        [HttpPost("get/List/Pick/Confirm")]
        public ActionResult<bool> GetListPickConfirm()
        {
            return this.Ok(this.machineProvider.GetListPickConfirm());
        }

        [HttpPost("get/List/Put/Confirm")]
        public ActionResult<bool> GetListPutConfirm()
        {
            return this.Ok(this.machineProvider.GetListPutConfirm());
        }

        [HttpPost("get/ResponseTimeoutMilliseconds")]
        public ActionResult<int> GetResponseTimeoutMilliseconds()
        {
            return this.Ok(this.machineProvider.GetResponseTimeoutMilliseconds());
        }

        [HttpPost("get/SensitiveCarpetsAlarm/enable")]
        public ActionResult<bool> GetSensitiveCarpetsAlarmEnable()
        {
            return this.Ok(this.machineProvider.IsSensitiveCarpetsBypass());
        }

        [HttpPost("get/SensitiveEdgeAlarm/enable")]
        public ActionResult<bool> GetSensitiveEdgeAlarmEnable()
        {
            return this.Ok(this.machineProvider.IsSensitiveEdgeBypass());
        }

        [HttpGet("statistics")]
        public async Task<ActionResult<MachineStatistics>> GetStatistics([FromServices] IMachinesWmsWebService machinesWebService)
        {
            if (machinesWebService is null)
            {
                throw new System.ArgumentNullException(nameof(machinesWebService));
            }

            var statistics = this.machineProvider.GetPresentStatistics();

            // TODO - Implement AreaFillRate in WMS
            //if (this.wmsSettingsProvider.IsEnabled)
            //{
            //    try
            //    {
            //        var machine = this.machineProvider.Get();

            // var wmsMachine = await machinesWebService.GetByIdAsync(machine.Id);

            //        statistics.AreaFillPercentage = wmsMachine.AreaFillRate;
            //    }
            //    catch (System.Exception)
            //    {
            //        // do nothing:
            //        // if the call fails, data from WMS will not be populated
            //    }
            //}

            return this.Ok(statistics);
        }

        [HttpPost("get/ToteBarcodeLength")]
        public ActionResult<int> GetToteBarcodeLength()
        {
            return this.Ok(this.machineProvider.GetToteBarcodeLength());
        }

        [HttpPost("get/touch/helper/enable")]
        public ActionResult<bool> GetTouchHelperEnable()
        {
            return this.Ok(this.machineProvider.IsTouchHelperEnabled());
        }

        [HttpPost("get/VerticalPositionToCalibrate")]
        public ActionResult<int> GetVerticalPositionToCalibrate()
        {
            return this.Ok(this.machineProvider.GetVerticalPositionToCalibrate());
        }

        [HttpPost("get/Waiting/List/Priority/Highlighted")]
        public ActionResult<int?> GetWaitingListPriorityHighlighted()
        {
            return this.Ok(this.machineProvider.GetWaitingListPriorityHighlighted());
        }

        [HttpPost("get/IsDisableQtyItemEditingPick")]
        public ActionResult<bool> IsDisableQtyItemEditingPick()
        {
            return this.Ok(this.machineProvider.IsDisableQtyItemEditingPick());
        }

        [HttpPost("get/IsEnableAddItem")]
        public ActionResult<bool> IsEnableAddItem()
        {
            return this.Ok(this.machineProvider.IsEnableAddItem());
        }

        [HttpPost("get/IsEnableAddItemDrapery")]
        public ActionResult<bool> IsEnableAddItemDrapery()
        {
            return this.Ok(this.machineProvider.IsEnableAddItemDrapery());
        }

        [HttpPost("get/IsEnableHandlingItemOperations")]
        public ActionResult<bool> IsEnableHandlingItemOperations()
        {
            return this.Ok(this.machineProvider.IsEnableHandlingItemOperations());
        }

        [HttpPost("get/IsRequestConfirmForLastOperationOnLoadingUnit")]
        public ActionResult<bool> IsRequestConfirmForLastOperationOnLoadingUnit()
        {
            return this.Ok(this.machineProvider.IsRequestConfirmForLastOperationOnLoadingUnit());
        }

        [HttpPost("get/SilenceSirenAlarm")]
        public ActionResult<bool> IsSilenceSirenAlarm()
        {
            return this.Ok(this.machineProvider.IsSilenceSirenAlarm());
        }

        [HttpPost("get/IsUpdatingStockByDifference")]
        public ActionResult<bool> IsUpdatingStockByDifference()
        {
            return this.Ok(this.machineProvider.IsUpdatingStockByDifference());
        }

        [HttpPost("set/bay/operation/params")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult SetBayOperationParams(DataModels.Machine machine)
        {
            this.machineProvider.SetBayOperationParams(machine);
            return this.Accepted();
        }

        [HttpPost("set/HeightAlarm")]
        public async Task<IActionResult> SetHeightAlarmAsync(bool value)
        {
            await this.machineProvider.SetHeightAlarm(value);
            return this.Ok();
        }

        [HttpPost("set/InverterResponseTimeout")]
        public ActionResult<IActionResult> SetInverterResponseTimeout(int value)
        {
            this.machineProvider.SetInverterResponseTimeout(value);
            return this.Ok();
        }

        [HttpPost("set/machine/id")]
        public async Task<IActionResult> SetMachineIdAsync(int newMachineId)
        {
            await this.machineProvider.SetMachineId(newMachineId);
            return this.Ok();
        }

        [HttpPost("set/ResponseTimeoutMilliseconds")]
        public ActionResult<IActionResult> SetResponseTimeoutMilliseconds(int value)
        {
            this.machineProvider.SetResponseTimeoutMilliseconds(value);
            return this.Ok();
        }

        [HttpPost("set/SensitiveCarpetsBypass")]
        public async Task<IActionResult> SetSensitiveCarpetsBypassAsync(bool value)
        {
            await this.machineProvider.SetSensitiveCarpetsBypass(value);
            return this.Ok();
        }

        [HttpPost("set/SensitiveEdgeBypass")]
        public async Task<IActionResult> SetSensitiveEdgeBypassAsync(bool value)
        {
            await this.machineProvider.SetSensitiveEdgeBypass(value);
            return this.Ok();
        }

        [HttpPost("set/SilenceSirenAlarm")]
        public async Task<IActionResult> SilenceSirenAlarmAsync()
        {
            await this.machineProvider.SetSilenceSirenAlarm(true);
            return this.Ok();
        }

        #endregion
    }
}
