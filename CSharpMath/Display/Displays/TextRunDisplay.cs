using CSharpMath.Atom;
using Color = CSharpMath.Structures.Color;
using System.Drawing;
using System.Linq;

namespace CSharpMath.Display.Displays {
  using FrontEnd;
  /// <summary>Corresponds to MTCTLineDisplay in iOSMath.</summary> 
  public class TextRunDisplay<TFont, TGlyph> : IDisplay<TFont, TGlyph> where TFont : IFont<TGlyph> {
    public TextRunDisplay(
      AttributedGlyphRun<TFont, TGlyph> run, 
      Range range, 
      TypesettingContext<TFont, TGlyph> context) {
      var font = run.Font;
      Run = run;
      Range = range;
      Width = context.GlyphBoundsProvider.GetTypographicWidth(font, run);
      // Compute ascent and descent
      var rects =
        context.GlyphBoundsProvider.GetBoundingRectsForGlyphs(font, Run.Glyphs, Run.GlyphInfos.Count);
      Ascent = rects.IsEmpty() ? 0 : rects.Max(rect => rect.Bottom); // Convert to non-flipped naming here, 
      Descent = rects.IsEmpty() ? 0 : rects.Max(rect => -rect.Y);
    }
    public AttributedGlyphRun<TFont, TGlyph> Run { get; }
    
    public Range Range { get; set; }
    public float Width { get; set; }
    public float Ascent { get; set; }
    public float Descent { get; set; }
    public PointF Position { get; set; }
    public bool HasScript { get; set; }
    public Color? TextColor { get; set; }
    public void Draw(IGraphicsContext<TFont, TGlyph> context) {
      context.SaveState();
      context.DrawGlyphRunWithOffset(Run, Position, TextColor);
      context.RestoreState();
    }
    public void SetTextColorRecursive(Color? textColor) => TextColor ??= textColor;
    public override string ToString() => Run.Text.ToString();
  }
}