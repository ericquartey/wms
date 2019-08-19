using System;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;

namespace Ferretto.VW.SignalRClientConsole
{
    internal class Program
    {
        #region Fields

        private static InstallationHubClient installationHub;

        #endregion

        #region Methods

        private static async Task Initialize()
        {
            installationHub = new InstallationHubClient("http://localhost:5000", "/installation-endpoint");
            await installationHub.ConnectAsync();
            installationHub.MessageNotified += MessageNotifiedMethod;
        }

        private static void Main(string[] args)
        {
            Console.WriteLine(">> SignalR Client Console App");
            Console.WriteLine("Press 'q' to Quit the app");
            Console.WriteLine("Press 'i' to Initialize the hub");
            Console.WriteLine();

            var bInitialized = false;
            var bExit = false;
            do
            {
                var c = Console.ReadKey().KeyChar;
                switch (char.ToUpper(c))
                {
                    case 'Q':
                        bExit = true;
                        break;

                    case 'I':
                        if (!bInitialized)
                        {
                            var initializeTask = new Task(async () => await Initialize());
                            initializeTask.Start();
                            Console.WriteLine();
                            Console.WriteLine("> Hub initialized");
                            bInitialized = true;
                        }

                        break;

                    default:
                        break;
                }
            }
            while (!bExit);

            Console.WriteLine();
            Console.WriteLine("Exit from App");
            return;
        }

        private static void MessageNotifiedMethod(object sender, MessageNotifiedEventArgs e)
        {
            var szMsg = string.Empty;
            if (e.NotificationMessage is NotificationMessageUI<SensorsChangedMessageData> ev)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                szMsg = string.Format("{0} - {1}", DateTime.Now.ToString(), $"Received NotificationMessageUI<SensorsChangedMessageData>. Object values: Sensors = [ {ev.Data.SensorsStates[0]}, {ev.Data.SensorsStates[1]}, {ev.Data.SensorsStates[2]}, {ev.Data.SensorsStates[3]}, {ev.Data.SensorsStates[4]}, {ev.Data.SensorsStates[5]}, {ev.Data.SensorsStates[6]}, {ev.Data.SensorsStates[7]} ]");
                Console.WriteLine(szMsg);
            }

            if (e.NotificationMessage is NotificationMessageUI<CalibrateAxisMessageData> cc)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                szMsg = string.Format("{0} - {1}", DateTime.Now.ToString(), $"Received NotificationMessageUI<CalibrateAxisMessageData>. Object values => Axis: {cc.Data.AxisToCalibrate.ToString()}, Status: {cc.Status.ToString()}");
                Console.WriteLine(szMsg);
            }

            if (e.NotificationMessage is NotificationMessageUI<SwitchAxisMessageData> sw)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                szMsg = string.Format("{0} - {1}", DateTime.Now.ToString(), $"Received NotificationMessageUI<SwitchAxisMessageData>. Object values => Axis:{sw.Data.AxisToSwitch.ToString()}, Status: {sw.Status.ToString()}");
                Console.WriteLine(szMsg);
            }

            if (e.NotificationMessage is NotificationMessageUI<ShutterPositioningMessageData> sp)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                szMsg = string.Format("{0} - {1}", DateTime.Now.ToString(), $"Received NotificationMessageUI<ShutterPositioningMessageData>. Object values => Direction: {sp.Data.ShutterMovementDirection.ToString()}, Position: {sp.Data.ShutterPosition.ToString()}, Status: {sp.Status.ToString()}");
                Console.WriteLine(szMsg);
            }

            if (e.NotificationMessage is NotificationMessageUI<ShutterTestStatusChangedMessageData> sc)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                szMsg = string.Format("{0} - {1}", DateTime.Now.ToString(), $"Received NotificationMessageUI<ShutterControlMessageData>. Object values => Position: {sc.Data.CurrentShutterPosition.ToString()}, #Cycles: {sc.Data.NumberCycles.ToString()}, Status: {sc.Status.ToString()}");
                Console.WriteLine(szMsg);
            }

            if (e.NotificationMessage is NotificationMessageUI<HomingMessageData> h)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                szMsg = string.Format("{0} - {1}", DateTime.Now.ToString(), $"Received NotificationMessageUI<HomingMessageData>. Object values => Axis to calibrate: {h.Data.AxisToCalibrate.ToString()}, Status: {h.Status.ToString()}");
                Console.WriteLine(szMsg);
            }

            // TEMP Check this message notification
            //if (e.NotificationMessage is NotificationMessageUI<CurrentPositionMessageData> cp)
            //{
            //    Console.ForegroundColor = ConsoleColor.Cyan;
            //    szMsg = string.Format("{0} - {1}", DateTime.Now.ToString(), $"Received NotificationMessageUI<CurrentPositionMessageData>. Object values =>  {cp.Data.BeltBurnishingPosition.ToString()}, {cp.Data.CurrentPosition.ToString()}, {cp.Data.ExecutedCycles.ToString()}");
            //    Console.WriteLine(szMsg);
            //}
            if (e.NotificationMessage is NotificationMessageUI<PositioningMessageData> vp)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                szMsg = string.Format("{0} - {1}", DateTime.Now.ToString(), $"Received NotificationMessageUI<PositioningMessageData>. Object values => Current position: {vp.Data.CurrentPosition.ToString()}, Status: {vp.Status.ToString()}");
                Console.WriteLine(szMsg);
            }

            if (e.NotificationMessage is NotificationMessageUI<ResolutionCalibrationMessageData> rc)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                szMsg = string.Format("{0} - {1}", DateTime.Now.ToString(), $"Received NotificationMessageUI<ResolutionCalibrationMessageData>. Object values => Init position: {rc.Data.ReadInitialPosition.ToString()}, Final position: {rc.Data.ReadFinalPosition.ToString()}, Status: {rc.Status.ToString()}");
                Console.WriteLine(szMsg);
            }
        }

        #endregion
    }
}
