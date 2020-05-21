using System;
using System.Collections.Generic;
using System.Linq;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer
{
    internal sealed class ServicingProvider : IServicingProvider
    {
        #region Fields

        private readonly DataLayerContext dataContext;

        private readonly IStatisticsDataProvider machineStatistics;

        #endregion

        #region Constructors

        public ServicingProvider(DataLayerContext dataContext, IStatisticsDataProvider machineStatistics)
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
                    var machineId = this.dataContext.MachineStatistics.LastOrDefault()?.Id;
                    var s = new ServicingInfo();
                    if (machineId.HasValue)
                    {
                        s.MachineStatisticsId = machineId.Value;
                    }
                    this.dataContext.ServicingInfo.Add(s);
                    this.dataContext.SaveChanges();

                    this.GenerateInstructions(s);
                    this.dataContext.SaveChanges();
                }
            }
        }

        public void ConfirmService()
        {
            lock (this.dataContext)
            {
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
                    this.dataContext.ServicingInfo.FirstOrDefault().InstallationDate = DateTime.Now;
                    this.dataContext.ServicingInfo.Update(this.dataContext.ServicingInfo.FirstOrDefault());

                    // Add new record
                    var s = new ServicingInfo();

                    s.InstallationDate = DateTime.Now;
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
                return this.dataContext.ServicingInfo.LastOrDefault();
            }
        }

        public IEnumerable<ServicingInfo> GetAll()
        {
            lock (this.dataContext)
            {
                return this.dataContext.ServicingInfo.ToList();
            }
        }

        public ServicingInfo GetById(int id)
        {
            lock (this.dataContext)
            {
                return this.dataContext.ServicingInfo.Where(s => s.Id == id).FirstOrDefault();
            }
        }

        public ServicingInfo GetInstallationInfo()
        {
            lock (this.dataContext)
            {
                return this.dataContext.ServicingInfo.Where(s => s.InstallationDate != null).FirstOrDefault();
            }
        }

        public ServicingInfo GetLastConfirmed()
        {
            lock (this.dataContext)
            {
                int dim = this.dataContext.ServicingInfo.Count();

                if (dim > 1)
                {
                    return this.dataContext.ServicingInfo.ElementAt(dim - 1);
                }
                else
                {
                    return null;
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
