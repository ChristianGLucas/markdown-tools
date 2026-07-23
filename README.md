# markdown-tools

Composable [Axiom](https://axiomide.com) nodes for Markdown processing — render
to HTML, extract plain text, extract structural outline, extract YAML front
matter, and roundtrip-normalize — over a single `MarkdownDoc` envelope,
wrapping the BSD-2-Clause [Markdig](https://github.com/xoofx/markdig)
CommonMark engine with pinned pipeline extensions.

Built for the Axiom marketplace (handle `christiangeorgelucas`).

## Use it from your agent or app

Every node in this package is a **live, auto-scaling API endpoint** on the
[Axiom](https://axiomide.com) marketplace — call it from an AI agent or your own
code, with nothing to self-host.

**📦 See it on the marketplace:**
https://dev.axiomide.com/marketplace/christiangeorgelucas/markdown-tools@0.1.0

**Hook it up to an AI agent (MCP).** Add Axiom's hosted MCP server to any MCP
client and every node becomes a typed tool your agent can call — search the
catalog, inspect a schema, and invoke it directly.

```bash
# Claude Code
claude mcp add --transport http axiom https://api.axiomide.com/mcp \
  --header "Authorization: Bearer $AXIOM_API_KEY"
```

Claude Desktop, Cursor, or any config-based client:

```json
{
  "mcpServers": {
    "axiom": {
      "type": "http",
      "url": "https://api.axiomide.com/mcp",
      "headers": { "Authorization": "Bearer YOUR_AXIOM_API_KEY" }
    }
  }
}
```

**Call it from the CLI.**

```bash
axiom invoke christiangeorgelucas/markdown-tools/RenderHtml --input '{ ... }'
```

**Call it over HTTP.**

```bash
curl -X POST https://api.axiomide.com/invocations/v1/nodes/christiangeorgelucas/markdown-tools/0.1.0/RenderHtml \
  -H "Authorization: Bearer $AXIOM_API_KEY" \
  -H 'Content-Type: application/json' \
  -d '{ ... }'
```

> Input/output schema for each node is on the marketplace page above, or via
> `axiom inspect node christiangeorgelucas/markdown-tools/RenderHtml`.

### Get started free

Install the CLI:

```bash
# macOS / Linux — Homebrew
brew install axiomide/tap/axiom

# macOS / Linux — install script
curl -fsSL https://raw.githubusercontent.com/AxiomIDE/axiom-releases/main/install.sh | sh
```

**Windows:** download the `windows/amd64` `.zip` from the
[releases page](https://github.com/AxiomIDE/axiom-releases/releases), unzip it,
and put `axiom.exe` on your `PATH`.

Then `axiom version` to verify, `axiom login` (GitHub or Google) to authenticate,
and create an API key under **Console → API Keys**. Docs and sign-up at
**[axiomide.com](https://axiomide.com)**.

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
