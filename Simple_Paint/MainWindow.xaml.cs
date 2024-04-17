﻿using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;

using System.Windows.Media.Imaging;

using Microsoft.Win32;


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

        public static List<ShapeToDraw> history = new List<ShapeToDraw>();
        public static string shapeType = "Line";
        public static ShapeToDraw copyShape = null;
        public static int copyCutState = 0;

        public static int code = 0;
        
        public const int NORMAL = 0;
        public const int ISSELECT = 1;
        public const int ISSELECTELEMENT = 2;
        public const int ISDRAGELEMENT = 3;
        public const int ISRESIZE = 4;


        public MainWindow()
        {
            InitializeComponent();
            ShapeToDraw.canvas = canvas;
            Caretaker.add(new Memento(history));
            curShape = new LineShape(new Point(0, 0), new Point(0, 0));
        }

        private void SaveSolidColorBrush(BinaryWriter writer, SolidColorBrush brush)
        {
            if (brush == null)
            {
                // Ghi một byte 0 để biểu thị rằng brush là null
                writer.Write((byte)0);
                return;
            }
            // Ghi một byte 1 để biểu thị rằng brush không phải là null
            writer.Write((byte)1);
            Color color = brush.Color;
            writer.Write(color.A);
            writer.Write(color.R);
            writer.Write(color.G);
            writer.Write(color.B);
        }

        private SolidColorBrush LoadSolidColorBrush(BinaryReader reader)
        {
            byte indicator = reader.ReadByte();
            if (indicator == 0)
            {
                // Nếu byte đầu tiên là 0, brush là null
                return null;
            }
            // Nếu không, đọc giá trị màu
            byte a = reader.ReadByte();
            byte r = reader.ReadByte();
            byte g = reader.ReadByte();
            byte b = reader.ReadByte();

            Color color = Color.FromArgb(a, r, g, b);
            return new SolidColorBrush(color);
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.InitialDirectory = Directory.GetCurrentDirectory() + "\\Saved Game";
            saveFileDialog.Filter = "Binary files (*.bin)|*.bin|All files (*.*)|*.*";

            if (saveFileDialog.ShowDialog() == true)
            {
                SaveShapes(history, saveFileDialog.FileName);
            }
        }


        private void SaveShapes(List<ShapeToDraw> shapes, string fileName)
        {
            using (FileStream fs = new FileStream(fileName, FileMode.Create))
            using (BinaryWriter writer = new BinaryWriter(fs))
            {
                writer.Write(shapes.Count);
                foreach (ShapeToDraw shape in shapes)
                {
                    writer.Write(shape.GetShapeType());
                    writer.Write(shape.StartPoint.X);
                    writer.Write(shape.StartPoint.Y);
                    writer.Write(shape.EndPoint.X);
                    writer.Write(shape.EndPoint.Y);
                    writer.Write(shape.curAngle);

                    writer.Write(shape.stroke.GetStrokeType());
                    writer.Write(shape.stroke.thickness);
                    SaveSolidColorBrush(writer, shape.stroke.fillColor);
                    SaveSolidColorBrush(writer, shape.stroke.borderColor);

                }
            }
        }




        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = Directory.GetCurrentDirectory() + "\\Saved Game";
            openFileDialog.Filter = "Binary files (*.bin)|*.bin|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                history = LoadShapes(openFileDialog.FileName);
                // Cập nhật UI hoặc canvas dựa trên các hình đã tải
            }
        }


        private List<ShapeToDraw> LoadShapes(string fileName)
        {
            List<ShapeToDraw> shapes = new List<ShapeToDraw>();

            using (FileStream fs = new FileStream(fileName, FileMode.Open))
            using (BinaryReader reader = new BinaryReader(fs))
            {
                int count = reader.ReadInt32();
                for (int i = 0; i < count; i++)
                {
                    string shapeType = reader.ReadString();
                    double startX = reader.ReadDouble();
                    double startY = reader.ReadDouble();
                    double endX = reader.ReadDouble();
                    double endY = reader.ReadDouble();
                    double curAngle = reader.ReadDouble();

                    string strokeType = reader.ReadString();
                    double thickness = reader.ReadDouble();
                    SolidColorBrush fillColor = LoadSolidColorBrush(reader);
                    SolidColorBrush borderColor = LoadSolidColorBrush(reader);

                    ShapeToDraw shape = FactoryShape.CreateShape(shapeType, strokeType, borderColor, thickness, fillColor);
                    shape.StartPoint = new Point(startX, startY);
                    shape.EndPoint = new Point(endX, endY);
                    shape.Rotate(curAngle);
                    shape.Draw();

                    shapes.Add(shape);
                }
            }

            return shapes;
        }








        private void Rotate_Button(object sender, RoutedEventArgs e)
        {
            // Thêm code để xử lý sự kiện khi nút được nhấn
            curShape.Rotate(45);
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.C && Keyboard.Modifiers == ModifierKeys.Control)
            {
                // Handle Ctrl + C here (e.g., perform copy operation)
                if (!isDraw)
                {
                    copyShape = curShape;
                    copyCutState = 0;
                }

                e.Handled = true; // Mark the event as handled to prevent further processing
            }
            else if (e.Key == Key.X && Keyboard.Modifiers == ModifierKeys.Control)
            {
                // Handle Ctrl + C here (e.g., perform copy operation)
                if (!isDraw)
                {
                    copyShape = curShape;
                    copyCutState = 1;
                }

                e.Handled = true; // Mark the event as handled to prevent further processing
            }
            else if (e.Key == Key.V && Keyboard.Modifiers == ModifierKeys.Control)
            {
                // Handle Ctrl + V here (e.g., perform paste operation)
                if(!isDraw && copyShape != null)
                {
                    if(copyCutState == 1)
                    {
                        copyShape.Remove();
                        history.Remove(copyShape);
                    }
                    copyShape = copyShape.Clone();
                    copyShape.StartPoint = new Point(copyShape.StartPoint.X + 20, copyShape.StartPoint.Y + 20);
                    copyShape.EndPoint = new Point(copyShape.EndPoint.X + 20, copyShape.EndPoint.Y + 20);
                    history.Add(copyShape);
                    copyShape.Draw();
                }
                e.Handled = true; // Mark the event as handled to prevent further processing
            }
        }

        // This method is called when the ToggleButton is checked
        private void CopyToClipboardToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            ToggleButton toggleButton = sender as ToggleButton;
            if (toggleButton != null)
            {
                // Perform actions when the button is checked
                // For instance, you could enable a mode in your application
                // that allows text to be copied to the clipboard when selected
                // You can also change the ToggleButton's content to indicate it's active
                toggleButton.Content = "Selecting";
                code = ISSELECT;
                // Suppose you have a TextBox named 'MyTextBox' and you want to copy its content
                // You can enable the logic here or simply copy the text to the clipboard directly.
                // Clipboard.SetText(MyTextBox.Text); // This line would copy the text directly
            }
        }

        // This method is called when the ToggleButton is unchecked
        private void CopyToClipboardToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            ToggleButton toggleButton = sender as ToggleButton;
            if (toggleButton != null)
            {
                // Perform actions to revert back to the normal state when the button is unchecked
                // For example, you might disable the copy-to-clipboard mode here
                // You can also revert the ToggleButton's content to its default state
                toggleButton.Content = "Select";
                code = NORMAL;

                // Any other cleanup or state restoration logic can go here
            }
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
            double width = (double)(newBottomRight.X - newTopLeft.X);
            double height = (double)(newBottomRight.Y - newTopLeft.Y);

            // Tạo một RenderTargetBitmap để chụp nội dung của hình chữ nhật
            var scale = VisualTreeHelper.GetDpi(canvas).DpiScaleX;
            var renderTarget = new RenderTargetBitmap(
                (int)Math.Round(width * scale),
                  (int)Math.Round(height * scale),
                    Math.Round(96 * scale),
                    Math.Round(96 * scale),
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
                    Viewbox = new Rect(newTopLeft.X, newTopLeft.Y+50, width, height)
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
            clearRectangale();
            code = NORMAL;

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
                    if (code == ISSELECTELEMENT)
                    {
                        curShape.stroke.thickness = thickness;
                        curShape.UpdateStartAndEndPoint();
                    }
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
                
                if (code == ISSELECTELEMENT)
                {
                    curShape.stroke = FactoryShape.CreateShape(shapeType, typeOfStroke, borderColorMain, thickness, fillColorMain).stroke;
                    curShape.UpdateStartAndEndPoint();
                }
            }
        }


        private void LineButton_Click(object sender, RoutedEventArgs e)
        {
            shapeType = "Line";

        }
        private void EllipseButton_Click(object sender, RoutedEventArgs e)
        {
            shapeType = "Ellipse";

        }
        private void RectangleButton_Click(object sender, RoutedEventArgs e)
        {
            shapeType = "Rectangle";
        }

        private void TriangleButton_Click(object sender, RoutedEventArgs e)
        {
            shapeType = "Triangle";
        }

        private void StarButton_Click(object sender, RoutedEventArgs e)
        {
            shapeType = "Star";
        }

        private void ArrowButton_Click(object sender, RoutedEventArgs e)
        {
            shapeType = "Arrow";
        }
        
        private void ArrowPentagonButton_Click(object sender, RoutedEventArgs e)
        {
            shapeType = "ArrowPentagon";
        }

        private void CollateButton_Click(object sender, RoutedEventArgs e)
        {
            shapeType = "Collate";
        }

        private void Canvas_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            Point bottomRight = e.GetPosition(canvas);
            if (bottomRight.Equals(topLeft))
            {
                curShape = null;
                curShape = GetShapeChoosen(bottomRight);

                if (curShape != null)
                {
                    code = ISSELECTELEMENT;
                }

            }
        }
        private void updateHistory()
        {
            for (int i = 0; i < history.Count; i++)
            {
                history[i].UpdateStartAndEndPoint();
            }
        }
        private ShapeToDraw GetShapeChoosen(Point point)
        {
            for (int i = history.Count - 1; i >= 0; i--)
            {
                if (history[i].IsPointInShape(point))
                {
                    return history[i];
                }
            }
            return null;
        }

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Check if it is a double click
            bool isDoubleClick = false;
            if (e.ClickCount == 2 && code != ISSELECTELEMENT)
            {
                // Handle double-click event here
                Canvas_DoubleClick(sender, e);
                isDoubleClick = true;
            }

            // Check if the clicked element is not a TextBox or its child
            if (!(e.OriginalSource is TextBox))
            {
                this.Focus();
            }

            if (e.LeftButton == MouseButtonState.Pressed && isDraw == false||(isDoubleClick))
            {
                Point point = e.GetPosition(canvas);
                topLeft = point;
                switch(code)
                {
                    case NORMAL:
                        if (point != null)
                        {
                            curShape = FactoryShape.CreateShape(shapeType, typeOfStroke, borderColorMain, thickness, fillColorMain);
                            curShape.StartPoint = new Point(point.X, point.Y);
                            curShape.EndPoint = new Point(point.X, point.Y);
                            curShape.Draw();
                            isDraw = true;
                        }
                        break;
                    case ISSELECT:
                        curShape = FactoryShape.CreateShape("Rectangle", "Dash", Brushes.Black, 1, null);
                        curShape.StartPoint = new Point(point.X, point.Y);
                        curShape.EndPoint = new Point(point.X, point.Y);
                        curShape.Draw();
                        isDraw = true;
                        break;
                    case ISSELECTELEMENT:
                        code = NORMAL;

                        if (curShape != null)
                        {
                            curShape.CreateBorderSelected();

                            if (curShape.IsInBorderRectangle(point) != 0)
                            {

                                code = ISRESIZE;
                            }
                            else if (curShape.StartDrag(point))
                            {

                                code = ISDRAGELEMENT;
                            }
                            else
                            {
                                curShape.RemoveBorderSelected();
                                curShape = null;
                            }
                        }
                        break;
                }

            }
        }
        

        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Point curPoint = e.GetPosition(canvas);
            if (e.LeftButton == MouseButtonState.Released )
            {
                if (isDraw)
                {
                    bottomRight = e.GetPosition(canvas);
                    if (bottomRight.Equals(topLeft))
                    {
                        curShape = null;
                        curShape = GetShapeChoosen(bottomRight);
                   /*     Stroke old = newcurShape.stroke;
                        curShape.stroke = new SolidStroke(Brushes.White, 3, Brushes.White);
                        curShape.Draw();
                        updateHistory();
                        curShape.stroke = old;*/

                        if (curShape != null)
                        {
                            code = ISSELECTELEMENT;
                        }

                    }
                    else
                    {
                        curShape.EndPoint = e.GetPosition(canvas);
                        if (curShape.StartPoint != curShape.EndPoint)
                        {
                            history.Add(curShape);
                            curShape.UpdateEndPoint();
                        }
                        Caretaker.add(new Memento(history));
                    }
                    
                    isDraw = false;
                }
                
                if(code == ISDRAGELEMENT || code == ISRESIZE)
                {
                    code = ISSELECTELEMENT;
                }

            }
            canvas.Cursor = Cursors.Arrow;

        }

        public void Mouse_DoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void AddText_Button(object sender, RoutedEventArgs e)
        {
            if (curShape != null && !isDraw)
            {
                var selectedSizeItem = SizeCombobox.SelectedItem as ComboBoxItem;
                string selectedSize = "12";
                if (selectedSizeItem != null)
                {
                    selectedSize = selectedSizeItem.Content.ToString();
                }
                var selectedFontItem = FontFamilyCombobox.SelectedItem as ComboBoxItem;
                string selectedFont = "Arial";
                if (selectedFontItem != null)
                {
                    selectedFont = selectedFontItem.Content.ToString();
                }

                curShape.attachTextBox(fillColorMain, borderColorMain, int.Parse(selectedSize), selectedFont);
            }
        }

        private void Undo_Button(object sender, RoutedEventArgs e)
        {
            Caretaker.undo();
        }

        private void Redo_Button(object sender, RoutedEventArgs e)
        {
            Caretaker.redo();
        }


        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            Point curPoint = e.GetPosition(canvas);

            if (isDraw)
            {

                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                {
                    if (curShape is EllipseShape || curShape is RectangleShape)
                    {
                        // I want the endpoint make the shape into circle

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
                    curShape.EndPoint = new Point(curPoint.X, curPoint.Y);
                    curShape.UpdateEndPoint();
                }
            }
            if(code == ISDRAGELEMENT)
            {
                curShape.Drag(curPoint);
            }
            if(code == ISRESIZE)
            {
                curShape.ResizeShape(curPoint);
            }
        }
        private void clearRectangale()
        {
            code = NORMAL;
            CopyToClipboardToggleButton.IsChecked = false;
            curShape.stroke = new SolidStroke(Brushes.White, 2, null);

            curShape.UpdateStartAndEndPoint();
            curShape.Remove();
            updateHistory();
            // curShape.Remove();
            isDraw = false;
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
                if (code == ISSELECTELEMENT)
                {
                    Caretaker.add(new Memento(history));
                    curShape.stroke.fillColor = fillColorMain;
                    curShape.UpdateStartAndEndPoint();
                    

                }

            }

        }

        private void BorderColor_Click(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = (RadioButton)sender;
            if (radioButton.IsChecked == true)
            {
                borderColorMain = (SolidColorBrush)radioButton.Tag;
                if (code == ISSELECTELEMENT)
                {
                    Caretaker.add(new Memento(history));
                    curShape.stroke.borderColor = borderColorMain.Clone();
                    curShape.UpdateStartAndEndPoint();
                    
                }
            }
        }
    }


}