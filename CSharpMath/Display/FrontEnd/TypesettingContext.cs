using System;

namespace CSharpMath.Display.FrontEnd;

/// <summary>
/// A wrapper class holding everything the core needs to have in order to layout the LaTeX.
/// </summary>
public class TypesettingContext<TFont, TGlyph>(
    Func<TFont, float, TFont> mathFontCloner,
    IGlyphBoundsProvider<TFont, TGlyph> glyphBoundsProvider,
    IGlyphFinder<TFont, TGlyph> glyphFinder,
    FontMathTable<TFont, TGlyph> mathTable)
    where TFont : IFont<TGlyph> {
    public IGlyphBoundsProvider<TFont, TGlyph> GlyphBoundsProvider { get; } = glyphBoundsProvider;
    public IGlyphFinder<TFont, TGlyph> GlyphFinder { get; } = glyphFinder;
    public FontMathTable<TFont, TGlyph> MathTable { get; } = mathTable;
    public Func<TFont, float, TFont> MathFontCloner { get; } = mathFontCloner;
}