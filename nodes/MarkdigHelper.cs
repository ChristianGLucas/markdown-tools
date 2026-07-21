// Shared helper logic for the markdown-tools node handlers.
//
// This is NOT a node — it is plain shared source under nodes/ that every
// node handler calls into: building a Markdig MarkdownPipelineBuilder from
// this package's canonical MarkdownDoc.extensions/advanced_extensions
// fields (with a stable, deliberately curated extension-name vocabulary —
// see messages.proto), plus a uniform structured-error-wrapping convention
// so a node never crashes on malformed input.
using System;
using System.Collections.Generic;
using Gen;
using Markdig;

namespace Nodes;

internal static class MarkdigHelper
{
    // Stable structured-error tokens returned in every *Result.Error field.
    public const string ErrInvalidArgument = "INVALID_ARGUMENT";
    public const string ErrInternal = "INTERNAL_ERROR";

    /// <summary>
    /// Builds a MarkdownPipelineBuilder from a MarkdownDoc's extensions list
    /// and advanced_extensions flag, using an explicit, curated mapping from
    /// this package's documented extension-name vocabulary (messages.proto)
    /// to Markdig's own UseXxx() pipeline methods — deliberately NOT
    /// Markdig's built-in string-configuration parser, so an unrecognized
    /// name is caught here and reported as a structured error rather than
    /// silently ignored or throwing an uncaught ArgumentException deeper in
    /// Markdig. Returns false (with `error` set) on the first unrecognized
    /// extension name; the builder is not yet .Build() — callers may add
    /// further extensions (e.g. ExtractFrontMatter forcing YAML) before
    /// building.
    /// </summary>
    public static bool TryBuildPipelineBuilder(
        IEnumerable<string> extensions,
        bool advancedExtensions,
        out MarkdownPipelineBuilder builder,
        out string error)
    {
        builder = new MarkdownPipelineBuilder();
        error = string.Empty;

        if (advancedExtensions)
        {
            builder.UseAdvancedExtensions();
        }

        foreach (var raw in extensions)
        {
            var name = (raw ?? string.Empty).Trim().ToLowerInvariant();
            if (name.Length == 0)
            {
                continue;
            }
            switch (name)
            {
                case "advanced":
                    builder.UseAdvancedExtensions();
                    break;
                case "pipetables":
                    builder.UsePipeTables();
                    break;
                case "gridtables":
                    builder.UseGridTables();
                    break;
                case "tasklists":
                    builder.UseTaskLists();
                    break;
                case "footnotes":
                    builder.UseFootnotes();
                    break;
                case "footers":
                    builder.UseFooters();
                    break;
                case "citations":
                    builder.UseCitations();
                    break;
                case "abbreviations":
                    builder.UseAbbreviations();
                    break;
                case "autoidentifiers":
                    builder.UseAutoIdentifiers();
                    break;
                case "autolinks":
                    builder.UseAutoLinks();
                    break;
                case "definitionlists":
                    builder.UseDefinitionLists();
                    break;
                case "emphasisextras":
                    builder.UseEmphasisExtras();
                    break;
                case "figures":
                    builder.UseFigures();
                    break;
                case "mathematics":
                    builder.UseMathematics();
                    break;
                case "customcontainers":
                    builder.UseCustomContainers();
                    break;
                case "smartypants":
                    builder.UseSmartyPants();
                    break;
                case "yaml":
                    builder.UseYamlFrontMatter();
                    break;
                case "diagrams":
                    builder.UseDiagrams();
                    break;
                case "genericattributes":
                    builder.UseGenericAttributes();
                    break;
                case "listextras":
                    builder.UseListExtras();
                    break;
                case "globalization":
                    builder.UseGlobalization();
                    break;
                case "alertblocks":
                    builder.UseAlertBlocks();
                    break;
                case "pragmalines":
                    builder.UsePragmaLines();
                    break;
                case "softlinebreakashardline":
                    builder.UseSoftlineBreakAsHardlineBreak();
                    break;
                default:
                    error = ErrInvalidArgument;
                    return false;
            }
        }

        return true;
    }

    public static HtmlResult TrySucceedHtml(Func<string> fn)
    {
        try
        {
            return new HtmlResult { Html = fn() ?? string.Empty, Error = string.Empty };
        }
        catch (Exception)
        {
            return new HtmlResult { Html = string.Empty, Error = ErrInternal };
        }
    }

    public static HtmlResult FailHtml(string code) => new HtmlResult { Html = string.Empty, Error = code };

    public static TextResult TrySucceedText(Func<string> fn)
    {
        try
        {
            return new TextResult { Text = fn() ?? string.Empty, Error = string.Empty };
        }
        catch (Exception)
        {
            return new TextResult { Text = string.Empty, Error = ErrInternal };
        }
    }

    public static TextResult FailText(string code) => new TextResult { Text = string.Empty, Error = code };

    public static MarkdownResult TrySucceedMarkdown(Func<string> fn)
    {
        try
        {
            return new MarkdownResult { Markdown = fn() ?? string.Empty, Error = string.Empty };
        }
        catch (Exception)
        {
            return new MarkdownResult { Markdown = string.Empty, Error = ErrInternal };
        }
    }

    public static MarkdownResult FailMarkdown(string code) => new MarkdownResult { Markdown = string.Empty, Error = code };
}
