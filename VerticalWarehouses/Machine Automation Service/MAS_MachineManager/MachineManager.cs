using System;
using System.Collections.Generic;
using System.Text;

namespace Ferretto.VW.MAS_MachineManager
{
	public class MachineManager
	{
		MachineManager()
		{
		}

		class MachineManagerCreator
		{
			static MachineManagerCreator() { }

			internal static readonly MachineManager uniqueInstance = new MachineManager();
		}

		public static MachineManager Instance
		{
			get { return MachineManagerCreator.uniqueInstance; }
		}
	}
}
