using System;
using System.Collections.Generic;
using System.Text;

namespace Ferretto.VW.MAS_IODriver
{
    public class NewRemoteIODriverMock : INewRemoteIODriver
    {
        #region Properties

        public List<Boolean> Inputs { get; set; }

        public String IPAddress { get; set; }

        public List<Boolean> Outputs { get; set; }

        public Int32 Port { get; set; }

        #endregion

        #region Methods

        public void Disconnect()
        {
            Console.WriteLine("RemoteIODriverMock Disconnect\n");
        }

        public void SwitchHorizontalToVertical()
        {
            Console.WriteLine("RemoteIODriverMock SwitchHorizontalToVertical\n");
        }

        public void SwitchVerticalToHorizontal()
        {
            Console.WriteLine("RemoteIODriverMock SwitchVerticalToHorizontal\n");
        }

        #endregion
    }
}
