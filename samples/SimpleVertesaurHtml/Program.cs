using System;
using System.Collections.Generic;
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
			foreach (var typeRecord in allTypeRecords.Where(x => x.CoreType.IsPublic)){
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
					if(!String.IsNullOrWhiteSpace(summary))
						writer.WriteElementString("div", summary);

					writer.WriteStartElement("hr");
					writer.WriteEndElement();

					writer.WriteStartElement("div");
					writer.WriteElementString("b","Namespace:");
					writer.WriteRaw(" " + typeRecord.Namespace);
					writer.WriteEndElement();

					writer.WriteStartElement("div");
					writer.WriteElementString("b", "Assembly:");
					writer.WriteRaw(" " + typeRecord.Parent.CoreAssembly.GetName().Name);
					writer.WriteEndElement();

					var remarks = typeRecord.Remarks;
					if (!String.IsNullOrWhiteSpace(remarks)){
						writer.WriteElementString("h2", "Remarks");
						writer.WriteStartElement("div");
						writer.WriteRaw(remarks.Replace("\n", "<br/>"));
						writer.WriteEndElement();
					}


					writer.WriteEndElement(); // body

					writer.WriteEndElement(); // html
				}
			}
		}

	}
}
