﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Services
{
    public interface IMachineService
    {
        #region Events

        event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Properties

        Bay Bay { get; }

        bool BayFirstPositionIsUpper { get; }

        BayNumber BayNumber { get; }

        IEnumerable<Bay> Bays { get; }

        IEnumerable<Cell> Cells { get; }

        List<CellPlus> CellsPlus { get; }

        double FreeSpaceBack { get; }

        double FreeSpaceFront { get; }

        bool HasBayExternal { get; }

        bool HasBayWithInverter { get; set; }

        bool HasCarousel { get; }

        bool HasShutter { get; }

        bool IsAxisTuningCompleted { get; }

        bool IsHoming { get; }

        bool IsMissionInError { get; }

        bool IsMissionInErrorByLoadUnitOperations { get; }

        bool IsShutterThreeSensors { get; }

        bool IsTuningCompleted { get; }

        IEnumerable<LoadingUnit> Loadunits { get; }

        MachineMode MachineMode { get; }

        MachinePowerState MachinePower { get; }

        Models.MachineStatus MachineStatus { get; }

        IEnumerable<Cell> OriginalCells { get; }
        SetupStatusCapabilities SetupStatus { get; set; }
        IEnumerable<Mission> Missions { get; }

        #endregion

        #region Methods

        void ClearNotifications();

        LoadingUnitLocation GetBayPositionSourceByDestination(bool isPositionDownSelected);

        Task GetCells();

        Task GetLoadUnits(bool details = false);

        Task GetTuningStatus();

        Task OnUpdateServiceAsync();

        Task ShutdownAsync();

        Task StartAsync();

        Task StopMovingByAllAsync();

        Task UpdateLoadUnitInBayAsync();

        #endregion
    }
}
