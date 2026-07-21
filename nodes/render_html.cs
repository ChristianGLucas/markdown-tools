/// <summary>
/// Render a Markdown document to HTML using Markdig's CommonMark-compliant
/// parser/renderer, with the requested pipeline extensions (extensions list
/// and/or advanced_extensions bundle) applied. Deterministic for a fixed
/// (markdown, extensions, advanced_extensions) triple. An unrecognized
/// extension name returns INVALID_ARGUMENT rather than being ignored.
/// </summary>
using Axiom;
using Gen;
using Markdig;

namespace Nodes;

public static class RenderHtmlNode
{
    /// <param name="ax">The AxiomContext: logging, secrets, reflection, mutation.</param>
    /// <param name="input">The decoded MarkdownDoc for this invocation.</param>
    public static HtmlResult RenderHtml(IAxiomContext ax, MarkdownDoc input)
    {
        ax.Log().Info("RenderHtml handling");

        if (!MarkdigHelper.TryBuildPipelineBuilder(input.Extensions, input.AdvancedExtensions, out var builder, out var buildError))
        {
            return MarkdigHelper.FailHtml(buildError);
        }

        var pipeline = builder.Build();
        var markdown = input.Markdown ?? string.Empty;
        return MarkdigHelper.TrySucceedHtml(() => Markdown.ToHtml(markdown, pipeline));
    }
}
