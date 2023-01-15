using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;
using Xceed.Wpf.Toolkit;

namespace Sperinski_Triangle_Visualizazion
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private int layers = 1;
        private bool started = false;
        private int _frameCounter = 0;
        private Stopwatch _stopWatch = new();
        private Point _pt;
        private Point _prept;
        private Line[] sierpinskiDrawBuffer;
        private Color sierpinskiColor = Colors.Black;
        private int StrokeThickness = 3;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void LayerAddButton_Click(object sender, RoutedEventArgs e)
        {
            if (layers + 1 > 7)
            {
                return;
            }
            layers += 1;
            LayersText.Text = layers.ToString();
        }

        private void LayerSubtarctButton_Click(object sender, RoutedEventArgs e)
        {
            if (layers - 1 <= 0)
            {
                return;
            }
            layers -= 1;
            LayersText.Text = layers.ToString();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LayersText.Text = layers.ToString();
            CompositionTarget.Rendering += Draw;
            UpdateTriangle("a", new EventArgs());
            colorPicker.IsOpen = true;
            colorPicker.ShowDropDownButton = false;
            ThicknessText.Text = StrokeThickness.ToString();
        }

        private void Start_Stop(object sender, RoutedEventArgs e)
        {
            if (!started)
            {
                started = true;
                StartButton.Content = "Stop";
            }
            else
            {
                started = false;
                StartButton.Content = "Restart";  
            }
        }

        protected void Draw(object sender, EventArgs e)
        {
            if (started)
            {
                if (_frameCounter++ == 0)
                {
                    _stopWatch.Start();
                }

                long frameRate = (long)(_frameCounter / this._stopWatch.Elapsed.TotalSeconds);
                _pt = Mouse.GetPosition(Window);
                if (_pt.X < 0)
                {
                    _pt.X = _prept.X;
                }
                else
                {
                    _prept.X = _pt.X;
                }
                if (_pt.Y < 0)
                {
                    _pt.Y = _prept.Y;
                }
                else
                {
                    _prept.Y = _pt.Y;
                }
                if (frameRate > 0)
                {
                    TimeElapsedText.Text = string.Format("Time Elapsed: {0, 0:N2}", _stopWatch.Elapsed.TotalSeconds);
                    FrameCounterText.Text = $"Frame Count: {_frameCounter}";
                    FPSText.Text = $"FPS: {frameRate}";
                    MouseXText.Text = string.Format("Mouse X: {0, 0:N2}", _pt.X);
                    MouseYText.Text = string.Format("Mouse Y: {0, 0:N2}", _pt.Y);
                }



                for(int i = 0; i < sierpinskiDrawBuffer.Length; i++)
                {
                    Canvas_Display.Children.Remove(sierpinskiDrawBuffer[i]);
                }
                for (int i = 0; i < sierpinskiDrawBuffer.Length; i++)
                {
                    Canvas_Display.Children.Add(sierpinskiDrawBuffer[i]);
                }


            }

        }

        private void UpdateTriangle(object sender, EventArgs e)
        {
            ComputedText.Text = "is Triangle Ready: False";
            if (sierpinskiDrawBuffer != null)
            {
                for (int i = 0; i < sierpinskiDrawBuffer.Length; i++)
                {
                    Canvas_Display.Children.Remove(sierpinskiDrawBuffer[i]);
                }
            }
            sierpinskiDrawBuffer = ComputeSierpinski(SizeSlider.Value, layers, Canvas_Display, sierpinskiColor, StrokeThickness);
            ComputedText.Text = "is Triangle Ready: True";
        }


        private void Reset(object sender, RoutedEventArgs e)
        {
            if(started)
            {
                return;
            }
            StartButton.Content = "Start";
            _stopWatch.Reset();
            _frameCounter = 0;
            TimeElapsedText.Text = "Time Elapsed: 0";
            FrameCounterText.Text = $"Frame Count: 0";
            FPSText.Text = $"FPS: 0";
            for (int i = 0; i < sierpinskiDrawBuffer.Length; i++)
            {
                Canvas_Display.Children.Remove(sierpinskiDrawBuffer[i]);
            }
        }

        private (SolidColorBrush, Color) GetHighContrast(Color basecolor)
        {
            const int treshold = 300;
            int r = basecolor.R;
            int g = basecolor.G;
            int b = basecolor.B;
            if (r + g + b > 128)
            {
                if((max(r, 0) - min(r, 0)) + (max(g, 0) - min(g, 0)) + (max(b, 0) - min(b, 0)) > treshold)
                {
                    return (new SolidColorBrush(Color.FromRgb(0, 0, 0)), Color.FromRgb(0, 0, 0));
                }
                else if ((max(r, 255 - r) - min(r, 255 - r)) + (max(g, 255 - g) - min(g, 255 - g)) + (max(b, 255 - b) - min(b, 255 - b)) > treshold)
                {
                    return (new SolidColorBrush(Color.FromRgb((byte)(255 - r), (byte)(255 - g), (byte)(255 - b))), Color.FromRgb((byte)(255 - r), (byte)(255 - g), (byte)(255 - b)));
                }
                else if ((max(r, (255 - r) / 2) - min(r, (255 - r) / 2)) + (max(g, (255 - g) / 2) - min(g, (255 - g) / 2)) + (max(b, (255 - b) / 2) - min(b, (255 - b) / 2)) > treshold)
                {
                    return (new SolidColorBrush(Color.FromRgb((byte)((255 - r) / 2), (byte)((255 - g) / 2), (byte)((255 - b) / 2))), Color.FromRgb((byte)((255 - r) / 2), (byte)((255 - g) / 2), (byte)((255 - b) / 2)));
                }
                else
                {
                    return (new SolidColorBrush(Color.FromRgb(0, 0, 0)), Color.FromRgb(0, 0, 0));
                }
            }
            else
            {
                if ((max(r, 255) - min(r, 255)) + (max(g, 255) - min(g, 255)) + (max(b, 255) - min(b, 255)) > treshold)
                {
                    return (new SolidColorBrush(Color.FromRgb(255, 255, 255)), Color.FromRgb(255, 255, 255));
                }
                else if ((max(r, 255 - r) - min(r, 255 - r)) + (max(g, 255 - g) - min(g, 255 - g)) + (max(b, 255 - b) - min(b, 255 - b)) > treshold)
                {
                    return (new SolidColorBrush(Color.FromRgb((byte)(255 - r), (byte)(255 - g), (byte)(255 - b))), Color.FromRgb((byte)(255 - r), (byte)(255 - g), (byte)(255 - b)));
                }
                else if ((max(r, (255 - r) / 2) - min(r, (255 - r) / 2)) + (max(g, (255 - g) / 2) - min(g, (255 - g) / 2)) + (max(b, (255 - b) / 2) - min(b, (255 - b) / 2)) > treshold)
                {
                    return (new SolidColorBrush(Color.FromRgb((byte)((255 - r) / 2), (byte)((255 - g) / 2), (byte)((255 - b) / 2))), Color.FromRgb((byte)((255 - r) / 2), (byte)((255 - g) / 2), (byte)((255 - b) / 2)));
                }
                else
                {
                    return (new SolidColorBrush(Color.FromRgb(255, 255, 255)), Color.FromRgb(255, 255, 255));
                }
            }
        }

        internal int max(int n1, int n2)
        {
            if (n1 > n2)
            {
                return n1;
            }
            else
            {
                return n2;
            }
        }

        internal int min(int n1, int n2)
        {
            if (n1 < n2)
            {
                return n1;
            }
            else
            {
                return n2;
            }
        }

        internal Line[] ComputeSierpinski(double size, int layers, Canvas canvas, Color color, int strokeWeight = 3, double Xoff = -1, double Yoff = -1)
        {
            if(Xoff == -1)
            {
                Xoff = canvas.ActualWidth;
            }
            if (Yoff == -1)
            {
                Yoff = canvas.ActualHeight;
            }
            List<Line> sierpinski = Triangle(new Point(Xoff, Yoff), size);
            if(layers == 0)
            {
                return sierpinski.ToArray();
            }
            else
            {
                sierpinski.AddRange(ComputeSierpinski(size / 2, layers - 1, canvas, color, strokeWeight, Xoff, Yoff));
                sierpinski.AddRange(ComputeSierpinski(size / 2, layers - 1, canvas, color, strokeWeight, Xoff - (size / 2), Yoff));
                sierpinski.AddRange(ComputeSierpinski(size / 2, layers - 1, canvas, color, strokeWeight, Xoff - (size / 4), Yoff - (((size * Math.Sqrt(3)) / 2) / 2)));
            }
            Line[] res = sierpinski.ToArray();
            foreach(Line line in res)
            {
                line.Stroke = new SolidColorBrush(color);
                line.StrokeThickness = strokeWeight;
            }
            return res;
        }

        internal List<Line> Triangle(Point p, double size)
        {
            Line[] triangle = new Line[3];
            triangle[0] = makeLine(p.X, p.X - size, p.Y, p.Y);
            triangle[1] = makeLine(p.X, p.X - (size / 2), p.Y, p.Y - ((size * Math.Sqrt(3)) / 2));
            triangle[2] = makeLine(p.X - size, p.X - (size / 2), p.Y, p.Y - ((size * Math.Sqrt(3)) / 2));
            return triangle.ToList();
        }

        internal Line makeLine(double x1, double x2, double y1, double y2)
        {
            Line line = new();
            line.X1 = x1;
            line.X2 = x2;
            line.Y1 = y1;
            line.Y2 = y2;
            return line;
        }

        private void colorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            sierpinskiColor = (Color)colorPicker.SelectedColor;
            UpdateTriangle("a", new EventArgs());
        }

        private void ThicknessSubtarctButton_Click(object sender, RoutedEventArgs e)
        {
            if (StrokeThickness - 1 < 0)
            {
                return;
            }
            StrokeThickness -= 1;
            ThicknessText.Text = StrokeThickness.ToString();
        }

        private void ThicknessAddButton_Click(object sender, RoutedEventArgs e)
        {
            StrokeThickness += 1;
            ThicknessText.Text = StrokeThickness.ToString();
        }
    }


}
