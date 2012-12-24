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

		private static readonly Regex EndSpaceRegex = new Regex(@"\n([ \t]*)$", RegexOptions.Compiled);

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

	}
}
