using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DandyDoc.Core;
using System.Xml;

namespace SimpleVertesaurHtml
{
	class Program
	{
		static void Main(string[] args){

			var assemblyGroup = AssemblyGroup.CreateFromFilePaths("./Vertesaur.Core.dll","./Vertesaur.Generation.dll");

			var currentDirectory = new DirectoryInfo("./");
			var outputDirectory = Path.Combine(currentDirectory.FullName, "vertesaur-html-out/");
			var outputDirectoryInfo = CreateDirectory(outputDirectory);

			Document(assemblyGroup, outputDirectoryInfo);

		}

		private static DirectoryInfo CreateDirectory(string directory){
			if (!Directory.Exists(directory)){
				return Directory.CreateDirectory(directory);
			}
			return new DirectoryInfo(directory);
		}

		private static void Document(AssemblyGroup assemblyGroup, DirectoryInfo outputDirectory){
			var allTypeRecords = assemblyGroup.SelectMany(x => x.TypeRecords);
			foreach (var typeRecord in allTypeRecords.Where(x => x.IsPublic)){
				var typeDirectory = Path.Combine(outputDirectory.FullName, String.Join("/", typeRecord.NamespaceParts));
				var typeDirectoryInfo = CreateDirectory(typeDirectory);
				var typeFileInfo = new FileInfo(Path.Combine(typeDirectory, typeRecord.Name + ".html"));
				using (var file = new FileStream(typeFileInfo.FullName, FileMode.Create, FileAccess.Write, FileShare.Read))
				using(var streamWriter = new StreamWriter(file))
				using (var writer = new XmlTextWriter(streamWriter)) {
					writer.WriteStartElement("html");

					writer.WriteStartElement("head");
					writer.WriteElementString("title", typeRecord.Name + " - " + typeRecord.Namespace);
					writer.WriteEndElement();

					writer.WriteStartElement("body");

					writer.WriteElementString("h1", typeRecord.Name + " Class");

					var summary = typeRecord.Summary;
					if (null != summary && !String.IsNullOrWhiteSpace(summary.NormalizedInnerXml)){
						writer.WriteStartElement("div");
						WriteHtmlForParsedXmlDoc(writer, summary);
						writer.WriteEndElement();
					}

					writer.WriteStartElement("hr");
					writer.WriteEndElement();

					writer.WriteStartElement("div");
					writer.WriteElementString("b","Location:");
					writer.WriteRaw(String.Format(
						" {0} within {1} ({2})",
						typeRecord.Namespace,
						typeRecord.Parent.Name,
						typeRecord.Parent.CoreAssemblyFilePath.Name
					));
					writer.WriteEndElement();


					var remarks = typeRecord.Remarks;
					foreach (var remark in remarks){
						writer.WriteElementString("h2", "Remarks");
						writer.WriteStartElement("div");
						WriteHtmlForParsedXmlDoc(writer, remark);
						writer.WriteEndElement();
					}

					writer.WriteEndElement(); // body

					writer.WriteEndElement(); // html
				}
			}
		}

		private static void WriteHtmlForParsedXmlDoc(XmlTextWriter writer, ParsedXmlDoc parsedDoc){
			Contract.Requires(null != writer);
			if (null == parsedDoc)
				return;
			var parts = parsedDoc.ParsedNormalized;
			if (null == parts || parts.Count == 0)
				return;

			foreach (var part in parts){
				if (part is ParsedXmlSeePart){
					WriteHtmlForSeeXmlDoc(writer, (ParsedXmlSeePart) part);
				}
				else if (part is ParsedXmlElementBase){
					var elementBase = (ParsedXmlElementBase)part;
					if (elementBase is ParsedXmlInlineCode){
						WriteHtmlForSimpleElement(writer, elementBase, "code");
					}
					else if (elementBase is ParsedXmlCodeBlock){
						WriteHtmlForSimpleElement(writer, elementBase, "pre");
					}
					else if (elementBase is ParsedXmlParagraph) {
						WriteHtmlForSimpleElement(writer, elementBase, "p");
					}
					else if (elementBase is ParsedXmlTermDescriptionList){
						WriteHtmlForTermDescriptionList(writer, (ParsedXmlTermDescriptionList) elementBase);
					}
					else{
						WriteHtmlForSimpleElement(writer, elementBase);
					}
				}
				else{
					writer.WriteRaw(part.RawXml);
				}
			}
		}

		private static void WriteHtmlForTermDescriptionList(XmlTextWriter writer, ParsedXmlTermDescriptionList termDescriptionList){
			var listType = termDescriptionList.ListType;
			if ("BULLET".Equals(listType, StringComparison.OrdinalIgnoreCase)){
				writer.WriteStartElement("ul");
				foreach (var row in termDescriptionList.AllRows){
					var term = row.Term;
					var description = row.Description;
					if (null == term && null == description)
						continue;
					
					writer.WriteStartElement("li");
					if (null != term){
						writer.WriteStartElement("b");
						WriteHtmlForParsedXmlDoc(writer, term);
						writer.WriteEndElement();
						if (null != description){
							writer.WriteStartElement("br");
							writer.WriteEndElement();
						}
					}

					if (null != description){
						writer.WriteStartElement("div");
						WriteHtmlForParsedXmlDoc(writer, description);
						writer.WriteEndElement();
					}

					writer.WriteEndElement();
				}
				writer.WriteEndElement();
			}
			else{
				throw new NotSupportedException("List type not supported: " + listType);
			}
		}

		private static void WriteHtmlForSimpleElement(XmlTextWriter writer, ParsedXmlElementBase element) {
			writer.WriteRaw(element.OuterPrefix);
			WriteHtmlForParsedXmlDoc(writer, element.SubParts);
			writer.WriteRaw(element.OuterSuffix);
		}

		private static void WriteHtmlForSimpleElement(XmlTextWriter writer, ParsedXmlElementBase element, string elementRename) {
			writer.WriteStartElement(elementRename);
			WriteHtmlForParsedXmlDoc(writer, element.SubParts);
			writer.WriteEndElement();
		}

		private static void WriteHtmlForSeeXmlDoc(XmlTextWriter writer, ParsedXmlSeePart seePart){
			writer.WriteStartElement("a");
			writer.WriteAttributeString("href", "#");
			writer.WriteString(seePart.QuickLabel);
			writer.WriteEndElement();
		}

	}
}
