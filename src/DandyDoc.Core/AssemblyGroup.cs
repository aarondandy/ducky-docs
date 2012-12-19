using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace DandyDoc.Core
{
	/// <summary>
	/// A group of assemblies that can be used to resolve types and members along with accompanying metadata.
	/// </summary>
	public class AssemblyGroup : Collection<AssemblyRecord>
	{

		private const string NullRecordExceptionMessage = "Null assembly records are not valid.";

		/// <summary>
		/// Creates an assembly group from the given file paths.
		/// </summary>
		/// <param name="paths">The file paths to load assemblies from.</param>
		/// <returns>A new group containing the loaded assemblies.</returns>
		public static AssemblyGroup CreateFromFilePaths(params string[] paths) {
			if(null == paths) throw new ArgumentNullException("paths");
			Contract.Ensures(Contract.Result<AssemblyGroup>() != null);
			var group = new AssemblyGroup();
			group.AddRange(paths.Select(path => AssemblyRecord.CreateFromFilePath(path, group)));
			return group;
		}

		public AssemblyGroup(){ }

		public AssemblyGroup(AssemblyRecord record) {
			if(null == record) throw new ArgumentNullException("record");
			Contract.Ensures(Count == 1);
			Add(record);
		}

		public AssemblyGroup(IEnumerable<AssemblyRecord> records) {
			if(null == records) throw new ArgumentNullException("records");
			Contract.EndContractBlock();
			AddRange(records);
		}

		public void AddRange(IEnumerable<AssemblyRecord> records) {
			if (null == records) throw new ArgumentNullException("records");
			Contract.EndContractBlock();

			var toAdd = records.ToList();
			if(toAdd.Any(x => null == x))
				throw new ArgumentException(NullRecordExceptionMessage, "records");

			foreach (var record in toAdd) {
				Add(record);
			}
		}

		protected override void InsertItem(int index, AssemblyRecord item) {
			if (null == item) throw new ArgumentNullException("item", NullRecordExceptionMessage);
			Contract.EndContractBlock();
			base.InsertItem(index, item);
		}

		protected override void SetItem(int index, AssemblyRecord item) {
			if (null == item) throw new ArgumentNullException("item", NullRecordExceptionMessage);
			Contract.EndContractBlock();
			base.SetItem(index, item);
		}

	}
}
