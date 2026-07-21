/// <summary>
/// Parse a Markdown document into Markdig's abstract syntax tree and return
/// its structural elements in document order: headings (with level and
/// auto-generated anchor id when autoidentifiers is enabled), hyperlinks,
/// fenced/indented code blocks (with language tag), and images.
/// </summary>
using System.Text;
using Axiom;
using Gen;
using Markdig;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace Nodes;

public static class ExtractOutlineNode
{
    /// <param name="ax">The AxiomContext: logging, secrets, reflection, mutation.</param>
    /// <param name="input">The decoded MarkdownDoc for this invocation.</param>
    public static OutlineResult ExtractOutline(IAxiomContext ax, MarkdownDoc input)
    {
        ax.Log().Info("ExtractOutline handling");

        if (!MarkdigHelper.TryBuildPipelineBuilder(input.Extensions, input.AdvancedExtensions, out var builder, out var buildError))
        {
            return new OutlineResult { Error = buildError };
        }

        var pipeline = builder.Build();
        var markdown = input.Markdown ?? string.Empty;

        try
        {
            var document = Markdig.Markdown.Parse(markdown, pipeline);
            var result = new OutlineResult();

            foreach (var heading in document.Descendants<HeadingBlock>())
            {
                var id = heading.GetAttributes().Id ?? string.Empty;
                result.Headings.Add(new Heading
                {
                    Level = heading.Level,
                    Text = InlineText(heading.Inline),
                    Id = id,
                });
            }

            foreach (var code in document.Descendants<CodeBlock>())
            {
                var language = string.Empty;
                if (code is FencedCodeBlock fenced && fenced.Info != null)
                {
                    language = fenced.Info;
                }
                result.CodeBlocks.Add(new MdCodeBlock
                {
                    Language = language,
                    Content = code.Lines.ToString(),
                });
            }

            foreach (var link in document.Descendants<LinkInline>())
            {
                if (link.IsImage)
                {
                    result.Images.Add(new MdImage
                    {
                        Alt = InlineText(link),
                        Url = link.Url ?? string.Empty,
                        Title = link.Title ?? string.Empty,
                    });
                }
                else
                {
                    result.Links.Add(new MdLink
                    {
                        Text = InlineText(link),
                        Url = link.Url ?? string.Empty,
                        Title = link.Title ?? string.Empty,
                    });
                }
            }

            return result;
        }
        catch (System.Exception)
        {
            return new OutlineResult { Error = MarkdigHelper.ErrInternal };
        }
    }

    // Flattens a ContainerInline's descendant text/code content into a
    // single string — the "rendered inline text" of a heading or link.
    private static string InlineText(ContainerInline? inline)
    {
        if (inline == null)
        {
            return string.Empty;
        }
        var sb = new StringBuilder();
        // Start at the container's FIRST CHILD, not the container itself —
        // when `inline` is e.g. a LinkInline nested inside a paragraph's
        // inline chain, `inline` has its own NextSibling pointing at
        // whatever text follows the link in the paragraph, which must NOT
        // be pulled into this link's own text.
        AppendInlineText(inline.FirstChild, sb);
        return sb.ToString();
    }

    private static void AppendInlineText(Markdig.Syntax.Inlines.Inline? inline, StringBuilder sb)
    {
        for (var node = inline; node != null; node = node.NextSibling)
        {
            switch (node)
            {
                case LiteralInline literal:
                    sb.Append(literal.Content.ToString());
                    break;
                case CodeInline code:
                    sb.Append(code.Content);
                    break;
                case LineBreakInline:
                    sb.Append(' ');
                    break;
                case ContainerInline container:
                    AppendInlineText(container.FirstChild, sb);
                    break;
            }
        }
    }
}
