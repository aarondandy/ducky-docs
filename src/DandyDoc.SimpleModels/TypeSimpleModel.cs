using System;
using DandyDoc.SimpleModels.Contracts;

namespace DandyDoc.SimpleModels
{
	public class TypeSimpleModel : ITypeSimpleModel
	{
		public string DisplayName {
			get { throw new NotImplementedException(); }
		}

		public string FullName {
			get { throw new NotImplementedException(); }
		}

		public string CRef {
			get { throw new NotImplementedException(); }
		}

		public ISimpleModelRepository RootRepository {
			get { throw new NotImplementedException(); }
		}
	}
}
