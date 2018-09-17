using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ferretto.VW.Utils;


namespace Ferretto.VW.InverterDriver
{
  public class CInverterDriver : IDriver
  {

  private CMyClass m_myClass;

    public CInverterDriver()
    {
      // TODO
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="szInitialMsg"></param>
    /// <returns></returns>
    public int Initialize(string szInitialMsg)
    {
      this.m_myClass = new CMyClass();

      // TODO Add your implementation code here

      this.m_myClass = null;

      return 0;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bool MyFunc()
    {
      return true;
    }

  }
}
