using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DandyDocSite.Infrastructure
{
	public static class DebugFlag
	{

		public static bool IsDebug(){
#if DEBUG
			return true;
#else
			return false;
#endif
		}

	}
}