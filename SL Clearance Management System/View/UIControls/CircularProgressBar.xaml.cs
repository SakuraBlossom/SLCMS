using System;
using System.Windows;
using System.Windows.Media;

namespace SLCMS.View.UIControls {
    /// <summary>
    /// Interaction logic for CircularProgressBar.xaml
    /// </summary>
    public partial class CircularProgressBar
    {
        public CircularProgressBar()
        {
            InitializeComponent();
            RenderArc();
        }


        public int Radius
        {
            get => (int)GetValue(RadiusProperty);
            set => SetValue(RadiusProperty, value);
        }

        public Brush SegmentColor
        {
            get => (Brush)GetValue(SegmentColorProperty);
            set => SetValue(SegmentColorProperty, value);
        }

        public int StrokeThickness
        {
            get => (int)GetValue(StrokeThicknessProperty);
            set => SetValue(StrokeThicknessProperty, value);
        }

        public double Percentage
        {
            get => (double)GetValue(PercentageProperty);
            set => SetValue(PercentageProperty, value);
        }

        // Using a DependencyProperty as the backing store for Percentage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PercentageProperty =
            DependencyProperty.Register("Percentage", typeof(double), typeof(CircularProgressBar), new PropertyMetadata(65d, new PropertyChangedCallback(OnPropertyChanged)));

        // Using a DependencyProperty as the backing store for StrokeThickness.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StrokeThicknessProperty =
            DependencyProperty.Register("StrokeThickness", typeof(int), typeof(CircularProgressBar), new PropertyMetadata(5));

        // Using a DependencyProperty as the backing store for SegmentColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SegmentColorProperty =
            DependencyProperty.Register("SegmentColor", typeof(Brush), typeof(CircularProgressBar), new PropertyMetadata(new SolidColorBrush(Colors.Red)));

        // Using a DependencyProperty as the backing store for Radius.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RadiusProperty =
            DependencyProperty.Register("Radius", typeof(int), typeof(CircularProgressBar), new PropertyMetadata(25, new PropertyChangedCallback(OnPropertyChanged)));
        
        private static void OnPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var circle = (sender as CircularProgressBar);
            if (circle == null)
                return;

            var expectedsize = circle.Radius * 2 + circle.StrokeThickness * 2;

            circle.Height = expectedsize;
            circle.Width = expectedsize;
            circle.RenderArc();
        }

        private Point ComputeCartesianCoordinate(double angle, double radius)
        {
            // convert to radians
            var angleRad = (Math.PI / 180.0) * (angle - 90);

            var x = radius * Math.Cos(angleRad);
            var y = radius * Math.Sin(angleRad);

            return new Point(x, y);
        }
        public void RenderArc()
        {
            var angle = (Percentage * 360) / 100;
            var startPoint = new Point(Radius, 0);
            var endPoint = ComputeCartesianCoordinate(angle, Radius);
            endPoint.X += Radius;
            endPoint.Y += Radius;
            
            pathRoot.Width = Radius * 2 + StrokeThickness;
            pathRoot.Height = Radius * 2 + StrokeThickness;
            pathRoot.Margin = new Thickness(StrokeThickness, StrokeThickness, 0, 0);

            pathFigure.StartPoint = startPoint;

            if (Math.Abs(startPoint.X - Math.Round(endPoint.X)) < 0.01 && Math.Abs(startPoint.Y - Math.Round(endPoint.Y)) < 0.01)
                endPoint.X -= 0.01;

            arcSegment.Point = endPoint;
            arcSegment.Size = new Size(Radius, Radius);
            arcSegment.IsLargeArc = (angle > 180.0);
        }
    }
}
