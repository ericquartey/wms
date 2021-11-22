using Ferretto.VW.CommonUtils.Enumerations;
using Prism.Mvvm;

namespace Ferretto.VW.App.Services
{
    public class Sensors : BindableBase
    {
        #region Fields

        private bool aCUBay1EMS1IND;

        private bool aCUBay1MF1ID;

        private bool aCUBay1S2IND;

        private bool aCUBay1S3IND;

        private bool aCUBay1S4IND;

        private bool aCUBay1S5IND;

        private bool aCUBay1S6IND;

        private bool aCUBay1STO;

        private bool aCUBay2EMS1IND;

        private bool aCUBay2MF1ID;

        private bool aCUBay2S2IND;

        private bool aCUBay2S3IND;

        private bool aCUBay2S4IND;

        private bool aCUBay2S5IND;

        private bool aCUBay2S6IND;

        private bool aCUBay2STO;

        private bool aCUBay3EMS1IND;

        private bool aCUBay3MF1ID;

        private bool aCUBay3S2IND;

        private bool aCUBay3S3IND;

        private bool aCUBay3S4IND;

        private bool aCUBay3S5IND;

        private bool aCUBay3S6IND;

        private bool aCUBay3STO;

        private bool aGLFree1Bay1;

        private bool aGLFree1Bay2;

        private bool aGLFree1Bay3;

        private bool aGLFree2Bay1;

        private bool aGLFree2Bay2;

        private bool aGLFree2Bay3;

        private bool aGLFree3Bay1;

        private bool aGLFree3Bay2;

        private bool aGLFree3Bay3;

        private bool aGLFree4Bay1;

        private bool aGLFree4Bay2;

        private bool aGLFree4Bay3;

        private bool aGLNormalFunctionBay1;

        private bool aGLNormalFunctionBay2;

        private bool aGLNormalFunctionBay3;

        private bool aGLPowerOnOffBay1;

        private bool aGLPowerOnOffBay2;

        private bool aGLPowerOnOffBay3;

        private bool aGLSensorAShutterBay1;

        private bool aGLSensorAShutterBay2;

        private bool aGLSensorAShutterBay3;

        private bool aGLSensorBShutterBay1;

        private bool aGLSensorBShutterBay2;

        private bool aGLSensorBShutterBay3;

        private bool aNGChainMF2ID;

        private bool aNGChainMF3ID;

        private bool aNGChainPowerOnOff;

        private bool aNGChainS2IND;

        private bool aNGChainS3IND;

        private bool aNGChainS4IND;

        private bool aNGChainS5IND;

        private bool aNGChainS6IND;

        private bool antiIntrusionBarrier2Bay1;

        private bool antiIntrusionBarrier2Bay2;

        private bool antiIntrusionBarrier2Bay3;

        private bool antiIntrusionBarrierBay1;

        private bool antiIntrusionBarrierBay2;

        private bool antiIntrusionBarrierBay3;

        private bool cradleEngineSelectedBay1;

        private bool elevatorEngineSelectedBay1;

        private bool elevatorOverrun;

        private bool fastStop;

        private bool free1Bay1;

        private bool free1Bay2;

        private bool free1Bay3;

        private bool free2Bay1;

        private bool free2Bay2;

        private bool free2Bay3;

        private bool free3Bay2;

        private bool free3Bay3;

        private bool free4Bay2;

        private bool free4Bay3;

        private bool free5Bay2;

        private bool free5Bay3;

        private bool free6Bay2;

        private bool free6Bay3;

        private bool free7Bay2;

        private bool free7Bay3;

        private bool freeAng1;

        private bool freeAng2;

        private bool freeAng3;

        private bool inverterInFault1;

        private bool lUPresentInBay1;

        private bool lUPresentInBay2;

        private bool lUPresentInBay3;

        private bool luPresentInMachineSideBay1;

        private bool luPresentInOperatorSideBay1;

        private bool lUPresentMiddleBottomBay1;

