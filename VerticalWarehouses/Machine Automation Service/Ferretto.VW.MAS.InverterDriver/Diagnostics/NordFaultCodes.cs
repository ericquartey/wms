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
            { 10, "1.0 Inverter overtemp." },
            { 11, "1.1 Intern. inverter temp" },
            { 20, "2.0 Motor overtemp. - PTC" },
            { 21, "2.1 Motor overtemp.I²t" },
            { 22, "2.2 Dig In overtemp" },
            { 30, "3.0 Overcurrent I²t lim." },
            { 31, "3.1 Overcurrent chopper" },
            { 32, "3.2 Overcurrent IGBT" },
            { 33, "3.3 Overcurrent IGBT fast" },
            { 34, "3.4 Overcurrent chopper" },
            { 37, "3.7 Power limit input" },
            { 40, "4.0 Module overcurrent" },
            { 41, "4.1 Overcurrent measurem." },
            { 50, "5.0 Overvoltage Ud" },
            { 51, "5.1 Mains overvoltage" },
            { 60, "6.0 Charging error" },
            { 61, "6.1 Mains low voltage" },
            { 70, "7.0 Mains Phase Failure" },
            { 71, "7.1 Phasefailure dc-link" },
            { 80, "8.0 Parameter loss(maximum EEPROM value exceeded)" },
            { 81, "8.1 Inverter ID error" },
            { 84, "8.4 Internal EEPROM error(Database version incorrect)" },
            { 87, "8.7 EEPROM copy differs" },
            { 90, "9.0 Communication error" },
            { 91, "9.1 Communication error" },
            { 92, "9.2 Communication error" },
            { 93, "9.3 Communication error" },
            { 94, "9.4 Communication error" },
            { 95, "9.5 Communication error" },
            { 96, "9.6 Communication error" },
            { 97, "9.7 Communication error" },
            { 98, "9.8 Communication error" },
            { 99, "9.9 Communication error" },
            { 100, "10.0 Bus time-out" },
            { 101, "10.1 Reserved" },
            { 102, "10.2 Bus Time-out XU5" },
            { 103, "10.3 Bus Time-out XU5" },
            { 104, "10.4 Init-error option" },
            { 105, "10.5 System error option" },
            { 106, "10.6 Ethernet cable" },
            { 107, "10.7 Reserved" },
            { 108, "10.8 System bus error" },
            { 109, "10.9 Module missingP120" },
            { 110, "11.0 Customer terminal" },
            { 120, "12.0 External watchdog" },
            { 121, "12.1 Limit moto./Customer" },
            { 122, "12.2 Limit gen" },
            { 123, "12.3 Torque limit" },
            { 124, "12.4 Current limit" },
            { 125, "12.5 Load monitor" },
            { 128, "12.8 Analog in. minimum" },
            { 129, "12.9 Analog in. maximum" },
            { 130, "13.0 Encoder error" },
            { 131, "13.1 Speed slip error" },
            { 132, "13.2 Disconnect. control" },
            { 133, "13.3 Slipfault encoder" },
            { 135, "13.5 Reserved" },
            { 136, "13.6 Reserved" },
            { 138, "13.8 Limit switch right" },
            { 139, "13.9 Limit switch left" },
            { 160, "16.0 Motor phase failure" },
            { 161, "16.1 Magn. current watch" },
            { 162, "16.2 Change phase direct." },
            { 170, "17.0 Change assembly grp." },
            { 180, "18.0 Safety circuit" },
            { 185, "18.5 Safety SS1" },
            { 186, "18.6 Safety system" },
            { 190, "19.0 Parameter ident" },
            { 191, "19.1 Rotor position" },
            { 192, "19.2 Rotor pos. North/South" },
            { 900, "90.0 System error" },
            { 910, "91.0 Update error" },
            { 911, "91.1 Update file" },
            { 912, "91.2 Update timeout" },
            { 913, "91.3 Type update file" },
            { 990, "99.0 System error" },
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
