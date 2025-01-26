using System;

namespace CSharpMath.Atom.Atoms;

public sealed class Space(Structures.Space space) : MathAtom {
    public float Length => space.Length;
    public bool IsMu => space.IsMu;
    public override bool ScriptsAllowed => false;
    public new Space Clone(bool finalize) => (Space)base.Clone(finalize);
    protected override MathAtom CloneInside(bool finalize) => new Space(space);
    public float ActualLength<TFont, TGlyph>
        (Display.FrontEnd.FontMathTable<TFont, TGlyph> mathTable, TFont font)
        where TFont : Display.FrontEnd.IFont<TGlyph> => space.ActualLength(mathTable, font);
    public override string DebugString => " ";
    public override bool Equals(object? obj) => obj is Space s && EqualsSpace(s);
    public bool EqualsSpace(Space otherSpace) =>
        EqualsAtom(otherSpace) && Math.Abs(Length - otherSpace.Length) < float.Epsilon && IsMu == otherSpace.IsMu;
    public override int GetHashCode() => (base.GetHashCode(), _space: space).GetHashCode();
}