using System.Drawing;


namespace CSharpMath.Atom.Atoms;

/// <summary>A placeholder for future input</summary>
public sealed class Placeholder(string nucleus, Color? color) : MathAtom(nucleus) {
    public Color? Color { get; set; } = color;
    public override bool ScriptsAllowed => true;
    public new Placeholder Clone(bool finalize) => (Placeholder)base.Clone(finalize);
    protected override MathAtom CloneInside(bool finalize) => new Placeholder(Nucleus, Color);
}