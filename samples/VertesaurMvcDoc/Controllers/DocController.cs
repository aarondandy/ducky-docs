using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DandyDoc.Core;

namespace VertesaurMvcDoc.Controllers
{
    public class DocController : Controller
    {

		public DocController(AssemblyGroup assemblyGroup) {
			DocumentationAssemblyGroup = assemblyGroup;
		}

		public AssemblyGroup DocumentationAssemblyGroup { get; private set; }

		public ActionResult Type(string cref){
			if (String.IsNullOrWhiteSpace(cref))
				return new HttpNotFoundResult();

			var result = DocumentationAssemblyGroup.ResolveCref(cref) as TypeRecord;
			if(null == result)
				return new HttpNotFoundResult();

			return View(result);
		}

		public ActionResult Member(string cref) {
			if(String.IsNullOrWhiteSpace(cref))
				return new HttpNotFoundResult();

			var result = DocumentationAssemblyGroup.ResolveCref(cref) as MemberRecord;
			if(null == result)
				return new HttpNotFoundResult();

			return View(result);
		}

    }
}
