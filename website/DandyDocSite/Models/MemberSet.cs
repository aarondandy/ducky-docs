using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using DandyDoc.SimpleModels.Contracts;

namespace DandyDocSite.Models
{
	public abstract class MemberSet
	{

		public static MemberSet<TModel> Create<TModel>(string title, IList<TModel> models)
			where TModel : class, ISimpleModel
		{
			Contract.Requires(!String.IsNullOrEmpty(title));
			Contract.Requires(models != null);
			return new MemberSet<TModel>(title, models);
		}

		protected MemberSet(string title){
			Contract.Requires(!String.IsNullOrEmpty(title));
			Title = title;
		}

		public abstract IEnumerable<ISimpleModel> SimpleModels { get; }

		public abstract bool HasModels { get; }

		public string Title { get; private set; }

		[ContractInvariantMethod]
		private void CodeContractInvariant(){
			Contract.Invariant(!String.IsNullOrEmpty(Title));
		}

	}

	public class MemberSet<TModel> : MemberSet
		where TModel : class, ISimpleModel
	{

		public MemberSet(string title, IList<TModel> models)
			: base(title)
		{
			Contract.Requires(models != null);
			Contract.Requires(!String.IsNullOrEmpty(title));
			Models = models;
		}

		public IList<TModel> Models { get; private set; }

		public override bool HasModels {
			get { return Models.Count > 0; }
		}

		[ContractInvariantMethod]
		private void CodeContractInvariant(){
			Contract.Invariant(null != Models);
		}

		public override IEnumerable<ISimpleModel> SimpleModels {
			get { return Models; }
		}
	}

}