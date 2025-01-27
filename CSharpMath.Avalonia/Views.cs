using System;
using System.Collections.Generic;
using CSharpMath.Atom;
using CSharpMath.Rendering.FrontEnd;
using CSharpMath.Structures;
using Typography.OpenFont;

using AvaloniaColor = Avalonia.Media.Color;
using AvaloniaInheritControl = Avalonia.Controls.Control;
using AvaloniaProperty = Avalonia.AvaloniaProperty;

namespace CSharpMath.Avalonia;

public class BaseView<TPainter, TContent> : AvaloniaInheritControl, ICSharpMathAPI<TContent, AvaloniaColor>
    where TPainter : Painter<AvaloniaCanvas, TContent, AvaloniaColor>, new() where TContent : class {
    public TPainter Painter { get; } = new();

    private static readonly TPainter StaticPainter = new();

    private static AvaloniaProperty CreateProperty<TThis, TValue>(
        string propertyName,
        bool affectsMeasure,
        Func<TPainter, TValue> defaultValueGet,
        Action<TPainter, TValue> propertySet,
        Action<TThis, TValue>? updateOtherProperty = null)
        where TThis : BaseView<TPainter, TContent> {
        var defaultValue = defaultValueGet(StaticPainter);
        var prop = AvaloniaProperty.Register<TThis, TValue>(propertyName, defaultValue);
        global::Avalonia.AvaloniaObjectExtensions.AddClassHandler<TThis>(prop.Changed, (t, e) => PropertyChanged(t, e.NewValue));
        return prop;

        void PropertyChanged(TThis @this, object? newValue) {
            // We need to support nullable classes and structs, so we cannot forbid null here
            // So this use of the null-forgiving operator should be blamed on non-generic PropertyChanged handlers
            var @new = (TValue)newValue!;
            propertySet(@this.Painter, @new);
            updateOtherProperty?.Invoke(@this, @new);
            if (affectsMeasure) @this.InvalidateMeasure();
            // Redraw immediately! No deferred drawing
            @this.InvalidateVisual();
        }
    }

    protected BaseView() {
        // Respect built-in styles
        Styles.Add(new global::Avalonia.Styling.Style(global::Avalonia.Styling.Selectors.Is<BaseView<TPainter, TContent>>) {
            Setters =
            {
                new global::Avalonia.Styling.Setter(TextColorProperty, new global::Avalonia.Markup.Xaml.MarkupExtensions.DynamicResourceExtension("SystemBaseHighColor"))
            }
        });
    }
    protected override global::Avalonia.Size MeasureOverride(global::Avalonia.Size availableSize) =>
        Painter.Measure((float)availableSize.Width) is { } rect
            ? new global::Avalonia.Size(rect.Width, rect.Height)
            : base.MeasureOverride(availableSize);

    private struct ReadOnlyProperty<TThis, TValue> where TThis : BaseView<TPainter, TContent> {
        public ReadOnlyProperty(string propertyName,
            Func<TPainter, TValue> getter) {
            Property = AvaloniaProperty.RegisterDirect<TThis, TValue>(propertyName, b => getter(b.Painter), null, getter(StaticPainter));
            _value = getter(StaticPainter);
        }

        private TValue _value;
        public readonly global::Avalonia.DirectProperty<TThis, TValue> Property;
        public void SetValue(TThis @this, TValue value) => @this.SetAndRaise(Property, ref _value, value);
    }

    private global::Avalonia.Point _origin;
    protected override void OnPointerPressed(global::Avalonia.Input.PointerPressedEventArgs e) {
        var point = e.GetCurrentPoint(this);
        if (point.Properties.IsLeftButtonPressed && EnablePanning) {
            _origin = point.Position;
        }
        base.OnPointerPressed(e);
    }
    protected override void OnPointerMoved(global::Avalonia.Input.PointerEventArgs e) {
        var point = e.GetCurrentPoint(this);
        if (point.Properties.IsLeftButtonPressed && EnablePanning) {
            var displacement = point.Position - _origin;
            _origin = point.Position;
            DisplacementX += (float)displacement.X;
            DisplacementY += (float)displacement.Y;
        }
        base.OnPointerMoved(e);
    }
    protected override void OnPointerReleased(global::Avalonia.Input.PointerReleasedEventArgs e) {
        var point = e.GetCurrentPoint(this);
        if (point.Properties.IsLeftButtonPressed && EnablePanning) {
            _origin = point.Position;
        }
        base.OnPointerReleased(e);
    }
    public override void Render(global::Avalonia.Media.DrawingContext context) {
        base.Render(context);
        var canvas = new AvaloniaCanvas(context, Bounds.Size);
        Painter.Draw(canvas, TextAlignment, Padding, DisplacementX, DisplacementY);
    }
    /// <summary>Requires touch events to be enabled in SkiaSharp/Xamarin.Forms</summary>
    public bool EnablePanning { get => (bool)GetValue(DisablePanningProperty)!; set => SetValue(DisablePanningProperty, value); }

    private static readonly AvaloniaProperty DisablePanningProperty = CreateProperty<BaseView<TPainter, TContent>, bool>(nameof(EnablePanning), false, _ => false, (_, __) => { });

    private static readonly System.Reflection.ParameterInfo[] DrawMethodParams = typeof(TPainter)
        .GetMethod(nameof(Painter<AvaloniaCanvas, TContent, AvaloniaColor>.Draw),
            [typeof(AvaloniaCanvas), typeof(TextAlignment), typeof(Thickness), typeof(float), typeof(float)])
        .GetParameters();

    private static T? Nullable<T>(T value) where T : struct => value;
    public (AvaloniaColor glyph, AvaloniaColor textRun)? GlyphBoxColor { get => ((AvaloniaColor glyph, AvaloniaColor textRun)?)GetValue(GlyphBoxColorProperty); set => SetValue(GlyphBoxColorProperty, value); }
    private static readonly AvaloniaProperty GlyphBoxColorProperty = CreateProperty<BaseView<TPainter, TContent>, (AvaloniaColor glyph, AvaloniaColor textRun)?>(nameof(GlyphBoxColor), false,
        p => p.GlyphBoxColor is var (glyph, textRun) ? Nullable((glyph, textRun)) : null,
        (p, v) => p.GlyphBoxColor = v is var (glyph, textRun) ? Nullable((glyph, textRun)) : null);
    public TContent? Content { get => (TContent)GetValue(ContentProperty)!; set => SetValue(ContentProperty, value); }
    private static readonly AvaloniaProperty ContentProperty = CreateProperty<BaseView<TPainter, TContent>, TContent?>(nameof(Content), true, p => p.Content, (p, v) => p.Content = v, (b, v) => { if (b.Painter.ErrorMessage == null) b.LaTeX = b.Painter.LaTeX; });

    [global::Avalonia.Metadata.Content]
    public string? LaTeX { get => (string?)GetValue(LaTeXProperty); set => SetValue(LaTeXProperty, value); }
    private static readonly AvaloniaProperty LaTeXProperty = CreateProperty<BaseView<TPainter, TContent>, string?>(nameof(LaTeX), true, p => p.LaTeX, (p, v) => p.LaTeX = v, (b, v) => (b.Content, b.ErrorMessage) = (b.Painter.Content, b.Painter.ErrorMessage));
    public bool DisplayErrorInline { get => (bool)GetValue(DisplayErrorInlineProperty)!; set => SetValue(DisplayErrorInlineProperty, value); }
    private static readonly AvaloniaProperty DisplayErrorInlineProperty = CreateProperty<BaseView<TPainter, TContent>, bool>(nameof(DisplayErrorInline), true, p => p.DisplayErrorInline, (p, v) => p.DisplayErrorInline = v);
    /// <summary>Unit of measure: points</summary>
    public float FontSize { get => (float)GetValue(FontSizeProperty)!; set => SetValue(FontSizeProperty, value); }
    private static readonly AvaloniaProperty FontSizeProperty = CreateProperty<BaseView<TPainter, TContent>, float>(nameof(FontSize), true, p => p.FontSize, (p, v) => p.FontSize = v);
    /// <summary>Unit of measure: points; Defaults to <see cref="FontSize"/>.</summary>
    public float? ErrorFontSize { get => (float?)GetValue(ErrorFontSizeProperty); set => SetValue(ErrorFontSizeProperty, value); }
    private static readonly AvaloniaProperty ErrorFontSizeProperty = CreateProperty<BaseView<TPainter, TContent>, float?>(nameof(ErrorFontSize), true, p => p.ErrorFontSize, (p, v) => p.ErrorFontSize = v);
    public IEnumerable<Typeface> LocalTypefaces { get => (IEnumerable<Typeface>)GetValue(LocalTypefacesProperty)!; set => SetValue(LocalTypefacesProperty, value); }
    private static readonly AvaloniaProperty LocalTypefacesProperty = CreateProperty<BaseView<TPainter, TContent>, IEnumerable<Typeface>>(nameof(LocalTypefaces), true, p => p.LocalTypefaces, (p, v) => p.LocalTypefaces = v);
    public AvaloniaColor TextColor { get => (AvaloniaColor)GetValue(TextColorProperty)!; set => SetValue(TextColorProperty, value); }
    private static readonly AvaloniaProperty TextColorProperty = CreateProperty<BaseView<TPainter, TContent>, AvaloniaColor>(nameof(TextColor), false, p => p.TextColor, (p, v) => p.TextColor = v);
    public AvaloniaColor HighlightColor { get => (AvaloniaColor)GetValue(HighlightColorProperty)!; set => SetValue(HighlightColorProperty, value); }
    private static readonly AvaloniaProperty HighlightColorProperty = CreateProperty<BaseView<TPainter, TContent>, AvaloniaColor>(nameof(HighlightColor), false, p => p.HighlightColor, (p, v) => p.HighlightColor = v);
    public AvaloniaColor ErrorColor { get => (AvaloniaColor)GetValue(ErrorColorProperty)!; set => SetValue(ErrorColorProperty, value); }
    private static readonly AvaloniaProperty ErrorColorProperty = CreateProperty<BaseView<TPainter, TContent>, AvaloniaColor>(nameof(ErrorColor), false, p => p.ErrorColor, (p, v) => p.ErrorColor = v);
    public TextAlignment TextAlignment { get => (TextAlignment)GetValue(TextAlignmentProperty)!; set => SetValue(TextAlignmentProperty, value); }
    private static readonly AvaloniaProperty TextAlignmentProperty = CreateProperty<BaseView<TPainter, TContent>, TextAlignment>(nameof(Rendering.FrontEnd.TextAlignment), false, p => (TextAlignment)DrawMethodParams[1].DefaultValue, (p, v) => { });
    public Thickness Padding { get => (Thickness)GetValue(PaddingProperty)!; set => SetValue(PaddingProperty, value); }
    private static readonly AvaloniaProperty PaddingProperty = CreateProperty<BaseView<TPainter, TContent>, Thickness>(nameof(Padding), false, p => (Thickness)(DrawMethodParams[2].DefaultValue ?? new Thickness()), (p, v) => { });
    public float DisplacementX { get => (float)GetValue(DisplacementXProperty)!; set => SetValue(DisplacementXProperty, value); }
    private static readonly AvaloniaProperty DisplacementXProperty = CreateProperty<BaseView<TPainter, TContent>, float>(nameof(DisplacementX), false, p => (float)DrawMethodParams[3].DefaultValue, (p, v) => { });
    public float DisplacementY { get => (float)GetValue(DisplacementYProperty)!; set => SetValue(DisplacementYProperty, value); }
    private static readonly AvaloniaProperty DisplacementYProperty = CreateProperty<BaseView<TPainter, TContent>, float>(nameof(DisplacementY), false, p => (float)DrawMethodParams[4].DefaultValue, (p, v) => { });
    public float Magnification { get => (float)GetValue(MagnificationProperty)!; set => SetValue(MagnificationProperty, value); }
    private static readonly AvaloniaProperty MagnificationProperty = CreateProperty<BaseView<TPainter, TContent>, float>(nameof(Magnification), false, p => p.Magnification, (p, v) => p.Magnification = v);
    public PaintStyle PaintStyle { get => (PaintStyle)GetValue(PaintStyleProperty)!; set => SetValue(PaintStyleProperty, value); }
    private static readonly AvaloniaProperty PaintStyleProperty = CreateProperty<BaseView<TPainter, TContent>, PaintStyle>(nameof(PaintStyle), false, p => p.PaintStyle, (p, v) => p.PaintStyle = v);
    public LineStyle LineStyle { get => (LineStyle)GetValue(LineStyleProperty)!; set => SetValue(LineStyleProperty, value); }
    private static readonly AvaloniaProperty LineStyleProperty = CreateProperty<BaseView<TPainter, TContent>, LineStyle>(nameof(LineStyle), false, p => p.LineStyle, (p, v) => p.LineStyle = v);
    public string? ErrorMessage { get => (string?)GetValue(ErrorMessageProperty); private set => ErrorMessagePropertyKey.SetValue(this, value); }
    private static readonly ReadOnlyProperty<BaseView<TPainter, TContent>, string?> ErrorMessagePropertyKey = new(nameof(ErrorMessage), p => p.ErrorMessage);
    private static readonly AvaloniaProperty ErrorMessageProperty = ErrorMessagePropertyKey.Property;
}
public class MathView : BaseView<MathPainter, MathList>;
public class TextView : BaseView<TextPainter, Rendering.Text.TextAtom>;