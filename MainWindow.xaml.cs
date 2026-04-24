using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace WpfPaint
{

    enum Tool
    {
        Pen,
        Rectangle,
        Ellipse,
        Text,
        Eraser
    }
    enum ColorType //factory method pattern
    {
        Black,
        Red,
        Blue,
        Green,
        Yellow,
        Purple
    }

    interface ICommand //command pattern
    {
        void Execute();
        void Undo();
    }

    class DrawCommand : ICommand
    {
        private Canvas canvas;
        private UIElement element;

        public DrawCommand(Canvas canvas, UIElement element)
        {
            this.canvas = canvas;
            this.element = element;
        }

        public void Execute()
        {
            canvas.Children.Add(element);
        }

        public void Undo()
        {
            canvas.Children.Remove(element);
        }
    }

    class BrushFactory //factory method tüm renkler tek bi yerden yönetiliyo
    {
        public static Brush CreateBrush(ColorType type)
        {
            switch (type)
            {
                case ColorType.Black:
                    return Brushes.Black;
                case ColorType.Red:
                    return Brushes.Red;
                case ColorType.Blue:
                    return Brushes.Blue;
                case ColorType.Green:
                    return Brushes.Green;
                case ColorType.Yellow:
                    return Brushes.Yellow;
                case ColorType.Purple:
                    return Brushes.Purple;
                default:
                    return Brushes.Black;
            }
        }
    }

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }




        Tool currentTool = Tool.Pen;
        bool isDrawing = false;
        Polyline currentLine;
        Brush currentBrush = Brushes.Black;
        double currentThickness = 2;
        Point startPoint;
        Shape currentShape;
        Stack<ICommand> undoStack = new();
        Stack<ICommand> redoStack = new();





        private void DrawCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            isDrawing = true;
            startPoint = e.GetPosition(DrawCanvas);


            if (currentTool == Tool.Text)
            {
                TextBox textBox = new TextBox
                {
                    Width = 150,
                    Height = 30,
                    FontFamily = new FontFamily("Arial"),
                    FontSize = 20,
                    Foreground = currentBrush,
                    Background = Brushes.Transparent,
                    BorderThickness = new Thickness(1)
                };

                Canvas.SetLeft(textBox, startPoint.X);
                Canvas.SetTop(textBox, startPoint.Y);

                DrawCanvas.Children.Add(textBox);
                textBox.Focus();

                textBox.LostFocus += (s, ev) =>
                {
                    if (string.IsNullOrWhiteSpace(textBox.Text))
                    {
                        DrawCanvas.Children.Remove(textBox);
                        return;
                    }

                    DrawCanvas.Children.Remove(textBox);

                    TextBlock text = new TextBlock
                    {
                        Text = textBox.Text,
                        FontFamily = textBox.FontFamily,
                        FontSize = textBox.FontSize,
                        Foreground = textBox.Foreground
                    };

                    Canvas.SetLeft(text, Canvas.GetLeft(textBox));
                    Canvas.SetTop(text, Canvas.GetTop(textBox));

                    ICommand cmd = new DrawCommand(DrawCanvas, text);
                    cmd.Execute();
                    undoStack.Push(cmd);
                    redoStack.Clear();
                };

                textBox.KeyDown += (s, ev) =>
                {
                    if (ev.Key == Key.Enter)
                    {
                        DrawCanvas.Focus(); 
                    }
                };

                isDrawing = false; 
                return;
            }

            if (currentTool == Tool.Eraser)
            {
                currentLine = new Polyline
                {
                    Stroke = Brushes.White, 
                    StrokeThickness = currentThickness * 3 
                };

                currentLine.Points.Add(startPoint);
                DrawCanvas.Children.Add(currentLine);
                return;
            }




            if (currentTool == Tool.Pen)
            {
                currentLine = new Polyline
                {
                    Stroke = currentBrush,
                    StrokeThickness = currentThickness
                };

                currentLine.Points.Add(startPoint);
                DrawCanvas.Children.Add(currentLine);
            }
            else
            {
                if (currentTool == Tool.Rectangle)
                    currentShape = new Rectangle();
                else if (currentTool == Tool.Ellipse)
                    currentShape = new Ellipse();

                currentShape.Stroke = currentBrush;
                currentShape.StrokeThickness = currentThickness;
                currentShape.Fill = Brushes.Transparent;

                Canvas.SetLeft(currentShape, startPoint.X);
                Canvas.SetTop(currentShape, startPoint.Y);

                DrawCanvas.Children.Add(currentShape);
            }


        }


        private void DrawCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isDrawing) return;

            Point pos = e.GetPosition(DrawCanvas);

            if (currentTool == Tool.Pen || currentTool == Tool.Eraser)
            {
                currentLine.Points.Add(pos);
            }
            else
            {
                double x = Math.Min(pos.X, startPoint.X);
                double y = Math.Min(pos.Y, startPoint.Y);

                double w = Math.Abs(pos.X - startPoint.X);
                double h = Math.Abs(pos.Y - startPoint.Y);

                Canvas.SetLeft(currentShape, x);
                Canvas.SetTop(currentShape, y);

                currentShape.Width = w;
                currentShape.Height = h;
            }
        }


        private void DrawCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!isDrawing) return;
            isDrawing = false;

            if (currentLine != null)
            {
                ICommand cmd = new DrawCommand(DrawCanvas, currentLine);
                undoStack.Push(cmd);
                redoStack.Clear();
            }

            if (currentShape != null)
            {
                ICommand cmd = new DrawCommand(DrawCanvas, currentShape);
                undoStack.Push(cmd);
                redoStack.Clear();
            }

            currentLine = null;
            currentShape = null;
        }



        private void PenTool_Click(object sender, RoutedEventArgs e)
        {
            currentTool = Tool.Pen;
        }
        private void ColorBlack_Click(object sender, RoutedEventArgs e)
        {
            currentBrush = BrushFactory.CreateBrush(ColorType.Black);
        }

        private void ColorRed_Click(object sender, RoutedEventArgs e)
        {
            currentBrush = BrushFactory.CreateBrush(ColorType.Red);
        }

        private void ColorBlue_Click(object sender, RoutedEventArgs e)
        {
            currentBrush = BrushFactory.CreateBrush(ColorType.Blue);
        }

        private void ColorGreen_Click(object sender, RoutedEventArgs e)
        {
            currentBrush = BrushFactory.CreateBrush(ColorType.Green);
        }

        private void ColorYellow_Click(object sender, RoutedEventArgs e)
        {
            currentBrush = BrushFactory.CreateBrush(ColorType.Yellow);
        }

        private void ColorPurple_Click(object sender, RoutedEventArgs e)
        {
            currentBrush = BrushFactory.CreateBrush(ColorType.Purple);
        }


        private void ThicknessSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            currentThickness = e.NewValue;
            if (ThicknessLabel != null)
                ThicknessLabel.Text = $"{e.NewValue:0} px";
        }

        private void RectangleTool_Click(object sender, RoutedEventArgs e)
        {
            currentTool = Tool.Rectangle;
        }

        private void EllipseTool_Click(object sender, RoutedEventArgs e)
        {
            currentTool = Tool.Ellipse;
        }

        private void Undo_Click(object sender, RoutedEventArgs e)
        {
            if (undoStack.Count == 0) return;

            ICommand cmd = undoStack.Pop();
            cmd.Undo();
            redoStack.Push(cmd);
        }


        private void Redo_Click(object sender, RoutedEventArgs e)
        {
            if (redoStack.Count == 0) return;

            ICommand cmd = redoStack.Pop();
            cmd.Execute();
            undoStack.Push(cmd);
        }

        private void TextTool_Click(object sender, RoutedEventArgs e)
        {
            currentTool = Tool.Text;
        }

        private void EraserTool_Click(object sender, RoutedEventArgs e)
        {
            currentTool = Tool.Eraser;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // kaydetme penceresi
            Microsoft.Win32.SaveFileDialog saveDialog = new Microsoft.Win32.SaveFileDialog();
            saveDialog.Filter = "Bitmap Dosyası|*.bmp|JPEG Dosyası|*.jpg|PNG Dosyası|*.png";
            saveDialog.FileName = "cizim";

            // kaydet buton
            if (saveDialog.ShowDialog() == true)
            {
                // canvas bitmape çevrilir
                RenderTargetBitmap bitmap = new RenderTargetBitmap(
                    (int)DrawCanvas.ActualWidth,
                    (int)DrawCanvas.ActualHeight,
                    96, 96,
                    PixelFormats.Pbgra32);

                bitmap.Render(DrawCanvas);

                // dosya uzantısına göre encoder
                BitmapEncoder encoder;
                string extension = System.IO.Path.GetExtension(saveDialog.FileName).ToLower();

                if (extension == ".jpg" || extension == ".jpeg")
                    encoder = new JpegBitmapEncoder();
                else if (extension == ".png")
                    encoder = new PngBitmapEncoder();
                else
                    encoder = new BmpBitmapEncoder();

                encoder.Frames.Add(BitmapFrame.Create(bitmap));

                // dosyaya yaz
                using (var file = System.IO.File.Create(saveDialog.FileName))
                {
                    encoder.Save(file);
                }

                MessageBox.Show("Kaydedildi!");
            }
        }
    }
}

