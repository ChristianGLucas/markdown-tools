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

public class ExtractFrontMatterTest
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

    // Independent oracle: YAML front matter is definitionally the text
    // between the opening and closing `---` delimiters (front-matter
    // convention shared by Jekyll/Hugo/Markdig) — the raw block must
    // contain exactly the source key:value lines, and the body must be
    // exactly what follows the closing delimiter.
    [Fact]
    public void TestExtractFrontMatter_PresentBlock_SplitsRawAndBody()
    {
        IAxiomContext ax = new TestContext();
        var input = new MarkdownDoc
        {
            Markdown = "---\ntitle: Test Doc\nauthor: Ada\n---\n\nBody content here.\n",
        };
        var result = ExtractFrontMatterNode.ExtractFrontMatter(ax, input);
        Assert.True(result.HasFrontMatter);
        Assert.Contains("title: Test Doc", result.FrontMatter);
        Assert.Contains("author: Ada", result.FrontMatter);
        Assert.DoesNotContain("---", result.FrontMatter);
        Assert.Equal("Body content here.", result.Body.Trim());
        Assert.Equal(string.Empty, result.Error);
    }

    // No leading `---` block => no front matter, and the body is the whole
    // original document, untouched.
    [Fact]
    public void TestExtractFrontMatter_NoBlock_ReturnsFullBodyUnchanged()
    {
        IAxiomContext ax = new TestContext();
        var input = new MarkdownDoc { Markdown = "# Just a heading\n\nNo front matter here.\n" };
        var result = ExtractFrontMatterNode.ExtractFrontMatter(ax, input);
        Assert.False(result.HasFrontMatter);
        Assert.Equal(string.Empty, result.FrontMatter);
        Assert.Equal(input.Markdown, result.Body);
        Assert.Equal(string.Empty, result.Error);
    }

    // This node must force-enable YAML front matter detection even when the
    // caller only requests advanced_extensions=true, because Markdig's own
    // advanced-extensions bundle deliberately excludes YAML front matter —
    // proves the node compensates rather than silently returning false.
    [Fact]
    public void TestExtractFrontMatter_AdvancedExtensionsAlone_StillDetectsFrontMatter()
    {
        IAxiomContext ax = new TestContext();
        var input = new MarkdownDoc
        {
            Markdown = "---\nkey: value\n---\n\nBody.\n",
            AdvancedExtensions = true,
        };
        var result = ExtractFrontMatterNode.ExtractFrontMatter(ax, input);
        Assert.True(result.HasFrontMatter);
        Assert.Contains("key: value", result.FrontMatter);
        Assert.Equal(string.Empty, result.Error);
    }

    [Fact]
    public void TestExtractFrontMatter_UnknownExtension_ReturnsStructuredError()
    {
        IAxiomContext ax = new TestContext();
        var input = new MarkdownDoc { Markdown = "---\nkey: value\n---\nBody.\n" };
        input.Extensions.Add("nope-not-real");
        var result = ExtractFrontMatterNode.ExtractFrontMatter(ax, input);
        Assert.False(result.HasFrontMatter);
        Assert.Equal("INVALID_ARGUMENT", result.Error);
    }
}
