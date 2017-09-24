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

namespace d3gamepad
{
    public partial class MainWindow : Window
    {
        private readonly GameController _gameController;
        private readonly InputSimulator _inputSimulator;
        private ControllerSettings _settings;

        public MainWindow()
        {
            InitializeComponent();

            // READ SETTINGS

            _settings = new ControllerSettings();
            _settings.UpdateScreenValues();

            SetCurrentSettings();

            TouchInjector.InitializeTouchInjection();

            _inputSimulator = new InputSimulator();

            _gameController = new GameController(new Controller(UserIndex.One), _settings, _inputSimulator);


            if (_gameController.IsConnected())
            {
                double msPerSecond = 1000;
                var msPerFrameRefresh = msPerSecond / _settings.refresh_rate;
                var timer = Observable.Interval(TimeSpan.FromMilliseconds(msPerFrameRefresh));

                timer
//                    .DoWhile(_gameController.IsConnected)
                    .Subscribe(_ => {
                        if (_gameController.IsConnected())
                        {
                            _gameController.Poll();
                        }
                    });
            }
            else
            {

                MessageBox.Show("No controller detected, closing...");
                Close();
            }

//            CompositionTarget.Rendering += _gameController.CompositionTarget_Rendering;

            SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;
        }

        private void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
        {
            _settings.UpdateScreenValues();
            DPI.Text = "" + _settings.ScreenScalingFactor;
        }

        private void SetCurrentSettings()
        {
            Left_stick_speed.Text = Convert.ToString(_settings.stick_speed);
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

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            
        }
        
        private void Refresh_rate_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (!Regex.IsMatch(Refresh_rate.Text, "[0-9]"))
            {
                var test = 1;
            }
            else
            {
                var test = 1;
            }
        }

        private void Left_stick_speed_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}