        private bool lUPresentMiddleBottomBay2;

        private bool lUPresentMiddleBottomBay3;

        private bool microCarterLeftSideBay1;

        private bool microCarterRightSideBay1;

        private bool microCarterLeftSideBay2;

        private bool microCarterRightSideBay2;

        private bool microCarterLeftSideBay3;

        private bool microCarterRightSideBay3;

        private bool mushroomEmergencyButtonBay1;

        private bool mushroomEmergencyButtonBay2;

        private bool mushroomEmergencyButtonBay3;

        private bool noValue;

        private bool powerOnOff;

        private bool profileCalibrationBay1;

        private bool profileCalibrationBay2;

        private bool profileCalibrationBay3;

        private bool robotOptionBay1;

        private bool robotOptionBay2;

        private bool robotOptionBay3;

        private bool runningState;

        private bool trolleyOptionBay1;

        private bool trolleyOptionBay2;

        private bool trolleyOptionBay3;

        private bool zeroPawlSensor;

        private bool zeroPawlSensorOneTon;

        private bool zeroVerticalSensor;

        #endregion

        #region Properties

        public bool ACUBay1EMS1IND { get => this.aCUBay1EMS1IND; set => this.SetProperty(ref this.aCUBay1EMS1IND, value); }

        public bool ACUBay1MF1ID { get => this.aCUBay1MF1ID; set => this.SetProperty(ref this.aCUBay1MF1ID, value); }

        public bool ACUBay1S2IND { get => this.aCUBay1S2IND; set => this.SetProperty(ref this.aCUBay1S2IND, value); }

        public bool ACUBay1S3IND { get => this.aCUBay1S3IND; set => this.SetProperty(ref this.aCUBay1S3IND, value); }

        public bool ACUBay1S4IND { get => this.aCUBay1S4IND; set => this.SetProperty(ref this.aCUBay1S4IND, value); }

        public bool ACUBay1S5IND { get => this.aCUBay1S5IND; set => this.SetProperty(ref this.aCUBay1S5IND, value); }

        public bool ACUBay1S6IND { get => this.aCUBay1S6IND; set => this.SetProperty(ref this.aCUBay1S6IND, value); }

        public bool ACUBay1STO { get => this.aCUBay1STO; set => this.SetProperty(ref this.aCUBay1STO, value); }

        public bool ACUBay2EMS1IND { get => this.aCUBay2EMS1IND; set => this.SetProperty(ref this.aCUBay2EMS1IND, value); }

        public bool ACUBay2MF1ID { get => this.aCUBay2MF1ID; set => this.SetProperty(ref this.aCUBay2MF1ID, value); }

        public bool ACUBay2S2IND { get => this.aCUBay2S2IND; set => this.SetProperty(ref this.aCUBay2S2IND, value); }

        public bool ACUBay2S3IND { get => this.aCUBay2S3IND; set => this.SetProperty(ref this.aCUBay2S3IND, value); }

        public bool ACUBay2S4IND { get => this.aCUBay2S4IND; set => this.SetProperty(ref this.aCUBay2S4IND, value); }

        public bool ACUBay2S5IND { get => this.aCUBay2S5IND; set => this.SetProperty(ref this.aCUBay2S5IND, value); }

        public bool ACUBay2S6IND { get => this.aCUBay2S6IND; set => this.SetProperty(ref this.aCUBay2S6IND, value); }

        public bool ACUBay2STO { get => this.aCUBay2STO; set => this.SetProperty(ref this.aCUBay2STO, value); }

        public bool ACUBay3EMS1IND { get => this.aCUBay3EMS1IND; set => this.SetProperty(ref this.aCUBay3EMS1IND, value); }

        public bool ACUBay3MF1ID { get => this.aCUBay3MF1ID; set => this.SetProperty(ref this.aCUBay3MF1ID, value); }

        public bool ACUBay3S2IND { get => this.aCUBay3S2IND; set => this.SetProperty(ref this.aCUBay3S2IND, value); }

