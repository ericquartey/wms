using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Ferretto.VW.Utils
{

    /// <summary>
    /// Base interface.
    /// IDriverBase interface is inherited by all device interface. 
    /// </summary>
    public interface IDriverBase
    {
        //! Initialize the device resources.
        bool Initialize();

        //! Terminate and release the device resources.
        void Terminate();

    }  // interface IDriverBase

        // Creo una Queue che salva i comandi ricevuti come bytes
        //public Queue<byte[]> Commands;

    public class Utils
    { }

}
