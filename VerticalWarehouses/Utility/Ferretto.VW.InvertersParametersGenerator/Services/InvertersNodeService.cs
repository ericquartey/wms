using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ferretto.VW.InvertersParametersGenerator.Extensions;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Newtonsoft.Json;
using NLog;

namespace Ferretto.VW.InvertersParametersGenerator.Models
{
    public class InvertersNodeService
    {
        #region Fields

        private const string FILENAME = @".\InvertersNode.json";

        private const short MASTERFIRSTINVERTERCODE = 924;

        private const short MASTERSECONDINVERTERCODE = 926;

        private const int SLAVEENDPARAMETERSET = 953;

        private const int SLAVEINITPARAMETERSET = 950;

        private const short SLAVEINVERTERCODE = 925;

        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            Formatting = Formatting.Indented
        };

        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Constructors

        public InvertersNodeService()
        {
            this.LoadNodes();
        }

        #endregion

        #region Properties

        public IEnumerable<InverterNode> Nodes { get; set; }

        #endregion

        #region Methods

        public static void Save(IEnumerable<InverterNode> nodes, string fileName)
        {
            var newInvertersNode = new InvertersNodeService()
            {
                Nodes = nodes
            };

            var objectString = JsonConvert.SerializeObject(newInvertersNode, SerializerSettings);
            File.WriteAllText(fileName, objectString);
        }

        public IEnumerable<InverterNode> BuildMachineInverterNode(IEnumerable<InverterParametersDataInfo> invertersParameters)
        {
            var machineNodes = new List<InverterNode>();

            var mainInverter = invertersParameters.First();
            var newMainAngInverter = this.GetInverterNode(mainInverter.InverterIndex, mainInverter.Description);
            machineNodes.Add(newMainAngInverter);

            // Adjust inverter node from slave1 to slave4
            var slave = InverterIndex.Slave1;
            var skipSlave = 0;
            var angInverterValueForFirstSlave = string.Empty;
            do
            {
                if (invertersParameters.SingleOrDefault(i => i.InverterIndex == (byte)slave) is InverterParametersDataInfo inverterParametersData)
                {
                    var inverterNode = this.GetInverterNode(inverterParametersData.InverterIndex, inverterParametersData.Description, skipSlave);
                    angInverterValueForFirstSlave = (string.IsNullOrEmpty(angInverterValueForFirstSlave)) ? inverterNode.Parameters.SingleOrDefault(p => p.Code == SLAVEINVERTERCODE).Value : angInverterValueForFirstSlave;
                    machineNodes.Add(inverterNode);
                    skipSlave = 0;
                }
                else
                {
                    skipSlave++;
                }

                slave = slave.Next();
            } while (slave != InverterIndex.Slave5);

            // Adjust inverter node from slave5 to slave7
            slave = InverterIndex.Slave5;
            skipSlave = 0;
            var angInverterValueForSecondSlave = string.Empty;
            do
            {
                if (invertersParameters.SingleOrDefault(i => i.InverterIndex == (byte)slave) is InverterParametersDataInfo inverterParametersData)
                {
                    var inverterNode = this.GetInverterNode(inverterParametersData.InverterIndex, inverterParametersData.Description, skipSlave);
                    angInverterValueForSecondSlave = (string.IsNullOrEmpty(angInverterValueForSecondSlave)) ? inverterNode.Parameters.SingleOrDefault(p => p.Code == SLAVEINVERTERCODE).Value : angInverterValueForSecondSlave;
                    machineNodes.Add(inverterNode);
                    skipSlave = 0;
                }
                else
                {
                    skipSlave++;
                }

                slave = slave.Next();
            } while (slave != InverterIndex.Slave7);

            // Adjust all slave inverters params first connected to main inverter
            newMainAngInverter.Parameters.SingleOrDefault(p => p.Code == MASTERFIRSTINVERTERCODE).Value = angInverterValueForFirstSlave;
            if (!string.IsNullOrEmpty(angInverterValueForSecondSlave))
            {
                newMainAngInverter.Parameters.SingleOrDefault(p => p.Code == MASTERSECONDINVERTERCODE).Value = angInverterValueForSecondSlave;
            }

            return machineNodes;
        }

        public void LoadNodes()
        {
            try
            {
                var nodesJsonFile = File.ReadAllText(FILENAME);
                var serviceAnon = new { Nodes = Array.Empty<InverterNode>() };
                serviceAnon = JsonConvert.DeserializeAnonymousType(nodesJsonFile, serviceAnon, SerializerSettings);
                this.Nodes = serviceAnon.Nodes.OrderBy(s => s.InverterIndex).ToArray();
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, $" Error loading nodes {Directory.GetCurrentDirectory()}");
                throw new InvalidOperationException(
                                  $"Can't continue, nodes load error from \"{Directory.GetCurrentDirectory()}\"");
            }
        }

        private InverterNode GetInverterNode(byte inverterIndex, string description, int skipValueCounter = 0)
        {
            var parameters = new List<InverterNodeParameter>();
            var node = this.Nodes.SingleOrDefault(n => n.InverterIndex == inverterIndex);
            if (node == null)
            {
                throw new ArgumentNullException($"On inverter '{inverterIndex}' parameters node not found, check file InvertersNode.json");
            }

            node.Parameters.ForEach(p => parameters.Add(new InverterNodeParameter(p.Code, p.Description, p.Value)));
            if (skipValueCounter > 0)
            {
                for (var i = SLAVEENDPARAMETERSET; i >= SLAVEINITPARAMETERSET; i--)
                {
                    if (i + skipValueCounter > SLAVEENDPARAMETERSET)
                    {
                        continue;
                    }

                    parameters.SingleOrDefault(p => p.Code == (i + skipValueCounter)).Value = parameters.SingleOrDefault(p => p.Code == i).Value;
                }

                for (var i = 0; i < skipValueCounter; i++)
                {
                    parameters.SingleOrDefault(p => p.Code == (SLAVEINITPARAMETERSET + i)).Value = "0";
                }
            }

            return new InverterNode(inverterIndex, description, parameters);
        }

        #endregion
    }
}
