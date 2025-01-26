using System.Text;

namespace CSharpMath.Atom.Atoms;

public sealed class Radical(MathList degree, MathList radicand) : MathAtom, IMathListContainer {
    public MathList Degree { get; } = degree;

    /// <summary>Whatever is under the square root sign</summary>
    public MathList Radicand { get; } = radicand;

    System.Collections.Generic.IEnumerable<MathList> IMathListContainer.InnerLists =>
        [Degree, Radicand];
    public new Radical Clone(bool finalize) => (Radical)base.Clone(finalize);
    protected override MathAtom CloneInside(bool finalize) =>
        new Radical(Degree.Clone(finalize), Radicand.Clone(finalize));
    public override bool ScriptsAllowed => true;
    public override string DebugString =>
        new StringBuilder(@"\sqrt")
            .AppendInBracketsOrNothing(Degree?.DebugString)
            .AppendInBracesOrLiteralNull(Radicand?.DebugString)
            .AppendDebugStringOfScripts(this).ToString();
    public override int GetHashCode() =>
        (base.GetHashCode(), Degree, Radicand).GetHashCode();
}