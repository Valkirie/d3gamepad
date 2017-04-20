using Microsoft.Win32;
using SharpDX.XInput;
using System;
using System.ComponentModel;
using System.Configuration;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using TCD.System.TouchInjection;
using WindowsInput;

namespace d3gamepad
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll", SetLastError = true)]
        static extern uint SendInput(uint nInputs, ref INPUT pInputs, int cbSize);

        [StructLayout(LayoutKind.Sequential)]
        struct INPUT
        {
            public SendInputEventType type;
            public MouseKeybdhardwareInputUnion mkhi;
        }
        [StructLayout(LayoutKind.Explicit)]
        struct MouseKeybdhardwareInputUnion
        {
            [FieldOffset(0)]
            public MouseInputData mi;

            [FieldOffset(0)]
            public KEYBDINPUT ki;

            [FieldOffset(0)]
            public HARDWAREINPUT hi;
        }
        [StructLayout(LayoutKind.Sequential)]
        struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }
        [StructLayout(LayoutKind.Sequential)]
        struct HARDWAREINPUT
        {
            public int uMsg;
            public short wParamL;
            public short wParamH;
        }
        struct MouseInputData
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public MouseEventFlags dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }
        [Flags]
        enum MouseEventFlags : uint
        {
            MOUSEEVENTF_MOVE = 0x0001,
            MOUSEEVENTF_LEFTDOWN = 0x0002,
            MOUSEEVENTF_LEFTUP = 0x0004,
            MOUSEEVENTF_RIGHTDOWN = 0x0008,
            MOUSEEVENTF_RIGHTUP = 0x0010,
            MOUSEEVENTF_MIDDLEDOWN = 0x0020,
            MOUSEEVENTF_MIDDLEUP = 0x0040,
            MOUSEEVENTF_XDOWN = 0x0080,
            MOUSEEVENTF_XUP = 0x0100,
            MOUSEEVENTF_WHEEL = 0x0800,
            MOUSEEVENTF_VIRTUALDESK = 0x4000,
            MOUSEEVENTF_ABSOLUTE = 0x8000
        }
        enum SendInputEventType : int
        {
            InputMouse,
            InputKeyboard,
            InputHardware
        }

        private PointerTouchInfo MakePointerTouchInfo(PointerInputType pointer, PointerFlags click, int x, int y, int radius, uint id, string type, uint orientation = 90, uint pressure = 32000)
        {
            PointerTouchInfo contact = new PointerTouchInfo();
            contact.PointerInfo.pointerType = pointer;

            contact.TouchFlags = TouchFlags.NONE;
            contact.Orientation = orientation;
            contact.Pressure = pressure;

            if (type == "Start")
                contact.PointerInfo.PointerFlags = PointerFlags.DOWN | PointerFlags.INRANGE | PointerFlags.INCONTACT;
            else if (type == "Move")
                contact.PointerInfo.PointerFlags = PointerFlags.UPDATE | PointerFlags.INRANGE | PointerFlags.INCONTACT;
            else if (type == "End")
                contact.PointerInfo.PointerFlags = PointerFlags.UP;
            else if (type == "EndToHover")
                contact.PointerInfo.PointerFlags = PointerFlags.UP | PointerFlags.INRANGE;
            else if (type == "Hover")
                contact.PointerInfo.PointerFlags = PointerFlags.UPDATE | PointerFlags.INRANGE;
            else if (type == "EndFromHover")
                contact.PointerInfo.PointerFlags = PointerFlags.UPDATE;

            contact.PointerInfo.PointerFlags |= click;

            contact.TouchMasks = TouchMask.CONTACTAREA | TouchMask.ORIENTATION | TouchMask.PRESSURE;
            contact.PointerInfo.PtPixelLocation.X = x;
            contact.PointerInfo.PtPixelLocation.Y = y;
            contact.PointerInfo.PointerId = id;
            contact.ContactArea.left = x - radius;
            contact.ContactArea.right = x + radius;
            contact.ContactArea.top = y - radius;
            contact.ContactArea.bottom = y + radius;
            return contact;
        }

        // THREAD VARIABLES
        bool force_move = false;
        bool force_stop = false;

        int force_ratio;
        int force_ratio_inc;

        bool stick1 = false;
        bool stick2 = false;

        // BUTTONS & TRIGGERS
        private Controller _controller;
        bool trigger1 = false;
        bool skill1 = false;
        bool skill2 = false;
        bool skill3 = false;
        bool skill4 = false;
        bool skill5 = false;
        bool skill6 = false;
        bool potion = false;
        bool map = false;
        bool start = false;
        bool character = false;
        bool skill = false;
        bool town_portal = false;
        bool show_item = false;
        bool spacebar = false;
        bool ctrl = false;

        // SETTINGS
        bool moveattack = false;
        bool vibration = false;

        int x_value;
        int y_value;
        int cursor_x;
        int cursor_y;

        PointerTouchInfo[] contacts = new PointerTouchInfo[1];
        bool touch = false;

        Vibration vb_null;
        Vibration vb_stick;
        Vibration vb_button;
        Vibration vb_heavy;

        // SCREEN VARIABLES
        int screenWidth;
        int screenWidth_round;
        int screenHeight;
        int c_screenWidth;
        int c_screenHeight;
        int character_ratio;
        int stick2_ratio;

        // XBOX VARIABLES
        int max;
        int max_stick;
        int deadzone;
        int vb_stick_value;
        float dpi_fix;

        // STATES VARIABLES
        State state;
        int old_state;

        // TEMP VARIABLES
        int gamepad_x;
        int gamepad_y;
        int gamepad_x2;
        int gamepad_y2;

        int returned_x;
        int returned_y;
        int returned_x2;
        int returned_y2;
        int x_ratio;
        int y_ratio;
        int stick_speed;
        int stick_speed2;

        Gamepad gamepadState;

        public void SetCursorPosition(int x, int y)
        {
            INPUT mouseInput = new INPUT();
            mouseInput.type = SendInputEventType.InputMouse;
            mouseInput.mkhi.mi.dwFlags = MouseEventFlags.MOUSEEVENTF_ABSOLUTE | MouseEventFlags.MOUSEEVENTF_MOVE;
            mouseInput.mkhi.mi.dx = x * 65535 / screenWidth;
            mouseInput.mkhi.mi.dy = y * 65535 / screenHeight;
            SendInput(1, ref mouseInput, Marshal.SizeOf(new INPUT()));
        }

        public void MouseClick(bool Left, bool Down)
        {
            INPUT mouseInput = new INPUT();
            mouseInput.type = SendInputEventType.InputMouse;
            if (Left)
            {
                if (Down)
                    mouseInput.mkhi.mi.dwFlags = MouseEventFlags.MOUSEEVENTF_LEFTDOWN;
                else
                    mouseInput.mkhi.mi.dwFlags = MouseEventFlags.MOUSEEVENTF_LEFTUP;
            }
            else
            {
                if (Down)
                    mouseInput.mkhi.mi.dwFlags = MouseEventFlags.MOUSEEVENTF_RIGHTDOWN;
                else
                    mouseInput.mkhi.mi.dwFlags = MouseEventFlags.MOUSEEVENTF_RIGHTUP;
            }
            SendInput(1, ref mouseInput, Marshal.SizeOf(new INPUT()));
        }

        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            state = _controller.GetState();
            gamepadState = state.Gamepad;

            if (gamepadState.GetHashCode() != old_state || stick2 || gamepadState.LeftTrigger > 0)
            {
                old_state = gamepadState.GetHashCode();

                Vector leftStick = Normalize(gamepadState.LeftThumbX, gamepadState.LeftThumbY, Gamepad.LeftThumbDeadZone);
                Vector rightStick = Normalize(gamepadState.RightThumbX, gamepadState.RightThumbY, Gamepad.RightThumbDeadZone);

                // ---------------------------------------------------------------------------------------------
                // BUTTONS HANDLING
                // ---------------------------------------------------------------------------------------------
                if (gamepadState.LeftTrigger > 0)
                {
                    if (!trigger1)
                    {
                        if(!moveattack)
                            ForceMove(false);
                        ForceStop(true);
                        force_ratio_inc = 3;
                        trigger1 = true;
                        SendVibration(vb_stick);
                    }
                }
                else
                {
                    if (trigger1)
                    {
                        ForceStop(false);
                        force_ratio_inc = 0;
                        trigger1 = false;
                        SendVibration(vb_null);
                    }
                }

                // ---------------------------------------------------------------------------------------------
                // SKILL 1
                // ---------------------------------------------------------------------------------------------
                if ((gamepadState.Buttons & GamepadButtonFlagsX) != 0)
                {
                    if (!skill1)
                    {
                        MouseClick(true, true);
                        skill1 = true;
                    }
                }
                else
                {
                    if (skill1)
                    {
                        MouseClick(true, false);
                        skill1 = false;
                    }
                }

                // ---------------------------------------------------------------------------------------------
                // SKILL 2
                // ---------------------------------------------------------------------------------------------
                if ((gamepadState.Buttons & GamepadButtonFlagsA) != 0)
                {
                    if (!skill2)
                    {
                        MouseClick(false, true);
                        skill2 = true;
                    }
                }
                else
                {
                    if (skill2)
                    {
                        MouseClick(false, false);
                        skill2 = false;
                    }
                }

                // ---------------------------------------------------------------------------------------------
                // SKILL 3
                // ---------------------------------------------------------------------------------------------
                if ((gamepadState.Buttons & GamepadButtonFlagsB) != 0)
                {
                    if (!skill3)
                    {
                        InputSimulator.SimulateKeyDown(VirtualKeyCode.VK_1);
                        skill3 = true;
                    }
                }
                else
                {
                    if (skill3)
                    {
                        InputSimulator.SimulateKeyUp(VirtualKeyCode.VK_1);
                        skill3 = false;
                    }
                }

                // ---------------------------------------------------------------------------------------------
                // SKILL 4
                // ---------------------------------------------------------------------------------------------
                if ((gamepadState.Buttons & GamepadButtonFlagsY) != 0)
                {
                    if (!skill4)
                    {
                        InputSimulator.SimulateKeyDown(VirtualKeyCode.VK_2);
                        skill4 = true;
                    }
                }
                else
                {
                    if (skill4)
                    {
                        InputSimulator.SimulateKeyUp(VirtualKeyCode.VK_2);
                        skill4 = false;
                    }
                }

                // ---------------------------------------------------------------------------------------------
                // SKILL 5
                // ---------------------------------------------------------------------------------------------
                if ((gamepadState.Buttons & GamepadButtonFlagsRightShoulder) != 0)
                {
                    if (!skill5)
                    {
                        InputSimulator.SimulateKeyDown(VirtualKeyCode.VK_3);
                        skill5 = true;
                    }
                }
                else
                {
                    if (skill5)
                    {
                        InputSimulator.SimulateKeyUp(VirtualKeyCode.VK_3);
                        skill5 = false;
                    }
                }

                // ---------------------------------------------------------------------------------------------
                // SKILL 6
                // ---------------------------------------------------------------------------------------------
                if (gamepadState.RightTrigger > 0)
                {
                    //MessageBox.Show("RightTrigger");
                    if (!skill6)
                    {
                        InputSimulator.SimulateKeyDown(VirtualKeyCode.VK_4);
                        skill6 = true;
                    }
                }
                else
                {
                    if (skill6)
                    {
                        InputSimulator.SimulateKeyUp(VirtualKeyCode.VK_4);
                        skill6 = false;
                    }
                }

                // ---------------------------------------------------------------------------------------------
                // POTION
                // ---------------------------------------------------------------------------------------------
                if ((gamepadState.Buttons & GamepadButtonFlagsLeftShoulder) != 0)
                {
                    if (!potion)
                    {
                        InputSimulator.SimulateKeyDown(VirtualKeyCode.VK_Q);
                        potion = true;
                    }
                }
                else
                {
                    if (potion)
                    {
                        InputSimulator.SimulateKeyUp(VirtualKeyCode.VK_Q);
                        potion = false;
                    }
                }

                // ---------------------------------------------------------------------------------------------
                // START
                // ---------------------------------------------------------------------------------------------
                if ((gamepadState.Buttons & GamepadButtonFlagsStart) != 0)
                {
                    if (!start)
                    {
                        InputSimulator.SimulateKeyDown(VirtualKeyCode.ESCAPE);
                        start = true;
                    }
                }
                else
                {
                    if (start)
                    {
                        InputSimulator.SimulateKeyUp(VirtualKeyCode.ESCAPE);
                        start = false;
                    }
                }

                // ---------------------------------------------------------------------------------------------
                // CHARACTER
                // ---------------------------------------------------------------------------------------------
                if ((gamepadState.Buttons & GamepadButtonFlagsBack) != 0)
                {
                    if (!character)
                    {
                        InputSimulator.SimulateKeyDown(VirtualKeyCode.VK_I);
                        character = true;
                    }
                }
                else
                {
                    if (character)
                    {
                        InputSimulator.SimulateKeyUp(VirtualKeyCode.VK_I);
                        character = false;
                    }
                }

                // ---------------------------------------------------------------------------------------------
                // DPAD SETTINGS
                // ---------------------------------------------------------------------------------------------

                // ---------------------------------------------------------------------------------------------
                // SHOW ITEMS
                // ---------------------------------------------------------------------------------------------
                if ((gamepadState.Buttons & GamepadButtonFlagsDPadUp) != 0)
                {
                    if (!show_item)
                    {
                        InputSimulator.SimulateKeyDown(VirtualKeyCode.LMENU);
                        show_item = true;
                    }
                }
                else
                {
                    if (show_item)
                    {
                        InputSimulator.SimulateKeyUp(VirtualKeyCode.LMENU);
                        show_item = false;
                    }
                }

                // ---------------------------------------------------------------------------------------------
                // TOWN PORTAL
                // ---------------------------------------------------------------------------------------------
                if ((gamepadState.Buttons & GamepadButtonFlagsDPadRight) != 0)
                {
                    if (!town_portal)
                    {
                        InputSimulator.SimulateKeyDown(VirtualKeyCode.VK_T);
                        town_portal = true;
                    }
                }
                else
                {
                    if (town_portal)
                    {
                        InputSimulator.SimulateKeyUp(VirtualKeyCode.VK_T);
                        town_portal = false;
                    }
                }

                // ---------------------------------------------------------------------------------------------
                // SKILL
                // ---------------------------------------------------------------------------------------------
                if ((gamepadState.Buttons & GamepadButtonFlagsDPadLeft) != 0)
                {
                    if (!skill)
                    {
                        InputSimulator.SimulateKeyDown(VirtualKeyCode.VK_S);
                        skill = true;
                    }
                }
                else
                {
                    if (skill)
                    {
                        InputSimulator.SimulateKeyUp(VirtualKeyCode.VK_S);
                        skill = false;
                    }
                }

                // ---------------------------------------------------------------------------------------------
                // MAP
                // ---------------------------------------------------------------------------------------------
                if ((gamepadState.Buttons & GamepadButtonFlagsDPadDown) != 0)
                {
                    if (!map)
                    {
                        InputSimulator.SimulateKeyDown(VirtualKeyCode.TAB);
                        map = true;
                    }
                }
                else
                {
                    if (map)
                    {
                        InputSimulator.SimulateKeyUp(VirtualKeyCode.TAB);
                        map = false;
                    }
                }

                // ---------------------------------------------------------------------------------------------
                // SPACEBAR
                // ---------------------------------------------------------------------------------------------
                if ((gamepadState.Buttons & GamepadButtonFlagsLeftThumb) != 0)
                {
                    if (!spacebar)
                    {
                        InputSimulator.SimulateKeyDown(VirtualKeyCode.SPACE);
                        spacebar = true;
                    }
                }
                else
                {
                    if (spacebar)
                    {
                        InputSimulator.SimulateKeyUp(VirtualKeyCode.SPACE);
                        spacebar = false;
                    }
                }

                // ---------------------------------------------------------------------------------------------
                // CTRL
                // ---------------------------------------------------------------------------------------------
                if ((gamepadState.Buttons & GamepadButtonFlagsRightThumb) != 0)
                {
                    if (!ctrl)
                    {
                        InputSimulator.SimulateKeyDown(VirtualKeyCode.LCONTROL);
                        ctrl = true;
                    }
                }
                else
                {
                    if (ctrl)
                    {
                        InputSimulator.SimulateKeyUp(VirtualKeyCode.LCONTROL);
                        ctrl = false;
                    }
                }

                // ---------------------------------------------------------------------------------------------
                // JOYSTICKS HANDLING
                // ---------------------------------------------------------------------------------------------

                // ---------------------------------------------------------------------------------------------
                // AIM & ATTACK
                // ---------------------------------------------------------------------------------------------
                if (leftStick.X != 0 || leftStick.Y != 0 && !stick2)
                {
                    if (!stick1)
                        stick1 = true;

                    cursor_x = 0; // Reset cursor for stick 2
                    cursor_y = 0; // Reset cursor for stick 2

                    gamepad_x = Convert.ToInt16(leftStick.X * stick_speed);
                    gamepad_y = Convert.ToInt16(leftStick.Y * stick_speed);
                    x_ratio = (gamepad_x * screenWidth_round) / max;
                    y_ratio = (gamepad_y * screenHeight) / max;

                    returned_x = x_ratio / (force_ratio - force_ratio_inc);
                    returned_y = -y_ratio / (force_ratio - force_ratio_inc);

                    x_value = c_screenWidth + returned_x;
                    y_value = c_screenHeight + returned_y;

                    if (!force_stop)
                    {
                        ForceMove(true);
                        if (!touch)
                        {
                            contacts[0] = MakePointerTouchInfo(PointerInputType.TOUCH, PointerFlags.NONE, x_value, y_value, 1, 1, "Start");
                            touch = true;
                        }
                        else
                            contacts[0] = MakePointerTouchInfo(PointerInputType.TOUCH, PointerFlags.NONE, x_value, y_value, 1, 1, "Hover");

                        TouchInjector.InjectTouchInput(1, contacts);
                    }
                    else
                    {
                        if (touch)
                        {
                            contacts[0] = MakePointerTouchInfo(PointerInputType.TOUCH, PointerFlags.NONE, x_value, y_value, 1, 1, "End");
                            TouchInjector.InjectTouchInput(1, contacts);
                            touch = false;
                        }
                        SetCursorPosition(x_value, y_value);
                    }
                }
                else
                {
                    if (stick1)
                    {
                        if (touch)
                        {
                            contacts[0] = MakePointerTouchInfo(PointerInputType.TOUCH, PointerFlags.NONE, x_value, y_value, 1, 1, "End");
                            TouchInjector.InjectTouchInput(1, contacts);
                            touch = false;
                        }
                        ForceMove(false);
                        stick1 = false;
                    }
                }

                // ---------------------------------------------------------------------------------------------
                // CURSOR
                // ---------------------------------------------------------------------------------------------
                if (rightStick.X != 0 || rightStick.Y != 0 && !stick1)
                {
                    if (!stick2)
                        stick2 = true;

                    gamepad_x2 = Convert.ToInt16(rightStick.X * stick_speed2);
                    gamepad_y2 = Convert.ToInt16(rightStick.Y * stick_speed2);
                    returned_x2 = gamepad_x2;
                    returned_y2 = -gamepad_y2;

                    if ((c_screenWidth + cursor_x + returned_x2) >= 0 && (c_screenWidth + cursor_x + returned_x2) <= screenWidth)
                        cursor_x += returned_x2;
                    if ((c_screenHeight + cursor_y + returned_y2) >= 0 && (c_screenHeight + cursor_y + returned_y2) <= screenHeight)
                        cursor_y += returned_y2;

                    x_value = c_screenWidth + cursor_x;
                    y_value = c_screenHeight + cursor_y;

                    if (!stick1)
                        SetCursorPosition(x_value, y_value);
                }
                else
                    if (stick2)
                        stick2 = false;
            }
        }

        public void ForceMove(bool f)
        {
            if (f)
            {
                force_move = true;
                InputSimulator.SimulateKeyDown(VirtualKeyCode.VK_E);
            }
            else
            {
                force_move = false;
                InputSimulator.SimulateKeyUp(VirtualKeyCode.VK_E);
            }
        }

        public void ForceStop(bool f)
        {
            if (f)
            {
                force_stop = true;
                InputSimulator.SimulateKeyDown(VirtualKeyCode.SHIFT);
            }
            else
            {
                force_stop = false;
                InputSimulator.SimulateKeyUp(VirtualKeyCode.SHIFT);
            }
        }

        static Vector Normalize(short rawX, short rawY, short threshold)
        {
            Vector value = new Vector(rawX, rawY);
            double magnitude = value.Length;
            Vector direction = value / (magnitude == 0 ? 1 : magnitude);

            double normalizedMagnitude = 0.0;
            if (magnitude - threshold > 0)
                normalizedMagnitude = Math.Min((magnitude - threshold) / (short.MaxValue - threshold), 1);

            return direction * normalizedMagnitude;
        }

        public GamepadButtonFlags ReadKeyString(string key)
        {
            switch(key)
            {
                case "A":
                    return GamepadButtonFlags.A;
                case "B":
                    return GamepadButtonFlags.B;
                case "Back":
                    return GamepadButtonFlags.Back;
                case "DPadDown":
                    return GamepadButtonFlags.DPadDown;
                case "DPadLeft":
                    return GamepadButtonFlags.DPadLeft;
                case "DPadRight":
                    return GamepadButtonFlags.DPadRight;
                case "DPadUp":
                    return GamepadButtonFlags.DPadUp;
                case "LeftShoulder":
                    return GamepadButtonFlags.LeftShoulder;
                case "LeftThumb":
                    return GamepadButtonFlags.LeftThumb;
                case "RightShoulder":
                    return GamepadButtonFlags.RightShoulder;
                case "RightThumb":
                    return GamepadButtonFlags.RightThumb;
                case "Start":
                    return GamepadButtonFlags.Start;
                case "X":
                    return GamepadButtonFlags.X;
                case "Y":
                    return GamepadButtonFlags.Y;
                default :
                    return GamepadButtonFlags.None;
            }
        }

        // DECLARE VARS
        GamepadButtonFlags GamepadButtonFlagsX;
        GamepadButtonFlags GamepadButtonFlagsA;
        GamepadButtonFlags GamepadButtonFlagsB;
        GamepadButtonFlags GamepadButtonFlagsY;
        GamepadButtonFlags GamepadButtonFlagsRightShoulder;
        GamepadButtonFlags GamepadButtonFlagsLeftShoulder;
        GamepadButtonFlags GamepadButtonFlagsStart;
        GamepadButtonFlags GamepadButtonFlagsBack;
        GamepadButtonFlags GamepadButtonFlagsDPadUp;
        GamepadButtonFlags GamepadButtonFlagsDPadRight;
        GamepadButtonFlags GamepadButtonFlagsDPadLeft;
        GamepadButtonFlags GamepadButtonFlagsDPadDown;
        GamepadButtonFlags GamepadButtonFlagsLeftThumb;
        GamepadButtonFlags GamepadButtonFlagsRightThumb;

        public MainWindow()
        {
            InitializeComponent();

            // READ SETTINGS
            if (!string.IsNullOrEmpty(ConfigurationSettings.AppSettings["force_ratio"]))
                force_ratio = Convert.ToInt16(ConfigurationSettings.AppSettings["force_ratio"]);
            if (!string.IsNullOrEmpty(ConfigurationSettings.AppSettings["stick2_ratio"]))
                stick2_ratio = Convert.ToInt16(ConfigurationSettings.AppSettings["stick2_ratio"]);
            if (!string.IsNullOrEmpty(ConfigurationSettings.AppSettings["character_ratio"]))
                character_ratio = Convert.ToInt16(ConfigurationSettings.AppSettings["character_ratio"]);
            if (!string.IsNullOrEmpty(ConfigurationSettings.AppSettings["max"]))
                max = Convert.ToInt32(ConfigurationSettings.AppSettings["max"]);
            if (!string.IsNullOrEmpty(ConfigurationSettings.AppSettings["max_stick"]))
                max_stick = Convert.ToInt16(ConfigurationSettings.AppSettings["max_stick"]);
            if (!string.IsNullOrEmpty(ConfigurationSettings.AppSettings["deadzone"]))
                deadzone = Convert.ToInt16(ConfigurationSettings.AppSettings["deadzone"]);
            if (!string.IsNullOrEmpty(ConfigurationSettings.AppSettings["stick_speed"]))
                stick_speed = Convert.ToInt16(ConfigurationSettings.AppSettings["stick_speed"]);
            if (!string.IsNullOrEmpty(ConfigurationSettings.AppSettings["moveattack"]))
                moveattack = Convert.ToBoolean(ConfigurationSettings.AppSettings["moveattack"]);
            if (!string.IsNullOrEmpty(ConfigurationSettings.AppSettings["vibration"]))
                vibration = Convert.ToBoolean(ConfigurationSettings.AppSettings["vibration"]);
            if (!string.IsNullOrEmpty(ConfigurationSettings.AppSettings["vb_stick"]))
                vb_stick_value = Convert.ToInt16(ConfigurationSettings.AppSettings["vb_stick"]);
            if (!string.IsNullOrEmpty(ConfigurationSettings.AppSettings["dpi_fix"]))
                dpi_fix = (float)Convert.ToDouble(ConfigurationSettings.AppSettings["dpi_fix"]);

            if (!string.IsNullOrEmpty(ConfigurationSettings.AppSettings["GamepadButtonFlagsX"]))
                GamepadButtonFlagsX = ReadKeyString(ConfigurationSettings.AppSettings["GamepadButtonFlagsX"]);
            if (!string.IsNullOrEmpty(ConfigurationSettings.AppSettings["GamepadButtonFlagsA"]))
                GamepadButtonFlagsA = ReadKeyString(ConfigurationSettings.AppSettings["GamepadButtonFlagsA"]);
            if (!string.IsNullOrEmpty(ConfigurationSettings.AppSettings["GamepadButtonFlagsB"]))
                GamepadButtonFlagsB = ReadKeyString(ConfigurationSettings.AppSettings["GamepadButtonFlagsB"]);
            if (!string.IsNullOrEmpty(ConfigurationSettings.AppSettings["GamepadButtonFlagsY"]))
                GamepadButtonFlagsY = ReadKeyString(ConfigurationSettings.AppSettings["GamepadButtonFlagsY"]);
            if (!string.IsNullOrEmpty(ConfigurationSettings.AppSettings["GamepadButtonFlagsRightShoulder"]))
                GamepadButtonFlagsRightShoulder = ReadKeyString(ConfigurationSettings.AppSettings["GamepadButtonFlagsRightShoulder"]);
            if (!string.IsNullOrEmpty(ConfigurationSettings.AppSettings["GamepadButtonFlagsLeftShoulder"]))
                GamepadButtonFlagsLeftShoulder = ReadKeyString(ConfigurationSettings.AppSettings["GamepadButtonFlagsLeftShoulder"]);
            if (!string.IsNullOrEmpty(ConfigurationSettings.AppSettings["GamepadButtonFlagsStart"]))
                GamepadButtonFlagsStart = ReadKeyString(ConfigurationSettings.AppSettings["GamepadButtonFlagsStart"]);
            if (!string.IsNullOrEmpty(ConfigurationSettings.AppSettings["GamepadButtonFlagsBack"]))
                GamepadButtonFlagsBack = ReadKeyString(ConfigurationSettings.AppSettings["GamepadButtonFlagsBack"]);
            if (!string.IsNullOrEmpty(ConfigurationSettings.AppSettings["GamepadButtonFlagsDPadUp"]))
                GamepadButtonFlagsDPadUp = ReadKeyString(ConfigurationSettings.AppSettings["GamepadButtonFlagsDPadUp"]);
            if (!string.IsNullOrEmpty(ConfigurationSettings.AppSettings["GamepadButtonFlagsDPadRight"]))
                GamepadButtonFlagsDPadRight = ReadKeyString(ConfigurationSettings.AppSettings["GamepadButtonFlagsDPadRight"]);
            if (!string.IsNullOrEmpty(ConfigurationSettings.AppSettings["GamepadButtonFlagsDPadLeft"]))
                GamepadButtonFlagsDPadLeft = ReadKeyString(ConfigurationSettings.AppSettings["GamepadButtonFlagsDPadLeft"]);
            if (!string.IsNullOrEmpty(ConfigurationSettings.AppSettings["GamepadButtonFlagsDPadDown"]))
                GamepadButtonFlagsDPadDown = ReadKeyString(ConfigurationSettings.AppSettings["GamepadButtonFlagsDPadDown"]);
            if (!string.IsNullOrEmpty(ConfigurationSettings.AppSettings["GamepadButtonFlagsLeftThumb"]))
                GamepadButtonFlagsLeftThumb = ReadKeyString(ConfigurationSettings.AppSettings["GamepadButtonFlagsLeftThumb"]);
            if (!string.IsNullOrEmpty(ConfigurationSettings.AppSettings["GamepadButtonFlagsRightThumb"]))
                GamepadButtonFlagsRightThumb = ReadKeyString(ConfigurationSettings.AppSettings["GamepadButtonFlagsRightThumb"]);

            UpdateScreen();

            TouchInjector.InitializeTouchInjection();

            _controller = new Controller(UserIndex.One);
            if (_controller.IsConnected)
                CompositionTarget.Rendering += CompositionTarget_Rendering;
            else
            {
                MessageBox.Show("No controller detected, closing...");
                this.Close();
            }
            
            SystemEvents.DisplaySettingsChanged += new EventHandler(SystemEvents_DisplaySettingsChanged);   

            // VIBRATION SETTINGS
            vb_stick.RightMotorSpeed = 0;
            vb_stick.LeftMotorSpeed = Convert.ToUInt16(vb_stick_value);
        }

        private void SendVibration(Vibration vb)
        {
            if(vibration)
                _controller.SetVibration(vb);
        }

        private void UpdateScreen()
        {
            // DPI SETTINGS
            Graphics graphics = Graphics.FromHwnd(IntPtr.Zero);
            float dpiX = (graphics.DpiX + dpi_fix) / 100;
            float dpiY = (graphics.DpiY + dpi_fix) / 100;

            // SCREEN SETTINGS
            screenWidth = Convert.ToInt16(System.Windows.SystemParameters.PrimaryScreenWidth * dpiX);
            screenHeight = Convert.ToInt16(System.Windows.SystemParameters.PrimaryScreenHeight * dpiY);

            screenWidth_round = screenHeight; // CIRCLE SCREEN
            c_screenWidth = screenWidth / 2;
            c_screenHeight = (screenHeight - (screenHeight / character_ratio)) / 2;
            stick_speed2 = c_screenWidth / stick2_ratio;
        }

        private void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
        {
            UpdateScreen();
        }
    }
}