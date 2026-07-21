/// <summary>
/// Re-render a Markdown document back to Markdown through Markdig's
/// NormalizeRenderer, producing a canonicalized form (consistent heading
/// style, list marker spacing, emphasis characters) with the requested
/// pipeline extensions applied. Idempotent: normalizing already-normalized
/// output returns it unchanged.
/// </summary>
using System.IO;
using Axiom;
using Gen;
using Markdig;
using Markdig.Renderers;
using Markdig.Renderers.Normalize;

namespace Nodes;

public static class NormalizeMarkdownNode
{
    /// <param name="ax">The AxiomContext: logging, secrets, reflection, mutation.</param>
    /// <param name="input">The decoded MarkdownDoc for this invocation.</param>
    public static MarkdownResult NormalizeMarkdown(IAxiomContext ax, MarkdownDoc input)
    {
        ax.Log().Info("NormalizeMarkdown handling");

        if (!MarkdigHelper.TryBuildPipelineBuilder(input.Extensions, input.AdvancedExtensions, out var builder, out var buildError))
        {
            return MarkdigHelper.FailMarkdown(buildError);
        }

        var pipeline = builder.Build();
        var markdown = input.Markdown ?? string.Empty;
        return MarkdigHelper.TrySucceedMarkdown(() =>
        {
            var document = Markdig.Markdown.Parse(markdown, pipeline);
            using var writer = new StringWriter();
            var renderer = new NormalizeRenderer(writer);
            pipeline.Setup(renderer);
            renderer.Render(document);
            writer.Flush();
            return writer.ToString();
        });
    }
}
