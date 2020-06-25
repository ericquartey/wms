using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml;

#nullable enable

namespace Ferretto.VW.Installer.Core
{
    public class AppManifest
    {
        #region Constructors

        public AppManifest(string assemblyIdentityVersion)
        {
            if (assemblyIdentityVersion is null)
            {
                throw new ArgumentNullException(nameof(assemblyIdentityVersion));
            }

            this.AssemblyIdentityVersion = assemblyIdentityVersion;
        }

        #endregion

        #region Properties

        public string AssemblyIdentityVersion { get; }

        #endregion

        #region Methods

        public static async Task<AppManifest?> FromFileAsync(string fullPath)
        {
            using var reader = new StreamReader(fullPath);
            using var xmlReader = XmlReader.Create(reader, new XmlReaderSettings() { Async = true });

            while (await xmlReader.ReadAsync())
            {
                if (xmlReader.NodeType == XmlNodeType.Element
                    &&
                    xmlReader.Name == "assemblyIdentity")
                {
                    if (xmlReader.HasAttributes)
                    {
                        var version = xmlReader.GetAttribute("version");

                        return new AppManifest(version);
                    }
                }
            }

            return null;
        }

        #endregion
    }
}
