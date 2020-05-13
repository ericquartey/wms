using System;
using System.Collections.Generic;
using System.Text;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.InvertersParametersGenerator.Models
{
    public class InverterParametersDataInfo : InverterParametersData
    {
        public InverterParametersDataInfo(InverterType inverterType, byte inverterIndex, string description) : base(inverterIndex, description)
        {
            this.Type = inverterType;
        }

        public InverterType Type { get; }
    }
}
