using System.Text;
using System.Drawing;

namespace CSharpMath.Atom.Atoms;

public sealed class Colored(Color color, MathList innerList) : MathAtom(string.Empty), IMathListContainer {
    public Color Color { get; set; } = color;
    public MathList InnerList { get; } = innerList;

    System.Collections.Generic.IEnumerable<MathList> IMathListContainer.InnerLists =>
        [InnerList];

    public override string DebugString =>
        new StringBuilder(@"\color")
            .AppendInBracesOrLiteralNull(Color.ToString())
            .AppendInBracesOrLiteralNull(InnerList.DebugString).ToString();
    public override bool ScriptsAllowed => false;
    public new Colored Clone(bool finalize) => (Colored)base.Clone(finalize);
    protected override MathAtom CloneInside(bool finalize) =>
        new Colored(Color, InnerList.Clone(finalize));
    public override int GetHashCode() => (base.GetHashCode(), Color, InnerList).GetHashCode();
}