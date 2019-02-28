using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS_IODriver
{
    public class NewRemoteIODriverMock : INewRemoteIODriver
    {
        #region Fields

        private readonly ILogger<NewRemoteIODriverMock> logger;

        #endregion

        #region Constructors

        public NewRemoteIODriverMock(ILogger<NewRemoteIODriverMock> logger)
        {
            this.logger = logger;
        }

        #endregion

        #region Properties

        public List<bool> Inputs { get; set; }

        public string IPAddress { get; set; }

        public List<bool> Outputs { get; set; }

        public int Port { get; set; }

        #endregion

        #region Methods

        public void Disconnect()
        {
            this.logger.Log(LogLevel.Debug, "NewRemoteIOMock Disconnect");
        }

        public void SwitchHorizontalToVertical()
        {
            this.logger.Log(LogLevel.Debug, "NewRemoteIOMock SwitchHorizontalToVertical");
        }

        public void SwitchVerticalToHorizontal()
        {
            this.logger.Log(LogLevel.Debug, "NewRemoteIOMock SwitchVerticalToHorizontal");
        }

        #endregion
    }
}
