using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace DandyDoc.Core
{
	public class AssemblyGroup : Collection<AssemblyRecord>
	{

		public static AssemblyGroup CreateFromFilePaths(params string[] paths) {
			return new AssemblyGroup(paths.Select(AssemblyRecord.CreateFromFilePath));
		}

		public AssemblyGroup(AssemblyRecord record) {
			if(null == record) throw new ArgumentNullException("record");
			Contract.EndContractBlock();
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
				throw new ArgumentException("Null records at not valid.", "records");

			foreach (var record in toAdd) {
				Add(record);
			}
		}

	}
}