        public bool ACUBay3S3IND { get => this.aCUBay3S3IND; set => this.SetProperty(ref this.aCUBay3S3IND, value); }

        public bool ACUBay3S4IND { get => this.aCUBay3S4IND; set => this.SetProperty(ref this.aCUBay3S4IND, value); }

        public bool ACUBay3S5IND { get => this.aCUBay3S5IND; set => this.SetProperty(ref this.aCUBay3S5IND, value); }

        public bool ACUBay3S6IND { get => this.aCUBay3S6IND; set => this.SetProperty(ref this.aCUBay3S6IND, value); }

        public bool ACUBay3STO { get => this.aCUBay3STO; set => this.SetProperty(ref this.aCUBay3STO, value); }

        public bool AGLFree1Bay1 { get => this.aGLFree1Bay1; set => this.SetProperty(ref this.aGLFree1Bay1, value); }

        public bool AGLFree1Bay2 { get => this.aGLFree1Bay2; set => this.SetProperty(ref this.aGLFree1Bay2, value); }

        public bool AGLFree1Bay3 { get => this.aGLFree1Bay3; set => this.SetProperty(ref this.aGLFree1Bay3, value); }

        public bool AGLFree2Bay1 { get => this.aGLFree2Bay1; set => this.SetProperty(ref this.aGLFree2Bay1, value); }

        public bool AGLFree2Bay2 { get => this.aGLFree2Bay2; set => this.SetProperty(ref this.aGLFree2Bay2, value); }

        public bool AGLFree2Bay3 { get => this.aGLFree2Bay3; set => this.SetProperty(ref this.aGLFree2Bay3, value); }

        public bool AGLFree3Bay1 { get => this.aGLFree3Bay1; set => this.SetProperty(ref this.aGLFree3Bay1, value); }

        public bool AGLFree3Bay2 { get => this.aGLFree3Bay2; set => this.SetProperty(ref this.aGLFree3Bay2, value); }

        public bool AGLFree3Bay3 { get => this.aGLFree3Bay3; set => this.SetProperty(ref this.aGLFree3Bay3, value); }

        public bool AGLFree4Bay1 { get => this.aGLFree4Bay1; set => this.SetProperty(ref this.aGLFree4Bay1, value); }

        public bool AGLFree4Bay2 { get => this.aGLFree4Bay2; set => this.SetProperty(ref this.aGLFree4Bay2, value); }

        public bool AGLFree4Bay3 { get => this.aGLFree4Bay3; set => this.SetProperty(ref this.aGLFree4Bay3, value); }

        public bool AGLNormalFunctionBay1 { get => this.aGLNormalFunctionBay1; set => this.SetProperty(ref this.aGLNormalFunctionBay1, value); }

        public bool AGLNormalFunctionBay2 { get => this.aGLNormalFunctionBay2; set => this.SetProperty(ref this.aGLNormalFunctionBay2, value); }

        public bool AGLNormalFunctionBay3 { get => this.aGLNormalFunctionBay3; set => this.SetProperty(ref this.aGLNormalFunctionBay3, value); }

        public bool AGLPowerOnOffBay1 { get => this.aGLPowerOnOffBay1; set => this.SetProperty(ref this.aGLPowerOnOffBay1, value); }

        public bool AGLPowerOnOffBay2 { get => this.aGLPowerOnOffBay2; set => this.SetProperty(ref this.aGLPowerOnOffBay2, value); }

        public bool AGLPowerOnOffBay3 { get => this.aGLPowerOnOffBay3; set => this.SetProperty(ref this.aGLPowerOnOffBay3, value); }

        public bool AGLSensorAShutterBay1 { get => this.aGLSensorAShutterBay1; set => this.SetProperty(ref this.aGLSensorAShutterBay1, value); }

        public bool AGLSensorAShutterBay2 { get => this.aGLSensorAShutterBay2; set => this.SetProperty(ref this.aGLSensorAShutterBay2, value); }

