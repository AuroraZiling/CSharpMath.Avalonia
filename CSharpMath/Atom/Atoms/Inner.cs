using System.Text;

namespace CSharpMath.Atom.Atoms;

/// <summary>An inner atom, i.e. embedded math list</summary>
public sealed class Inner(Boundary left, MathList innerList, Boundary right) : MathAtom, IMathListContainer {
    public MathList InnerList { get; } = innerList;
    public Boundary LeftBoundary { get; } = left;
    public Boundary RightBoundary { get; } = right;

    System.Collections.Generic.IEnumerable<MathList> IMathListContainer.InnerLists =>
        [InnerList];
    public override bool ScriptsAllowed => true;
    public new Inner Clone(bool finalize) => (Inner)base.Clone(finalize);
    protected override MathAtom CloneInside(bool finalize) =>
        new Inner(LeftBoundary, InnerList.Clone(finalize), RightBoundary);

    private bool EqualsInner(Inner otherInner) =>
        EqualsAtom(otherInner)
        && InnerList.NullCheckingStructuralEquality(otherInner.InnerList)
        && LeftBoundary.NullCheckingStructuralEquality(otherInner.LeftBoundary)
        && RightBoundary.NullCheckingStructuralEquality(otherInner.RightBoundary);
    public override bool Equals(object? obj) => obj is Inner i && EqualsInner(i);
    public override int GetHashCode() =>
        (base.GetHashCode(), InnerList, LeftBoundary, RightBoundary).GetHashCode();
    public override string DebugString =>
        new StringBuilder(@"\inner")
            .AppendInBracesOrEmptyBraces(LeftBoundary.Nucleus)
            .AppendInBracesOrLiteralNull(InnerList.DebugString)
            .AppendInBracesOrEmptyBraces(RightBoundary.Nucleus)
            .AppendDebugStringOfScripts(this).ToString();
}