using System;

namespace CSharpMath.Atom.Atoms;

public sealed class Space : MathAtom {
    private readonly Structures.Space _space;

    public Space(Structures.Space space) {
        _space = space;
    }

    public float Length => _space.Length;
    public bool IsMu => _space.IsMu;
    public override bool ScriptsAllowed => false;
    public new Space Clone(bool finalize) => (Space)base.Clone(finalize);
    protected override MathAtom CloneInside(bool finalize) => new Space(_space);
    public float ActualLength<TFont, TGlyph>
        (Display.FrontEnd.FontMathTable<TFont, TGlyph> mathTable, TFont font)
        where TFont : Display.FrontEnd.IFont<TGlyph> => _space.ActualLength(mathTable, font);
    public override string DebugString => " ";
    public override bool Equals(object? obj) => obj is Space s && EqualsSpace(s);
    public bool EqualsSpace(Space otherSpace) =>
        EqualsAtom(otherSpace) && Math.Abs(Length - otherSpace.Length) < float.Epsilon && IsMu == otherSpace.IsMu;
    public override int GetHashCode() => (base.GetHashCode(), _space).GetHashCode();
}