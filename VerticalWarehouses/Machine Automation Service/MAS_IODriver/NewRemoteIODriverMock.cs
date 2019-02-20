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

        public List<Boolean> Inputs { get; set; }

        public String IPAddress { get; set; }

        public List<Boolean> Outputs { get; set; }

        public Int32 Port { get; set; }

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