        public bool AGLSensorAShutterBay3 { get => this.aGLSensorAShutterBay3; set => this.SetProperty(ref this.aGLSensorAShutterBay3, value); }

        public bool AGLSensorBShutterBay1 { get => this.aGLSensorBShutterBay1; set => this.SetProperty(ref this.aGLSensorBShutterBay1, value); }

        public bool AGLSensorBShutterBay2 { get => this.aGLSensorBShutterBay2; set => this.SetProperty(ref this.aGLSensorBShutterBay2, value); }

        public bool AGLSensorBShutterBay3 { get => this.aGLSensorBShutterBay3; set => this.SetProperty(ref this.aGLSensorBShutterBay3, value); }

        public bool ANGChainMF2ID { get => this.aNGChainMF2ID; set => this.SetProperty(ref this.aNGChainMF2ID, value); }

        public bool ANGChainMF3ID { get => this.aNGChainMF3ID; set => this.SetProperty(ref this.aNGChainMF3ID, value); }

        public bool ANGChainPowerOnOff { get => this.aNGChainPowerOnOff; set => this.SetProperty(ref this.aNGChainPowerOnOff, value); }

        public bool ANGChainS2IND { get => this.aNGChainS2IND; set => this.SetProperty(ref this.aNGChainS2IND, value); }

        public bool ANGChainS3IND { get => this.aNGChainS3IND; set => this.SetProperty(ref this.aNGChainS3IND, value); }

        public bool ANGChainS4IND { get => this.aNGChainS4IND; set => this.SetProperty(ref this.aNGChainS4IND, value); }

        public bool ANGChainS5IND { get => this.aNGChainS5IND; set => this.SetProperty(ref this.aNGChainS5IND, value); }

        public bool ANGChainS6IND { get => this.aNGChainS6IND; set => this.SetProperty(ref this.aNGChainS6IND, value); }

        public bool AntiIntrusionBarrier2Bay1 { get => this.antiIntrusionBarrier2Bay1; set => this.SetProperty(ref this.antiIntrusionBarrier2Bay1, value); }

        public bool AntiIntrusionBarrier2Bay2 { get => this.antiIntrusionBarrier2Bay2; set => this.SetProperty(ref this.antiIntrusionBarrier2Bay2, value); }

        public bool AntiIntrusionBarrier2Bay3 { get => this.antiIntrusionBarrier2Bay3; set => this.SetProperty(ref this.antiIntrusionBarrier2Bay3, value); }

        public bool AntiIntrusionBarrierBay1 { get => this.antiIntrusionBarrierBay1; set => this.SetProperty(ref this.antiIntrusionBarrierBay1, value); }

        public bool AntiIntrusionBarrierBay2 { get => this.antiIntrusionBarrierBay2; set => this.SetProperty(ref this.antiIntrusionBarrierBay2, value); }

        public bool AntiIntrusionBarrierBay3 { get => this.antiIntrusionBarrierBay3; set => this.SetProperty(ref this.antiIntrusionBarrierBay3, value); }

        public bool CradleEngineSelectedBay1 { get => this.cradleEngineSelectedBay1; set => this.SetProperty(ref this.cradleEngineSelectedBay1, value); }

        public bool ElevatorEngineSelectedBay1 { get => this.elevatorEngineSelectedBay1; set => this.SetProperty(ref this.elevatorEngineSelectedBay1, value); }

        public bool ElevatorOverrun { get => this.elevatorOverrun; set => this.SetProperty(ref this.elevatorOverrun, value); }

        public bool FastStop { get => this.fastStop; set => this.SetProperty(ref this.fastStop, value); }

        public bool Free1Bay1 { get => this.free1Bay1; set => this.SetProperty(ref this.free1Bay1, value); }

        public bool Free1Bay2 { get => this.free1Bay2; set => this.SetProperty(ref this.free1Bay2, value); }

        public bool Free1Bay3 { get => this.free1Bay3; set => this.SetProperty(ref this.free1Bay3, value); }

