namespace CSharpMath.Atom.Atoms;

/// <summary>AMSMath class 5: Right/closing delimiter, e.g. ), ], }, \rangle</summary>
public sealed class Close(string nucleus) : MathAtom(nucleus) {
    public override bool ScriptsAllowed => true;
    public new Close Clone(bool finalize) => (Close)base.Clone(finalize);
    protected override MathAtom CloneInside(bool finalize) => new Close(Nucleus);
}