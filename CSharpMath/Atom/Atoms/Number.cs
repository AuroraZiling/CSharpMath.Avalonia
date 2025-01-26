namespace CSharpMath.Atom.Atoms;

/// <summary>Class 0: ordinary for Arabic numerals 0–9 and the decimal dot .</summary>
public sealed class Number(string number) : MathAtom(number) {
    public override bool ScriptsAllowed => true;
    public new Number Clone(bool finalize) => (Number)base.Clone(finalize);
    protected override MathAtom CloneInside(bool finalize) => new Number(Nucleus);
    public Ordinary ToOrdinary(
        System.Func<string, FontStyle, string> fontChanger) =>
        ApplyCommonPropertiesOn(false, new Ordinary(fontChanger(Nucleus, FontStyle)));
}