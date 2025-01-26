using System.Drawing;

namespace CSharpMath.Display;

public class GlyphInfo<TGlyph>(TGlyph glyph, float kern = 0) {
    public TGlyph Glyph { get; } = glyph;
    public float KernAfterGlyph { get; set; } = kern;
    public Color? Foreground { get; set; }
    public void Deconstruct(out TGlyph glyph, out float kernAfter, out Color? foreground) =>
        (glyph, kernAfter, foreground) = (Glyph, KernAfterGlyph, Foreground);
}