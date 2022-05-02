using System;
using System.Collections.Generic;
using System.Text;

namespace Ferretto.VW.MAS.InverterDriver.Diagnostics
{
    public class NordFaultCodes : IInverterFaultCodes
    {
        #region Fields

        private readonly Dictionary<int, string> Errors = new Dictionary<int, string>()
        {
            { 0, "No fault" },
            { 10, "Inverter overtemp." },
            { 11, "Intern. inverter temp" },
            { 20, "Motor overtemp. - PTC" },
            { 21, "Motor overtemp.I²t" },
            { 22, "Dig In overtemp" },
            { 30, "Overcurrent I²t lim." },
            { 31, "Overcurrent chopper" },
            { 32, "Overcurrent IGBT" },
            { 33, "Overcurrent IGBT fast" },
            { 34, "Overcurrent chopper" },
            { 37, "Power limit input" },
            { 40, "Module overcurrent" },
            { 41, "Overcurrent measurem." },
            { 50, "Overvoltage Ud" },
            { 51, "Mains overvoltage" },
            { 60, "Charging error" },
            { 61, "Mains low voltage" },
            { 70, "Mains Phase Failure" },
            { 71, "Phasefailure dc-link" },
            { 80, "Parameter loss(maximum EEPROM value exceeded)" },
            { 81, "Inverter ID error" },
            { 84, "Internal EEPROM error(Database version incorrect)" },
            { 87, "EEPROM copy differs" },
            { 90, "Communication error" },
            { 91, "Communication error" },
            { 92, "Communication error" },
            { 93, "Communication error" },
            { 94, "Communication error" },
            { 95, "Communication error" },
            { 96, "Communication error" },
            { 97, "Communication error" },
            { 98, "Communication error" },
            { 99, "Communication error" },
            { 100, "Bus time-out" },
            { 101, "Reserved" },
            { 102, "Bus Time-out XU5" },
            { 103, "Bus Time-out XU5" },
            { 104, "Init-error option" },
            { 105, "System error option" },
            { 106, "Ethernet cable" },
            { 107, "Reserved" },
            { 108, "System bus error" },
            { 109, "Module missingP120" },
            { 110, "Customer terminal" },
            { 120, "External watchdog" },
            { 121, "Limit moto./Customer" },
            { 122, "Limit gen" },
            { 123, "Torque limit" },
            { 124, "Current limit" },
            { 125, "Load monitor" },
            { 128, "Analog in. minimum" },
            { 129, "Analog in. maximum" },
            { 130, "Encoder error" },
            { 131, "Speed slip error" },
            { 132, "Disconnect. control" },
            { 133, "Slipfault encoder" },
            { 135, "Reserved" },
            { 136, "Reserved" },
            { 138, "Limit switch right" },
            { 139, "Limit switch left" },
            { 160, "Motor phase failure" },
            { 161, "Magn. current watch" },
            { 162, "Change phase direct." },
            { 170, "Change assembly grp." },
            { 180, "Safety circuit" },
            { 185, "Safety SS1" },
            { 186, "Safety system" },
            { 190, "Parameter ident" },
            { 191, "Rotor position" },
            { 192, "Rotor pos. North/South" },
            { 900, "System error" },
            { 910, "Update error" },
            { 911, "Update file" },
            { 912, "Update timeout" },
            { 913, "Type update file" },
            { 990, "System error" },
        };

        #endregion

        #region Methods

        public string GetErrorByCode(int code)
        {
            return this.Errors.ContainsKey(code) ? this.Errors[code] : "Unknown error";
        }

        #endregion
    }
}
