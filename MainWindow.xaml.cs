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
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Drawing.Imaging;

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

        Bitmap screenPixel = new Bitmap(1, 1, PixelFormat.Format32bppArgb);
        public Color GetColorAt(System.Drawing.Point location)
        {
            using (Graphics gdest = Graphics.FromImage(screenPixel))
            {
                using (Graphics gsrc = Graphics.FromHwnd(IntPtr.Zero))
                {
                    IntPtr hSrcDC = gsrc.GetHdc();
                    IntPtr hDC = gdest.GetHdc();
                    int retval = BitBlt(hDC, 0, 0, 1, 1, hSrcDC, location.X, location.Y, (int)CopyPixelOperation.SourceCopy);
                    gdest.ReleaseHdc();
                    gsrc.ReleaseHdc();
                }
            }

            return screenPixel.GetPixel(0, 0);
        }

        public MainWindow()
        {
            InitializeComponent();

            // READ SETTINGS
            _settings = new ControllerSettings();

            TouchInjector.InitializeTouchInjection();

            _inputSimulator = new InputSimulator();

            _gameController = new GameController(new Controller(UserIndex.One), _settings, _inputSimulator);

            if (_gameController.IsConnected())
            {
                double msPerSecond = 1000;
                var msPerFrameRefresh = msPerSecond / _settings.refresh_rate;
                var timer = Observable.Interval(TimeSpan.FromMilliseconds(msPerFrameRefresh));

                timer.Subscribe(_ => {
                    if (_gameController.IsConnected())
                    {
                        _gameController.Poll();
                        _settings.UpdateScreenValues();
                        Application.Current.Dispatcher.Invoke(new Action(() => testImage.Margin = new Thickness(_settings.d3_Rect.Left, _settings.d3_Rect.Top,0,0)));
                    }

                    /* Nominal values at 1080p
                    // x: 1280
                    // y: 968
                    
                    System.Drawing.Point cursor = new System.Drawing.Point(1280, 968);
                    //GetCursorPos(ref cursor);
                    Color c = GetColorAt(cursor);

                    if (c.R == 160 && c.G == 169 && c.B == 173)
                        Application.Current.Dispatcher.Invoke(new Action(() => Grid1.Visibility = Visibility.Visible)); 
                    else
                        Application.Current.Dispatcher.Invoke(new Action(() => Grid1.Visibility = Visibility.Hidden)); 

                    //MessageBox.Show("x:" + cursor.X + ",y:" + cursor.Y + "\n RGB:" + c.R + " " + c.G + " " + c.B); */
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