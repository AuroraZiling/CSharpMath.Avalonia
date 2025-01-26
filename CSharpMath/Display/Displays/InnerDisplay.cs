using System.Drawing;
using CSharpMath.Atom;

namespace CSharpMath.Display.Displays;

using FrontEnd;
public class InnerDisplay<TFont, TGlyph>(
    ListDisplay<TFont, TGlyph> inner,
    IGlyphDisplay<TFont, TGlyph>? left,
    IGlyphDisplay<TFont, TGlyph>? right,
    Range range)
    : IDisplay<TFont, TGlyph>
    where TFont : IFont<TGlyph> {
    ///<summary>A display representing the inner list that can be wrapped in delimiters.
    ///It's position is relative to the parent is not treated as a sub-display.</summary>
    public ListDisplay<TFont, TGlyph> Inner { get; } = inner;

    ///<summary>A display representing the left delimiter.
    ///Its position is relative to the parent and is not treated as a sub-display.</summary>
    public IGlyphDisplay<TFont, TGlyph>? Left { get; } = left;

    ///<summary>A display representing the right delimiter.
    ///Its position is relative to the parent and is not treated as a sub-display.</summary>
    public IGlyphDisplay<TFont, TGlyph>? Right { get; } = right;

    public float Ascent => System.Math.Max(Left?.Ascent ?? 0, System.Math.Max(Right?.Ascent ?? 0, Inner.Ascent));
    public float Descent => System.Math.Max(Left?.Descent ?? 0, System.Math.Max(Right?.Descent ?? 0, Inner.Descent));
    public float Width => (Left?.Width ?? 0) + Inner.Width + (Right?.Width ?? 0);

    public Range Range { get; } = range;

    public PointF Position {
        get;
        set {
            field = value;
            if (Left != null) {
                Left.Position = value;
                Inner.Position = value with { X = value.X + Left.Width };
            } else Inner.Position = value;

            if (Right != null)
                Right.Position = value with { X = Inner.Position.X + Inner.Width };
        }
    }

    public bool HasScript { get; set; }
    public void Draw(IGraphicsContext<TFont, TGlyph> context) {
        this.DrawBackground(context);
        Left?.Draw(context);
        Right?.Draw(context);
        Inner.Draw(context);
    }

    public Color? TextColor { get; set; }
    public void SetTextColorRecursive(Color? textColor) {
        TextColor ??= textColor;
        Left?.SetTextColorRecursive(textColor);
        Right?.SetTextColorRecursive(textColor);
        Inner.SetTextColorRecursive(textColor);
    }
    public Color? BackColor { get; set; }

    public override string ToString() => $@"\inner[{Left}][{Right}]{{{Inner}}}";
}