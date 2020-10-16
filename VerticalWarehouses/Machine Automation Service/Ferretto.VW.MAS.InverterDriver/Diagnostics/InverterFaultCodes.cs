using System;
using System.Collections.Generic;
using System.Text;

namespace Ferretto.VW.MAS.InverterDriver.Diagnostics
{
    public static class InverterFaultCodes
    {
        #region Fields

        private static readonly Dictionary<int, string> Errors = new Dictionary<int, string>()
        {
            { 0, "No fault" },
            // Overload
            { 0x0100, "Frequency inverter overloaded." },
            { 0x0102, "Frequency inverter overloaded (60 s), check load behavior." },
            { 0x0103, "Short-term overload (1 s), check motor and application parameters." },
            // Heat Sink
            { 0x0200, "Heat sink temperature too high, check cooling and fan." },
            { 0x0201, "Temperature sensor defective or ambient temperature too low." },
            // Inside
            { 0x0300, "Inside temperature too high, check cooling and fan." },
            { 0x0301, "Inside temperature too low, check electrical cabinet heating." },
            // Motor Connection
            { 0x0400, "Motor temperature too high or sensor defective, check connection S6IND." },
            { 0x0401, "Motor protection switch tripped, check drive." },
            { 0x0402, "V-belt monitoring reports no load on the drive." },
            { 0x0403, "Phase failure, check motor and wiring." },
            { 0x0404, "Blocked movement. Check motor and gears." },
            { 0x0405, "Start monitoring. Check brake & limiting parameters at start, like in example Current limit 728, Intelligent currents, etc." },
            // Output current
            { 0x0500, "Overloaded, check load situation and ramps." },
            { 0x0501, "Instantaneous output current value too high. Check load." },
            { 0x0502, "Dynamic Phase current limitation. Check load." },
            { 0x0503, "Short circuit or earth fault, check motor and wiring." },
            { 0x0504, "Overloaded, check load situation and current value limit controller." },
            { 0x0505, "Asymmetric motor current, check current and wiring." },
            { 0x0506, "Motor phase current too high, check motor and wiring." },
            { 0x0507, "Message from phase monitoring, check motor and wiring." },
            // DC –Link Voltage
            { 0x0700, "DC link voltage too high, check deceleration ramps and connected brake resistor." },
            { 0x0701, "DC link voltage too low, check mains voltage." },
            { 0x0702, "Power failure, check mains voltage and circuit." },
            { 0x0703, "Phase failure, check mains fuses and circuit." },
            { 0x0704, "Reference DC link limitation 680 too small, check mains voltage." },
            { 0x0705, "Brake chopper Trigger threshold 506 too small, check mains voltage." },
            { 0x0706, "Motor chopper Trigger threshold 507 too small, check mains voltage." },
            // Electronics voltage
            { 0x0801, "Electronics voltage DC 24 V too low, check control terminal." },
            { 0x0804, "Electronics voltage too high, check wiring of control terminals." },
            // Brake chopper
            { 0x1010, "Brake Chopper Overcurrent; refer to chapter 19.4 “Brake Chopper and Brake Resistance”." },
            // overrun
            { 0x0C13, "1ms-Task list overrun. Switch off power and switch on again after a few seconds." },
            // Output frequency
            { 0x1100, "Output frequency too high, check control signals and settings." },
            { 0x1101, "Max. frequency reached by control, check deceleration ramps and connected brake resistor." },
            { 0x1110, "Overspeed. Check Application manual “Crane drives”." },
            // Safety function STO
            { 0x1201, "Diagnosis error of function STO; at least one of the shut-down paths STOA and STOB is defective. Check units connected to shut-down paths; check cabling and EMC." },
            { 0x1204, "Software self-diagnosis has detected an internal error. Consult BONFIGLIOLI customer service." },
            { 0x1205, "Fault message of 5-second monitoring. Shut-down paths STOA and STOB were not actuated at the same time, but with an offset of more than 5 seconds. Check addressing of shut-down paths or control of protective circuitry." },
            // Motor Connection
            { 0x1300, "Earth fault on output, check motor and wiring." },
            { 0x1301, "Set IDC compensation limit 415 reached, check motor and cabling, increase limit, if necessary." },
            { 0x1310, "Minimum current monitoring, check motor and wiring." },
            // Control Connection
            { 0x1401, "Reference value on multifunctional input 1 faulty, check signal." },
            { 0x1402, "Reference value MF4IA faulty, check signal." },
            { 0x1407, "Overcurrent on multifunctional input 1, check signal." },
            { 0x1421, "Resolver fault. Check resolver connection and speed." },
            { 0x1422, "Resolver counting fault. Check resolver connection." },
            { 0x1423, "Resolver pole pair number incorrect. Check parameter of pole pairs." },
            { 0x1424, "Resolver connection fault. Check resolver connection." },
            { 0x1430, "Encoder signal defective, check connections S4IND and S5IND." },
            { 0x1431, "One track of the speed sensor signal is missing, check connections." },
            { 0x1432, "Direction of rotation of speed sensor wrong, check connections." },
            { 0x1433, "Encoder 2: Division Marks Fault. Check encoder settings." },
            { 0x1434, "Too less Division Marks Fault. Check encoder settings." },
            { 0x1435, "Too many Division Marks Fault. Check encoder settings." },
            { 0x1436, "Encoder 1: Division Marks Fault. Correct Division Marks 491 of encoder 1; refer to chapter 11.4.2 “Division marks, speed sensor 1”." },
            { 0x1437, "The encoder is disabled. In configurations 210, 211 and 230 an encoder must be activated. Set parameter Operation Mode 490 to an evaluation mode (not to “0 – off). If an expansion module is installed and parameter Actual Speed source 766 is set to “2 – Speed Sensor 2“, parameter Operation Mode 493 (speed sensor 2) must be set to an evaluation mode." },
            { 0x1450, "KTY Temperature Measurement Failure. Check KTY connection." },
            { 0x1454, "External error; drive responded according to parameter setting for Operation mode ext. error 535. Error was triggered via the logic signal or digital input signal assigned to parameter External error 183." },
            // Positioning
            // TODO: Finish coding of errors (positioning manual required)
        };

        #endregion

        #region Methods

        public static string GetErrorByCode(int code)
        {
            return Errors.ContainsKey(code) ? Errors[code] : "Unknown error";
        }

        #endregion
    }
}
