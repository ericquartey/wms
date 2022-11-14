using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.VW.MAS.DataLayer
{
    internal sealed class AccessoriesDataProvider : IAccessoriesDataProvider
    {
        #region Fields

        private readonly IBaysDataProvider baysDataProvider;

        private readonly DataLayerContext dataContext;

        #endregion

        #region Constructors

        public AccessoriesDataProvider(
            IBaysDataProvider baysDataProvider,
            DataLayerContext dataContext)
        {
            this.dataContext = dataContext;
            this.baysDataProvider = baysDataProvider;
        }

        #endregion

        #region Methods

        public bool CheckAccessories()
        {
            lock (this.dataContext)
            {
                var bars = this.dataContext.Bays.Include(b => b.Accessories)
                        .ThenInclude(a => a.AlphaNumericBar);
                if (bars != null)
                {
                    foreach (var bar in bars)
                    {
                        if (bar.Accessories != null
                            && bar.Accessories.AlphaNumericBar != null)
                        {
                            if (bar.Accessories.AlphaNumericBar.Field1 is null)
                            {
                                bar.Accessories.AlphaNumericBar.Field1 = "ItemCode";
                                this.dataContext.SaveChanges();
                            }
                            if (bar.Accessories.AlphaNumericBar.Field2 is null)
                            {
                                bar.Accessories.AlphaNumericBar.Field2 = "ItemDescription";
                                this.dataContext.SaveChanges();
                            }
                        }
                    }
                }
                return true;
            }
        }

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

        public AlphaNumericBar GetAlphaNumericBar(BayNumber bayNumber)
        {
            lock (this.dataContext)
            {
                try
                {
                    return this.dataContext.Bays.Include(b => b.Accessories)
                        .ThenInclude(a => a.AlphaNumericBar)
                        .Select(s => new { s.Number, s.Accessories.AlphaNumericBar })
                        .First(b => b.Number == bayNumber)
                        .AlphaNumericBar;
                }
                catch
                {
                    return null;
                }
            }
        }

        public LaserPointer GetLaserPointer(BayNumber bayNumber)
        {
            lock (this.dataContext)
            {
                try
                {
                    return this.dataContext.Bays.Include(b => b.Accessories)
                        .ThenInclude(a => a.LaserPointer)
                        .Select(s => new { s.Number, s.Accessories.LaserPointer })
                        .First(b => b.Number == bayNumber)
                        .LaserPointer;
                }
                catch
                {
                    return null;
                }
            }
        }

        public void UpdateAlphaNumericBar(
                    BayNumber bayNumber,
            bool isEnabled,
            string ipAddress,
            int port,
            AlphaNumericBarSize size,
            int maxMessageLength,
            bool clearOnClose,
            bool? useGet = false,
            List<string> messageFields = null)

        {
            lock (this.dataContext)
            {
                var barBay = this.dataContext.Bays.Include(b => b.Accessories)
                        .ThenInclude(a => a.AlphaNumericBar)
                        .Single(b => b.Number == bayNumber);

                barBay.Accessories.AlphaNumericBar.IsEnabledNew = isEnabled;
                barBay.Accessories.AlphaNumericBar.IpAddress = IPAddress.Parse(ipAddress);
                barBay.Accessories.AlphaNumericBar.TcpPort = port;
                barBay.Accessories.AlphaNumericBar.Size = size;
                barBay.Accessories.AlphaNumericBar.MaxMessageLength = maxMessageLength;
                barBay.Accessories.AlphaNumericBar.ClearAlphaBarOnCloseView = clearOnClose;
                barBay.Accessories.AlphaNumericBar.UseGet = useGet;
                if (messageFields != null && messageFields.Count > 0)
                {
                    barBay.Accessories.AlphaNumericBar.Field1 = string.Empty;
                    barBay.Accessories.AlphaNumericBar.Field2 = string.Empty;
                    barBay.Accessories.AlphaNumericBar.Field3 = string.Empty;
                    barBay.Accessories.AlphaNumericBar.Field4 = string.Empty;
                    barBay.Accessories.AlphaNumericBar.Field5 = string.Empty;
                    int iField = 1;
                    foreach (var messageField in messageFields)
                    {
                        switch (iField)
                        {
                            case 1:
                                barBay.Accessories.AlphaNumericBar.Field1 = messageField;
                                break;

                            case 2:
                                barBay.Accessories.AlphaNumericBar.Field2 = messageField;
                                break;

                            case 3:
                                barBay.Accessories.AlphaNumericBar.Field3 = messageField;
                                break;

                            case 4:
                                barBay.Accessories.AlphaNumericBar.Field4 = messageField;
                                break;

                            case 5:
                                barBay.Accessories.AlphaNumericBar.Field5 = messageField;
                                break;
                        }
                        iField++;
                    }
                }

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
            if (portName is null && isEnabled)
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

        public void UpdateCardReaderSettings(BayNumber bayNumber, bool isEnabled, string tokenRegex, bool isLocal)
        {
            lock (this.dataContext)
            {
                var bay = this.dataContext.Bays
                    .Include(b => b.Accessories)
                    .ThenInclude(a => a.CardReader)
                    .Single(b => b.Number == bayNumber);

                bay.Accessories.CardReader.IsEnabledNew = isEnabled;
                bay.Accessories.CardReader.TokenRegex = tokenRegex;
                bay.Accessories.CardReader.IsLocal = isLocal;

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
            if (portName is null && isEnabled)
            {
                throw new ArgumentNullException(nameof(portName));
            }

            lock (this.dataContext)
            {
                var bay = this.dataContext.Bays
                    .Include(b => b.Accessories)
                    .ThenInclude(a => a.TokenReader)
                    .Single(b => b.Number == bayNumber);

                if (bay.Accessories.TokenReader is null)
                {
                    bay.Accessories.TokenReader = new TokenReader();
                    bay.Accessories.TokenReader.IsEnabledNew = isEnabled;
                    bay.Accessories.TokenReader.PortName = portName;

                    this.dataContext.Accessories.Add(bay.Accessories.TokenReader);
                }
                else
                {
                    bay.Accessories.TokenReader.IsEnabledNew = isEnabled;
                    bay.Accessories.TokenReader.PortName = portName;

                    this.dataContext.Accessories.Update(bay.Accessories.TokenReader);
                }
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

        public void UpdateWeightingScaleSettings(BayNumber bayNumber, bool isEnabled, string ipAddress, int port, WeightingScaleModelNumber modelNumber)
        {
            lock (this.dataContext)
            {
                var bay = this.dataContext.Bays
                    .Include(b => b.Accessories)
                    .ThenInclude(a => a.WeightingScale)
                    .ThenInclude(r => r.DeviceInformation)
                    .Single(b => b.Number == bayNumber);

                if (bay.Accessories.WeightingScale is null)
                {
                    bay.Accessories.WeightingScale = new WeightingScale();
                    bay.Accessories.WeightingScale.IsEnabledNew = isEnabled;
                    bay.Accessories.WeightingScale.IpAddress = IPAddress.Parse(ipAddress);
                    bay.Accessories.WeightingScale.TcpPort = port;
                    this.dataContext.Accessories.Add(bay.Accessories.WeightingScale);
                }
                else
                {
                    bay.Accessories.WeightingScale.IsEnabledNew = isEnabled;
                    bay.Accessories.WeightingScale.IpAddress = IPAddress.Parse(ipAddress);
                    bay.Accessories.WeightingScale.TcpPort = port;
                }

                if (bay.Accessories.WeightingScale.DeviceInformation is null)
                {
                    bay.Accessories.WeightingScale.DeviceInformation = new DeviceInformation();
                }
                bay.Accessories.WeightingScale.DeviceInformation.ModelNumber = modelNumber.ToString();

                this.dataContext.Accessories.Update(bay.Accessories.WeightingScale);
                this.dataContext.SaveChanges();
            }
        }

        #endregion
    }
}
