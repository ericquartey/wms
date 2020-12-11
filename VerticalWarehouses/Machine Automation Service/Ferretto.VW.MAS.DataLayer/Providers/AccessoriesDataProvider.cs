using System;
using System.Linq;
using System.Net;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.VW.MAS.DataLayer
{
    internal sealed class AccessoriesDataProvider : IAccessoriesDataProvider
    {
        #region Fields

        private readonly DataLayerContext dataContext;

        #endregion

        #region Constructors

        public AccessoriesDataProvider(DataLayerContext dataContext)
        {
            this.dataContext = dataContext;
        }

        #endregion

        #region Methods

        public BayAccessories GetAccessories(BayNumber bayNumber)
        {
            lock (this.dataContext)
            {
                var bay = this.dataContext.Bays
                    .Include(b => b.Accessories)
                        .ThenInclude(a => a.AlphaNumericBar)
                        .ThenInclude(a => a.DeviceInformation)
                    .Include(b => b.Accessories)
                        .ThenInclude(a => a.BarcodeReader)
                        .ThenInclude(a => a.DeviceInformation)
                    .Include(b => b.Accessories)
                        .ThenInclude(a => a.CardReader)
                        .ThenInclude(a => a.DeviceInformation)
                    .Include(b => b.Accessories)
                        .ThenInclude(a => a.LabelPrinter)
                        .ThenInclude(a => a.DeviceInformation)
                    .Include(b => b.Accessories)
                        .ThenInclude(a => a.LaserPointer)
                        .ThenInclude(a => a.DeviceInformation)
                    .Include(b => b.Accessories)
                        .ThenInclude(a => a.TokenReader)
                        .ThenInclude(a => a.DeviceInformation)
                    .Include(b => b.Accessories)
                        .ThenInclude(a => a.WeightingScale)
                        .ThenInclude(a => a.DeviceInformation)
                    .AsNoTracking()
                    .SingleOrDefault(b => b.Number == bayNumber);

                return bay.Accessories ?? new BayAccessories();
            }
        }

        public void UpdateAlphaNumericBar(BayNumber bayNumber, bool isEnabled, string ipAddress, int port)
        {
            lock (this.dataContext)
            {
                var barBay = this.dataContext.Bays.Include(b => b.Accessories)
                        .ThenInclude(a => a.AlphaNumericBar)
                        .Single(b => b.Number == bayNumber);

                barBay.Accessories.AlphaNumericBar.IsEnabledNew = isEnabled;
                barBay.Accessories.AlphaNumericBar.IpAddress = IPAddress.Parse(ipAddress);
                barBay.Accessories.AlphaNumericBar.TcpPort = port;

                this.dataContext.Accessories.Update(barBay.Accessories.AlphaNumericBar);
                this.dataContext.SaveChanges();
            }
        }

        public void UpdateBarcodeReaderDeviceInfo(BayNumber bayNumber, DeviceInformation deviceInformation)
        {
            lock (this.dataContext)
            {
                var bay = this.dataContext.Bays
                    .Include(b => b.Accessories)
                    .ThenInclude(a => a.BarcodeReader)
                    .ThenInclude(r => r.DeviceInformation)
                    .Single(b => b.Number == bayNumber);

                if (bay.Accessories.BarcodeReader.DeviceInformation is null)
                {
                    bay.Accessories.BarcodeReader.DeviceInformation = new DeviceInformation();
                    this.dataContext.Accessories.Update(bay.Accessories.BarcodeReader);
                }

                var deviceInfo = bay.Accessories.BarcodeReader.DeviceInformation;

                deviceInfo.SerialNumber = deviceInformation.SerialNumber;
                deviceInfo.FirmwareVersion = deviceInformation.FirmwareVersion;
                deviceInfo.ManufactureDate = deviceInformation.ManufactureDate;
                deviceInfo.ModelNumber = deviceInformation.ModelNumber;

                this.dataContext.DeviceInformation.Update(deviceInfo);
                this.dataContext.SaveChanges();
            }
        }

        public void UpdateBarcodeReaderSettings(BayNumber bayNumber, bool isEnabled, string portName)
        {
            if (portName is null)
            {
                throw new ArgumentNullException(nameof(portName));
            }

            lock (this.dataContext)
            {
                var bay = this.dataContext.Bays
                    .Include(b => b.Accessories)
                    .ThenInclude(a => a.BarcodeReader)
                    .Single(b => b.Number == bayNumber);

                bay.Accessories.BarcodeReader.IsEnabledNew = isEnabled;
                bay.Accessories.BarcodeReader.PortName = portName;

                this.dataContext.Accessories.Update(bay.Accessories.BarcodeReader);
                this.dataContext.SaveChanges();
            }
        }

        public void UpdateCardReaderSettings(BayNumber bayNumber, bool isEnabled, string tokenRegex)
        {
            lock (this.dataContext)
            {
                var bay = this.dataContext.Bays
                    .Include(b => b.Accessories)
                    .ThenInclude(a => a.CardReader)
                    .Single(b => b.Number == bayNumber);

                bay.Accessories.CardReader.IsEnabledNew = isEnabled;
                bay.Accessories.CardReader.TokenRegex = tokenRegex;

                this.dataContext.Accessories.Update(bay.Accessories.CardReader);
                this.dataContext.SaveChanges();
            }
        }

        public void UpdateLabelPrinterSettings(BayNumber bayNumber, bool isEnabled, string printerName)
        {
            lock (this.dataContext)
            {
                var bay = this.dataContext.Bays
                    .Include(b => b.Accessories)
                    .ThenInclude(a => a.LabelPrinter)
                    .Single(b => b.Number == bayNumber);

                bay.Accessories.LabelPrinter.IsEnabledNew = isEnabled;
                bay.Accessories.LabelPrinter.Name = printerName;

                this.dataContext.Accessories.Update(bay.Accessories.LabelPrinter);
                this.dataContext.SaveChanges();
            }
        }

        public void UpdateLaserPointer(BayNumber bayNumber, bool isEnabled, string ipAddress, int port, double xOffset, double yOffset, double zOffsetLowerPosition, double zOffsetUpperPosition)
        {
            lock (this.dataContext)
            {
                var laserPointerBay = this.dataContext.Bays.Include(b => b.Accessories)
                        .ThenInclude(a => a.LaserPointer)
                        .Single(b => b.Number == bayNumber);

                var laserPointer = laserPointerBay.Accessories.LaserPointer;

                laserPointer.IsEnabledNew = isEnabled;
                laserPointer.IpAddress = IPAddress.Parse(ipAddress);
                laserPointer.TcpPort = port;
                laserPointer.XOffset = xOffset;
                laserPointer.YOffset = yOffset;
                laserPointer.ZOffsetLowerPosition = zOffsetLowerPosition;
                laserPointer.ZOffsetUpperPosition = zOffsetUpperPosition;

                this.dataContext.Accessories.Update(laserPointer);
                this.dataContext.SaveChanges();
            }
        }

        public void UpdateTokenReaderSettings(BayNumber bayNumber, bool isEnabled, string portName)
        {
            if (portName is null)
            {
                throw new ArgumentNullException(nameof(portName));
            }

            lock (this.dataContext)
            {
                var bay = this.dataContext.Bays
                    .Include(b => b.Accessories)
                    .ThenInclude(a => a.TokenReader)
                    .Single(b => b.Number == bayNumber);

                bay.Accessories.TokenReader.IsEnabledNew = isEnabled;
                bay.Accessories.TokenReader.PortName = portName;

                this.dataContext.Accessories.Update(bay.Accessories.TokenReader);
                this.dataContext.SaveChanges();
            }
        }

        public void UpdateWeightingScaleDeviceInfo(BayNumber bayNumber, DeviceInformation deviceInformation)
        {
            lock (this.dataContext)
            {
                var bay = this.dataContext.Bays
                    .Include(b => b.Accessories)
                    .ThenInclude(a => a.WeightingScale)
                    .ThenInclude(r => r.DeviceInformation)
                    .Single(b => b.Number == bayNumber);

                if (bay.Accessories.WeightingScale.DeviceInformation is null)
                {
                    bay.Accessories.WeightingScale.DeviceInformation = new DeviceInformation();
                    this.dataContext.Accessories.Update(bay.Accessories.WeightingScale);
                }

                var deviceInfo = bay.Accessories.WeightingScale.DeviceInformation;

                deviceInfo.SerialNumber = deviceInformation.SerialNumber;
                deviceInfo.FirmwareVersion = deviceInformation.FirmwareVersion;
                deviceInfo.ManufactureDate = deviceInformation.ManufactureDate;
                deviceInfo.ModelNumber = deviceInformation.ModelNumber;

                this.dataContext.DeviceInformation.Update(deviceInfo);
                this.dataContext.SaveChanges();
            }
        }

        public void UpdateWeightingScaleSettings(BayNumber bayNumber, bool isEnabled, string ipAddress, int port)
        {
            lock (this.dataContext)
            {
                var bay = this.dataContext.Bays
                    .Include(b => b.Accessories)
                    .ThenInclude(a => a.WeightingScale)
                    .Single(b => b.Number == bayNumber);

                if (bay.Accessories.WeightingScale is null)
                {
                    bay.Accessories.WeightingScale = new WeightingScale();
                    bay.Accessories.WeightingScale.IsEnabledNew = isEnabled;
                    bay.Accessories.WeightingScale.IpAddress = IPAddress.Parse(ipAddress);
                    bay.Accessories.WeightingScale.TcpPort = port;
                }
                else
                {
                    bay.Accessories.WeightingScale.IsEnabledNew = isEnabled;
                    bay.Accessories.WeightingScale.IpAddress = IPAddress.Parse(ipAddress);
                    bay.Accessories.WeightingScale.TcpPort = port;
                }

                this.dataContext.Accessories.Update(bay.Accessories.WeightingScale);
                this.dataContext.SaveChanges();
            }
        }

        #endregion
    }
}
