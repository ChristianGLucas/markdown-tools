/// <summary>
/// Extract a Markdown document's visible plain text with all formatting
/// markup (emphasis markers, link syntax, heading hashes, code fences, HTML
/// tags) stripped, using Markdig's plain-text renderer.
/// </summary>
using Axiom;
using Gen;
using Markdig;

namespace Nodes;

public static class ExtractPlainTextNode
{
    /// <param name="ax">The AxiomContext: logging, secrets, reflection, mutation.</param>
    /// <param name="input">The decoded MarkdownDoc for this invocation.</param>
    public static TextResult ExtractPlainText(IAxiomContext ax, MarkdownDoc input)
    {
        ax.Log().Info("ExtractPlainText handling");

        if (!MarkdigHelper.TryBuildPipelineBuilder(input.Extensions, input.AdvancedExtensions, out var builder, out var buildError))
        {
            return MarkdigHelper.FailText(buildError);
        }

        var pipeline = builder.Build();
        var markdown = input.Markdown ?? string.Empty;
        return MarkdigHelper.TrySucceedText(() => Markdown.ToPlainText(markdown, pipeline));
    }
}
