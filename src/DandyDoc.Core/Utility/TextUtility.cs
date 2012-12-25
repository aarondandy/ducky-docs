using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DandyDoc.Core.Utility
{
	internal static class TextUtility
	{

		[Obsolete]
		private static readonly Regex EndSpaceRegex = new Regex(@"\n([ \t]*)$", RegexOptions.Compiled);

		private static readonly Regex StartSpaceEndNodeRegex = new Regex(@"^(\s+)\S");

		[Obsolete]
		public static string ExtractIndentedNormalizedInnerText(string rawInnerText){
			if (String.IsNullOrEmpty(rawInnerText))
				return rawInnerText;
			var match = EndSpaceRegex.Match(rawInnerText);
			if (!match.Success)
				return rawInnerText;

			var indent = match.Groups[1].Value;
			if (String.IsNullOrEmpty(indent))
				indent = String.Empty;

			var lines = new List<string>();
			using (var stringReader = new StringReader(rawInnerText)) {
				string line = null;
				while (null != (line = stringReader.ReadLine()))
					lines.Add(line);
			}

			if (lines.Count == 0)
				return String.Empty;

			// remove the trailing blank lines
			while (lines.Count > 0 && String.IsNullOrWhiteSpace(lines[lines.Count - 1]))
				lines.RemoveAt(lines.Count - 1);

			// remove the leading blank lines
			while (lines.Count > 0 && String.IsNullOrWhiteSpace(lines[0]))
				lines.RemoveAt(0);

			if (lines.Count == 0)
				return String.Empty;

			var resultBuilder = new StringBuilder();
			for (int lineIndex = 0; lineIndex < lines.Count; lineIndex++) {
				if (0 != lineIndex)
					resultBuilder.Append('\n');
				var line = lines[lineIndex];
				if (line.StartsWith(indent))
					line = line.Substring(indent.Length);
				resultBuilder.Append(line);
			}
			return resultBuilder.ToString();
		}

		public static string NormalizeAndUnindentElement(string rawXml) {
			if (String.IsNullOrEmpty(rawXml))
				return rawXml;

			var lines = new List<string>();
			using (var stringReader = new StringReader(rawXml)) {
				string line;
				while (null != (line = stringReader.ReadLine()))
					lines.Add(line);
			}

			if (lines.Count == 0)
				return String.Empty;

			// remove the trailing blank lines
			while (lines.Count > 0 && String.IsNullOrWhiteSpace(lines[lines.Count - 1]))
				lines.RemoveAt(lines.Count - 1);

			// remove the leading blank lines
			while (lines.Count > 0 && String.IsNullOrWhiteSpace(lines[0]))
				lines.RemoveAt(0);

			if (lines.Count == 0)
				return String.Empty;

			// remove any indentation from the open tag
			var match = StartSpaceEndNodeRegex.Match(lines[0]);
			if (match.Success && match.Groups[1].Success){
				lines[0] = lines[0].Substring(match.Groups[1].Value.Length);
			}

			if (lines.Count == 1)
				return lines[0];

			match = StartSpaceEndNodeRegex.Match(lines[lines.Count - 1]);
			if (match.Success && match.Groups[1].Success){
				var indent = match.Groups[1].Value;
				for (int i = 1; i < lines.Count; i++){
					var line = lines[i];
					if (line.StartsWith(indent))
						lines[i] = line.Substring(indent.Length);
				}
			}

			var resultBuilder = new StringBuilder(lines[0]); // add the first item
			resultBuilder.Append(lines[1]); // add the 2nd item without a line break

			// add the others with a line break, but not the last
			for (int i = 2; i < lines.Count-1; i++){
				resultBuilder.Append('\n');
				resultBuilder.Append(lines[i]);
			}

			// the final is added without a line break
			if (lines.Count > 2){
				resultBuilder.Append(lines[lines.Count - 1]);
			}
			return resultBuilder.ToString();

			/*var match = EndSpaceRegex.Match(rawXml);
			if (!match.Success)
				return rawXml;

			var indent = match.Groups[1].Value;
			if (String.IsNullOrEmpty(indent))
				indent = String.Empty;

			var lines = new List<string>();
			using (var stringReader = new StringReader(rawXml)) {
				string line = null;
				while (null != (line = stringReader.ReadLine()))
					lines.Add(line);
			}

			if (lines.Count == 0)
				return String.Empty;

			// remove the trailing blank lines
			while (lines.Count > 0 && String.IsNullOrWhiteSpace(lines[lines.Count - 1]))
				lines.RemoveAt(lines.Count - 1);

			// remove the leading blank lines
			while (lines.Count > 0 && String.IsNullOrWhiteSpace(lines[0]))
				lines.RemoveAt(0);

			if (lines.Count == 0)
				return String.Empty;

			var resultBuilder = new StringBuilder();
			for (int lineIndex = 0; lineIndex < lines.Count; lineIndex++) {
				if (0 != lineIndex)
					resultBuilder.Append('\n');
				var line = lines[lineIndex];
				if (line.StartsWith(indent))
					line = line.Substring(indent.Length);
				resultBuilder.Append(line);
			}
			return resultBuilder.ToString();*/
		}
	}
}
