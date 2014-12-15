using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckyDocs.SiteBuilder
{
    public class SiteBuilderRequest
    {
        /// <summary>
        /// Assemblies to generate documentation models for.
        /// </summary>
        public List<string> TargetAssemblies { get; set; }

        /// <summary>
        /// Locations of XML documentation files or folders containing the required XML documentation files.
        /// </summary>
        public List<string> XmlDocLocations { get; set; }

        /// <summary>
        /// Static content conversions.
        /// </summary>
        public StaticBuilderRequest StaticContent { get; set; }
    }
}
