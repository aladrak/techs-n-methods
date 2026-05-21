namespace SocketApp.ProcessDispatcher.Maui.Controls;

public sealed class SimpleLineChart : GraphicsView
{
    private readonly ChartDrawable _drawable;

    public SimpleLineChart(string title, double minimum, double maximum, Color lineColor)
    {
        _drawable = new ChartDrawable(title, minimum, maximum, lineColor);
        Drawable = _drawable;
        HeightRequest = 230;
        MinimumHeightRequest = 200;
    }

    public void SetValues(IReadOnlyList<double> values)
    {
        _drawable.Values = values.ToArray();
        Invalidate();
    }

    private sealed class ChartDrawable : IDrawable
    {
        private readonly string _title;
        private readonly double _minimum;
        private readonly double _maximum;
        private readonly Color _lineColor;

        public ChartDrawable(string title, double minimum, double maximum, Color lineColor)
        {
            _title = title;
            _minimum = minimum;
            _maximum = maximum;
            _lineColor = lineColor;
        }

        public double[] Values { get; set; } = [];

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            canvas.FillColor = Colors.White;
            canvas.FillRectangle(dirtyRect);

            const float left = 48;
            const float top = 34;
            const float right = 16;
            const float bottom = 32;

            float chartX = dirtyRect.X + left;
            float chartY = dirtyRect.Y + top;
            float chartWidth = Math.Max(1, dirtyRect.Width - left - right);
            float chartHeight = Math.Max(1, dirtyRect.Height - top - bottom);

            canvas.FontColor = Colors.Black;
            canvas.FontSize = 14;
            canvas.DrawString(_title, dirtyRect.X, dirtyRect.Y + 4, dirtyRect.Width, 24, HorizontalAlignment.Center, VerticalAlignment.Center);

            canvas.StrokeColor = Colors.LightGray;
            canvas.StrokeSize = 1;

            for (int index = 0; index <= 4; index++)
            {
                float y = chartY + (chartHeight * index / 4);
                canvas.DrawLine(chartX, y, chartX + chartWidth, y);
            }

            canvas.StrokeColor = Colors.Gray;
            canvas.DrawRectangle(chartX, chartY, chartWidth, chartHeight);

            canvas.FontColor = Colors.DimGray;
            canvas.FontSize = 11;
            canvas.DrawString(_maximum.ToString("0.##"), dirtyRect.X + 4, chartY - 8, left - 8, 20, HorizontalAlignment.Right, VerticalAlignment.Center);
            canvas.DrawString(_minimum.ToString("0.##"), dirtyRect.X + 4, chartY + chartHeight - 10, left - 8, 20, HorizontalAlignment.Right, VerticalAlignment.Center);

            if (Values.Length == 0)
            {
                canvas.FontColor = Colors.Gray;
                canvas.FontSize = 12;
                canvas.DrawString("Ожидание данных", chartX, chartY, chartWidth, chartHeight, HorizontalAlignment.Center, VerticalAlignment.Center);
                return;
            }

            if (Values.Length == 1)
            {
                DrawPoint(canvas, chartX + chartWidth, GetY(chartY, chartHeight, Values[0]));
                return;
            }

            PathF path = new();

            for (int index = 0; index < Values.Length; index++)
            {
                float x = chartX + (chartWidth * index / (Values.Length - 1));
                float y = GetY(chartY, chartHeight, Values[index]);

                if (index == 0)
                    path.MoveTo(x, y);
                else
                    path.LineTo(x, y);
            }

            canvas.StrokeColor = _lineColor;
            canvas.StrokeSize = 2;
            canvas.DrawPath(path);

            DrawPoint(canvas, chartX + chartWidth, GetY(chartY, chartHeight, Values[^1]));
        }

        private float GetY(float chartY, float chartHeight, double value)
        {
            double boundedValue = Math.Clamp(value, _minimum, _maximum);
            double ratio = (boundedValue - _minimum) / (_maximum - _minimum);
            return chartY + chartHeight - ((float)ratio * chartHeight);
        }

        private void DrawPoint(ICanvas canvas, float x, float y)
        {
            canvas.FillColor = _lineColor;
            canvas.FillCircle(x, y, 4);
        }
    }
}
