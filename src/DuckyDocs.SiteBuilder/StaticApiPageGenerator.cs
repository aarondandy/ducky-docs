using DuckyDocs.CodeDoc;
using RazorEngine;
using RazorEngine.Templating;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckyDocs.SiteBuilder
{
    public class StaticApiPageGenerator
    {
        public DirectoryInfo TemplateDirectory { get; set; }

        public DirectoryInfo OutputDirectory { get; set; }

        public FileInfo Generate(StaticApiPageRequest request)
        {
            if (request == null) throw new ArgumentNullException("request");
            Contract.Ensures(Contract.Result<FileInfo>() != null);

            var model = request.Member;
            if (model == null)
                throw new ArgumentException("requested model not found");

            if (model is CodeDocType)
                return GenerateType((CodeDocType)model);
            if (model is CodeDocEvent)
                return GenerateSimple((CodeDocEvent)model, "_event");
            if (model is CodeDocField)
                return GenerateSimple((CodeDocField)model, "_field");
            if (model is CodeDocMethod)
                return GenerateSimple((CodeDocMethod)model, "_method");
            if (model is CodeDocNamespace)
                return GenerateSimple((CodeDocNamespace)model, "_namespace");
            if (model is CodeDocProperty)
                return GenerateSimple((CodeDocProperty)model, "_property");

            throw new NotSupportedException();
        }

        private FileInfo GenerateType(CodeDocType model)
        {
            Contract.Requires(model != null);
            Contract.Ensures(Contract.Result<FileInfo>() != null);
            if (model is CodeDocDelegate)
            {
                return GenerateSimple((CodeDocDelegate)model, "_delegate");
            }

            return GenerateSimple(model, "_type");
        }

        private FileInfo GenerateSimple<TModel>(TModel model, string template) where TModel : CodeDocSimpleMember
        {
            Contract.Requires(model != null);
            Contract.Ensures(Contract.Result<FileInfo>() != null);
            var result = ExecuteTemplate(template, model, new DynamicViewBag());
            var filePath = CreateFilePath(model);
            File.WriteAllText(filePath.FullName, result);
            return filePath;
        }

        private FileInfo CreateFilePath(CodeDocSimpleMember model)
        {
            Contract.Requires(model != null);
            Contract.Ensures(Contract.Result<FileInfo>() != null);
            var slugChars = model.CRefText.ToCharArray();
            for (int i = 0; i < slugChars.Length; ++i)
            {
                var c = slugChars[i];
                if (c == '#' || c == ':')
                {
                    slugChars[i] = '-';
                }
            }

            var fileName = new string(slugChars) + ".html";
            return new FileInfo(Path.Combine(OutputDirectory.FullName, fileName));
        }

        private string ExecuteTemplate<TModel>(string templateName, TModel model, DynamicViewBag viewBag)
        {
            var cacheKey = templateName + typeof(TModel).Name;
            var resolved = Razor.Resolve<TModel>(cacheKey, model);
            if (resolved == null)
            {
                var templatePath = Path.Combine(TemplateDirectory.FullName, Path.ChangeExtension(templateName, "cshtml"));
                var templateContnet = File.ReadAllText(templatePath);
                Razor.Compile<TModel>(templateContnet, cacheKey);
            }

            return Razor.Run<TModel>(cacheKey, model, viewBag);
        }
        
    }
}
