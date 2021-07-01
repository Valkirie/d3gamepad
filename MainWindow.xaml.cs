using System;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using WindowsInput;
using Microsoft.Win32;
using SharpDX.XInput;
using TCD.System.TouchInjection;
using System.Windows.Media;
using System.Windows.Ink;
using System.Windows.Shapes;
using System.Windows.Media.Effects;
using System.Diagnostics;
using System.Windows.Media.Animation;
using System.Threading;
using System.Text;
using System.Windows.Input;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Windows.Interop;

namespace d3gamepad
{
    public partial class MainWindow : Window
    {
        private static GameController _gameController;
        private static InputSimulator _inputSimulator;
        private static ControllerSettings _settings;
        
        [DllImport("User32.Dll")]
        public static extern long SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);

        public static Bitmap CaptureFromScreen(System.Drawing.Point location, int width, int height)
        {
            Bitmap bmpScreenCapture = new Bitmap(width, height);
            Graphics p = Graphics.FromImage(bmpScreenCapture);
            p.CopyFromScreen(location.X,location.Y, 0, 0, new System.Drawing.Size(width, height), CopyPixelOperation.SourceCopy);
            p.Dispose();

            return bmpScreenCapture;
        }

        public static System.Drawing.Color GetColorFromScreen(System.Drawing.Point location)
        {
            Bitmap map = CaptureFromScreen(location,1,1);
            System.Drawing.Color c = map.GetPixel(0, 0);
            map.Dispose();

            return c;
        }

        static double default_w_ratio = 2560;
        static double default_h_ratio = 1080;
        static double default_padding = 11;
        static double default_cell = 56;
        static double default_bottom = 22;
        static double default_checkui = 17;
        static double default_check_inventory_w = 17;
        static double default_check_inventory_h = 26;

        static double default_life_check_high = 920; // from top
        static double default_life_check_diff = 142; // from check_high
        static double default_life_check_left = 420; // from middle

        static double current_h_ratio, current_w_ratio;

        static double compute_h_ratio, compute_w_ratio;

        static double current_padding,current_cell,current_bottom,current_checkui;
        static double current_check_inventory_w, current_check_inventory_h;
        static double current_life_check_diff, current_life_check_high, current_life_check_left;
        static double draw_pos_x, draw_pos_y;

        private static void ComputeScreenValues()
        {
            current_h_ratio = _settings.UIHeight;
            current_w_ratio = _settings.UIWidth;

            compute_h_ratio = current_h_ratio / default_h_ratio;
            compute_w_ratio = current_w_ratio / default_w_ratio;

            current_padding = default_padding * compute_h_ratio;

            current_cell = default_cell * compute_h_ratio;
            current_bottom = default_bottom * compute_h_ratio;
            current_checkui = default_checkui * compute_h_ratio;

            current_check_inventory_w = default_check_inventory_w * compute_h_ratio;
            current_check_inventory_h = default_check_inventory_h * compute_h_ratio;

            current_life_check_diff = default_life_check_diff * compute_h_ratio;
            current_life_check_high = default_life_check_high * compute_h_ratio;
            current_life_check_left = default_life_check_left * compute_h_ratio;
        }

        private static string GetActiveWindowTitle()
        {
            const int nChars = 256;
            StringBuilder Buff = new StringBuilder(nChars);
            IntPtr handle = GetForegroundWindow();

            if (GetWindowText(handle, Buff, nChars) > 0)
            {
                return Buff.ToString();
            }
            return null;
        }

        private static bool IsInGame()
        {
            return GetActiveWindowTitle() == "Diablo III";
        }

        private static bool IsInMap()
        {
            double cursor_x = _settings.d3_Rect.Left + _settings.c_d3Width - (34 * compute_h_ratio);
            double cursor_y = _settings.d3_Rect.Bottom - current_checkui;

            System.Drawing.Point cursor = new System.Drawing.Point((int)cursor_x, (int)cursor_y);
            System.Drawing.Color c = GetColorFromScreen(cursor);

            if (c.G > c.R && c.G > c.B && c.R <= 50 && c.G >= 100 && c.B <= 50)
                return true;

            return false;
        }

