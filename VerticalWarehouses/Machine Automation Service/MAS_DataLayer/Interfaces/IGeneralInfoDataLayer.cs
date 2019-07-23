﻿using System;
using System.Threading.Tasks;

namespace Ferretto.VW.MAS.DataLayer.Interfaces
{
    public interface IGeneralInfoDataLayer
    {
        #region Properties

        string Address { get; }

        bool AlfaNumBay1 { get; }

        bool AlfaNumBay2 { get; }

        bool AlfaNumBay3 { get; }

        int Barrier1Height { get; }

        int Barrier2Height { get; }

        int Barrier3Height { get; }

        decimal Bay1Height1 { get; }

        decimal Bay1Height2 { get; }

        decimal Bay1Position1 { get; }

        decimal Bay1Position2 { get; }

        int Bay1Type { get; }

        decimal Bay2Height1 { get; }

        decimal Bay2Height2 { get; }

        decimal Bay2Position1 { get; }

        decimal Bay2Position2 { get; }

        int Bay2Type { get; }

        decimal Bay3Height1 { get; }

        decimal Bay3Height2 { get; }

        decimal Bay3Position1 { get; }

        decimal Bay3Position2 { get; }

        int Bay3Type { get; }

        int BaysQuantity { get; }

        string City { get; }

        string ClientCode { get; }

        string ClientName { get; }

        string Country { get; }

        int DrawersQuantity { get; }

        decimal Height { get; }

        DateTime InstallationDate { get; }

        bool LaserBay1 { get; }

        bool LaserBay2 { get; }

        bool LaserBay3 { get; }

        string Latitude { get; }

        string Longitude { get; }

        int MaxAcceptedBai1Height { get; }

        int MaxAcceptedBai2Height { get; }

        int MaxAcceptedBai3Height { get; }

        decimal MaxGrossWeight { get; }

        string Model { get; }

        string Order { get; }

        DateTime ProductionDate { get; }

        string Province { get; }

        string Serial { get; }

        int Shutter1Type { get; }

        int Shutter2Type { get; }

        int Shutter3Type { get; }

        string Zip { get; }

        #endregion
    }
}
