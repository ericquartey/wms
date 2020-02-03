using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.VW.App.Scaffolding.Exceptions
{
    public class ScaffoldingException : Exception
    {
        public ScaffoldingException(string message) : base(message)
        {
        }
    }
}
