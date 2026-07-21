using Axiom;
using Gen;
using System.Collections.Generic;
using Xunit;

namespace Nodes;

public class RenderHtmlTest
{
    // A no-op AxiomContext a node author edits to drive a specific scenario.
    // Reflection exposes an empty graph, mutation is a sink. Implement only
    // what your assertions need.
    private sealed class TestContext : IAxiomContext
    {
        public IAxiomContext.ILogger Log() => new NoopLog();
        public IAxiomContext.ISecrets Secrets() => new NoopSecrets();
        public string ExecutionId() => "test-execution-id";
        public string FlowId() => "test-flow-id";
        public string TenantId() => "test-tenant-id";
        public IAxiomContext.IReflection Reflection() => new NoopReflection();
        public IAxiomContext.IMutation Mutation() => new NoopMutation();

        private sealed class NoopLog : IAxiomContext.ILogger
        {
            public void Debug(string m, IDictionary<string, string>? a = null) {}
            public void Info(string m, IDictionary<string, string>? a = null) {}
            public void Warn(string m, IDictionary<string, string>? a = null) {}
            public void Error(string m, IDictionary<string, string>? a = null) {}
        }
        private sealed class NoopSecrets : IAxiomContext.ISecrets
        {
            public (string Value, bool Found) Get(string name) => ("", false);
            public IAxiomContext.SecretStatus Status(string name) => IAxiomContext.SecretStatus.Unset;
        }
        private sealed class NoopReflection : IAxiomContext.IReflection
        {
            public IAxiomContext.IFlowReflection Flow() => new NoopFlow();
            private sealed class NoopFlow : IAxiomContext.IFlowReflection
            {
                public IReadOnlyList<IAxiomContext.ReflectionNode> Nodes() => new List<IAxiomContext.ReflectionNode>();
                public IReadOnlyList<IAxiomContext.ReflectionEdge> Edges() => new List<IAxiomContext.ReflectionEdge>();
                public IReadOnlyList<IAxiomContext.ReflectionEdge> LoopEdges() => new List<IAxiomContext.ReflectionEdge>();
                public IAxiomContext.FlowPosition Position() => new IAxiomContext.FlowPosition(0, 0, new Dictionary<int, int>(), new List<string>());
                public string GraphId() => "";
            }
        }
        private sealed class NoopMutation : IAxiomContext.IMutation
        {
            public IAxiomContext.IFlowMutation Flow() => new FlowMut();
            private sealed class FlowMut : IAxiomContext.IFlowMutation
            {
                public int AddNode(string pkg, string ver, IAxiomContext.CanvasPosition? pos) => 0;
                public void AddEdge(int src, int dst, IAxiomContext.EdgeCondition? cond) {}
            }
        }
    }

    // Independent oracle: CommonMark spec example 1 — a plain ATX heading
    // renders to a fixed, spec-defined HTML shape with no extensions needed.
    [Fact]
    public void TestRenderHtml_PlainHeading_MatchesCommonMarkSpec()
    {
        IAxiomContext ax = new TestContext();
        var input = new MarkdownDoc { Markdown = "# Hello World" };
        var result = RenderHtmlNode.RenderHtml(ax, input);
        Assert.Equal("<h1>Hello World</h1>\n", result.Html);
        Assert.Equal(string.Empty, result.Error);
    }

    // Independent oracle: CommonMark emphasis rules — ** wraps <strong>, * wraps <em>.
    [Fact]
    public void TestRenderHtml_Emphasis_MatchesCommonMarkSpec()
    {
        IAxiomContext ax = new TestContext();
        var input = new MarkdownDoc { Markdown = "This is **bold** and *italic*." };
        var result = RenderHtmlNode.RenderHtml(ax, input);
        Assert.Equal("<p>This is <strong>bold</strong> and <em>italic</em>.</p>\n", result.Html);
        Assert.Equal(string.Empty, result.Error);
    }

    // Independent oracle: GitHub-Flavored-Markdown pipe-table spec — a 2-column,
    // 1-data-row pipe table renders to a fixed <table>/<thead>/<tbody> shape.
    // Requires the "pipetables" extension; plain CommonMark treats "|" as
    // literal text, so this also proves the extensions list is actually wired in.
    [Fact]
    public void TestRenderHtml_PipeTableExtension_MatchesGfmTableSpec()
    {
        IAxiomContext ax = new TestContext();
        var input = new MarkdownDoc
        {
            Markdown = "| A | B |\n| - | - |\n| 1 | 2 |\n",
        };
        input.Extensions.Add("pipetables");
        var result = RenderHtmlNode.RenderHtml(ax, input);
        Assert.Contains("<table>", result.Html);
        Assert.Contains("<th>A</th>", result.Html);
        Assert.Contains("<th>B</th>", result.Html);
        Assert.Contains("<td>1</td>", result.Html);
        Assert.Contains("<td>2</td>", result.Html);
        Assert.Equal(string.Empty, result.Error);
    }

    // Without the extension, a pipe table is NOT recognized as a table by
    // plain CommonMark — proves extensions are opt-in, not always-on.
    [Fact]
    public void TestRenderHtml_PipeTableWithoutExtension_IsNotATable()
    {
        IAxiomContext ax = new TestContext();
        var input = new MarkdownDoc { Markdown = "| A | B |\n| - | - |\n| 1 | 2 |\n" };
        var result = RenderHtmlNode.RenderHtml(ax, input);
        Assert.DoesNotContain("<table>", result.Html);
        Assert.Equal(string.Empty, result.Error);
    }

    [Fact]
    public void TestRenderHtml_UnknownExtension_ReturnsStructuredError()
    {
        IAxiomContext ax = new TestContext();
        var input = new MarkdownDoc { Markdown = "# Hi" };
        input.Extensions.Add("not-a-real-extension");
        var result = RenderHtmlNode.RenderHtml(ax, input);
        Assert.Equal(string.Empty, result.Html);
        Assert.Equal("INVALID_ARGUMENT", result.Error);
    }

    [Fact]
    public void TestRenderHtml_EmptyInput_RendersEmptyHtml()
    {
        IAxiomContext ax = new TestContext();
        var input = new MarkdownDoc { Markdown = "" };
        var result = RenderHtmlNode.RenderHtml(ax, input);
        Assert.Equal(string.Empty, result.Html);
        Assert.Equal(string.Empty, result.Error);
    }
}
