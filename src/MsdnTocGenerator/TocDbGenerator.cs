using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SQLite;
using System.Data.SqlClient;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Dapper;
using MsdnTocGenerator.MsdnContentService;

namespace MsdnTocGenerator
{
	public class TocDbGenerator : IDisposable
	{

		public string RootAssetId { get; set; }

		public string Version { get; set; }

		public string Locale { get; set; }

		public FileInfo DbFileInfo { get; private set; }

		protected IDbConnection Connection { get; private set; }

		protected ContentServicePortTypeClient TocClient { get; private set; }

		private appId MyAppId { get; set; }

		public TocDbGenerator() {
			TocClient = new ContentServicePortTypeClient(
				new BasicHttpBinding(){MaxReceivedMessageSize = 1024 * 1024},
				new EndpointAddress("http://services.msdn.microsoft.com/ContentServices/ContentService.asmx"));
			RootAssetId = "AssetId:2c606a4d-a51a-bdd7-020c-73f9081c4e33";
			Version = "VS.110";
			Locale = "en-us";

			DbFileInfo = new FileInfo("msdn_toc.sqlite");
			if (File.Exists(DbFileInfo.FullName)){
				DbFileInfo.Delete();
				Thread.Sleep(250);
			}
			Connection = new SQLiteConnection(String.Format(@"data source=""{0}"";", DbFileInfo.FullName));
			Connection.Open();
			Connection.Execute("CREATE TABLE \"Toc\" (\"id\" TEXT PRIMARY KEY NOT NULL, \"parent\" TEXT, \"contentId\" TEXT, \"alias\" TEXT, \"guid\" TEXT, \"title\" TEXT)");
			MyAppId = new appId();
		}

		private XmlElement GetNamespaceTocElement(string assetId, string version, string locale) {
			var request = new getContentRequest{
				contentIdentifier = assetId,
				locale = locale,
				version = version,
				requestedDocuments = new[] { new requestedDocument { type = documentTypes.primary, selector = "Mtps.Toc" }}
			};
			var root = TocClient.GetContent(MyAppId, request).primaryDocuments.SingleOrDefault();
			if (null == root)
				return null;
			return root.Any;
		}

		public void GetTocNode(string assetId, string version, string locale, string parentAssetId = null) {
			var tocXmlElement = GetNamespaceTocElement(assetId, version, locale);
			if (null == tocXmlElement)
				return;

			Contract.Assume(assetId == tocXmlElement.Attributes["toc:SubTree"].Value);
			Contract.Assume(version == tocXmlElement.Attributes["toc:SubTreeVersion"].Value);
			Contract.Assume(locale == tocXmlElement.Attributes["toc:SubTreeLocale"].Value);

			var targetId = tocXmlElement.Attributes.GetValueOrDefault("toc:Target");
			var targetLocale = tocXmlElement.Attributes.GetValueOrDefault("toc:TargetLocale");
			var targetVersion = tocXmlElement.Attributes.GetValueOrDefault("toc:TargetVersion");
			Contract.Assume(!String.IsNullOrEmpty(targetId));
			Contract.Assume(!String.IsNullOrEmpty(targetLocale));
			Contract.Assume(!String.IsNullOrEmpty(targetVersion));

			var title = tocXmlElement.Attributes["toc:Title"].Value;
			var contentRequest = new getContentRequest{
				contentIdentifier = targetId,
				locale = targetLocale,
				version = targetVersion
			};
			var contentResponse = TocClient.GetContent(MyAppId, contentRequest);
			var contentId = contentResponse.contentId;
			var contentGuid = contentResponse.contentGuid;
			var contentAlias = contentResponse.contentAlias;

			Console.WriteLine("Processing {0}", title);
			Connection.Execute("INSERT INTO toc(id,parent,contentId,alias,guid,title) values(@id,@parent,@contentId,@alias,@guid,@title)", new { id = targetId, parent = parentAssetId, contentId, alias = contentAlias, guid = contentGuid, title });

			foreach(var namespaceNode in tocXmlElement.ChildNodes.Cast<XmlElement>()){
				var subTreeAssetId = namespaceNode.Attributes.GetValueOrDefault("toc:SubTree");
				var subTreeVersion = namespaceNode.Attributes.GetValueOrDefault("toc:SubTreeVersion");
				var subTreeLocale = namespaceNode.Attributes.GetValueOrDefault("toc:SubTreeLocale");
				if (!String.IsNullOrEmpty(subTreeAssetId)) {
					Contract.Assume(!String.IsNullOrEmpty(subTreeVersion));
					Contract.Assume(!String.IsNullOrEmpty(subTreeLocale));
					GetTocNode(subTreeAssetId, subTreeVersion, subTreeLocale, targetId);
				}
			}

		}

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing){
			Connection.Dispose();
		}

		~TocDbGenerator(){
			Dispose(false);
		}

	}
}
