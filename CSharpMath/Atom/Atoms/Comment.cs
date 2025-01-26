namespace CSharpMath.Atom.Atoms;

public sealed class Comment(string nucleus) : MathAtom(nucleus) {
    public override bool ScriptsAllowed => false;
    public new Comment Clone(bool finalize) => (Comment)base.Clone(finalize);
    protected override MathAtom CloneInside(bool finalize) => new Comment(Nucleus);
}