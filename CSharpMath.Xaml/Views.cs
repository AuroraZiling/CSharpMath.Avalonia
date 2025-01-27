using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using CSharpMath.Atom;
using CSharpMath.Rendering.FrontEnd;
using TextAlignment = CSharpMath.Rendering.FrontEnd.TextAlignment;
using Thickness = CSharpMath.Structures.Thickness;
using Typeface = Typography.OpenFont.Typeface;

// ReSharper disable MemberCanBePrivate.Global
namespace CSharpMath.Avalonia;

public class BaseView<TPainter, TContent> : Control, ICSharpMathAPI<TContent, Color>
    where TPainter : Painter<AvaloniaCanvas, TContent, Color>, new()
    where TContent : class
{
    public TPainter Painter { get; } = new();
    private static readonly TPainter StaticPainter = new();
    private Point _origin;

    protected BaseView()
    {
        Styles.Add(new global::Avalonia.Styling.Style(global::Avalonia.Styling.Selectors.Is<BaseView<TPainter, TContent>>)
        {
            Setters =
            {
                new global::Avalonia.Styling.Setter(TextColorProperty,
                    new global::Avalonia.Markup.Xaml.MarkupExtensions.DynamicResourceExtension("SystemBaseHighColor"))
            }
        });
    }

    #region Dependency Property Helpers
    private static AvaloniaProperty CreateProperty<TThis, TValue>(
        string propertyName,
        bool affectsMeasure,
        Func<TPainter, TValue> defaultValueGet,
        Action<TPainter, TValue> propertySet,
        Action<TThis, TValue>? updateOtherProperty = null)
        where TThis : BaseView<TPainter, TContent>
    {
        var defaultValue = defaultValueGet(StaticPainter);
        var prop = AvaloniaProperty.Register<TThis, TValue>(propertyName, defaultValue);
        prop.Changed.AddClassHandler<TThis>((t, e) => PropertyChanged(t, e.NewValue));
        return prop;

        void PropertyChanged(TThis @this, object? newValue)
        {
            var @new = (TValue)newValue!;
            propertySet(@this.Painter, @new);
            updateOtherProperty?.Invoke(@this, @new);
            if (affectsMeasure) @this.InvalidateMeasure();
            @this.InvalidateVisual();
        }
    }

    private struct ReadOnlyProperty<TThis, TValue> where TThis : BaseView<TPainter, TContent> {
        public ReadOnlyProperty(string propertyName,
            Func<TPainter, TValue> getter) {
            Property = AvaloniaProperty.RegisterDirect<TThis, TValue>(propertyName, b => getter(b.Painter), null, getter(StaticPainter));
            _value = getter(StaticPainter);
        }

        private TValue _value;
        public readonly DirectProperty<TThis, TValue> Property;
        public void SetValue(TThis @this, TValue value) => @this.SetAndRaise(Property, ref _value, value);
    }
    #endregion

    #region Measurement and Rendering
    protected override Size MeasureOverride(Size availableSize) =>
        Painter.Measure((float)availableSize.Width) is var rect
            ? new Size(rect.Width, rect.Height)
            : base.MeasureOverride(availableSize);

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        var canvas = new AvaloniaCanvas(context, Bounds.Size);
        Painter.Draw(canvas, TextAlignment, Padding, DisplacementX, DisplacementY);
    }
    #endregion

    #region Pointer Handling
    private bool IsPanningActive(global::Avalonia.Input.PointerPoint point) =>
        EnablePanning && point.Properties.IsLeftButtonPressed;

    protected override void OnPointerPressed(global::Avalonia.Input.PointerPressedEventArgs e)
    {
        var point = e.GetCurrentPoint(this);
        if (IsPanningActive(point)) _origin = point.Position;
        base.OnPointerPressed(e);
    }

    protected override void OnPointerMoved(global::Avalonia.Input.PointerEventArgs e)
    {
        var point = e.GetCurrentPoint(this);
        if (IsPanningActive(point))
        {
            var displacement = point.Position - _origin;
            _origin = point.Position;
            DisplacementX += (float)displacement.X;
            DisplacementY += (float)displacement.Y;
        }
        base.OnPointerMoved(e);
    }

    protected override void OnPointerReleased(global::Avalonia.Input.PointerReleasedEventArgs e)
    {
        var point = e.GetCurrentPoint(this);
        if (IsPanningActive(point)) _origin = point.Position;
        base.OnPointerReleased(e);
    }
    #endregion

    private static readonly System.Reflection.ParameterInfo[] DrawMethodParams = typeof(TPainter)
        .GetMethod(nameof(Painter<AvaloniaCanvas, TContent, Color>.Draw),
            [typeof(AvaloniaCanvas), typeof(TextAlignment), typeof(Thickness), typeof(float), typeof(float)])
        .GetParameters();

    #region Dependency Properties

    public bool EnablePanning
    {
        get => (bool)GetValue(EnablePanningProperty)!;
        set => SetValue(EnablePanningProperty, value);
    }
    public static readonly AvaloniaProperty EnablePanningProperty =
        CreateProperty<BaseView<TPainter, TContent>, bool>(nameof(EnablePanning), false, _ => false, (_, _) => { });

    public (Color glyph, Color textRun)? GlyphBoxColor
    {
        get => ((Color glyph, Color textRun)?)GetValue(GlyphBoxColorProperty);
        set => SetValue(GlyphBoxColorProperty, value);
    }
    public static readonly AvaloniaProperty GlyphBoxColorProperty =
        CreateProperty<BaseView<TPainter, TContent>, (Color glyph, Color textRun)?>(
            nameof(GlyphBoxColor), false,
            p => p.GlyphBoxColor,
            (p, v) => p.GlyphBoxColor = v);

    public TContent? Content
    {
        get => (TContent)GetValue(ContentProperty)!;
        set => SetValue(ContentProperty, value);
    }
    public static readonly AvaloniaProperty ContentProperty =
        CreateProperty<BaseView<TPainter, TContent>, TContent?>(
            nameof(Content), true, p => p.Content, (p, v) => p.Content = v,
            (b, _) => { if (b.Painter.ErrorMessage == null) b.LaTeX = b.Painter.LaTeX; });

    [global::Avalonia.Metadata.Content]
    public string? LaTeX
    {
        get => (string?)GetValue(LaTeXProperty);
        set => SetValue(LaTeXProperty, value);
    }
    public static readonly AvaloniaProperty LaTeXProperty =
        CreateProperty<BaseView<TPainter, TContent>, string?>(
            nameof(LaTeX), true, p => p.LaTeX, (p, v) => p.LaTeX = v,
            (b, _) => (b.Content, b.ErrorMessage) = (b.Painter.Content, b.Painter.ErrorMessage));

    public bool DisplayErrorInline
    {
        get => (bool)GetValue(DisplayErrorInlineProperty)!;
        set => SetValue(DisplayErrorInlineProperty, value);
    }
    public static readonly AvaloniaProperty DisplayErrorInlineProperty =
        CreateProperty<BaseView<TPainter, TContent>, bool>(nameof(DisplayErrorInline), true, p => p.DisplayErrorInline, (p, v) => p.DisplayErrorInline = v);

    public float FontSize
    {
        get => (float)GetValue(FontSizeProperty)!;
        set => SetValue(FontSizeProperty, value);
    }
    public static readonly AvaloniaProperty FontSizeProperty =
        CreateProperty<BaseView<TPainter, TContent>, float>(nameof(FontSize), true, p => p.FontSize, (p, v) => p.FontSize = v);

    public float? ErrorFontSize
    {
        get => (float?)GetValue(ErrorFontSizeProperty);
        set => SetValue(ErrorFontSizeProperty, value);
    }
    public static readonly AvaloniaProperty ErrorFontSizeProperty =
        CreateProperty<BaseView<TPainter, TContent>, float?>(nameof(ErrorFontSize), true, p => p.ErrorFontSize, (p, v) => p.ErrorFontSize = v);

    public IEnumerable<Typeface> LocalTypefaces
    {
        get => (IEnumerable<Typeface>)GetValue(LocalTypefacesProperty)!;
        set => SetValue(LocalTypefacesProperty, value);
    }
    public static readonly AvaloniaProperty LocalTypefacesProperty =
        CreateProperty<BaseView<TPainter, TContent>, IEnumerable<Typeface>>(nameof(LocalTypefaces), true, p => p.LocalTypefaces, (p, v) => p.LocalTypefaces = v);

    public Color TextColor
    {
        get => (Color)GetValue(TextColorProperty)!;
        set => SetValue(TextColorProperty, value);
    }
    public static readonly AvaloniaProperty TextColorProperty =
        CreateProperty<BaseView<TPainter, TContent>, Color>(nameof(TextColor), false, p => p.TextColor, (p, v) => p.TextColor = v);

    public Color HighlightColor
    {
        get => (Color)GetValue(HighlightColorProperty)!;
        set => SetValue(HighlightColorProperty, value);
    }
    public static readonly AvaloniaProperty HighlightColorProperty =
        CreateProperty<BaseView<TPainter, TContent>, Color>(nameof(HighlightColor), false, p => p.HighlightColor, (p, v) => p.HighlightColor = v);

    public Color ErrorColor
    {
        get => (Color)GetValue(ErrorColorProperty)!;
        set => SetValue(ErrorColorProperty, value);
    }
    public static readonly AvaloniaProperty ErrorColorProperty =
        CreateProperty<BaseView<TPainter, TContent>, Color>(nameof(ErrorColor), false, p => p.ErrorColor, (p, v) => p.ErrorColor = v);

    public TextAlignment TextAlignment
    {
        get => (TextAlignment)GetValue(TextAlignmentProperty)!;
        set => SetValue(TextAlignmentProperty, value);
    }
    public static readonly AvaloniaProperty TextAlignmentProperty =
        CreateProperty<BaseView<TPainter, TContent>, TextAlignment>(nameof(TextAlignment), false, p => (TextAlignment)DrawMethodParams[1].DefaultValue, (_, _) => { });

    public Thickness Padding
    {
        get => (Thickness)GetValue(PaddingProperty)!;
        set => SetValue(PaddingProperty, value);
    }
    public static readonly AvaloniaProperty PaddingProperty =
        CreateProperty<BaseView<TPainter, TContent>, Thickness>(nameof(Padding), false, p => (Thickness)(DrawMethodParams[2].DefaultValue ?? new Thickness()), (_, _) => { });

    public float DisplacementX
    {
        get => (float)GetValue(DisplacementXProperty)!;
        set => SetValue(DisplacementXProperty, value);
    }
    public static readonly AvaloniaProperty DisplacementXProperty =
        CreateProperty<BaseView<TPainter, TContent>, float>(nameof(DisplacementX), false, p => (float)DrawMethodParams[3].DefaultValue, (_, _) => { });

    public float DisplacementY
    {
        get => (float)GetValue(DisplacementYProperty)!;
        set => SetValue(DisplacementYProperty, value);
    }
    public static readonly AvaloniaProperty DisplacementYProperty =
        CreateProperty<BaseView<TPainter, TContent>, float>(nameof(DisplacementY), false, p => (float)DrawMethodParams[4].DefaultValue, (_, _) => { });

    public float Magnification
    {
        get => (float)GetValue(MagnificationProperty)!;
        set => SetValue(MagnificationProperty, value);
    }
    public static readonly AvaloniaProperty MagnificationProperty =
        CreateProperty<BaseView<TPainter, TContent>, float>(nameof(Magnification), false, p => p.Magnification, (p, v) => p.Magnification = v);

    public PaintStyle PaintStyle
    {
        get => (PaintStyle)GetValue(PaintStyleProperty)!;
        set => SetValue(PaintStyleProperty, value);
    }
    public static readonly AvaloniaProperty PaintStyleProperty =
        CreateProperty<BaseView<TPainter, TContent>, PaintStyle>(nameof(PaintStyle), false, p => p.PaintStyle, (p, v) => p.PaintStyle = v);

    public LineStyle LineStyle
    {
        get => (LineStyle)GetValue(LineStyleProperty)!;
        set => SetValue(LineStyleProperty, value);
    }
    public static readonly AvaloniaProperty LineStyleProperty =
        CreateProperty<BaseView<TPainter, TContent>, LineStyle>(nameof(LineStyle), false, p => p.LineStyle, (p, v) => p.LineStyle = v);

    public string? ErrorMessage
    {
        get => (string?)GetValue(ErrorMessageProperty);
        private set => ErrorMessagePropertyKey.SetValue(this, value);
    }

    private static readonly ReadOnlyProperty<BaseView<TPainter, TContent>, string?> ErrorMessagePropertyKey = new(nameof(ErrorMessage), p => p.ErrorMessage);
    public static readonly AvaloniaProperty ErrorMessageProperty = ErrorMessagePropertyKey.Property;
    #endregion
}

public class MathView : BaseView<MathPainter, MathList>;
public class TextView : BaseView<TextPainter, Rendering.Text.TextAtom>;