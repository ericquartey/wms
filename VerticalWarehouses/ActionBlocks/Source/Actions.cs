using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ferretto.VW.Utils;
using Ferretto.VW.InverterDriver;

namespace Ferretto.VW.ActionBlocks
{
  public class CActions
  {
    private readonly int m_myItem;

    public CActions()
    {
      this.m_myItem = 42;

      CInverterDriver myDriver = new CInverterDriver();
      myDriver.Initialize("My string");

    }

  }
}
