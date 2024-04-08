﻿using System.IO;
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

namespace Simple_Paint
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ShapeToDraw curShape;
        bool isDraw = false;
        // Default setting
        public static SolidColorBrush fillColorMain = null;
        public static SolidColorBrush borderColorMain = Brushes.Black;
        public static double thickness = 1;
        public static Stroke stroke = new SolidStroke(Brushes.Black, 1,null);
        public static string typeOfStroke = "Solid";

        public static Point topLeft = new Point(0, 0);
        public static Point bottomRight = new Point(0, 0);

        public MainWindow()
        {
            InitializeComponent();
            ShapeToDraw.canvas = canvas;
            curShape = new LineShape(new Point(0, 0), new Point(0, 0));
        }


        private void CopyToClipboard_Click(object sender, RoutedEventArgs e)
        {
            // Tính toán lại để đảm bảo đúng cặp điểm góc trái trên và góc phải dưới
            double left = Math.Min(topLeft.X, bottomRight.X);
            double top = Math.Min(topLeft.Y, bottomRight.Y);
            double right = Math.Max(topLeft.X, bottomRight.X);
            double bottom = Math.Max(topLeft.Y, bottomRight.Y);

            // Tạo điểm góc trái trên và góc phải dưới mới
            Point newTopLeft = new Point(left, top);
            Point newBottomRight = new Point(right, bottom);

            // Tính toán kích thước của hình chữ nhật
            int width = (int)(newBottomRight.X - newTopLeft.X);
            int height = (int)(newBottomRight.Y - newTopLeft.Y);

            // Tạo một RenderTargetBitmap để chụp nội dung của hình chữ nhật
            var scale = VisualTreeHelper.GetDpi(canvas).DpiScaleX;
            var renderTarget = new RenderTargetBitmap(
                (int)(width * scale),
                (int)(height * scale),
                96 * scale,
                96 * scale,
                PixelFormats.Pbgra32
            );

            // Tạo một DrawingVisual để render hình ảnh
            DrawingVisual visual = new DrawingVisual();
            using (DrawingContext context = visual.RenderOpen())
            {
                // Chỉ định vùng để chụp (phần của Canvas cần sao chép)
                VisualBrush visualBrush = new VisualBrush(canvas)
                {
                    ViewboxUnits = BrushMappingMode.Absolute,
                    Viewbox = new Rect(newTopLeft.X, newTopLeft.Y, width, height)
                };

                context.DrawRectangle(visualBrush, null, new Rect(0, 0, width, height));
            }

            // Render hình ảnh và sao chép nó vào clipboard
            renderTarget.Render(visual);

            // Tạo một hình ảnh từ bitmap
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(renderTarget));

            using (var stream = new MemoryStream())
            {
                encoder.Save(stream);
                stream.Seek(0, SeekOrigin.Begin); // Quay trở lại đầu stream trước khi đọc

                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = stream;
                bitmapImage.EndInit();

                // Lưu hình ảnh vào clipboard dưới dạng PNG
                Clipboard.SetImage(bitmapImage);
            }
        }




        private void ThicknessComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.SelectedItem != null)
            {
                // Assuming the ComboBox items are strings representing thicknesses
                ComboBoxItem typeItem = (ComboBoxItem)comboBox.SelectedItem;
                string selectedThickness = typeItem.Content.ToString();

                if (double.TryParse(selectedThickness, out double thicknessValue))
                {
                    thickness = double.Parse(selectedThickness);
                }
                else
                {
                    MessageBox.Show(selectedThickness + " is not a valid thickness value.");
                }
            }
        }
        private void BorderStyleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.SelectedItem != null)
            {
                ComboBoxItem typeItem = (ComboBoxItem)comboBox.SelectedItem;
                string type = typeItem.Content.ToString();

                typeOfStroke = type;
            }
        }
        private void LineButton_Click(object sender, RoutedEventArgs e)
        {
            if (curShape is not LineShape)
            {
                curShape = new LineShape(new Point(0, 0), new Point(0, 0));
            }
        }
        private void EllipseButton_Click(object sender, RoutedEventArgs e)
        {
            if (curShape is not EllipseShape)
            {
                curShape = new EllipseShape(new Point(0, 0), new Point(0, 0));
            }
        }
        private void RectangleButton_Click(object sender, RoutedEventArgs e)
        {
            if (curShape is not RectangleShape)
            {
                curShape = new RectangleShape(new Point(0, 0), new Point(0, 0));
            }
        }

        private void TriangleButton_Click(object sender, RoutedEventArgs e)
        {
            if (curShape is not TriangleShape)
            {
                curShape = new TriangleShape(new Point(0, 0), new Point(0, 0));
            }
        }

        private void StarButton_Click(object sender, RoutedEventArgs e)
        {
            if (curShape is not StarShape)
            {
                curShape = new StarShape(new Point(0, 0), new Point(0, 0));
            }
        }

        private void ArrowButton_Click(object sender, RoutedEventArgs e)
        {
            if (curShape is not ArrowShape)
            {
                curShape = new ArrowShape(new Point(0, 0), new Point(0, 0));
            }
        }
        
        private void ArrowPentagonButton_Click(object sender, RoutedEventArgs e)
        {
            if (curShape is not PentagonArrowShape)
            {
                curShape = new PentagonArrowShape(new Point(0, 0), new Point(0, 0));
            }
        }

        private void CollateButton_Click(object sender, RoutedEventArgs e)
        {
            if (curShape is not CollateShape)
            {
                curShape = new CollateShape(new Point(0, 0), new Point(0, 0));
            }
        }

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed && isDraw == false)
            {
                Stroke stroke = new SolidStroke(borderColorMain, thickness, fillColorMain);
                if (typeOfStroke == "Solid")
                {
                    stroke = new SolidStroke(borderColorMain, thickness, fillColorMain);
                }
                else if (typeOfStroke == "Dash")
                {
                    stroke = new DashStroke(borderColorMain, thickness, fillColorMain);
                }
                else if (typeOfStroke == "Dot")
                {
                    stroke = new DotStroke(borderColorMain, thickness, fillColorMain);
                }
                else if (typeOfStroke == "DashDotDot")
                {
                    stroke = new DashDotDotStroke(borderColorMain, thickness, fillColorMain);
                }
                curShape.stroke = stroke;

                Point point = e.GetPosition(canvas);
                topLeft = point;
                if (point != null) {
                    curShape.StartPoint = point;
                    curShape.EndPoint = point;
                    curShape.Draw();
                    isDraw = true;
                }
                
            }
            

        }

        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Released && isDraw)
            {
                bottomRight = e.GetPosition(canvas);
                curShape.EndPoint = e.GetPosition(canvas);
                isDraw = false;
            }
        }

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDraw)
            {
                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                {
                    if (curShape is EllipseShape || curShape is RectangleShape)
                    {
                        // I want the endpoint make the shape into circle

                        Point curPoint = e.GetPosition(canvas);
                        // Tính toán độ dài cạnh của hình vuông
                        double sideLength = Math.Min(Math.Abs(curPoint.X - curShape.StartPoint.X), Math.Abs(curPoint.Y - curShape.StartPoint.Y));

                        // Tính toán tọa độ của endPoint
                        double endX = curShape.StartPoint.X + (curPoint.X > curShape.StartPoint.X ? sideLength : -sideLength);
                        double endY = curShape.StartPoint.Y + (curPoint.Y > curShape.StartPoint.Y ? sideLength : -sideLength);

                        // Tạo ra endPoint từ tọa độ tính toán được
                        Point endPoint = new Point(endX, endY);

                        curShape.EndPoint = endPoint;
                        curShape.UpdateEndPoint();


                    }

                }
                else
                {
                    curShape.EndPoint = e.GetPosition(canvas);
                    curShape.UpdateEndPoint();
                }
            }
        }

        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void FillColor_Click(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = (RadioButton)sender;
            if (radioButton.IsChecked == true)
            {
                fillColorMain = (SolidColorBrush)radioButton.Tag;
            }
            if (curShape != null)
            {
                
            }
        }

        private void BorderColor_Click(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = (RadioButton)sender;
            if (radioButton.IsChecked == true)
            {
                borderColorMain = (SolidColorBrush)radioButton.Tag;
            }
            if (curShape != null)
            {
                
            }
        }
    }


}