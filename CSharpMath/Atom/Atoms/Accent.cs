using System.Text;

namespace CSharpMath.Atom.Atoms;

/// <summary>An accented atom</summary>
public sealed class Accent(string value, MathList? innerList = null) : MathAtom(value), IMathListContainer {
    public MathList InnerList { get; } = innerList ?? new MathList();

    System.Collections.Generic.IEnumerable<MathList> IMathListContainer.InnerLists =>
        [InnerList];

    public override string DebugString =>
        new StringBuilder(@"\accent")
            .AppendInBracesOrLiteralNull(Nucleus)
            .AppendInBracesOrLiteralNull(InnerList.DebugString)
            .ToString();
    public new Accent Clone(bool finalize) => (Accent)base.Clone(finalize);
    protected override MathAtom CloneInside(bool finalize) =>
        new Accent(Nucleus, InnerList.Clone(finalize));
    public override bool ScriptsAllowed => true;
    public bool EqualsAccent(Accent other) =>
        EqualsAtom(other) && InnerList.NullCheckingStructuralEquality(other?.InnerList);
    public override bool Equals(object? obj) => obj is Accent a && EqualsAccent(a);
    public override int GetHashCode() => (base.GetHashCode(), InnerList).GetHashCode();
}