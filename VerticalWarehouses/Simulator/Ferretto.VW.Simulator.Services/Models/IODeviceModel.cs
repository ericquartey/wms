using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;

namespace Ferretto.VW.Simulator.Services.Models
{
    public class IODeviceModel : BindableBase
    {
        #region Fields

        public byte[] Buffer;

        private ObservableCollection<IOsModel> ios = new ObservableCollection<IOsModel>();

        #endregion

        #region Constructors

        public IODeviceModel()
        {
            this.IOs.Add(new IOsModel("Id:00", false));
            this.IOs.Add(new IOsModel("Id:01", false));
            this.IOs.Add(new IOsModel("Id:02", false));
            this.IOs.Add(new IOsModel("Id:03", false));
            this.IOs.Add(new IOsModel("Id:04", false));
            this.IOs.Add(new IOsModel("Id:05", false));
            this.IOs.Add(new IOsModel("Id:06", false));
            this.IOs.Add(new IOsModel("Id:07", false));
            this.IOs.Add(new IOsModel("Id:08", false));
            this.IOs.Add(new IOsModel("Id:09", false));
            this.IOs.Add(new IOsModel("Id:10", false));
            this.IOs.Add(new IOsModel("Id:11", false));
            this.IOs.Add(new IOsModel("Id:12", false));
            this.IOs.Add(new IOsModel("Id:13", false));
            this.IOs.Add(new IOsModel("Id:14", false));
            this.IOs.Add(new IOsModel("Id:15", false));
        }

        #endregion

        #region Properties

        public bool Enabled { get; set; } = true;

        public byte FirmwareVersion { get; set; } = 0x10;

        public int Id { get; set; }

        public ObservableCollection<IOsModel> IOs { get => this.ios; set { this.SetProperty(ref this.ios, value); } }

        public ushort IOValue
        {
            get
            {
                ushort result = 0;
                for (int i = 0; i < this.IOs.Count; i++)
                {
                    if (this.IOs[i].Value)
                    {
                        result += (ushort)Math.Pow(2, i);
                    }
                }
                return result;
            }
        }

        #endregion
    }
}
