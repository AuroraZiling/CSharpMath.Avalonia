using System.Text;

namespace CSharpMath.Atom.Atoms;

/// <summary>An overlined atom</summary>
public sealed class Overline(MathList innerList) : MathAtom, IMathListContainer {
    public MathList InnerList { get; } = innerList;

    System.Collections.Generic.IEnumerable<MathList> IMathListContainer.InnerLists =>
        [InnerList];
    public override bool ScriptsAllowed => true;
    public new Overline Clone(bool finalize) => (Overline)base.Clone(finalize);
    protected override MathAtom CloneInside(bool finalize) =>
        new Overline(InnerList.Clone(finalize));
    public override string DebugString =>
        new StringBuilder(@"\overline")
            .AppendInBracesOrLiteralNull(InnerList?.DebugString)
            .ToString();

    private bool EqualsOverline(Overline other) =>
        EqualsAtom(other) && InnerList.NullCheckingStructuralEquality(other.InnerList);
    public override bool Equals(object? obj) => obj is Overline o && EqualsOverline(o);
    public override int GetHashCode() =>
        (base.GetHashCode(), InnerList).GetHashCode();
}