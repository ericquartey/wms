using System;
using System.Collections.Generic;
using System.Text;

namespace Ferretto.VW.MAS.InverterDriver.Contracts
{
    public class InverterBlockDefinition
    {
        #region Constructors

        public InverterBlockDefinition(InverterIndex systemIndex, InverterParameterId parameterId, InverterDataset dataSetIndex = InverterDataset.ActualDataset)
        {
            this.SystemIndex = systemIndex;
            this.ParameterId = parameterId;
            this.DataSetIndex = dataSetIndex;
        }

        #endregion

        #region Properties

        public InverterDataset DataSetIndex { get; set; }

        public InverterParameterId ParameterId { get; set; }

        public InverterIndex SystemIndex { get; set; }

        #endregion
    }
}
