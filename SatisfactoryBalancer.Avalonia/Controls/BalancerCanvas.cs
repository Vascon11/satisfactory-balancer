using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using SatisfactoryBalancer.Avalonia.Services;
using SatisfactoryBalancer.Domain.Aggregates;

namespace SatisfactoryBalancer.Avalonia.Controls;

public class BalancerCanvas : Control
{
    private NetworkLayoutResult? _layout;

    // Transform state (absolute, in screen pixels)
    private double _zoom      = 1.0;
    private Point  _panOffset = new(0, 0);

    // Pan drag state
    private bool  _isPanning;
    private Point _dragStart;
    private Point _panAtDragStart;

    private const double ZoomStep = 1.15;
    private const double ZoomMin  = 0.05;
    private const double ZoomMax  = 10.0;

    private static readonly IPen EdgePen =
        new Pen(new SolidColorBrush(Color.FromRgb(148, 163, 184)), 1.5);

    private static readonly Dictionary<string, IBrush> NodeBrushes = new()
    {
        ["green"]  = new SolidColorBrush(Color.FromRgb(34,  197, 94)),
        ["blue"]   = new SolidColorBrush(Color.FromRgb(59,  130, 246)),
        ["orange"] = new SolidColorBrush(Color.FromRgb(249, 115, 22)),
        ["red"]    = new SolidColorBrush(Color.FromRgb(239, 68,  68)),
        ["gray"]   = new SolidColorBrush(Color.FromRgb(107, 114, 128)),
    };

    private static readonly IBrush BgBrush = new SolidColorBrush(Color.FromRgb(15, 23, 42));

    public BalancerCanvas()
    {
        ClipToBounds = true;
    }

    public void SetNetwork(BalancerNetwork? network)
    {
        _layout = network != null ? NetworkLayoutService.Compute(network) : null;
        ResetView();
        InvalidateVisual();
    }

    // Reseta zoom e pan para o estado "fit to screen"
    private void ResetView()
    {
        if (_layout == null || _layout.IsTooLarge || Bounds.Width == 0)
        {
            _zoom      = 1.0;
            _panOffset = new Point(0, 0);
            return;
        }

        double scaleX = _layout.SvgWidth  > 0 ? (Bounds.Width  - 40) / _layout.SvgWidth  : 1;
        double scaleY = _layout.SvgHeight > 0 ? (Bounds.Height - 40) / _layout.SvgHeight : 1;
        _zoom = Math.Min(scaleX, scaleY);

        _panOffset = new Point(
            (Bounds.Width  - _layout.SvgWidth  * _zoom) / 2,
            (Bounds.Height - _layout.SvgHeight * _zoom) / 2);
    }

    // ── Eventos de mouse ──────────────────────────────────────────────────────

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            _isPanning      = true;
            _dragStart      = e.GetPosition(this);
            _panAtDragStart = _panOffset;
            Cursor          = new Cursor(StandardCursorType.SizeAll);
            e.Pointer.Capture(this);
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (_isPanning)
        {
            var pos    = e.GetPosition(this);
            _panOffset = new Point(
                _panAtDragStart.X + pos.X - _dragStart.X,
                _panAtDragStart.Y + pos.Y - _dragStart.Y);
            InvalidateVisual();
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        _isPanning = false;
        Cursor     = Cursor.Default;
        e.Pointer.Capture(null);
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);

        var mouse     = e.GetPosition(this);
        double factor = e.Delta.Y > 0 ? ZoomStep : 1.0 / ZoomStep;
        double newZoom = Math.Clamp(_zoom * factor, ZoomMin, ZoomMax);

        // Zoom toward cursor: adjust pan so the point under the mouse stays fixed
        _panOffset = new Point(
            mouse.X - (mouse.X - _panOffset.X) * (newZoom / _zoom),
            mouse.Y - (mouse.Y - _panOffset.Y) * (newZoom / _zoom));

