using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace Ferretto.VW.Installer.Core
{
    internal class MergeAppConfig : Step
    {
        #region Constructors

        public MergeAppConfig(int number, string title, string description, string newPathName, string oldPathName, string log, MachineRole machineRole, SetupMode setupMode, bool skipOnResume)
            : base(number, title, description, log,  machineRole, setupMode, skipOnResume)
        {
            this.NewPathName = InterpolateVariables(newPathName);
            this.OldPathName = InterpolateVariables(oldPathName);
        }

        #endregion

        #region Properties

        public string NewPathName { get; }

        public string OldPathName { get; }

        #endregion

        #region Methods

        protected override Task<StepStatus> OnApplyAsync()
        {
            try
            {
                this.MergeDocuments();

                return Task.FromResult(StepStatus.Done);
            }
            catch (Exception ex)
            {
                this.LogError(ex.Message);
                return Task.FromResult(StepStatus.Failed);
            }
        }

        protected override Task<StepStatus> OnRollbackAsync()
        {
            try
            {
                throw new NotImplementedException();

                return Task.FromResult(StepStatus.RolledBack);
            }
            catch (Exception ex)
            {
                this.LogError(ex.Message);
                return Task.FromResult(StepStatus.RollbackFailed);
            }
        }

        private static XmlNode DeepCloneToDoc(XmlNode sourceNode, XmlDocument targetDoc)
        {
            var targetNode = targetDoc.CreateNode(sourceNode.NodeType, sourceNode.Name, sourceNode.NamespaceURI);

            foreach (XmlAttribute attrib in sourceNode.Attributes)
            {
                targetNode.Attributes.Append(targetDoc.CreateAttribute(attrib.Prefix, attrib.LocalName, attrib.NamespaceURI));
            }

            foreach (XmlNode child in sourceNode.ChildNodes)
            {
                targetNode.AppendChild(DeepCloneToDoc(child, targetDoc));
            }

            return targetNode;
        }

        private static void MergeAttributes(XmlNode oldNode, XmlNode newNode, XmlDocument targetDoc, XmlNode targetNode)
        {
            IEnumerable<XmlNode> fromOld;
            if (oldNode.Attributes is null)
            {
                fromOld = new List<XmlNode>();
            }
            else
            {
                foreach (XmlAttribute attribute in oldNode.Attributes)
                {
                    var targetAttribute = targetDoc.CreateAttribute(attribute.Prefix, attribute.LocalName, attribute.NamespaceURI);
                    targetAttribute.Value = attribute.Value;
                    targetNode.Attributes.Append(targetAttribute);
                }

                fromOld = oldNode.Attributes.Cast<XmlAttribute>();
            }

            IEnumerable<XmlNode> fromNew;
            if (oldNode.Attributes is null)
            {
                fromNew = new List<XmlNode>();
            }
            else
            {
                fromNew = newNode.Attributes.Cast<XmlAttribute>();
            }

            var toAdd = fromNew.Where(attr => !fromOld.Any(ata => ata.Name == attr.Name));

            foreach (var attribute in toAdd)
            {
                var targetAttribute = targetDoc.CreateAttribute(attribute.Prefix, attribute.LocalName, attribute.NamespaceURI);
                targetAttribute.Value = attribute.Value;
                targetNode.Attributes.Append(targetAttribute);
            }
        }

        private void MergeDocuments()
        {
            var oldDocument = new XmlDocument();
            var newDocument = new XmlDocument();
            var mergedDocument = new XmlDocument();

            oldDocument.Load(this.OldPathName);
            newDocument.Load(this.NewPathName);

            this.MergeNodes(oldDocument, newDocument, mergedDocument);

            throw new NotImplementedException();

            mergedDocument.Save(this.NewPathName + ".new.xml");
        }

        private XmlNode MergeNodes(XmlNode oldNode, XmlNode newNode, XmlDocument targetDocument)
        {
            if (oldNode.NodeType == XmlNodeType.Document)
            {
                targetDocument.AppendChild(
                    this.MergeNodes(oldNode.FirstChild, newNode.FirstChild, targetDocument));
                targetDocument.AppendChild(
                    this.MergeNodes(oldNode.LastChild, newNode.LastChild, targetDocument));

                return targetDocument;
            }

            var targetNode = targetDocument.CreateNode(oldNode.NodeType, oldNode.Name, oldNode.NamespaceURI);
            this.LogInformation($"Creating node: {targetNode.Name}");
            MergeAttributes(oldNode, newNode, targetDocument, targetNode);

            var childsFromOld = oldNode.ChildNodes.Cast<XmlNode>().ToArray();
            var childsFromNew = newNode.ChildNodes.Cast<XmlNode>().ToArray();
            this.LogInformation($"Node {oldNode.Name}: old# {childsFromOld.Length} new {childsFromNew.Length}.");

            var uniquesFromOld = childsFromOld.Where(ch => !childsFromNew.Any(chb => chb.Name == ch.Name)).ToArray();
            var uniquesFromNew = childsFromNew.Where(ch => !childsFromOld.Any(chb => chb.Name == ch.Name)).ToArray();

            foreach (var unique in uniquesFromOld)
            {
                targetNode.AppendChild(DeepCloneToDoc(unique, targetDocument));
            }

            foreach (var unique in uniquesFromNew)
            {
                targetNode.AppendChild(DeepCloneToDoc(unique, targetDocument));
            }

            childsFromOld.Where(o => childsFromNew.Any(n => n.Name == o.Name));
            childsFromNew.Where(n => childsFromOld.Any(o => o.Name == n.Name));

            // discriminant is not the name of the node but its key!!!!
            var duplicateNodes = childsFromOld.Join(
                childsFromNew,
                on => on.Name,
                nn => nn.Name,
                (on, nn) => new { on, nn }).ToArray();

            this.LogInformation($"Node {oldNode.Name}: there are {duplicateNodes.Length} nodes.");
            foreach (var duplicateNode in duplicateNodes)
            {
                var mergedNode = this.MergeNodes(duplicateNode.on, duplicateNode.nn, targetDocument);
                targetNode.AppendChild(mergedNode);
            }

            return targetNode;
        }

        #endregion
    }
}
