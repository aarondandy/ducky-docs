using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckyDocs.SiteBuilder
{
    public class StaticPageConverter
    {

        public string DestinationRoot { get; set; }

        public async Task<IEnumerable<StaticPageBuilderResponse>> ConvertAsync(StaticBuilderRequest request)
        {
            if (request == null) throw new ArgumentNullException("request");

            var sourceFile = new FileInfo(request.Source);
            if (sourceFile.Exists)
            {
                var singleResponse = await ConvertFileAsync(request, sourceFile);
                return new[] { singleResponse };
            }

            var sourceDirectory = new DirectoryInfo(request.Source);
            if (sourceDirectory.Exists)
            {
                return await ConvertDirectoryAsync(request, sourceDirectory);
            }

            return Enumerable.Empty<StaticPageBuilderResponse>();
        }

        private Task<StaticPageBuilderResponse> ConvertFileAsync(StaticBuilderRequest request, FileInfo sourceFile)
        {
            // TODO: find the thing to use for the conversion
            return ConvertFileAsync(request, sourceFile, null);
        }

        private async Task<StaticPageBuilderResponse> ConvertFileAsync(StaticBuilderRequest request, FileInfo sourceFile, object fileConverter)
        {
            throw new NotImplementedException();
        }

        private async Task<IEnumerable<StaticPageBuilderResponse>> ConvertDirectoryAsync(StaticBuilderRequest request, DirectoryInfo sourceDirectory)
        {
            IEnumerable<DirectoryInfo> directories;
            if (request.Recursive)
            {
                directories = SearchAllDirectories(sourceDirectory);
            }
            else
            {
                directories = new[] { sourceDirectory };
            }

            var results = new List<StaticPageBuilderResponse>();
            var sourceFullPath = sourceDirectory.FullName;
            foreach (var directory in directories)
            {
                var directoryFullName = directory.FullName;
                Contract.Assume(directoryFullName.StartsWith(sourceFullPath));
                var relativePath = directoryFullName.Remove(sourceFullPath.Length);

                foreach (var file in directory.EnumerateFiles())
                {
                    var result = await ConvertFileAsync(request, file);
                    if (result != null)
                    {
                        results.Add(result);
                    }
                }
            }

            return results;
        }

        private IEnumerable<DirectoryInfo> SearchAllDirectories(DirectoryInfo sourceDirectory)
        {
            Contract.Requires(sourceDirectory != null);

            var searchStack = new Stack<DirectoryInfo>();
            searchStack.Push(sourceDirectory);

            while (searchStack.Count > 0)
            {
                var searchDirectory = searchStack.Pop();
                yield return searchDirectory;

                foreach (var directory in searchDirectory.GetDirectories())
                {
                    searchStack.Push(directory);
                }
            }
        }
    }
}
