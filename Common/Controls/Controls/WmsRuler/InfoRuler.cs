using System.Windows.Controls;

namespace Ferretto.Common.Controls
{
    public enum OriginHorizontal { Left, Right }

    public enum OriginVertical { Top, Bottom }

    public class InfoRuler
    {
        #region Properties

        public Orientation OrientationRuler { get; set; }

        public OriginHorizontal OriginHorizontal { get; set; }

        public OriginVertical OriginVertical { get; set; }

        #endregion
    }
}
