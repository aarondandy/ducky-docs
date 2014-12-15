using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckyDocs.SiteBuilder
{
    public class StaticPageConverterRequest
    {

        /// <summary>
        /// The source directory or file to convert.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Specifies that conversion should be recursive when the source is a directory.
        /// </summary>
        public bool Recursive { get; set; }

        /// <summary>
        /// The destination directory to output the converted files.
        /// </summary>
        public string RelativeDestination { get; set; }

    }
}
