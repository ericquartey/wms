using System;
using System.Collections.Generic;
using System.Linq;
using Ferretto.VW.MAS.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.DataLayer
{
    internal sealed class ServicingProvider : IServicingProvider
    {
        #region Fields

        private readonly DataLayerContext dataContext;

        private readonly ILogger<ServicingProvider> logger;

        private readonly IStatisticsDataProvider machineStatistics;

        #endregion

        #region Constructors

        public ServicingProvider(DataLayerContext dataContext,
            ILogger<ServicingProvider> logger,
            IStatisticsDataProvider machineStatistics)
        {
            this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.machineStatistics = machineStatistics ?? throw new ArgumentNullException(nameof(machineStatistics));
        }

        #endregion

        #region Methods

        public void CheckServicingInfo()
        {
            lock (this.dataContext)
            {
                if (this.dataContext.ServicingInfo.LastOrDefault() == null)
                {
                    var s = new ServicingInfo();

                    s.ServiceStatus = MachineServiceStatus.Valid;

                    var machineId = this.dataContext.MachineStatistics.LastOrDefault()?.Id;
                    if (machineId.HasValue)
                    {
                        s.MachineStatisticsId = machineId.Value;
                    }

                    this.dataContext.ServicingInfo.Add(s);
                    this.dataContext.SaveChanges();
                }
            }
        }

        public void ConfirmInstruction(int instructionId)
        {
            lock (this.dataContext)
            {
                try
                {
                    // Confirm setup date in actual record
                    var instruction = this.dataContext.Instructions.LastOrDefault(s => s.Id == instructionId);
                    instruction.InstructionStatus = MachineServiceStatus.Completed;
                    instruction.MaintenanceDate = DateTime.UtcNow;
                    instruction.IsDone = true;
                    if (instruction.Definition.CounterName != null)
                    {
                        instruction.IntCounter = this.DiffCount(instruction);
                    }
                    this.dataContext.Instructions.Update(instruction);

                    // Update record
                    //var s = new ServicingInfo();

                    //s.LastServiceDate = DateTime.Now;
                    //s.NextServiceDate = DateTime.Now.AddDays((double)this.dataContext.Instructions.LastOrDefault(s => s.Id == instructionId).Definition.MaxDays);
                    //s.ServiceStatus = MachineServiceStatus.Valid;
                    this.dataContext.SaveChanges();
                }
                catch (Exception)
                {
                    //do nothing
                }
            }
        }

        public void ConfirmService()
        {
            lock (this.dataContext)
            {
                // Confirm setup date in actual record
                this.dataContext.ServicingInfo.Last().ServiceStatus = MachineServiceStatus.Completed;
                this.dataContext.ServicingInfo.Update(this.dataContext.ServicingInfo.Last());

                // Add new record
                var s = new ServicingInfo();

                s.LastServiceDate = DateTime.Now;
                s.NextServiceDate = DateTime.Now.AddYears(1);
                s.ServiceStatus = MachineServiceStatus.Valid;

                s.MachineStatisticsId = this.machineStatistics.ConfirmAndCreateNew();

                this.dataContext.ServicingInfo.Add(s);
                this.dataContext.SaveChanges();

                this.GenerateInstructions(s);
                this.dataContext.SaveChanges();
            }
        }

        public void ConfirmSetup()
        {
            lock (this.dataContext)
            {
                if (this.dataContext.ServicingInfo.Count() == 1)
                {
                    // Confirm setup date in actual record
                    this.dataContext.ServicingInfo.FirstOrDefault().ServiceStatus = MachineServiceStatus.Completed;
                    this.dataContext.ServicingInfo.FirstOrDefault().InstallationDate = DateTime.Now;
                    this.dataContext.ServicingInfo.Update(this.dataContext.ServicingInfo.FirstOrDefault());

                    // Add new record
                    var s = new ServicingInfo();

                    s.LastServiceDate = DateTime.Now;
                    s.NextServiceDate = DateTime.Now.AddYears(1);
                    s.ServiceStatus = MachineServiceStatus.Valid;

                    s.MachineStatisticsId = this.machineStatistics.ConfirmAndCreateNew();

                    this.dataContext.ServicingInfo.Add(s);
                    this.dataContext.SaveChanges();

                    this.GenerateInstructions(s);
                    this.dataContext.SaveChanges();
                }
            }
        }

        public int DiffCount(Instruction ins)
        {
            try
            {
                var diffCount = 0;
                var allStat = this.dataContext.ServicingInfo
                    .Include(i => i.Instructions)
                    .Include(i => i.MachineStatistics)
                    .ToArray()
                    .OrderBy(o => o.Id);
                if (ins.Definition.CounterName != null
                    && ins.Definition.CounterName.Length > 0
                    && allStat.Any()
                    )
                {
                    var lastStat = allStat.Last().MachineStatistics;
                    var countedStat = allStat.LastOrDefault(s => s.Instructions != null && s.Instructions.Any(i => i.Definition.Id == ins.Definition.Id && i.IntCounter.HasValue && i.IsDone));
                    switch (ins.Definition.CounterName)
                    {
                        case nameof(lastStat.AreaFillPercentage):
                            if (countedStat != null)
                            {
                                diffCount = (int)allStat.Where(a => a.Id >= countedStat.Id).Sum(s => s.MachineStatistics.AreaFillPercentage);
                                //diffCount -= countedStat.Instructions.FirstOrDefault(i => i.Definition.Id == ins.Definition.Id)?.IntCounter.Value ?? 0;
                            }
                            else
                            {
                                diffCount = (int)allStat.Sum(s => s.MachineStatistics.AreaFillPercentage);
                            }
                            break;

                        case nameof(lastStat.AutomaticTimePercentage):
                            if (countedStat != null)
                            {
                                diffCount = (int)allStat.Where(a => a.Id >= countedStat.Id).Sum(s => s.MachineStatistics.AutomaticTimePercentage);
                                //diffCount -= countedStat.Instructions.FirstOrDefault(i => i.Definition.Id == ins.Definition.Id)?.IntCounter.Value ?? 0;
                            }
                            else
                            {
                                diffCount = (int)allStat.Sum(s => s.MachineStatistics.AutomaticTimePercentage);
                            }
                            break;

                        //case nameof(lastStat.TotalAutomaticTime):
                        //    if (countedStat != null)
                        //    {
                        //        diffCount = (int)allStat.Where(a => a.Id >= countedStat.Id).Sum(s => s.MachineStatistics.TotalAutomaticTime);
                        //        diffCount -= countedStat.Instructions.FirstOrDefault(i => i.Definition.Id == ins.Definition.Id)?.IntCounter.Value ?? 0;
                        //    }
                        //    else
                        //    {
                        //        diffCount = (int)allStat.Sum(s => s.MachineStatistics.TotalAutomaticTime);
                        //    }
                        //    break;

                        case nameof(lastStat.TotalBayChainKilometers2):
                            if (countedStat != null)
                            {
                                diffCount = (int)allStat.Where(a => a.Id >= countedStat.Id).Sum(s => s.MachineStatistics.TotalBayChainKilometers2);
                                //diffCount -= countedStat.Instructions.FirstOrDefault(i => i.Definition.Id == ins.Definition.Id)?.IntCounter.Value ?? 0;
                            }
                            else
                            {
                                diffCount = (int)allStat.Sum(s => s.MachineStatistics.TotalBayChainKilometers2);
                            }
                            break;

                        case nameof(lastStat.TotalBayChainKilometers3):
                            if (countedStat != null)
                            {
                                diffCount = (int)allStat.Where(a => a.Id >= countedStat.Id).Sum(s => s.MachineStatistics.TotalBayChainKilometers3);
                                //diffCount -= countedStat.Instructions.FirstOrDefault(i => i.Definition.Id == ins.Definition.Id)?.IntCounter.Value ?? 0;
                            }
                            else
                            {
                                diffCount = (int)allStat.Sum(s => s.MachineStatistics.TotalBayChainKilometers3);
                            }
                            break;

                        case nameof(lastStat.TotalHorizontalAxisCycles):
                            if (countedStat != null)
                            {
                                diffCount = (int)allStat.Where(a => a.Id >= countedStat.Id).Sum(s => s.MachineStatistics.TotalHorizontalAxisCycles);
                                //diffCount -= countedStat.Instructions.FirstOrDefault(i => i.Definition.Id == ins.Definition.Id)?.IntCounter.Value ?? 0;
                            }
                            else
                            {
                                diffCount = (int)allStat.Sum(s => s.MachineStatistics.TotalHorizontalAxisCycles);
                            }
                            break;

                        case nameof(lastStat.TotalHorizontalAxisKilometers):
                            if (countedStat != null)
                            {
                                diffCount = (int)allStat.Where(a => a.Id >= countedStat.Id).Sum(s => s.MachineStatistics.TotalHorizontalAxisKilometers);
                                //diffCount -= countedStat.Instructions.FirstOrDefault(i => i.Definition.Id == ins.Definition.Id)?.IntCounter.Value ?? 0;
                            }
                            else
                            {
                                diffCount = (int)allStat.Sum(s => s.MachineStatistics.TotalHorizontalAxisKilometers);
                            }
                            break;

                        case nameof(lastStat.TotalLoadUnitsInBay1):
                            if (countedStat != null)
                            {
                                diffCount = (int)allStat.Where(a => a.Id >= countedStat.Id).Sum(s => s.MachineStatistics.TotalLoadUnitsInBay1);
                                //diffCount -= countedStat.Instructions.FirstOrDefault(i => i.Definition.Id == ins.Definition.Id)?.IntCounter.Value ?? 0;
                            }
                            else
                            {
                                diffCount = (int)allStat.Sum(s => s.MachineStatistics.TotalLoadUnitsInBay1);
                            }
                            break;

                        case nameof(lastStat.TotalLoadUnitsInBay2):
                            if (countedStat != null)
                            {
                                diffCount = (int)allStat.Where(a => a.Id >= countedStat.Id).Sum(s => s.MachineStatistics.TotalLoadUnitsInBay2);
                                //diffCount -= countedStat.Instructions.FirstOrDefault(i => i.Definition.Id == ins.Definition.Id)?.IntCounter.Value ?? 0;
                            }
                            else
                            {
                                diffCount = (int)allStat.Sum(s => s.MachineStatistics.TotalLoadUnitsInBay2);
                            }
                            break;

                        case nameof(lastStat.TotalLoadUnitsInBay3):
                            if (countedStat != null)
                            {
                                diffCount = (int)allStat.Where(a => a.Id >= countedStat.Id).Sum(s => s.MachineStatistics.TotalLoadUnitsInBay3);
                                //diffCount -= countedStat.Instructions.FirstOrDefault(i => i.Definition.Id == ins.Definition.Id)?.IntCounter.Value ?? 0;
                            }
                            else
                            {
                                diffCount = (int)allStat.Sum(s => s.MachineStatistics.TotalLoadUnitsInBay3);
                            }
                            break;

                        //case nameof(lastStat.TotalMissionTime):
                        //    if (countedStat != null)
                        //    {
                        //        diffCount = (int)allStat.Where(a => a.Id >= countedStat.Id).Sum(s => s.MachineStatistics.TotalMissionTime);
                        //        diffCount -= countedStat.Instructions.FirstOrDefault(i => i.Definition.Id == ins.Definition.Id)?.IntCounter.Value ?? 0;
                        //    }
                        //    else
                        //    {
                        //        diffCount = (int)allStat.Sum(s => s.MachineStatistics.TotalMissionTime);
                        //    }
                        //    break;

                        //case nameof(lastStat.TotalPowerOnTime):
                        //    if (countedStat != null)
                        //    {
                        //        diffCount = (int)allStat.Where(a => a.Id >= countedStat.Id).Sum(s => s.MachineStatistics.TotalPowerOnTime);
                        //        diffCount -= countedStat.Instructions.FirstOrDefault(i => i.Definition.Id == ins.Definition.Id)?.IntCounter.Value ?? 0;
                        //    }
                        //    else
                        //    {
                        //        diffCount = (int)allStat.Sum(s => s.MachineStatistics.TotalPowerOnTime);
                        //    }
                        //    break;

                        case nameof(lastStat.TotalVerticalAxisCycles):
                            if (countedStat != null)
                            {
                                diffCount = allStat.Where(a => a.Id >= countedStat.Id).Sum(s => s.MachineStatistics.TotalVerticalAxisCycles);
                                //diffCount -= countedStat.Instructions.FirstOrDefault(i => i.Definition.Id == ins.Definition.Id)?.IntCounter.Value ?? 0;
                            }
                            else
                            {
                                diffCount = allStat.Sum(s => s.MachineStatistics.TotalVerticalAxisCycles);
                            }
                            break;

                        case nameof(lastStat.TotalVerticalAxisKilometers):
                            if (countedStat != null)
                            {
                                diffCount = (int)allStat.Where(a => a.Id >= countedStat.Id).Sum(s => s.MachineStatistics.TotalVerticalAxisKilometers);
                                //diffCount -= countedStat.Instructions.FirstOrDefault(i => i.Definition.Id == ins.Definition.Id)?.IntCounter.Value ?? 0;
                            }
                            else
                            {
                                diffCount = (int)allStat.Sum(s => s.MachineStatistics.TotalVerticalAxisKilometers);
                            }
                            break;

                        case nameof(lastStat.TotalWeightBack):
                            if (countedStat != null)
                            {
                                diffCount = (int)allStat.Where(a => a.Id >= countedStat.Id).Sum(s => s.MachineStatistics.TotalWeightBack);
                                //diffCount -= countedStat.Instructions.FirstOrDefault(i => i.Definition.Id == ins.Definition.Id)?.IntCounter.Value ?? 0;
                            }
                            else
                            {
                                diffCount = (int)allStat.Sum(s => s.MachineStatistics.TotalWeightBack);
                            }
                            break;

                        case nameof(lastStat.TotalWeightFront):
                            if (countedStat != null)
                            {
                                diffCount = (int)allStat.Where(a => a.Id >= countedStat.Id).Sum(s => s.MachineStatistics.TotalWeightFront);
                                //diffCount -= countedStat.Instructions.FirstOrDefault(i => i.Definition.Id == ins.Definition.Id)?.IntCounter.Value ?? 0;
                            }
                            else
                            {
                                diffCount = (int)allStat.Sum(s => s.MachineStatistics.TotalWeightFront);
                            }
                            break;

                        case nameof(lastStat.UsageTimePercentage):
                            if (countedStat != null)
                            {
                                diffCount = (int)allStat.Where(a => a.Id >= countedStat.Id).Sum(s => s.MachineStatistics.UsageTimePercentage);
                                //diffCount -= countedStat.Instructions.FirstOrDefault(i => i.Definition.Id == ins.Definition.Id)?.IntCounter.Value ?? 0;
                            }
                            else
                            {
                                diffCount = (int)allStat.Sum(s => s.MachineStatistics.UsageTimePercentage);
                            }
                            break;

                        case nameof(lastStat.WeightCapacityPercentage):
                            if (countedStat != null)
                            {
                                diffCount = (int)allStat.Where(a => a.Id >= countedStat.Id).Sum(s => s.MachineStatistics.WeightCapacityPercentage);
                                //diffCount -= countedStat.Instructions.FirstOrDefault(i => i.Definition.Id == ins.Definition.Id)?.IntCounter.Value ?? 0;
                            }
                            else
                            {
                                diffCount = (int)allStat.Sum(s => s.MachineStatistics.WeightCapacityPercentage);
                            }
                            break;

                        case nameof(lastStat.TotalBayChainKilometers1):
                            if (countedStat != null)
                            {
                                diffCount = (int)allStat.Where(a => a.Id >= countedStat.Id).Sum(s => s.MachineStatistics.TotalBayChainKilometers1);
                                //diffCount -= countedStat.Instructions.FirstOrDefault(i => i.Definition.Id == ins.Definition.Id)?.IntCounter.Value ?? 0;
                            }
                            else
                            {
                                diffCount = (int)allStat.Sum(s => s.MachineStatistics.TotalBayChainKilometers1);
                            }
                            break;

                        case nameof(lastStat.TotalMissions):
                            if (countedStat != null)
                            {
                                diffCount = allStat.Where(a => a.Id >= countedStat.Id).Sum(s => s.MachineStatistics.TotalMissions);
                                //diffCount -= countedStat.Instructions.FirstOrDefault(i => i.Definition.Id == ins.Definition.Id)?.IntCounter.Value ?? 0;
                            }
                            else
                            {
                                diffCount = allStat.Sum(s => s.MachineStatistics.TotalMissions);
                            }
                            break;

                        default:
                            diffCount = 0;
                            this.logger.LogWarning("Instruction.Definition.CounterName unmanaged");
                            break;
                    }
                }
                else
                {
                    throw new System.ArgumentNullException(nameof(ins));
                }
                return diffCount;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public ServicingInfo GetActual()
        {
            lock (this.dataContext)
            {
                //ServicingInfo si = this.dataContext.ServicingInfo.LastOrDefault();
                //si.MachineStatistics = this.machineStatistics.GetById((int)si.MachineStatisticsId);
                //si.Instructions = this.dataContext.Instructions.Where(s => si.Id == s.ServicingInfo.Id).ToList();

                ServicingInfo si = this.dataContext.ServicingInfo
                   .Include(s => s.Instructions)
                   .ThenInclude(e => e.Definition)
                   .Include(s => s.MachineStatistics)
                   .Where(s => s.Id == s.MachineStatisticsId).LastOrDefault();
                return si;
            }
        }

        public IEnumerable<ServicingInfo> GetAll()
        {
            lock (this.dataContext)
            {
                List<ServicingInfo> silNew = new List<ServicingInfo>();

                List<ServicingInfo> silDB = this.dataContext.ServicingInfo.ToList();

                foreach (ServicingInfo si in silDB)
                {
                    si.MachineStatistics = this.machineStatistics.GetById((int)si.MachineStatisticsId);
                    si.Instructions = this.dataContext.Instructions.Include(e => e.Definition).Where(s => si.Id == s.ServicingInfo.Id).ToList();
                    silNew.Add(si);
                }

                return silNew.ToList();

                //return this.dataContext.ServicingInfo.ToList();
            }
        }

        public ServicingInfo GetById(int id)
        {
            lock (this.dataContext)
            {
                ServicingInfo si = this.dataContext.ServicingInfo
                    .Include(s => s.Instructions)
                    .ThenInclude(e => e.Definition)
                    .Include(s => s.MachineStatistics)
                    .Where(s => s.Id == id).FirstOrDefault();
                //si.MachineStatistics = this.machineStatistics.GetById((int)si.MachineStatisticsId);
                //si.Instructions = this.dataContext.Instructions.Where(s => si.Id == s.ServicingInfo.Id).ToList();
                return si;
            }
        }

        public ServicingInfo GetInstallationInfo()
        {
            lock (this.dataContext)
            {
                ServicingInfo si = this.dataContext.ServicingInfo
                    .Include(s => s.Instructions)
                    .ThenInclude(e => e.Definition)
                    .Include(s => s.MachineStatistics)
                    .Where(s => s.InstallationDate != null).FirstOrDefault();

                if (si != null)
                {
                    si.MachineStatistics = this.machineStatistics.GetById((int)si.MachineStatisticsId);
                    si.Instructions = this.dataContext.Instructions.Where(s => si.Id == s.ServicingInfo.Id).ToList();

                    return si;
                }
                else
                {
                    ServicingInfo siTot = this.dataContext.ServicingInfo
                        .Include(s => s.Instructions)
                        .ThenInclude(e => e.Definition)
                        .Include(s => s.MachineStatistics)
                        .FirstOrDefault();

                    if (siTot == null)
                    {
                        siTot = new ServicingInfo();

                        siTot.MachineStatisticsId = this.machineStatistics.GetActual().Id;

                        this.dataContext.ServicingInfo.Add(siTot);
                        this.dataContext.SaveChanges();
                    }

                    siTot.MachineStatistics = this.machineStatistics.GetById((int)siTot.MachineStatisticsId);
                    siTot.Instructions = this.dataContext.Instructions.Where(s => siTot.Id == s.ServicingInfo.Id).ToList();

                    return siTot;
                }
            }
        }

        public ServicingInfo GetLastConfirmed()
        {
            lock (this.dataContext)
            {
                int dim = this.dataContext.ServicingInfo.Count();

                if (dim > 1)
                {
                    ServicingInfo si = this.dataContext.ServicingInfo
                        .Include(s => s.Instructions)
                        .ThenInclude(e => e.Definition)
                        .Include(s => s.MachineStatistics)
                        .Where(S => S.ServiceStatus == MachineServiceStatus.Completed).LastOrDefault();
                    si.MachineStatistics = this.machineStatistics.GetById((int)si.MachineStatisticsId);
                    si.Instructions = this.dataContext.Instructions.Where(s => si.Id == s.ServicingInfo.Id).ToList();

                    return si;
                }
                else
                {
                    return null;
                }
            }
        }

        public ServicingInfo GetLastValid()
        {
            lock (this.dataContext)
            {
                int dim = this.dataContext.ServicingInfo.Count();

                if (dim > 1)
                {
                    ServicingInfo si = this.dataContext.ServicingInfo
                        .Include(s => s.Instructions)
                        .ThenInclude(e => e.Definition)
                        .Include(s => s.MachineStatistics)
                        .Where(S => S.ServiceStatus == MachineServiceStatus.Valid).LastOrDefault();
                    si.MachineStatistics = this.machineStatistics.GetById((int)si.MachineStatisticsId);
                    si.Instructions = this.dataContext.Instructions.Where(s => si.Id == s.ServicingInfo.Id).ToList();

                    return si;
                }
                else
                {
                    return null;
                }
            }
        }

        public bool IsAnyInstructionExpired()
        {
            bool expired = false;

            var service = this.dataContext.ServicingInfo.Last();

            var instructions = this.dataContext.Instructions.Where(s => s.ServicingInfo.Id == service.Id).ToList();
            foreach (var ins in instructions)
            {
                if (ins.InstructionStatus == MachineServiceStatus.Expired)
                {
                    expired = true;
                }
            }

            return expired;
        }

        public bool IsAnyInstructionExpiring()
        {
            bool expiring = false;

            var service = this.dataContext.ServicingInfo.Last();

            var instructions = this.dataContext.Instructions.Where(s => s.ServicingInfo.Id == service.Id).ToList();
            foreach (var ins in instructions)
            {
                if (ins.InstructionStatus == MachineServiceStatus.Expiring)
                {
                    expiring = true;
                }
            }

            return expiring;
        }

        public void RefreshDescription(int servicingInfoId)
        {
            lock (this.dataContext)
            {
                try
                {
                    var instruction = this.dataContext.Instructions.Include(n => n.Definition).Where(s => s.ServicingInfo.Id == servicingInfoId).ToList();
                    foreach (var ins in instruction)
                    {
                        ins.Definition.GetDescription(ins.Definition.InstructionType);
                        this.dataContext.Instructions.Update(ins);
                        this.dataContext.SaveChanges();
                    }
                }
                catch (Exception)
                {
                    //do nothing
                }
            }
        }

        public void SetIsToDo(int instructionId)
        {
            lock (this.dataContext)
            {
                try
                {
                    // Confirm setup date in actual record
                    var instruction = this.dataContext.Instructions.LastOrDefault(s => s.Id == instructionId);
                    instruction.IsToDo = true;
                    instruction.InstructionStatus = MachineServiceStatus.Expiring;
                    this.dataContext.Instructions.Update(instruction);
                    this.dataContext.SaveChanges();
                }
                catch (Exception)
                {
                    //do nothing
                }
            }
        }

        public void UpdateServiceStatus()
        {
            lock (this.dataContext)
            {
                try
                {
                    // Confirm setup date in actual record
                    var service = this.dataContext.ServicingInfo.Last();
                    if (service.ServiceStatus == MachineServiceStatus.Expired)
                    {
                        this.logger.LogWarning(Resources.General.MaintenanceStateExpired);
                    }
                    if (service.ServiceStatus == MachineServiceStatus.Expiring
                        && service.NextServiceDate != null
                        )
                    {
                        var diff = service.NextServiceDate.Value.Subtract(DateTime.UtcNow);
                        if (diff.TotalDays <= 0)
                        {
                            service.ServiceStatus = MachineServiceStatus.Expired;
                            this.dataContext.ServicingInfo.Update(service);
                        }
                        this.logger.LogWarning(Resources.General.MaintenanceStateExpiring);
                    }
                    if (service.ServiceStatus == MachineServiceStatus.Valid
                        && service.NextServiceDate != null
                        )
                    {
                        var diff = service.NextServiceDate.Value.Subtract(DateTime.UtcNow);
                        if (diff.TotalDays <= 30)
                        {
                            service.ServiceStatus = MachineServiceStatus.Expiring;
                            this.dataContext.ServicingInfo.Update(service);
                        }
                    }

                    var instructions = this.dataContext.Instructions.Include(n => n.Definition).Where(s => s.ServicingInfo.Id == service.Id).ToList();
                    var machine = this.dataContext.Machines.LastOrDefault();
                    foreach (var ins in instructions)
                    {
                        if (ins.InstructionStatus == MachineServiceStatus.Expired)
                        {
                            this.logger.LogWarning(Resources.General.MaintenanceStateExpired);
                        }
                        if (ins.InstructionStatus == MachineServiceStatus.Expiring
                            && ins.MaintenanceDate != null)
                        {
                            if (ins.Definition?.CounterName != null)
                            {
                                var diffCount = this.DiffCount(ins);
                                //var diffCountPercent = (diffCount * machine.ExpireCountPrecent) / 100;
                                var diff = ins.MaintenanceDate.Value.Subtract(DateTime.UtcNow);
                                if ((ins.Definition.MaxDays.HasValue && diff.TotalDays <= ins.Definition.MaxDays)
                                    || diffCount <= 0
                                    )
                                {
                                    ins.InstructionStatus = MachineServiceStatus.Expired;
                                    this.dataContext.Instructions.Update(ins);
                                    //this.logger.LogWarning(Resources.General.MaintenanceStateExpiring);
                                }
                            }
                            else if (ins.Definition.MaxDays.HasValue)
                            {
                                var diff = ins.MaintenanceDate.Value.Subtract(DateTime.UtcNow);
                                if (diff.TotalDays <= ins.Definition.MaxDays)
                                {
                                    ins.InstructionStatus = MachineServiceStatus.Expired;
                                    this.dataContext.Instructions.Update(ins);
                                    //this.logger.LogWarning(Resources.General.MaintenanceStateExpiring);
                                }
                            }
                        }
                        if (ins.InstructionStatus == MachineServiceStatus.Valid
                            && ins.MaintenanceDate != null
                            )
                        {
                            if (ins.Definition?.CounterName != null)
                            {
                                var diffCount = this.DiffCount(ins);
                                var diffCountPercent = (diffCount * machine.ExpireCountPrecent) / 100;
                                var diff = ins.MaintenanceDate.Value.Subtract(DateTime.UtcNow);
                                if ((ins.Definition.MaxDays.HasValue && diff.TotalDays <= ins.Definition.MaxDays + machine.ExpireDays)
                                    || diffCount >= diffCountPercent)
                                {
                                    ins.InstructionStatus = MachineServiceStatus.Expiring;
                                    ins.IsToDo = true;
                                    this.dataContext.Instructions.Update(ins);
                                }
                            }
                            else if (ins.Definition.MaxDays.HasValue)
                            {
                                var diff = ins.MaintenanceDate.Value.Subtract(DateTime.UtcNow);
                                if (diff.TotalDays <= ins.Definition.MaxDays + machine.ExpireDays)
                                {
                                    ins.InstructionStatus = MachineServiceStatus.Expiring;
                                    ins.IsToDo = true;
                                    this.dataContext.Instructions.Update(ins);
                                }
                            }
                        }
                    }
                    this.dataContext.SaveChanges();
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, ex.Message);
                }
            }
        }

        private void GenerateInstructions(ServicingInfo s)
        {
            var instructionDefinitions = this.dataContext.InstructionDefinitions.ToList();
            if (instructionDefinitions.Any())
            {
                foreach (var definition in instructionDefinitions)
                {
                    var instruction = new Instruction();
                    instruction.ServicingInfo = s;
                    instruction.Definition = definition;
                    instruction.MaintenanceDate = DateTime.UtcNow;
                    this.dataContext.Instructions.Add(instruction);
                }
            }
        }

        #endregion
    }
}
