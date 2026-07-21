// TESTS — delete this block when done ─────────────────────────────────────────
// Tests are required to publish this package. The publish pipeline runs your
// tests as a quality gate — a package will not be published if tests fail or
// do not meet the minimum requirements.
//
// Requirements checked before publishing:
//   - At least one test per node
//   - All tests must pass
//   - Output fields must be meaningfully asserted — not just null-checked
//
// The generated test below is a starting point. Replace the TODO comment with
// real assertions that verify your node returns correct data for known inputs.
// Think: given a specific input, what should the output fields contain?
//
// Run your tests locally at any time:
//   axiom test

using Axiom;
using Gen;
using System.Collections.Generic;
using Xunit;

namespace Nodes;

public class ExtractOutlineTest
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

    // Independent oracle: heading level is definitionally the count of
    // leading '#' characters (ATX heading, CommonMark spec) — hand-derivable
    // without running the parser.
    [Fact]
    public void TestExtractOutline_Headings_LevelMatchesHashCount()
    {
        IAxiomContext ax = new TestContext();
        var input = new MarkdownDoc { Markdown = "# Title\n\n## Section One\n\n### Sub Section\n" };
        var result = ExtractOutlineNode.ExtractOutline(ax, input);
        Assert.Equal(3, result.Headings.Count);
        Assert.Equal(1, result.Headings[0].Level);
        Assert.Equal("Title", result.Headings[0].Text);
        Assert.Equal(2, result.Headings[1].Level);
        Assert.Equal("Section One", result.Headings[1].Text);
        Assert.Equal(3, result.Headings[2].Level);
        Assert.Equal("Sub Section", result.Headings[2].Text);
        Assert.Equal(string.Empty, result.Error);
    }

    // Independent oracle: [text](url "title") is CommonMark's defined inline
    // link syntax — text/url/title map directly to the three source fields.
    [Fact]
    public void TestExtractOutline_Link_FieldsMatchSourceSyntax()
    {
        IAxiomContext ax = new TestContext();
        var input = new MarkdownDoc { Markdown = "Go to [Example](https://example.com \"Example Site\") now." };
        var result = ExtractOutlineNode.ExtractOutline(ax, input);
        Assert.Single(result.Links);
        Assert.Equal("Example", result.Links[0].Text);
        Assert.Equal("https://example.com", result.Links[0].Url);
        Assert.Equal("Example Site", result.Links[0].Title);
        Assert.Equal(string.Empty, result.Error);
    }

    // Independent oracle: a fenced code block's info string is definitionally
    // its language tag (CommonMark spec); content is the literal fenced text.
    [Fact]
    public void TestExtractOutline_FencedCodeBlock_LanguageAndContent()
    {
        IAxiomContext ax = new TestContext();
        var input = new MarkdownDoc { Markdown = "```python\nprint(\"hi\")\n```\n" };
        var result = ExtractOutlineNode.ExtractOutline(ax, input);
        Assert.Single(result.CodeBlocks);
        Assert.Equal("python", result.CodeBlocks[0].Language);
        Assert.Contains("print(\"hi\")", result.CodeBlocks[0].Content);
        Assert.Equal(string.Empty, result.Error);
    }

    // Independent oracle: ![alt](url) is CommonMark's defined image syntax —
    // it must be reported as an image, never as a link.
    [Fact]
    public void TestExtractOutline_Image_ReportedSeparatelyFromLinks()
    {
        IAxiomContext ax = new TestContext();
        var input = new MarkdownDoc { Markdown = "![A cat](https://example.com/cat.png)" };
        var result = ExtractOutlineNode.ExtractOutline(ax, input);
        Assert.Empty(result.Links);
        Assert.Single(result.Images);
        Assert.Equal("A cat", result.Images[0].Alt);
        Assert.Equal("https://example.com/cat.png", result.Images[0].Url);
        Assert.Equal(string.Empty, result.Error);
    }

    [Fact]
    public void TestExtractOutline_UnknownExtension_ReturnsStructuredError()
    {
        IAxiomContext ax = new TestContext();
        var input = new MarkdownDoc { Markdown = "# Hi" };
        input.Extensions.Add("does-not-exist");
        var result = ExtractOutlineNode.ExtractOutline(ax, input);
        Assert.Empty(result.Headings);
        Assert.Equal("INVALID_ARGUMENT", result.Error);
    }
}