        public bool Free2Bay1 { get => this.free2Bay1; set => this.SetProperty(ref this.free2Bay1, value); }

        public bool Free2Bay2 { get => this.free2Bay2; set => this.SetProperty(ref this.free2Bay2, value); }

        public bool Free2Bay3 { get => this.free2Bay3; set => this.SetProperty(ref this.free2Bay3, value); }

        public bool Free3Bay2 { get => this.free3Bay2; set => this.SetProperty(ref this.free3Bay2, value); }

        public bool Free3Bay3 { get => this.free3Bay3; set => this.SetProperty(ref this.free3Bay3, value); }

        public bool Free4Bay2 { get => this.free4Bay2; set => this.SetProperty(ref this.free4Bay2, value); }

        public bool Free4Bay3 { get => this.free4Bay3; set => this.SetProperty(ref this.free4Bay3, value); }

        public bool Free5Bay2 { get => this.free5Bay2; set => this.SetProperty(ref this.free5Bay2, value); }

        public bool Free5Bay3 { get => this.free5Bay3; set => this.SetProperty(ref this.free5Bay3, value); }

        public bool Free6Bay2 { get => this.free6Bay2; set => this.SetProperty(ref this.free6Bay2, value); }

        public bool Free6Bay3 { get => this.free6Bay3; set => this.SetProperty(ref this.free6Bay3, value); }

        public bool Free7Bay2 { get => this.free7Bay2; set => this.SetProperty(ref this.free7Bay2, value); }

        public bool Free7Bay3 { get => this.free7Bay3; set => this.SetProperty(ref this.free7Bay3, value); }

        public bool FreeAng1 { get => this.freeAng1; set => this.SetProperty(ref this.freeAng1, value); }

        public bool FreeAng2 { get => this.freeAng2; set => this.SetProperty(ref this.freeAng2, value); }

        public bool FreeAng3 { get => this.freeAng3; set => this.SetProperty(ref this.freeAng3, value); }

        public bool InverterInFault1 { get => this.inverterInFault1; set => this.SetProperty(ref this.inverterInFault1, value); }

        public bool LUPresentInBay1 { get => this.lUPresentInBay1; set => this.SetProperty(ref this.lUPresentInBay1, value); }

        public bool LUPresentInBay2 { get => this.lUPresentInBay2; set => this.SetProperty(ref this.lUPresentInBay2, value); }

        public bool LUPresentInBay3 { get => this.lUPresentInBay3; set => this.SetProperty(ref this.lUPresentInBay3, value); }

        public bool LuPresentInMachineSide { get => this.luPresentInMachineSideBay1; set => this.SetProperty(ref this.luPresentInMachineSideBay1, value); }

        public bool LuPresentInOperatorSide { get => this.luPresentInOperatorSideBay1; set => this.SetProperty(ref this.luPresentInOperatorSideBay1, value); }

        public bool LUPresentMiddleBottomBay1 { get => this.lUPresentMiddleBottomBay1; set => this.SetProperty(ref this.lUPresentMiddleBottomBay1, value); }

        public bool LUPresentMiddleBottomBay2 { get => this.lUPresentMiddleBottomBay2; set => this.SetProperty(ref this.lUPresentMiddleBottomBay2, value); }

        public bool LUPresentMiddleBottomBay3 { get => this.lUPresentMiddleBottomBay3; set => this.SetProperty(ref this.lUPresentMiddleBottomBay3, value); }

        public bool MicroCarterLeftSideBay1 { get => this.microCarterLeftSideBay1; set => this.SetProperty(ref this.microCarterLeftSideBay1, value); }

        public bool MicroCarterRightSideBay1 { get => this.microCarterRightSideBay1; set => this.SetProperty(ref this.microCarterRightSideBay1, value); }

        public bool MicroCarterLeftSideBay2 { get => this.microCarterLeftSideBay2; set => this.SetProperty(ref this.microCarterLeftSideBay2, value); }

