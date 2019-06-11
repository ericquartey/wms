using System;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Data;

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
            Console.WriteLine("SignalR Client Console App");
            Console.WriteLine("Press 'C' to start the connection");
            Console.WriteLine("Press 'Q' to close the app");

            while (true)
            {
                var c = Console.ReadKey().KeyChar;
                Console.WriteLine();
                switch (c)
                {
                    case 'c':
                    case 'C':
                        var initializeTask = new Task(() => Initialize());
                        initializeTask.Start();
                        break;

                    case 'q':
                    case 'Q':
                        return;

                    default:
                        break;
                }
            }
        }

        private static void MessageNotifiedMethod(object sender, MessageNotifiedEventArgs e)
        {
            if (e.NotificationMessage is NotificationMessageUI<SensorsChangedMessageData> ev)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"Received NotificationMessageUI<SensorsChangedMessageData>. Object values: {ev.Data.SensorsStates.ToString()}");
            }
            if (e.NotificationMessage is NotificationMessageUI<CalibrateAxisMessageData> cc)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Received NotificationMessageUI<CalibrateAxisMessageData>. Object values: {cc.Data.AxisToCalibrate.ToString()}, {cc.Status.ToString()}");
            }
            if (e.NotificationMessage is NotificationMessageUI<SwitchAxisMessageData> sw)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Received NotificationMessageUI<SwitchAxisMessageData>. Object values: {sw.Data.AxisToSwitch.ToString()}");
            }
            if (e.NotificationMessage is NotificationMessageUI<ShutterPositioningMessageData> sp)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Received NotificationMessageUI<ShutterPositioningMessageData>. Object values: {sp.Data.ShutterMovementDirection.ToString()}, {sp.Data.ShutterPosition.ToString()}");
            }
            if (e.NotificationMessage is NotificationMessageUI<ShutterControlMessageData> sc)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Received NotificationMessageUI<ShutterControlMessageData>. Object values: {sc.Data.CurrentShutterPosition.ToString()}, {sc.Data.NumberCycles.ToString()}");
            }

            if (e.NotificationMessage is NotificationMessageUI<HomingMessageData> h)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Received NotificationMessageUI<HomingMessageData>. Object values: {h.Data.AxisToCalibrate.ToString()}");
            }

            if (e.NotificationMessage is NotificationMessageUI<CurrentPositionMessageData> cp)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"Received NotificationMessageUI<CurrentPositionMessageData>. Object values: {cp.Data.BeltBurnishingPosition.ToString()}, {cp.Data.CurrentPosition.ToString()}, {cp.Data.ExecutedCycles.ToString()}");
            }

            if (e.NotificationMessage is NotificationMessageUI<PositioningMessageData> vp)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Received NotificationMessageUI<PositioningMessageData>. Object values: {vp.Data.CurrentPosition.ToString()}");
            }

            if (e.NotificationMessage is NotificationMessageUI<ResolutionCalibrationMessageData> rc)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Received NotificationMessageUI<ResolutionCalibrationMessageData>. Object values: {rc.Data.ReadInitialPosition.ToString()}, {rc.Data.ReadFinalPosition.ToString()}");
            }
        }

        #endregion
    }
}