        _zoom = newZoom;
        InvalidateVisual();
    }

    // Quando o tamanho muda (primeira vez ou redimensionamento), refaz o fit
    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        if (!_isPanning)
            ResetView();
        InvalidateVisual();
    }

    // ── Render ────────────────────────────────────────────────────────────────

    public override void Render(DrawingContext ctx)
    {
        var bounds = new Rect(0, 0, Bounds.Width, Bounds.Height);
        ctx.DrawRectangle(BgBrush, null, bounds);

        if (_layout == null || _layout.IsTooLarge)
        {
            DrawCenteredText(ctx, _layout?.IsTooLarge == true
                ? "Rede muito grande para visualizar (>120 nós)"
                : "Configure as entradas e clique em Gerar", bounds);
            return;
        }

        using var _ = ctx.PushTransform(
            Matrix.CreateTranslation(_panOffset.X, _panOffset.Y) *
            Matrix.CreateScale(_zoom, _zoom));

        DrawEdges(ctx);
        DrawNodes(ctx);
    }

    // ── Helpers de desenho ────────────────────────────────────────────────────

    private void DrawEdges(DrawingContext ctx)
    {
        foreach (var edge in _layout!.Edges)
        {
            double cx1 = edge.X1 + (edge.X2 - edge.X1) * 0.45;
            double cx2 = edge.X1 + (edge.X2 - edge.X1) * 0.55;

            var geo = new StreamGeometry();
            using (var gc = geo.Open())
            {
                gc.BeginFigure(new Point(edge.X1, edge.Y1), false);
                gc.CubicBezierTo(
                    new Point(cx1, edge.Y1),
                    new Point(cx2, edge.Y2),
                    new Point(edge.X2, edge.Y2));
            }

            ctx.DrawGeometry(null, EdgePen, geo);
            DrawArrow(ctx, new Point(cx2, edge.Y2), new Point(edge.X2, edge.Y2));
        }
    }

    private static void DrawArrow(DrawingContext ctx, Point from, Point to)
    {
        var dir = to - from;
        double len = Math.Sqrt(dir.X * dir.X + dir.Y * dir.Y);
        if (len < 0.001) return;
        var unit = new Point(dir.X / len, dir.Y / len);
        var perp = new Point(-unit.Y * 4, unit.X * 4);

        var arrow = new StreamGeometry();
        using (var gc = arrow.Open())
        {
            gc.BeginFigure(to, true);
            gc.LineTo(new Point(to.X - unit.X * 8 + perp.X, to.Y - unit.Y * 8 + perp.Y));
            gc.LineTo(new Point(to.X - unit.X * 8 - perp.X, to.Y - unit.Y * 8 - perp.Y));
            gc.EndFigure(true);
        }

        ctx.DrawGeometry(new SolidColorBrush(Color.FromRgb(148, 163, 184)), null, arrow);
    }

    private void DrawNodes(DrawingContext ctx)
    {
        foreach (var node in _layout!.Nodes)
        {
            var brush = NodeBrushes.GetValueOrDefault(node.Color switch
            {
                "#22c55e" => "green",
                "#3b82f6" => "blue",
                "#f97316" => "orange",
                "#ef4444" => "red",
                _         => "gray"
            }, NodeBrushes["gray"]);

            ctx.DrawRectangle(brush, null,
                new RoundedRect(new Rect(node.X, node.Y, node.Width, node.Height), 6));

            var ft = new FormattedText(
                node.Label,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("monospace", FontStyle.Normal, FontWeight.Bold),
                11,
                Brushes.White);

            ctx.DrawText(ft, new Point(
                node.X + (node.Width  - ft.Width)  / 2,
                node.Y + (node.Height - ft.Height) / 2));
        }
    }

    private static void DrawCenteredText(DrawingContext ctx, string text, Rect bounds)
    {
        var ft = new FormattedText(
            text,
            System.Globalization.CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface("sans-serif"),
            14,
            new SolidColorBrush(Color.FromRgb(100, 116, 139)));

        ctx.DrawText(ft, new Point(
            (bounds.Width  - ft.Width)  / 2,
            (bounds.Height - ft.Height) / 2));
    }
}
