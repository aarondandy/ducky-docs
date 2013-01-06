using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MsdnTocGenerator
{

	class Program
	{

		static void Main(string[] args){
			var tocDbGenerator = new TocDbGenerator();
			tocDbGenerator.GetTocNode(tocDbGenerator.RootAssetId, tocDbGenerator.Version, tocDbGenerator.Locale);
			;
		}
	}
}
