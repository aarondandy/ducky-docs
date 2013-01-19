using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DandyDoc.Overlays.MsdnLinks
{
	public class MtpsNavigationNode : MtpsNodeCore
	{

		private static readonly ReadOnlyCollection<MtpsNodeCore> EmptyChildrenCollection
			= new ReadOnlyCollection<MtpsNodeCore>(new MtpsNodeCore[0]);

		public MtpsNavigationNode(
			MtpsIdentifier subTreeId,
			MtpsIdentifier targetId,
			MtpsNodeCore parent = null,
			bool phantom = false,
			Guid? guid = null,
			string contentId = null,
			string alias = null,
			string title = null,
			IList<MtpsNodeCore> childLinks = null
		)
			: base(subTreeId, targetId, title, parent, phantom)
		{
			Guid = guid;
			ContentId = contentId;
			Alias = alias;
			ChildLinks = null == childLinks ? EmptyChildrenCollection : new ReadOnlyCollection<MtpsNodeCore>(childLinks);
		}


		
		public Guid? Guid { get; private set; }
		public string ContentId { get; private set; }
		public string Alias { get; private set; }
		public ReadOnlyCollection<MtpsNodeCore> ChildLinks { get; private set; }

	}
}
