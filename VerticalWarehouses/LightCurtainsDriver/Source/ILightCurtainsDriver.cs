using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ferretto.VW.Utils;


namespace Ferretto.VW.LightCurtainsDriver
{
    public interface ILightCurtainsDriver
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        bool Initialize();

        /// <summary>
        /// 
        /// </summary>
        void Terminate();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        bool GetHeight(out float height);


    }
}
