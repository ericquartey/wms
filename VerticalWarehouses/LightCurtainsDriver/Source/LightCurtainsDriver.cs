using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ferretto.VW.Utils;


namespace Ferretto.VW.LightCurtainsDriver
{
    /// <summary>
    /// Photolight curtains manager class.
    /// The ReeR Light Curtains is connected to the PanelPC via USB.
    /// </summary>
    public class CLightCurtains : ILightCurtainsDriver, IDriverBase, IDisposable
    {
        // TODO Add your implementation code here


        /// <summary>
        /// Default c-tor.
        /// </summary>
        public CLightCurtains()
        {
            // TODO Add your implementation code here
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool Initialize()
        {
            // TODO Add your implementation code here
            return true;
        }


        /// <summary>
        /// 
        /// </summary>
        public void Terminate()
        {
            // TODO Add your implementation code here
        }


        /// <summary>
        /// Release resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // TODO Add your implementation code here
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="height"></param>
        /// <returns></returns>
        public bool GetHeight(out float height)
        {
            // TODO Add your implementation code here
            height = 42.0f;

            return true;
        }
    }
}
