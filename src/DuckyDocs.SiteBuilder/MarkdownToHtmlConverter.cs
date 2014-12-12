using CommonMark;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckyDocs.SiteBuilder
{
    public class MarkdownToHtmlConverter
    {

        public void Convert(TextReader reader, TextWriter writer)
        {
            if (reader == null) throw new ArgumentNullException("reader");
            if (writer == null) throw new ArgumentNullException("writer");
            Contract.EndContractBlock();

            CommonMarkSettings settings = null;
            var parsedDocument = CommonMarkConverter.ProcessStage1(reader, settings);
            CommonMarkConverter.ProcessStage2(parsedDocument, settings);
            CommonMarkConverter.ProcessStage3(parsedDocument, writer, settings);
        }

    }
}
