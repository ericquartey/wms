using System.Collections.Generic;
using System.Net;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.Utils.Enumerations;

namespace Ferretto.VW.MAS.DataLayer.Providers.Interfaces
{
    public interface IBaysProvider
    {


        #region Properties

        int BayInverterPosition { get; }

        int ShutterInverterPosition { get; }

        #endregion



        #region Methods

        Bay Activate(BayIndex bayIndex);

        Bay AssignMissionOperation(BayIndex bayIndex, int? missionId, int? missionOperationId);

        void Create(Bay bay);

        Bay Deactivate(BayIndex bayIndex);

        IEnumerable<Bay> GetAll();

        Bay GetByIndex(BayIndex bayIndex);

        BayIndex GetByInverterIndex(InverterIndex inverterIndex);

        BayIndex GetByIoIndex(IoIndex ioIndex);

        Bay GetByIpAddress(IPAddress remoteIpAddress);

        BayIndex GetByMovementType(IPositioningMessageData data);

        InverterIndex GetInverterIndexByMovementType(IPositioningMessageData data, BayIndex bayIndex);

        List<InverterIndex> GetInverterList(BayIndex bayIndex);

        IoIndex GetIoDevice(BayIndex bayIndex);

        void Update(BayIndex bayIndex, string ipAddress, BayType bayType);

        Bay UpdatePosition(int bayNumber, int position, decimal height);

        #endregion
    }
}
