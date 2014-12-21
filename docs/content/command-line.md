# Build Documentation from the Command Line

While you can create your own documentation tool Ducky Docs already has a basic one to get your started. The "DuckyDocs.Console" tool can be used to build a documentation website from markdown, razor templates, assemblies and other metadata.

## Command Line Parameters

<dl>

<dt><code>--output {directory}</code> or <code>-o {directory}</code></dt>
<dd>
<strong>Required</strong> Can be used to specify the output directory.
</dd>

<dt><code>--docs {directory}</code> or <code>-d {directory}</code></dt>
<dd>
Specifies a folder where content documentation files can be found in a markdown format.
</dd>

<dt><code>--apitargets {files}</code> or <code>-a {files}</code></dt>
<dd>
Comma separated list of assemblies (.dll files) to generate API documentation for.
</dd>

<dt><code>--templates {directory}</code> or <code>-t {directory}</code></dt>
<dd>
Specifies a folder where specific Razor templates can be found. Required for API documentation generation.
</dd>

<dt><code>--apixml {paths}</code> or <code>-x {paths}</code></dt>
<dd>
Comma separated list of files or folders with API XML documentation metadata.
</dd>

<dt><code>--nosplash</code> or <code>-s</code></dt>
<dd>
Activate this option to disable the splash message.
</dd>

</dl>

## Generating Content Documentation Files

Content documentation can be generated from a folder of markdown files buy supplying that folder to the `--docs` option. Markdown files are located recursively within that folder and saved after conversion to the same relative folder within the output directory. The conversion process simply converts the markdown to basic HTML but an optional `_template.cshtml` Razor file can be placed within the documentation source directory to control how the HTML is rendered.

## Generating API Documentation Files

All API documentation will be saved as a flat listing within an "api" folder in the output directory. The API documentation is generated from Razor views located in the supplied template folder. The following template files are used for their respective member type:

* `_delegate.cshtml`
* `_event.cshtml`
* `_field.cshtml`
* `_method.cshtml`
* `_namespace.cshtml`
* `_property.cshtml`
* `_type.cshtml`