        private bool IsInInventory()
        {
            double cursor_x = _settings.d3_Rect.Right - 2 - current_check_inventory_w;
            double cursor_y = _settings.d3_Rect.Top + current_check_inventory_h;

            System.Drawing.Point cursor = new System.Drawing.Point((int)cursor_x, (int)cursor_y);
            System.Drawing.Color c = GetColorFromScreen(cursor);

            if (c.R > c.G && c.G > c.B && c.R >= 60 && c.B <= 20)
                return true;

            return false;
        }

        private static int CheckHealth()
        {
            double cursor_x = _settings.d3_Rect.Left + _settings.c_d3Width;
            cursor_x -= current_life_check_left;

            double health_max_y = _settings.d3_Rect.Top + current_life_check_high;
            double health_min_y = health_max_y + current_life_check_diff;

            double life_percent = 100;
            double life_cursor = current_life_check_diff;
            double table_cursor = 0;
            double R, G, B = 255;

            Bitmap map = CaptureFromScreen(new System.Drawing.Point((int)cursor_x, (int)health_max_y), 1, (int)current_life_check_diff);
            for (double y = health_max_y; y < health_min_y; y++)
            {
                life_cursor--;
                System.Drawing.Color c = map.GetPixel(0, (int)table_cursor);

                R = c.R > 0 ? c.R : 1;
                G = c.G > 0 ? c.G : 1;
                B = c.B > 0 ? c.B : 1;

                if (R / G > 2 && R / B > 2 && R > 50 && R < 140)
                    break;
                else
                    life_percent = life_cursor / current_life_check_diff * 100;

                if(table_cursor + 1 < map.Height)
                    table_cursor++;
            }
            map.Dispose();

            return (int)life_percent;
        }

        public static void ThreadHealth()
        {
            int health_known = 100;
            while (Thread.CurrentThread.IsAlive)
            {
                if (_gameController.IsConnected())
                {
                    if (IsInGame() && IsInMap())
                    {
                        int health_current = CheckHealth();
                        int damage = health_known - health_current;
                        int vibra = Math.Min(Math.Abs(damage) * 5000, 60000);

                        if (damage >= _settings.rumble_min_dmg && health_current != 0)
                            _gameController.SendVibration(vibra, vibra);

                        health_known = health_current;
                    }
                }

                Thread.Sleep(_settings.refresh_rate);
            }
        }

        public static void ThreadGamepad()
        {
            while (Thread.CurrentThread.IsAlive)
            {
                if (_gameController.IsConnected())
                    _gameController.Poll();

                Thread.Sleep(_settings.refresh_rate);
            }
        }

