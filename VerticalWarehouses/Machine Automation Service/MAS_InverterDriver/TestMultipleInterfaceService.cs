using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.MAS_DataLayer;
using Microsoft.Extensions.Hosting;

namespace Ferretto.VW.InverterDriver
{
    public class TestMultipleInterfaceService : BackgroundService, IDataLayer, IWriteLogService
    {
        #region Fields

        private int data;

        #endregion

        #region Constructors

        public TestMultipleInterfaceService(int actualData)
        {
            this.data = actualData;
        }

        #endregion

        #region Methods

        public List<Cell> GetCellList()
        {
            throw new NotImplementedException();
        }

        public decimal GetDecimalConfigurationValue(ConfigurationValueEnum configurationValueEnum)
        {
            throw new NotImplementedException();
        }

        public decimal GetDecimalRuntimeValue(RuntimeValueEnum runtimeValueEnum)
        {
            throw new NotImplementedException();
        }

        public ReturnMissionPosition GetFreeBlockPosition(decimal drawerHeight)
        {
            throw new NotImplementedException();
        }

        public int GetIntegerConfigurationValue(ConfigurationValueEnum configurationValueEnum)
        {
            throw new NotImplementedException();
        }

        public int GetIntegerRuntimeValue(RuntimeValueEnum runtimeValueEnum)
        {
            throw new NotImplementedException();
        }

        public IPAddress GetIPAddressConfigurationValue(ConfigurationValueEnum configurationValueEnum)
        {
            throw new NotImplementedException();
        }

        public string GetStringConfigurationValue(ConfigurationValueEnum configurationValueEnum)
        {
            throw new NotImplementedException();
        }

        public string GetStringRuntimeValue(RuntimeValueEnum runtimeValueEnum)
        {
            throw new NotImplementedException();
        }

        public void LogWriting(CommandMessage command_EventParameter)
        {
            throw new NotImplementedException();
        }

        public bool SetCellList(List<Cell> listCells)
        {
            throw new NotImplementedException();
        }

        public void SetDecimalConfigurationValue(ConfigurationValueEnum configurationValueEnum, decimal value)
        {
            throw new NotImplementedException();
        }

        public void SetDecimalRuntimeValue(RuntimeValueEnum runtimeValueEnum, decimal value)
        {
            throw new NotImplementedException();
        }

        public void SetIntegerConfigurationValue(ConfigurationValueEnum configurationValueEnum, int value)
        {
            throw new NotImplementedException();
        }

        public void SetIntegerRuntimeValue(RuntimeValueEnum runtimeValueEnum, int value)
        {
            throw new NotImplementedException();
        }

        public void SetStringConfigurationValue(ConfigurationValueEnum configurationValueEnum, string value)
        {
            throw new NotImplementedException();
        }

        public void SetStringRuntimeValue(RuntimeValueEnum runtimeValueEnum, string value)
        {
            throw new NotImplementedException();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }

        #endregion
    }
}
