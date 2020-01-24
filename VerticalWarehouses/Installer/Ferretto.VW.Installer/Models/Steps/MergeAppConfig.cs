using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Ferretto.VW.Installer
{
    internal class MergeAppConfig : Step
    {
        #region Constructors

        public MergeAppConfig(int number, string title, string description, string newPathName, string oldPathName)
            : base(number, title, description)
        {
            this.NewPathName = this.InterpolateVariables(newPathName);
            this.OldPathName = this.InterpolateVariables(oldPathName);
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
                this.MergeDocs();

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

        private static XmlNode DeepCloneToDoc(XmlNode NodeToClone, XmlDocument TargetDoc)
        {
            var newNode = TargetDoc.CreateNode(NodeToClone.NodeType, NodeToClone.Name, NodeToClone.NamespaceURI);

            foreach (XmlAttribute attrib in NodeToClone.Attributes)
            {
                newNode.Attributes.Append(TargetDoc.CreateAttribute(attrib.Prefix, attrib.LocalName, attrib.NamespaceURI));
            }

            foreach (XmlNode child in NodeToClone.ChildNodes)
            {
                newNode.AppendChild(DeepCloneToDoc(NodeToClone, TargetDoc));
            }

            return newNode;
        }

        private static int FindElementIndex(XmlElement element)
        {
            var parentNode = element.ParentNode;
            if (parentNode is XmlDocument)
            {
                return 1;
            }

            var parent = (XmlElement)parentNode;
            int index = 1;
            foreach (XmlNode candidate in parent.ChildNodes)
            {
                if (candidate is XmlElement && candidate.Name == element.Name)
                {
                    if (candidate == element)
                    {
                        return index;
                    }
                    index++;
                }
            }
            throw new ArgumentException("Couldn't find element within parent");
        }

        private static string FindXPath(XmlNode node)
        {
            var builder = new StringBuilder();
            while (node != null)
            {
                switch (node.NodeType)
                {
                    case XmlNodeType.Attribute:
                        builder.Insert(0, "/@" + node.Name);
                        node = ((XmlAttribute)node).OwnerElement;
                        break;

                    case XmlNodeType.Element:
                        int index = FindElementIndex((XmlElement)node);
                        builder.Insert(0, "/" + node.Name + "[" + index + "]");
                        node = node.ParentNode;
                        break;

                    case XmlNodeType.Document:
                        return "/";

                    default:
                        throw new ArgumentException("Only elements and attributes are supported");
                }
            }
            throw new ArgumentException("Node was not in a document");
        }

        private void MergeDocs()
        {
            var docOld = new XmlDocument();
            var docNew = new XmlDocument();
            var docMerged = new XmlDocument();

            docOld.Load(this.OldPathName);
            docNew.Load(this.NewPathName);

            var childsFromOld = docOld.ChildNodes.Cast<XmlNode>();
            var childsFromNew = docNew.ChildNodes.Cast<XmlNode>();

            var uniquesFromOld = childsFromOld.Where(ch => !childsFromNew.Any(chb => chb.Name == ch.Name));
            var uniquesFromNew = childsFromNew.Where(ch => !childsFromOld.Any(chb => chb.Name == ch.Name));

            foreach (var unique in uniquesFromOld)
            {
                docMerged.AppendChild(DeepCloneToDoc(unique, docMerged));
            }

            foreach (var unique in uniquesFromNew)
            {
                docMerged.AppendChild(DeepCloneToDoc(unique, docMerged));
            }

            var duplicates = from chA in childsFromOld
                             from chB in childsFromNew
                             where chA.Name == chB.Name
                             select new { A = chA, B = chB };

            foreach (var grp in duplicates)
            {
                docMerged.AppendChild(this.MergeNodes(grp.A, grp.B, docMerged));
            }

            docMerged.Save(this.NewPathName + ".new.xml");
        }

        private XmlNode MergeNodes(XmlNode A, XmlNode B, XmlDocument TargetDoc)
        {
            var merged = TargetDoc.CreateNode(A.NodeType, A.Name, A.NamespaceURI);

            foreach (XmlAttribute attrib in A.Attributes)
            {
                merged.Attributes.Append(TargetDoc.CreateAttribute(attrib.Prefix, attrib.LocalName, attrib.NamespaceURI));
            }

            var fromA = A.Attributes.Cast<XmlAttribute>();

            var fromB = B.Attributes.Cast<XmlAttribute>();

            var toAdd = fromB.Where(attr => !fromA.Any(ata => ata.Name == attr.Name));

            foreach (var attrib in toAdd)
            {
                merged.Attributes.Append(TargetDoc.CreateAttribute(attrib.Prefix, attrib.LocalName, attrib.NamespaceURI));
            }

            var childsFromA = A.ChildNodes.Cast<XmlNode>();
            var childsFromB = B.ChildNodes.Cast<XmlNode>();

            var uniquesFromA = childsFromA.Where(ch => !childsFromB.Any(chb => chb.Name == ch.Name));
            var uniquesFromB = childsFromB.Where(ch => !childsFromA.Any(chb => chb.Name == ch.Name));

            foreach (var unique in uniquesFromA)
            {
                merged.AppendChild(DeepCloneToDoc(unique, TargetDoc));
            }

            foreach (var unique in uniquesFromA)
            {
                merged.AppendChild(DeepCloneToDoc(unique, TargetDoc));
            }

            var duplicates = from chA in childsFromA
                             from chB in childsFromB
                             where chA.Name == chB.Name
                             select new { A = chA, B = chB };

            foreach (var grp in duplicates)
            {
                merged.AppendChild(this.MergeNodes(grp.A, grp.B, TargetDoc));
            }

            return merged;
        }

        #endregion
    }
}
