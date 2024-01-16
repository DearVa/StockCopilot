using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using StockCopilot.Models;

namespace StockCopilot.Views.Controls;

public sealed class KLineBarPlot : Canvas
{
    public static readonly DirectProperty<KLineBarPlot, Range2D> XRangeProperty =
        AvaloniaProperty.RegisterDirect<KLineBarPlot, Range2D>(
            nameof(XRange),
            static self => self.XRange,
            static (self, value) => self.XRange = value,
            new Range2D(0, 100),
            enableDataValidation: true);

    /// <summary>
    /// 表示X轴数据的开始和结束，
    /// 例如(11.4, 51.4)代表图像的左侧位于第11.4个数据的位置，即图像左侧是下标为11的数据，宽度的0.4 (Width + Margin)，右侧位于51.4
    /// </summary>
    public Range2D XRange { get; set; } = new(0, 100);

    public static readonly DirectProperty<KLineBarPlot, IReadOnlyList<KLineBarItem>?> BarItemsProperty =
        AvaloniaProperty.RegisterDirect<KLineBarPlot, IReadOnlyList<KLineBarItem>?>(
            nameof(BarItems),
            static self => self.BarItems,
            static (self, value) => self.BarItems = value);

    public IReadOnlyList<KLineBarItem>? BarItems
    {
        get => barItems;
        set
        {
            if (ReferenceEquals(barItems, value)) return;
            if (barItems is INotifyCollectionChanged oldNotify)
            {
                oldNotify.CollectionChanged -= BarItems_OnCollectionChanged;
            }
            barItems = value;
            if (barItems is INotifyCollectionChanged newNotify)
            {
                newNotify.CollectionChanged += BarItems_OnCollectionChanged;
            }
            InvalidateArrange();
        }
    }

    public IReadOnlyList<KLineBarItem>? barItems;

    public static readonly StyledProperty<IBrush?> ForegroundProperty =
        AvaloniaProperty.Register<KLineBarPlot, IBrush?>(
            nameof(Foreground),
            inherits: true);

    public IBrush? Foreground
    {
        get => GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }

    public static readonly StyledProperty<Thickness> PaddingProperty =
        AvaloniaProperty.Register<KLineBarPlot, Thickness>(
            nameof(Padding),
            defaultValue: new Thickness(0, 48, 0, 16),
            inherits: true);

    public Thickness Padding
    {
        get => GetValue(PaddingProperty);
        set => SetValue(PaddingProperty, value);
    }

    private readonly List<Line> barLines = [];
    private readonly Dictionary<Color, IBrush> brushCache = [];

    private const double BarWidth = 10d;
    private const double BarMargin = 4d;
    private const int MinBarCount = 30;
    private const int MaxBarCount = 200;

    static KLineBarPlot() =>
        AffectsArrange<KLineBarPlot>(
            XRangeProperty,
            BarItemsProperty,
            ForegroundProperty);

    public KLineBarPlot()
    {
        Focusable = true;

        hoveredLine = new Line
        {
            StrokeThickness = 1d,
            Stroke = Brushes.Black,
            ZIndex = 999
        };
        Children.Add(hoveredLine);

        hoveredTextBlock = new TextBlock
        {
            Foreground = Brushes.Black,
            ZIndex = 999
        };
        Children.Add(hoveredTextBlock);
    }

