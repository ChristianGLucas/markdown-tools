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

public class NormalizeMarkdownTest
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

    // Independent oracle #1 (formal invariant, not self-consistency): a
    // normalizer's defining property is IDEMPOTENCE — normalizing already-
    // normalized output must be a no-op. This is true by definition of what
    // "normalized form" means, independent of Markdig's specific rendering
    // choices, so it is a real correctness property, not a tautological
    // round-trip through the same call.
    [Fact]
    public void TestNormalizeMarkdown_Idempotent_SecondPassIsNoOp()
    {
        IAxiomContext ax = new TestContext();
        var input = new MarkdownDoc { Markdown = "Title\n=====\n\n*  item one\n*  item two\n" };
        var firstPass = NormalizeMarkdownNode.NormalizeMarkdown(ax, input);
        Assert.Equal(string.Empty, firstPass.Error);

        var secondInput = new MarkdownDoc { Markdown = firstPass.Markdown };
        var secondPass = NormalizeMarkdownNode.NormalizeMarkdown(ax, secondInput);

        Assert.Equal(firstPass.Markdown, secondPass.Markdown);
        Assert.Equal(string.Empty, secondPass.Error);
    }

    // Independent oracle #2: normalization must be semantics-preserving — the
    // plain-text content of a document must survive normalization exactly,
    // even though its Markdown surface syntax may change. This exercises a
    // different code path (ExtractPlainText's renderer) than Normalize's own
    // renderer, so it is not the same operation checked against itself.
    [Fact]
    public void TestNormalizeMarkdown_PreservesPlainTextContent()
    {
        IAxiomContext ax = new TestContext();
        var original = "Title\n=====\n\nSome **bold** text with a [link](https://example.com).\n";
        var normalizeInput = new MarkdownDoc { Markdown = original };
        var normalized = NormalizeMarkdownNode.NormalizeMarkdown(ax, normalizeInput);
        Assert.Equal(string.Empty, normalized.Error);

        var originalText = ExtractPlainTextNode.ExtractPlainText(ax, new MarkdownDoc { Markdown = original });
        var normalizedText = ExtractPlainTextNode.ExtractPlainText(ax, new MarkdownDoc { Markdown = normalized.Markdown });

        Assert.Equal(originalText.Text, normalizedText.Text);
    }

    [Fact]
    public void TestNormalizeMarkdown_EmptyInput_ReturnsEmptyMarkdown()
    {
        IAxiomContext ax = new TestContext();
        var input = new MarkdownDoc { Markdown = "" };
        var result = NormalizeMarkdownNode.NormalizeMarkdown(ax, input);
        Assert.Equal(string.Empty, result.Markdown);
        Assert.Equal(string.Empty, result.Error);
    }

    [Fact]
    public void TestNormalizeMarkdown_UnknownExtension_ReturnsStructuredError()
    {
        IAxiomContext ax = new TestContext();
        var input = new MarkdownDoc { Markdown = "# Hi" };
        input.Extensions.Add("totally-bogus");
        var result = NormalizeMarkdownNode.NormalizeMarkdown(ax, input);
        Assert.Equal(string.Empty, result.Markdown);
        Assert.Equal("INVALID_ARGUMENT", result.Error);
    }
}
