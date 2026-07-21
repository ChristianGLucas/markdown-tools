/// <summary>
/// Split a Markdown document into its leading `---`-delimited YAML front
/// matter block (if present) and the remaining body, using Markdig's YAML
/// front matter extension. Always enables that extension internally,
/// regardless of the request's extensions/advanced_extensions, since
/// detecting front matter is this node's whole purpose.
/// </summary>
using System.Linq;
using Axiom;
using Gen;
using Markdig;
using Markdig.Extensions.Yaml;
using Markdig.Syntax;

namespace Nodes;

public static class ExtractFrontMatterNode
{
    /// <param name="ax">The AxiomContext: logging, secrets, reflection, mutation.</param>
    /// <param name="input">The decoded MarkdownDoc for this invocation.</param>
    public static FrontMatterResult ExtractFrontMatter(IAxiomContext ax, MarkdownDoc input)
    {
        ax.Log().Info("ExtractFrontMatter handling");

        if (!MarkdigHelper.TryBuildPipelineBuilder(input.Extensions, input.AdvancedExtensions, out var builder, out var buildError))
        {
            return new FrontMatterResult { Error = buildError };
        }

        // Front matter detection is this node's whole purpose, so always
        // ensure the YAML front matter extension is active — Markdig's
        // UseAdvancedExtensions() bundle deliberately excludes it.
        builder.UseYamlFrontMatter();
        var pipeline = builder.Build();
        var markdown = input.Markdown ?? string.Empty;

        try
        {
            var document = Markdig.Markdown.Parse(markdown, pipeline);
            var yamlBlock = document.Descendants<YamlFrontMatterBlock>().FirstOrDefault();

            if (yamlBlock == null)
            {
                return new FrontMatterResult
                {
                    HasFrontMatter = false,
                    FrontMatter = string.Empty,
                    Body = markdown,
                };
            }

            var rawFrontMatter = yamlBlock.Lines.ToString();

            var bodyStart = yamlBlock.Span.End + 1;
            var body = bodyStart < markdown.Length
                ? markdown.Substring(bodyStart).TrimStart('\r', '\n')
                : string.Empty;

            return new FrontMatterResult
            {
                HasFrontMatter = true,
                FrontMatter = rawFrontMatter,
                Body = body,
            };
        }
        catch (System.Exception)
        {
            return new FrontMatterResult { Error = MarkdigHelper.ErrInternal };
        }
    }
}