    private void BarItems_OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (barItems == null || barItems.Count == 0 || XRange.Span <= 0) return;
        var span = XRange.Span;
        XRange = new Range2D(barItems.Count - span - 1, barItems.Count - 1);
        InvalidateArrange();
    }
    
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == ForegroundProperty)
        {
            hoveredLine.Stroke = Foreground;
            hoveredTextBlock.Foreground = Foreground;
        }
        
        base.OnPropertyChanged(change);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        // 当BarItems为null时，不进行任何绘制

        // 1. 当XRange为(0, 1)时，每个KLineBarItem的宽度刚好为BarWidth，间距为BarMargin
        // 2. 当XRange缩放时，就需要动态计算需要多少个rectangles。这个列表只进行扩充，不进行缩减。
        //    并且所有的rectangles创建之后就直接添加到当前控件内
        // 3. 根据当前所需的rectangles数量，将他们设为Visible，并且设置位置、大小和颜色；其余的设为Invisible
        //    当前视图的YRange是根据所有可见的rectangles的Value来计算的
        //    rectangles的Value可正可负，绘制时，X轴位于正中间不变，YRange上下对称

        if (barItems == null || barItems.Count == 0 || XRange.Span <= 0)
        {
            return base.ArrangeOverride(finalSize);
        }

        var (pl, pt, pr, pb) = Padding;
        var (startIndex, endIndex) = ((int)Math.Floor(XRange.Start), (int)Math.Floor(XRange.End));
        var actualBarCount = Math.Min(endIndex - startIndex, barItems.Count);
        var actualBarTotalWidth = (finalSize.Width - pl - pr) / XRange.Span;
        var actualBarWidth = actualBarTotalWidth * BarWidth / (BarWidth + BarMargin);

        var startXPos = (startIndex - XRange.Start) * actualBarTotalWidth + pl;
        var maxValue = 0d;

        for (var i = 0; i < actualBarCount; i++)
        {
            Line barLine;
            if (barLines.Count < i + 1)
            {
                barLine = new Line { IsVisible = false };
                barLines.Add(barLine);
                Children.Add(barLine);
            }
            else
            {
                barLine = barLines[i];
            }

            if (i + startIndex >= 0 && i + startIndex < barItems.Count - 1)
            {
                var xPos = startXPos + (i + 0.5d) * actualBarTotalWidth;
                var barItem = barItems[i + startIndex];
                if (!brushCache.TryGetValue(barItem.Color, out var brush))
                {
                    brush = new SolidColorBrush(barItem.Color);
                    brushCache.Add(barItem.Color, brush);
                }
                barLine.Stroke = brush;

                barLine.StrokeThickness = actualBarWidth;
                barLine.StartPoint = new Point(xPos, (finalSize.Height - pt - pb) / 2 + pt);
                barLine.EndPoint = new Point(xPos, -barItem.Value); // Let's set this later

                maxValue = Math.Max(maxValue, Math.Abs(barItem.Value));

                barLine.IsVisible = true;
            }
            else
            {
                barLine.IsVisible = false;
            }
        }

        maxValue = Math.Max(maxValue, 0.001);
        for (var i = 0; i < actualBarCount; i++)
        {
            var barLine = barLines[i];
            if (barLine.EndPoint.Y == 0)
            {
                barLine.StartPoint = new Point(
                    barLine.StartPoint.X,
                    (finalSize.Height - pt - pb) / 2 + pt - 0.5d);
                barLine.EndPoint = new Point(
                    barLine.EndPoint.X,
                    (finalSize.Height - pt - pb) / 2 + pt + 0.5d);
            }
            else
            {
                barLine.EndPoint = new Point(
                    barLine.EndPoint.X,
                    (finalSize.Height - pt - pb) / 2 * (1 + barLine.EndPoint.Y / maxValue) + pt);
            }
        }

        // Hide unused rectangles
        for (var i = actualBarCount; i < barLines.Count; i++)
        {
            barLines[i].IsVisible = false;
        }

        ArrangeHoveredIndicator();

        return base.ArrangeOverride(finalSize);
    }

    private Point? previousPointerPressedPoint;

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        if (previousPointerPressedPoint != null) return;
        previousPointerPressedPoint = e.GetPosition(this);
        e.Pointer.Capture(this);
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        previousPointerPressedPoint = null;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        e.Pointer.Capture(null);
        previousPointerPressedPoint = null;
    }

    private Point pointerPoint;

    private readonly Line hoveredLine;
    private readonly TextBlock hoveredTextBlock;

    private void ArrangeHoveredIndicator()
    {
        if (barItems is not { Count: > 0 }) return;

        var (pl, pt, pr, pb) = Padding;
        var startIndex = (int)Math.Floor(XRange.Start);
        var actualBarTotalWidth = (Bounds.Width - pl - pr) / XRange.Span;
        var actualBarWidth = actualBarTotalWidth * BarWidth / (BarWidth + BarMargin);

        var startXPos = (startIndex - XRange.Start) * actualBarTotalWidth + pl;
        var index = (int)Math.Floor((pointerPoint.X - startXPos) / actualBarTotalWidth) + startIndex;
        index = Math.Clamp(index, 0, barItems.Count - 1);
        var x = (index - startIndex) * actualBarTotalWidth + startXPos + actualBarTotalWidth / 2;

        hoveredLine.StartPoint = new Point(x, pt);
        hoveredLine.EndPoint = new Point(x, Bounds.Height - pb);

        hoveredTextBlock.Text = barItems[index].ToString();
        hoveredTextBlock.Measure(Size.Infinity);

        var textSize = hoveredTextBlock.DesiredSize;
        SetLeft(hoveredTextBlock, Math.Clamp(x - textSize.Width / 2, 0, Bounds.Width - pl - pr - textSize.Width));
        SetTop(hoveredTextBlock, 8d);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        if (barItems == null) return;
        
        pointerPoint = e.GetPosition(this);

        if (previousPointerPressedPoint != null)
        {
            var delta = pointerPoint.X - previousPointerPressedPoint.Value.X;
            previousPointerPressedPoint = pointerPoint;

            var span = XRange.Span;
            var start = XRange.Start - delta / Bounds.Width * span;
            var end = XRange.End - delta / Bounds.Width * span;

            if (start < -BarWidth)
            {
                start = -BarWidth;
                end = -BarWidth + span;
            }
            if (end > barItems.Count + BarWidth)
            {
                end = barItems.Count + BarWidth;
                start = barItems.Count + BarWidth - span;
            }
            
            XRange = new Range2D(start, end);
            InvalidateMeasure();
        }

        // 鼠标移动时，hoveredLine吸附到最近一个BarLine的中心
        ArrangeHoveredIndicator();
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        if (barItems == null) return;
        
        var delta = e.Delta.Y;
        var scale = delta < 0 ? 1.1d : 0.9d;
        var center = e.GetPosition(this);
        var x = center.X / Bounds.Width;

        var oldSpan = XRange.Span;
        var span = oldSpan * scale;

        var start = XRange.Start + (x * (oldSpan - span));
        var end = XRange.End - ((1 - x) * (oldSpan - span));

        switch (end - start)
        {
            case < MinBarCount:
            {
                var midPoint = (start + end) / 2;
                start = midPoint - MinBarCount / 2d;
                end = midPoint + MinBarCount / 2d;
                break;
            }
            case > MaxBarCount:
            {
                var midPoint = (start + end) / 2;
                start = midPoint - MaxBarCount / 2d;
                end = midPoint + MaxBarCount / 2d;
                break;
            }
        }
        
        if (start < -BarWidth)
        {
            start = -BarWidth;
            end = -BarWidth + span;
        }
        if (end > barItems.Count + BarWidth)
        {
            end = barItems.Count + BarWidth;
            start = barItems.Count + BarWidth - span;
        }

        XRange = new Range2D(start, end);
        InvalidateMeasure();
    }
}