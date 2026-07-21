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

public class ExtractPlainTextTest
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

    // Independent oracle: stripping Markdown formatting from a single-line
    // sentence with emphasis markup must leave exactly the human-readable
    // words with no markup characters — a hand-derivable expectation from
    // what "plain text" means, independent of Markdig's internals.
    [Fact]
    public void TestExtractPlainText_StripsEmphasisMarkup()
    {
        IAxiomContext ax = new TestContext();
        var input = new MarkdownDoc { Markdown = "This is **bold** and *italic* text." };
        var result = ExtractPlainTextNode.ExtractPlainText(ax, input);
        Assert.Equal("This is bold and italic text.\n", result.Text);
        Assert.Equal(string.Empty, result.Error);
    }

    // A link's plain text is its display text, not its URL or the [..](..) syntax.
    [Fact]
    public void TestExtractPlainText_LinkYieldsDisplayTextOnly()
    {
        IAxiomContext ax = new TestContext();
        var input = new MarkdownDoc { Markdown = "See [the docs](https://example.com/docs) for more." };
        var result = ExtractPlainTextNode.ExtractPlainText(ax, input);
        Assert.Equal("See the docs for more.\n", result.Text);
        Assert.DoesNotContain("https://", result.Text);
        Assert.Equal(string.Empty, result.Error);
    }

    [Fact]
    public void TestExtractPlainText_EmptyInput_ReturnsEmptyText()
    {
        IAxiomContext ax = new TestContext();
        var input = new MarkdownDoc { Markdown = "" };
        var result = ExtractPlainTextNode.ExtractPlainText(ax, input);
        Assert.Equal(string.Empty, result.Text);
        Assert.Equal(string.Empty, result.Error);
    }

    [Fact]
    public void TestExtractPlainText_UnknownExtension_ReturnsStructuredError()
    {
        IAxiomContext ax = new TestContext();
        var input = new MarkdownDoc { Markdown = "hello" };
        input.Extensions.Add("bogus");
        var result = ExtractPlainTextNode.ExtractPlainText(ax, input);
        Assert.Equal(string.Empty, result.Text);
        Assert.Equal("INVALID_ARGUMENT", result.Error);
    }
}