        public bool MicroCarterRightSideBay2 { get => this.microCarterRightSideBay2; set => this.SetProperty(ref this.microCarterRightSideBay2, value); }

        public bool MicroCarterLeftSideBay3 { get => this.microCarterLeftSideBay3; set => this.SetProperty(ref this.microCarterLeftSideBay3, value); }

        public bool MicroCarterRightSideBay3 { get => this.microCarterRightSideBay3; set => this.SetProperty(ref this.microCarterRightSideBay3, value); }

        public bool MushroomEmergencyButtonBay1 { get => this.mushroomEmergencyButtonBay1; set => this.SetProperty(ref this.mushroomEmergencyButtonBay1, value); }

        public bool MushroomEmergencyButtonBay2 { get => this.mushroomEmergencyButtonBay2; set => this.SetProperty(ref this.mushroomEmergencyButtonBay2, value); }

        public bool MushroomEmergencyButtonBay3 { get => this.mushroomEmergencyButtonBay3; set => this.SetProperty(ref this.mushroomEmergencyButtonBay3, value); }

        public bool NoValue { get => this.noValue; set => this.SetProperty(ref this.noValue, value); }

        public bool PowerOnOff { get => this.powerOnOff; set => this.SetProperty(ref this.powerOnOff, value); }

        public bool ProfileCalibrationBay1 { get => this.profileCalibrationBay1; set => this.SetProperty(ref this.profileCalibrationBay1, value); }

        public bool ProfileCalibrationBay2 { get => this.profileCalibrationBay2; set => this.SetProperty(ref this.profileCalibrationBay2, value); }

        public bool ProfileCalibrationBay3 { get => this.profileCalibrationBay3; set => this.SetProperty(ref this.profileCalibrationBay3, value); }

        public bool RobotOptionBay1 { get => this.robotOptionBay1; set => this.SetProperty(ref this.robotOptionBay1, value); }

        public bool RobotOptionBay2 { get => this.robotOptionBay2; set => this.SetProperty(ref this.robotOptionBay2, value); }

        public bool RobotOptionBay3 { get => this.robotOptionBay3; set => this.SetProperty(ref this.robotOptionBay3, value); }

        public bool RunningState { get => this.runningState; set => this.SetProperty(ref this.runningState, value); }

        public bool TrolleyOptionBay1 { get => this.trolleyOptionBay1; set => this.SetProperty(ref this.trolleyOptionBay1, value); }

        public bool TrolleyOptionBay2 { get => this.trolleyOptionBay2; set => this.SetProperty(ref this.trolleyOptionBay2, value); }

        public bool TrolleyOptionBay3 { get => this.trolleyOptionBay3; set => this.SetProperty(ref this.trolleyOptionBay3, value); }

        public bool ZeroPawlSensor { get => this.zeroPawlSensor; set => this.SetProperty(ref this.zeroPawlSensor, value); }

        public bool ZeroPawlSensorOneTon { get => this.zeroPawlSensorOneTon; set => this.SetProperty(ref this.zeroPawlSensorOneTon, value); }

        public bool ZeroVerticalSensor { get => this.zeroVerticalSensor; set => this.SetProperty(ref this.zeroVerticalSensor, value); }

        #endregion

        #region Methods

        public void Update(bool[] sensorStates)
        {
            if (sensorStates is null)
            {
                return;
            }

            foreach (var value in System.Enum.GetValues(typeof(IOMachineSensors)))
            {
                var propertyInfo = this.GetType().GetProperty(System.Enum.GetName(typeof(IOMachineSensors), value));

                if (propertyInfo is null)
                {
                    NLog.LogManager.GetCurrentClassLogger().Warn($"Unable to decode sensor '{value}'.");
                }

                if (propertyInfo != null && (int)value < sensorStates.Length)
                {
                    propertyInfo.SetValue(this, sensorStates[(int)value]);
                }
            }
        }

        #endregion
    }
}
