using System.Drawing;
using CSharpMath.Atom;

namespace CSharpMath.Display.Displays {
  using FrontEnd;
  public class GlyphDisplay<TFont, TGlyph>(
      TGlyph glyph,
      Range range,
      TFont font,
      float ascent,
      float descent,
      float width)
      : IGlyphDisplay<TFont, TGlyph>
      where TFont : IFont<TGlyph> {
      public float Ascent {
          get => field - ShiftDown;
      } = ascent;

      public float Descent {
          get => field + ShiftDown;
      } = descent;

      public float Width { get; } = width;
      public Range Range { get; } = range;
    public PointF Position { get; set; }
    public bool HasScript { get; set; }
    public float ShiftDown { get; set; }
    public TGlyph Glyph { get; } = glyph;
    public TFont Font { get; } = font;

    public void Draw(IGraphicsContext<TFont, TGlyph> context) {
      this.DrawBackground(context);
      context.SaveState();
      using var glyphs = new Structures.RentedArray<TGlyph>(Glyph);
      using var positions = new Structures.RentedArray<PointF>(new PointF());
      context.Translate(new PointF(Position.X, Position.Y - ShiftDown));
      context.SetTextPosition(new PointF());
      context.DrawGlyphsAtPoints(glyphs.Result, Font, positions.Result, TextColor);
      context.RestoreState();
    }
    public Color? TextColor { get; set; }
    public void SetTextColorRecursive(Color? textColor) => TextColor ??= textColor;
    public Color? BackColor { get; set; }
    public override string ToString() => Glyph?.ToString() ?? "<null>";
  }
}
