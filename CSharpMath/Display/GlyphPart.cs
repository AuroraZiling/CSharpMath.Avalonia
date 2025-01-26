namespace CSharpMath.Display;

/// <summary>Represents a part of a glyph used for constructing a large vertical or horizontal glyph.</summary>
public class GlyphPart<TGlyph>(
    TGlyph glyph,
    float fullAdvance,
    float startConnectorLength,
    float endConnectorLength,
    bool isExtender) {
    public TGlyph Glyph { get; } = glyph;
    public float FullAdvance { get; } = fullAdvance;
    public float StartConnectorLength { get; } = startConnectorLength;
    public float EndConnectorLength { get; } = endConnectorLength;

    /// <summary>If the glyph is an extender, it can be skipped or repeated.</summary>
    public bool IsExtender { get; } = isExtender;

    public override string ToString() =>
        $"[{nameof(GlyphPart<TGlyph>)}: {nameof(Glyph)}={Glyph}, {nameof(FullAdvance)}={FullAdvance}, {nameof(StartConnectorLength)}={StartConnectorLength}, {nameof(EndConnectorLength)}={EndConnectorLength}, {nameof(IsExtender)}={IsExtender}]";
}