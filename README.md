# markdown-tools

Composable [Axiom](https://axiom.dev) nodes for Markdown processing — render
to HTML, extract plain text, extract structural outline, extract YAML front
matter, and roundtrip-normalize — over a single `MarkdownDoc` envelope,
wrapping the BSD-2-Clause [Markdig](https://github.com/xoofx/markdig)
CommonMark engine with pinned pipeline extensions.

Built for the Axiom marketplace (handle `christiangeorgelucas`).

## Nodes

- **RenderHtml** — Markdown → HTML (CommonMark, with opt-in GFM/extension
  support: pipe/grid tables, task lists, footnotes, autolinks, definition
  lists, emphasis extras, YAML front matter, and more).
- **ExtractPlainText** — Markdown → plain text, with all formatting markup
  stripped.
- **ExtractOutline** — Markdown → structural outline: headings (with level
  and optional auto-generated anchor id), hyperlinks, fenced/indented code
  blocks (with language tag), and images, all in document order.
- **ExtractFrontMatter** — Markdown → `(front_matter, body)`, splitting a
  leading `---`-delimited YAML front matter block from the rest of the
  document. Always detects front matter regardless of the request's other
  extension flags.
- **NormalizeMarkdown** — Markdown → Markdown, re-rendered through Markdig's
  `NormalizeRenderer` into a canonicalized form (idempotent, semantics-
  preserving).

Every node is stateless, deterministic for a fixed input, and returns a
structured `error` field on malformed input rather than crashing — including
on an unrecognized pipeline-extension name.

## Pipeline extensions

Every node takes the same `MarkdownDoc` envelope: `markdown` (the source
text), `extensions` (a list of Markdig extension names to enable — see
`messages/messages.proto` for the full vocabulary), and `advanced_extensions`
(a shortcut for Markdig's own `UseAdvancedExtensions()` bundle — note this
bundle deliberately excludes YAML front matter, Emoji, SmartyPants, and
softline-as-hardline; request those individually if needed).

## License

MIT. Markdig itself is BSD-2-Clause with zero further transitive
dependencies on `net8.0`.
