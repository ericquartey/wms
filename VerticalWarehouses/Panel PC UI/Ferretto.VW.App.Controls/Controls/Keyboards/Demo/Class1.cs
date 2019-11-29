using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace Ferretto.VW.App.Controls.Controls.Keyboards.Demo
{
    internal class Class1
    {
        #region Constructors

        public Class1()
        {
            XmlSerializer ser = new XmlSerializer(typeof(Demo.KeyboardDefinition));

            string filename = Path.Combine(
                @"C:\Source\WMS - Work\VerticalWarehouses\Panel PC UI\Ferretto.VW.App.Controls\Controls\Keyboards\Demo\",
                @"tr_TR.xml");

            Demo.KeyboardDefinition myk = ser.Deserialize(new FileStream(filename, FileMode.Open)) as Demo.KeyboardDefinition;

            if (myk != null)
            {
                var k = JsonConvert.SerializeObject(myk);
                var items = JsonConvert.DeserializeObject<Demo.KeyboardDefinition>(k);
            }
        }

        #endregion
    }
}