        public static void ThreadUI()
        {
            while (Thread.CurrentThread.IsAlive)
            {
                _settings.UpdateScreenValues();

                if (_settings.hasChanged)
                {
                    ComputeScreenValues();
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        myForm.Height = _settings.d3Height;
                        myForm.Width = _settings.d3Width;
                        myForm.Top = _settings.d3_Rect.Top;
                        myForm.Left = _settings.d3_Rect.Left;

                        myGamepad.Width = (int)current_cell;
                        myGamepad.Height = (int)current_cell;
                        Canvas.SetTop(myGamepad, 40 * compute_h_ratio); // temp
                        Canvas.SetLeft(myGamepad, _settings.d3Width - (90 * compute_h_ratio)); // temp

                        draw_pos_x = _settings.c_d3Width - (current_padding / 2) - current_cell;
                        draw_pos_y = _settings.d3Height - current_cell - current_bottom;

                        for (int i = 0; i < 4; i++)
                            draw_pos_x -= (current_cell + current_padding);

                        foreach(UIElement item in myCanvas.Children)
                        {
                            if (item is System.Windows.Shapes.Rectangle)
                            {
                                ((System.Windows.Shapes.Rectangle)item).Width = (int)current_cell / 2;
                                ((System.Windows.Shapes.Rectangle)item).Height = (int)current_cell / 2;

                                Canvas.SetTop(item, draw_pos_y);
                                Canvas.SetLeft(item, draw_pos_x);

                                draw_pos_x += current_cell;
                                draw_pos_x += current_padding;
                            }
                        }

                        draw_pos_x += current_cell / 2;
                    }));
                }

                Application.Current.Dispatcher.BeginInvoke((Action)(() => {
                    if (IsInGame())
                    {
                        myCanvas.Visibility = Visibility.Visible;

                        if (_gameController.IsConnected())
                            myGamepad.Visibility = Visibility.Visible;
                        else
                            myGamepad.Visibility = Visibility.Hidden;

                        if (IsInMap())
                        {
                            foreach (UIElement item in myCanvas.Children)
                                if (item is System.Windows.Shapes.Rectangle)
                                    item.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            foreach (UIElement item in myCanvas.Children)
                                if (item is System.Windows.Shapes.Rectangle)
                                    item.Visibility = Visibility.Hidden;
                        }
                    }
                    else
                        myCanvas.Visibility = Visibility.Hidden;
                }));

                Thread.Sleep(100);
            }
        }

        public static Bitmap GetImageByName(string imageName)
        {
            System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();
            string resourceName = asm.GetName().Name + ".Properties.Resources";
            var rm = new System.Resources.ResourceManager(resourceName, asm);
            return (Bitmap)rm.GetObject(imageName);
        }

        public ImageSource ImageSourceFromBitmap(Bitmap bmp)
        {
            var handle = bmp.GetHbitmap();
            try
            {
                return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally { DeleteObject(handle); }
        }

        static d3gamepad.MainWindow myForm;
        static Canvas myCanvas;
        static System.Windows.Controls.Image myGamepad;
        public MainWindow()
        {
            InitializeComponent();

            // TOUCH SETTINGS
            TouchInjector.InitializeTouchInjection();
            _inputSimulator = new InputSimulator();

            // CONTROLLER SETTINGS
            _settings = new ControllerSettings();
            _gameController = new GameController(new Controller(UserIndex.One), _settings, _inputSimulator);

            // STATICS
            myForm = Form1;
            myCanvas = Canvas1;
            myGamepad = GamepadIco;

            string ico = "Menu";
            for (int i = 0; i < 7; i ++)
            {
                switch (i)
                {
                    case 0: ico = "B"; break;
                    case 1: ico = "Y"; break;
                    case 2: ico = "RB"; break;
                    case 3: ico = "RT"; break;
                    case 4: ico = "X"; break;
                    case 5: ico = "A"; break;
                    case 6: ico = "LB"; break;
                }

                System.Windows.Shapes.Rectangle rect = new System.Windows.Shapes.Rectangle
                {
                    Fill = new ImageBrush
                    {
                        ImageSource = ImageSourceFromBitmap(GetImageByName("XBOne_" + ico))
                    },
                    Stretch = Stretch.Uniform,
                };

                myCanvas.Children.Add(rect);
            }

            Thread myThread = new Thread(new ThreadStart(ThreadHealth));
            myThread.Start();

            Thread myThread2 = new Thread(new ThreadStart(ThreadGamepad));
            myThread2.Start();

            Thread myThread3 = new Thread(new ThreadStart(ThreadUI));
            myThread3.Start();
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT
        {
            public MouseKeybdhardwareInputUnion mkhi;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct MouseKeybdhardwareInputUnion
        {
            [FieldOffset(0)] public MouseInputData mi;

            [FieldOffset(0)] public readonly KEYBDINPUT ki;

            [FieldOffset(0)] public readonly HARDWAREINPUT hi;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KEYBDINPUT
        {
            public readonly ushort wVk;
            public readonly ushort wScan;
            public readonly uint dwFlags;
            public readonly uint time;
            public readonly IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct HARDWAREINPUT
        {
            public readonly int uMsg;
            public readonly short wParamL;
            public readonly short wParamH;
        }

        private struct MouseInputData
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint time;
            public IntPtr dwExtraInfo;
        }
    }
}