using System;
using System.Threading.Tasks;

namespace Ferretto.VW.MAS_DataLayer.Interfaces
{
    public interface IGeneralInfoConfigurationDataLayer
    {
        #region Properties

        Task<string> Address { get; }

        Task<bool> AlfaNumBay1 { get; }

        Task<bool> AlfaNumBay2 { get; }

        Task<bool> AlfaNumBay3 { get; }

        Task<int> Barrier1Height { get; }

        Task<int> Barrier2Height { get; }

        Task<int> Barrier3Height { get; }

        Task<decimal> Bay1Height1 { get; }

        Task<decimal> Bay1Height2 { get; }

        Task<decimal> Bay1Position1 { get; }

        Task<decimal> Bay1Position2 { get; }

        Task<int> Bay1Type { get; }

        Task<decimal> Bay2Height1 { get; }

        Task<decimal> Bay2Height2 { get; }

        Task<decimal> Bay2Position1 { get; }

        Task<decimal> Bay2Position2 { get; }

        Task<int> Bay2Type { get; }

        Task<decimal> Bay3Height1 { get; }

        Task<decimal> Bay3Height2 { get; }

        Task<decimal> Bay3Position1 { get; }

        Task<decimal> Bay3Position2 { get; }

        Task<int> Bay3Type { get; }

        Task<int> BaysQuantity { get; }

        Task<string> City { get; }

        Task<string> ClientCode { get; }

        Task<string> ClientName { get; }

        Task<string> Country { get; }

        Task<int> DrawersQuantity { get; }

        Task<decimal> Height { get; }

        Task<DateTime> InstallationDate { get; }

        Task<bool> LaserBay1 { get; }

        Task<bool> LaserBay2 { get; }

        Task<bool> LaserBay3 { get; }

        Task<string> Latitude { get; }

        Task<string> Longitude { get; }

        Task<int> MaxAcceptedBai1Height { get; }

        Task<int> MaxAcceptedBai2Height { get; }

        Task<int> MaxAcceptedBai3Height { get; }

        Task<decimal> MaxWeight { get; }

        Task<string> Model { get; }

        Task<string> Order { get; }

        Task<DateTime> ProductionDate { get; }

        Task<string> Province { get; }

        Task<string> Serial { get; }

        Task<int> Shutter1Type { get; }

        Task<int> Shutter2Type { get; }

        Task<int> Shutter3Type { get; }

        Task<string> Zip { get; }

        #endregion
    }
}
