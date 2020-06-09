using System;
using System.Collections.Generic;
using System.Linq;
using Ferretto.VW.MAS.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NLog;
using Remotion.Linq.Parsing.Structure.IntermediateModel;

namespace Ferretto.VW.MAS.DataLayer
{
    internal sealed class ServicingProvider : IServicingProvider
    {
        #region Fields

        private readonly DataLayerContext dataContext;

        private readonly IStatisticsDataProvider machineStatistics;

        #endregion

        #region Constructors

        public ServicingProvider(DataLayerContext dataContext,
            IStatisticsDataProvider machineStatistics)
        {
            this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
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
                    instruction.IsDone = true;
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
                    si.Instructions = this.dataContext.Instructions.Where(s => si.Id == s.ServicingInfo.Id).ToList();
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
                ServicingInfo si = this.dataContext.ServicingInfo.Where(s => s.InstallationDate != null).FirstOrDefault();

                if (si != null)
                {
                    si.MachineStatistics = this.machineStatistics.GetById((int)si.MachineStatisticsId);
                    si.Instructions = this.dataContext.Instructions.Where(s => si.Id == s.ServicingInfo.Id).ToList();

                    return si;
                }
                else
                {
                    ServicingInfo siTot = this.dataContext.ServicingInfo.FirstOrDefault();

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
                    ServicingInfo si = this.dataContext.ServicingInfo.Where(S => S.ServiceStatus == MachineServiceStatus.Completed).LastOrDefault();
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
                    ServicingInfo si = this.dataContext.ServicingInfo.Where(S => S.ServiceStatus == MachineServiceStatus.Valid).LastOrDefault();
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
                    var logger = LogManager.GetCurrentClassLogger();

                    // Confirm setup date in actual record
                    var service = this.dataContext.ServicingInfo.Last();
                    if (service.ServiceStatus == MachineServiceStatus.Expired)
                    {
                        logger.Warn(Resources.General.MaintenanceStateExpired);
                        //this.Logger.LogWarning(Resources.General.MaintenanceStateExpired);
                    }
                    if (service.ServiceStatus == MachineServiceStatus.Expiring
                        && service.NextServiceDate != null
                        )
                    {
                        var diff = service.NextServiceDate.Value.Subtract(DateTime.UtcNow);
                        if (diff.TotalDays <= 0)
                        {
                            service.ServiceStatus = MachineServiceStatus.Expired;
                            this.dataContext.ServicingInfo.Update(this.dataContext.ServicingInfo.Last());
                        }
                        logger.Warn(Resources.General.MaintenanceStateExpiring);
                        //this.Logger.LogWarning(Resources.General.MaintenanceStateExpiring);
                    }
                    if (service.ServiceStatus == MachineServiceStatus.Valid
                        && service.NextServiceDate != null
                        )
                    {
                        var diff = service.NextServiceDate.Value.Subtract(DateTime.UtcNow);
                        if (diff.TotalDays <= 30)
                        {
                            service.ServiceStatus = MachineServiceStatus.Expiring;
                            this.dataContext.ServicingInfo.Update(this.dataContext.ServicingInfo.Last());
                        }
                    }

                    var instructions = this.dataContext.Instructions.Where(s => s.ServicingInfo.Id == service.Id).ToList();
                    var machine = this.dataContext.Machines.LastOrDefault();
                    foreach (var ins in instructions)
                    {
                        if (ins.InstructionStatus == MachineServiceStatus.Expired)
                        {
                            logger.Warn(Resources.General.MaintenanceStateExpired);
                            //this.Logger.LogWarning(Resources.General.MaintenanceStateExpired);
                        }
                        if (ins.InstructionStatus == MachineServiceStatus.Expiring
                            && ins.MaintenanceDate != null
                            )
                        {
                            if (ins.Definition.CounterName != null && ins.IntCounter != null)
                            {
                                var diffCount = ins.IntCounter - ins.Definition.MaxRelativeCount;
                                var diffCountPercent = (diffCount * machine.ExpireCountPrecent) / 100;
                                var diff = ins.MaintenanceDate.Value.Subtract(DateTime.UtcNow);
                                if (diff.TotalDays <= ins.Definition.MaxDays || diffCount > diffCountPercent)
                                {
                                    ins.InstructionStatus = MachineServiceStatus.Expired;
                                    ins.IsToDo = true;
                                    this.dataContext.Instructions.Update(ins);
                                }
                                logger.Warn(Resources.General.MaintenanceStateExpiring);
                            }
                            else
                            {
                                var diff = ins.MaintenanceDate.Value.Subtract(DateTime.UtcNow);
                                if (diff.TotalDays <= ins.Definition.MaxDays)
                                {
                                    ins.InstructionStatus = MachineServiceStatus.Expired;
                                    ins.IsToDo = true;
                                    this.dataContext.Instructions.Update(ins);
                                }
                                logger.Warn(Resources.General.MaintenanceStateExpiring);
                            }
                        }
                        if (ins.InstructionStatus == MachineServiceStatus.Valid
                            && ins.MaintenanceDate != null
                            )
                        {
                            var diff = ins.MaintenanceDate.Value.Subtract(DateTime.UtcNow);
                            if (diff.TotalDays <= ins.Definition.MaxDays)
                            {
                                ins.InstructionStatus = MachineServiceStatus.Expiring;
                                this.dataContext.Instructions.Update(ins);
                            }
                        }
                    }
                }
                catch (Exception)
                {
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
                    this.dataContext.Instructions.Add(instruction);
                }
            }
        }

        #endregion
    }
}
