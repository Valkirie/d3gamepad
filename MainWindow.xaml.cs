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
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Media.Effects;
using System.Diagnostics;
using System.Windows.Media.Animation;
using System.Threading;
using System.Text;
using System.Windows.Input;

namespace d3gamepad
{
    public partial class MainWindow : Window
    {
        private readonly GameController _gameController;
        private readonly InputSimulator _inputSimulator;
        private ControllerSettings _settings;

        [DllImport("user32.dll")]
        static extern bool GetCursorPos(ref System.Drawing.Point lpPoint);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern int BitBlt(IntPtr hDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, int dwRop);
        
        [DllImport("User32.Dll")]
        public static extern long SetCursorPos(int x, int y);

        System.Drawing.Bitmap screenPixel = new System.Drawing.Bitmap(1, 1, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        public System.Drawing.Color GetColorAt(System.Drawing.Point location)
        {
            using (System.Drawing.Graphics gdest = System.Drawing.Graphics.FromImage(screenPixel))
            {
                using (System.Drawing.Graphics gsrc = System.Drawing.Graphics.FromHwnd(IntPtr.Zero))
                {
                    IntPtr hSrcDC = gsrc.GetHdc();
                    IntPtr hDC = gdest.GetHdc();
                    int retval = BitBlt(hDC, 0, 0, 1, 1, hSrcDC, location.X, location.Y, (int)System.Drawing.CopyPixelOperation.SourceCopy);
                    gdest.ReleaseHdc();
                    gsrc.ReleaseHdc();
                }
            }

            return screenPixel.GetPixel(0, 0);
        }

        double default_w_ratio = 2560;
        double default_h_ratio = 1080;

        double default_padding = 11;
        double default_cell = 56;
        double default_bottom = 22;
        double default_checkui = 44;

        double default_check_inventory_w = 17;
        double default_check_inventory_h = 26;

        double current_h_ratio;
        double current_w_ratio;

        double compute_h_ratio;
        double compute_w_ratio;

        double current_padding;
        double current_cell;
        double current_bottom;
        double current_checkui;
        double current_check_inventory_w;
        double current_check_inventory_h;
        double draw_pos_x, draw_pos_y;

        private void ComputeScreenValues()
        {
            current_h_ratio = (double)_settings.UIHeight;
            current_w_ratio = (double)_settings.UIWidth;

            compute_h_ratio = current_h_ratio / default_h_ratio;
            compute_w_ratio = current_w_ratio / default_w_ratio;

            current_padding = default_padding * compute_h_ratio;

            current_cell = default_cell * compute_h_ratio;
            current_bottom = default_bottom * compute_h_ratio;
            current_checkui = default_checkui * compute_h_ratio;

            current_check_inventory_w = default_check_inventory_w * compute_h_ratio;
            current_check_inventory_h = default_check_inventory_h * compute_h_ratio;
        }

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        private string GetActiveWindowTitle()
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

        private bool IsInGame()
        {
            return GetActiveWindowTitle() == "Diablo III";
        }

        private bool IsInMap()
        {
            double cursor_x = _settings.d3_Rect.Left + draw_pos_x;
            double cursor_y = _settings.d3_Rect.Bottom - current_checkui;

            System.Drawing.Point cursor = new System.Drawing.Point((int)cursor_x, (int)cursor_y);
            System.Drawing.Color c = GetColorAt(cursor);

            if (c.R > c.B && c.R > c.G && c.R >= 90)
                return true;

            return false;
        }

        private bool IsInInventory()
        {
            double cursor_x = _settings.d3_Rect.Right - 2 - current_check_inventory_w;
            double cursor_y = _settings.d3_Rect.Top + current_check_inventory_h;

            System.Drawing.Point cursor = new System.Drawing.Point((int)cursor_x, (int)cursor_y);
            System.Drawing.Color c = GetColorAt(cursor);

            if (c.R > c.G && c.G > c.B && c.R >= 60 && c.B <= 20)
                return true;

            return false;
        }

        public MainWindow()
        {
            InitializeComponent();

            // TOUCH SETTINGS
            TouchInjector.InitializeTouchInjection();
            _inputSimulator = new InputSimulator();

            // CONTROLLER SETTINGS
            _settings = new ControllerSettings();
            _gameController = new GameController(new Controller(UserIndex.One), _settings, _inputSimulator);

            if (_gameController.IsConnected())
            {
                var timer = Observable.Interval(TimeSpan.FromMilliseconds(_settings.refresh_rate));

                timer.Subscribe(_ =>
                {
                    if (_gameController.IsConnected())
                    {
                        _gameController.Poll();

                        if (_settings.hasChanged)
                        {
                            ComputeScreenValues();

                            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                            {
                                // Clear UI
                                myCanvas.Children.Clear();

                                // Reset UI size
                                Form1.Height = _settings.d3Height;
                                Form1.Width = _settings.d3Width;
                                Form1.Top = _settings.d3_Rect.Top;
                                Form1.Left = _settings.d3_Rect.Left;

                                draw_pos_x = _settings.c_d3Width - (current_padding / 2);
                                draw_pos_y = _settings.d3Height - current_cell - (default_bottom * compute_h_ratio);

                                for (int i = 0; i < 5; i++)
                                    draw_pos_x -= current_cell;

                                for (int i = 0; i < 4; i++)
                                    draw_pos_x -= current_padding;

                                if (_settings.DisplayModeWindowMode == 1)
                                    draw_pos_y -= 8;

                                string ico;
                                for (int i = -9; i <= 3; i += 2)
                                {
                                    switch (i)
                                    {
                                        case -9: ico = "B"; break;
                                        case -7: ico = "Y"; break;
                                        case -5: ico = "RB"; break;
                                        case -3: ico = "RT"; break;
                                        case -1: ico = "X"; break;
                                        case 1: ico = "A"; break;
                                        case 3: ico = "LB"; break;
                                        default: ico = "B"; break;
                                    }

                                    Rectangle rect = new Rectangle
                                    {
                                        Name = "button_" + ico,
                                        Fill = new ImageBrush
                                        {
                                            ImageSource = new System.Windows.Media.Imaging.BitmapImage(new Uri(".\\Ressources\\XBOne_" + ico + ".png", UriKind.Relative))
                                        },
                                        Width = (int)current_cell / 2,
                                        Height = (int)current_cell / 2,
                                        Stretch = Stretch.Uniform,
                                    };

                                    Canvas.SetTop(rect, draw_pos_y);
                                    Canvas.SetLeft(rect, draw_pos_x);
                                    myCanvas.Children.Add(rect);

                                    if (i <= 1)
                                    {
                                        draw_pos_x += current_cell;
                                        draw_pos_x += current_padding;
                                    }
                                }

                                draw_pos_x += current_cell / 2;
                            }));
                        }

                        if (IsInGame() && IsInMap())
                            Application.Current.Dispatcher.BeginInvoke((Action)(() => { myCanvas.Visibility = Visibility.Visible; }));
                        else
                            Application.Current.Dispatcher.BeginInvoke((Action)(() => { myCanvas.Visibility = Visibility.Hidden; }));
                    }
                });
            }
            else
            {
                MessageBox.Show("No controller detected, closing...");
                Close();
            }
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, ref INPUT pInputs, int cbSize);

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