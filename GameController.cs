using System;
using System.Windows;
using WindowsInput;
using WindowsInput.Native;
using SharpDX.XInput;
using TCD.System.TouchInjection;

namespace d3gamepad
{
    public class GameController
    {
        private InputSimulator _inputSimulator;
        private Controller _controller;
        private readonly ControllerSettings _settings;


        // THREAD VARIABLES
        private bool force_move = false;
        private bool force_stop = false;

        private int force_ratio_inc;

        private bool stick1 = false;
        private bool stick2 = false;

        // BUTTONS & TRIGGERS
        public bool trigger1 = false;
        public bool skill1 = false;
        public bool skill2 = false;
        public bool skill3 = false;
        public bool skill4 = false;
        public bool skill5 = false;
        public bool skill6 = false;
        public bool potion = false;
        public bool map = false;
        public bool start = false;
        public bool character = false;
        public bool skill = false;
        public bool town_portal = false;
        public bool show_item = false;
        public bool spacebar = false;
        public bool ctrl = false;

        // SETTINGS
        private bool moveattack = false;
        private bool vibration = false;

        private float x_value;
        private float y_value;
        private int cursor_x;
        private int cursor_y;

        private PointerTouchInfo[] contacts = new PointerTouchInfo[1];
        private bool touch = false;
        private Vibration vb_null;
        private Vibration vb_stick;
        private Vibration vb_button;
        private Vibration vb_heavy;

        // STATES VARIABLES
        private State state;
        private int old_state;
        private Gamepad gamepadState;

        // VKC
        private VirtualKeyCode VKC_ForceMove;
        private VirtualKeyCode VKC_ForceStop;
        private VirtualKeyCode VKC_Skill3;
        private VirtualKeyCode VKC_Skill4;
        private VirtualKeyCode VKC_Skill5;
        private VirtualKeyCode VKC_Skill6;
        private VirtualKeyCode VKC_Potion;
        private VirtualKeyCode VKC_Character;
        private VirtualKeyCode VKC_TownPortal;
        private VirtualKeyCode VKC_Skill;

        private VirtualKeyCode VKC_Start;
        private VirtualKeyCode VKC_Inventory;
        private VirtualKeyCode VKC_MAP;

        private VirtualKeyCode VKC_SKIP;
        private VirtualKeyCode VKC_HINTS;

        public VirtualKeyCode StringToVKC(string str)
        {
            switch (str)
            {
                case "VK_E":
                    return VirtualKeyCode.VK_E;
                case "VK_1":
                    return VirtualKeyCode.VK_1;
                case "VK_2":
                    return VirtualKeyCode.VK_2;
                case "VK_3":
                    return VirtualKeyCode.VK_3;
                case "VK_4":
                    return VirtualKeyCode.VK_4;
                case "VK_Q":
                    return VirtualKeyCode.VK_Q;
                case "VK_I":
                    return VirtualKeyCode.VK_I;
                case "VK_T":
                    return VirtualKeyCode.VK_T;
                case "VK_S":
                    return VirtualKeyCode.VK_S;
                case "ESCAPE":
                    return VirtualKeyCode.ESCAPE;
                case "LMENU":
                    return VirtualKeyCode.LMENU;
                case "TAB":
                    return VirtualKeyCode.TAB;
                case "LCONTROL":
                    return VirtualKeyCode.LCONTROL;
                case "SPACE":
                    return VirtualKeyCode.SPACE;
                case "SHIFT":
                    return VirtualKeyCode.SHIFT;
                default:
                    return VirtualKeyCode.VK_0;
            }
        }

        public GameController(Controller controller, ControllerSettings settings, InputSimulator simulator)
        {
            _controller = controller;
            _settings = settings;
            _inputSimulator = simulator;

            // VIBRATION SETTINGS
            vb_stick.RightMotorSpeed = 0;
            vb_stick.LeftMotorSpeed = Convert.ToUInt16(_settings.vb_stick_value);

            // DEFINE VKCs
            VKC_ForceMove = StringToVKC(_settings.VKC_ForceMove);
            VKC_ForceStop = StringToVKC(_settings.VKC_ForceStop);
            VKC_Skill3 = StringToVKC(_settings.VKC_Skill3);
            VKC_Skill4 = StringToVKC(_settings.VKC_Skill4);
            VKC_Skill5 = StringToVKC(_settings.VKC_Skill5);
            VKC_Skill6 = StringToVKC(_settings.VKC_Skill6);
            VKC_Potion = StringToVKC(_settings.VKC_Potion);
            VKC_Character = StringToVKC(_settings.VKC_Character);
            VKC_TownPortal = StringToVKC(_settings.VKC_TownPortal);
            VKC_Skill = StringToVKC(_settings.VKC_Skill);

            VKC_Start = StringToVKC(_settings.VKC_Start);
            VKC_Inventory = StringToVKC(_settings.VKC_Inventory);
            VKC_MAP = StringToVKC(_settings.VKC_MAP);

            VKC_SKIP = StringToVKC(_settings.VKC_SKIP);
            VKC_HINTS = StringToVKC(_settings.VKC_HINTS);
        }

        private PointerTouchInfo MakePointerTouchInfo(PointerInputType pointer, PointerFlags click, int x, int y,
            int radius, uint id, string type, uint orientation = 90, uint pressure = 32000)
        {
            var contact = new PointerTouchInfo
            {
                PointerInfo = { pointerType = pointer },
                TouchFlags = TouchFlags.NONE,
                Orientation = orientation,
                Pressure = pressure
            };

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


        public void Poll()
        {
            state = _controller.GetState();
            gamepadState = state.Gamepad;

            if (_settings.UpdateScreenValues())
            {
                cursor_x = _settings.d3_Rect.Left + _settings.c_d3Width; // Reset cursor for stick 2
                cursor_y = _settings.d3_Rect.Top + _settings.c_d3Height; // Reset cursor for stick 2
            }

            if (gamepadState.GetHashCode() != old_state || stick2 || gamepadState.LeftTrigger > 0)
            {
                old_state = gamepadState.GetHashCode();

                var leftStick = Normalize(gamepadState.LeftThumbX, gamepadState.LeftThumbY, Gamepad.LeftThumbDeadZone);
                var rightStick = Normalize(gamepadState.RightThumbX, gamepadState.RightThumbY, Gamepad.RightThumbDeadZone);

                // ---------------------------------------------------------------------------------------------
                // BUTTONS HANDLING
                // ---------------------------------------------------------------------------------------------
                if (gamepadState.LeftTrigger > 0)
                {
                    if (!trigger1)
                    {
                        if (!moveattack)
                            ForceMove(false);
                        ForceStop(true);
                        force_ratio_inc = 2;
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
                if ((gamepadState.Buttons & _settings.GamepadButtonFlagsX) != 0)
                {
                    if (!skill1)
                    {
                        _inputSimulator.Mouse.LeftButtonDown();
                        skill1 = true;
                    }
                }
                else
                {
                    if (skill1)
                    {
                        _inputSimulator.Mouse.LeftButtonUp();
                        skill1 = false;
                    }
                }

                // ---------------------------------------------------------------------------------------------
                // SKILL 2
                // ---------------------------------------------------------------------------------------------
                if ((gamepadState.Buttons & _settings.GamepadButtonFlagsA) != 0)
                {
                    if (!skill2)
                    {
                        _inputSimulator.Mouse.RightButtonDown();
                        skill2 = true;
                    }
                }
                else
                {
                    if (skill2)
                    {
                        _inputSimulator.Mouse.RightButtonUp();
                        skill2 = false;
                    }
                }

                // ---------------------------------------------------------------------------------------------
                // SKILL 3
                // ---------------------------------------------------------------------------------------------
                if ((gamepadState.Buttons & _settings.GamepadButtonFlagsB) != 0)
                {
                    if (!skill3)
                    {
                        _inputSimulator.Keyboard.KeyDown(VKC_Skill3);
                        skill3 = true;
                    }
                }
                else
                {
                    if (skill3)
                    {
                        _inputSimulator.Keyboard.KeyUp(VKC_Skill3);
                        skill3 = false;
                    }
                }

                // ---------------------------------------------------------------------------------------------
                // SKILL 4
                // ---------------------------------------------------------------------------------------------
                if ((gamepadState.Buttons & _settings.GamepadButtonFlagsY) != 0)
                {
                    if (!skill4)
                    {
                        _inputSimulator.Keyboard.KeyDown(VKC_Skill4);
                        skill4 = true;
                    }
                }
                else
                {
                    if (skill4)
                    {
                        _inputSimulator.Keyboard.KeyUp(VKC_Skill4);
                        skill4 = false;
                    }
                }

                // ---------------------------------------------------------------------------------------------
                // SKILL 5
                // ---------------------------------------------------------------------------------------------
                if ((gamepadState.Buttons & _settings.GamepadButtonFlagsRightShoulder) != 0)
                {
                    if (!skill5)
                    {
                        _inputSimulator.Keyboard.KeyDown(VKC_Skill5);
                        skill5 = true;
                    }
                }
                else
                {
                    if (skill5)
                    {
                        _inputSimulator.Keyboard.KeyUp(VKC_Skill5);
                        skill5 = false;
                    }
                }

                // ---------------------------------------------------------------------------------------------
                // SKILL 6
                // ---------------------------------------------------------------------------------------------
                if (gamepadState.RightTrigger > 0)
                {
                    if (!skill6)
                    {
                        _inputSimulator.Keyboard.KeyDown(VKC_Skill6);
                        skill6 = true;
                    }
                }
                else
                {
                    if (skill6)
                    {
                        _inputSimulator.Keyboard.KeyUp(VKC_Skill6);
                        skill6 = false;
                    }
                }

                // ---------------------------------------------------------------------------------------------
                // POTION
                // ---------------------------------------------------------------------------------------------
                if ((gamepadState.Buttons & _settings.GamepadButtonFlagsLeftShoulder) != 0)
                {
                    if (!potion)
                    {
                        _inputSimulator.Keyboard.KeyDown(VKC_Potion);
                        potion = true;
                    }
                }
                else
                {
                    if (potion)
                    {
                        _inputSimulator.Keyboard.KeyUp(VKC_Potion);
                        potion = false;
                    }
                }

                // ---------------------------------------------------------------------------------------------
                // START
                // ---------------------------------------------------------------------------------------------
                if ((gamepadState.Buttons & _settings.GamepadButtonFlagsStart) != 0)
                {
                    if (!start)
                    {
                        _inputSimulator.Keyboard.KeyDown(VKC_Start);
                        start = true;
                    }
                }
                else
                {
                    if (start)
                    {
                        _inputSimulator.Keyboard.KeyUp(VKC_Start);
                        start = false;
                    }
                }

                // ---------------------------------------------------------------------------------------------
                // CHARACTER
                // ---------------------------------------------------------------------------------------------
                if ((gamepadState.Buttons & _settings.GamepadButtonFlagsBack) != 0)
                {
                    if (!character)
                    {
                        _inputSimulator.Keyboard.KeyDown(VKC_Character);
                        character = true;
                    }
                }
                else
                {
                    if (character)
                    {
                        _inputSimulator.Keyboard.KeyUp(VKC_Character);
                        character = false;
                    }
                }

                // ---------------------------------------------------------------------------------------------
                // DPAD SETTINGS
                // ---------------------------------------------------------------------------------------------

                // ---------------------------------------------------------------------------------------------
                // SHOW ITEMS
                // ---------------------------------------------------------------------------------------------
                if ((gamepadState.Buttons & _settings.GamepadButtonFlagsDPadUp) != 0)
                {
                    if (!show_item)
                    {
                        _inputSimulator.Keyboard.KeyDown(VKC_Inventory);
                        show_item = true;
                    }
                }
                else
                {
                    if (show_item)
                    {
                        _inputSimulator.Keyboard.KeyUp(VKC_Inventory);
                        show_item = false;
                    }
                }

                // ---------------------------------------------------------------------------------------------
                // TOWN PORTAL
                // ---------------------------------------------------------------------------------------------
                if ((gamepadState.Buttons & _settings.GamepadButtonFlagsDPadRight) != 0)
                {
                    if (!town_portal)
                    {
                        _inputSimulator.Keyboard.KeyDown(VKC_TownPortal);
                        town_portal = true;
                    }
                }
                else
                {
                    if (town_portal)
                    {
                        _inputSimulator.Keyboard.KeyUp(VKC_TownPortal);
                        town_portal = false;
                    }
                }

                // ---------------------------------------------------------------------------------------------
                // SKILL
                // ---------------------------------------------------------------------------------------------
                if ((gamepadState.Buttons & _settings.GamepadButtonFlagsDPadLeft) != 0)
                {
                    if (!skill)
                    {
                        _inputSimulator.Keyboard.KeyDown(VKC_Skill);
                        skill = true;
                    }
                }
                else
                {
                    if (skill)
                    {
                        _inputSimulator.Keyboard.KeyUp(VKC_Skill);
                        skill = false;
                    }
                }

                // ---------------------------------------------------------------------------------------------
                // MAP
                // ---------------------------------------------------------------------------------------------
                if ((gamepadState.Buttons & _settings.GamepadButtonFlagsDPadDown) != 0)
                {
                    if (!map)
                    {
                        _inputSimulator.Keyboard.KeyDown(VKC_MAP);
                        map = true;
                    }
                }
                else
                {
                    if (map)
                    {
                        _inputSimulator.Keyboard.KeyUp(VKC_MAP);
                        map = false;
                    }
                }

                // ---------------------------------------------------------------------------------------------
                // SPACEBAR
                // ---------------------------------------------------------------------------------------------
                if ((gamepadState.Buttons & _settings.GamepadButtonFlagsLeftThumb) != 0)
                {
                    if (!spacebar)
                    {
                        _inputSimulator.Keyboard.KeyDown(VKC_SKIP);
                        spacebar = true;
                    }
                }
                else
                {
                    if (spacebar)
                    {
                        _inputSimulator.Keyboard.KeyUp(VKC_SKIP);
                        spacebar = false;
                    }
                }

                // ---------------------------------------------------------------------------------------------
                // CTRL
                // ---------------------------------------------------------------------------------------------
                if ((gamepadState.Buttons & _settings.GamepadButtonFlagsRightThumb) != 0)
                {
                    if (!ctrl)
                    {
                        _inputSimulator.Keyboard.KeyDown(VKC_HINTS);
                        ctrl = true;
                    }
                }
                else
                {
                    if (ctrl)
                    {
                        _inputSimulator.Keyboard.KeyUp(VKC_HINTS);
                        ctrl = false;
                    }
                }

                // ---------------------------------------------------------------------------------------------
                // JOYSTICKS HANDLING
                // ---------------------------------------------------------------------------------------------

                // ---------------------------------------------------------------------------------------------
                // AIM & ATTACK : Fixed
                // ---------------------------------------------------------------------------------------------
                if (leftStick.X != 0 || leftStick.Y != 0 && !stick2)
                {
                    if (!stick1)
                        stick1 = true;

                    cursor_x = _settings.d3_Rect.Left + _settings.c_d3Width; // Reset cursor for stick 2
                    cursor_y = _settings.d3_Rect.Top + _settings.c_d3Height; // Reset cursor for stick 2

                    float gamepad_x = Convert.ToInt32(leftStick.X * (float)_settings.stick_speed);
                    float gamepad_y = Convert.ToInt32(leftStick.Y * (float)_settings.stick_speed);
                    float x_ratio = gamepad_x * (float)_settings.d3Height / (float)_settings.max;
                    float y_ratio = gamepad_y * (float)_settings.d3Height / (float)_settings.max;

                    float returned_x = x_ratio / ((float)_settings.force_ratio - (float)force_ratio_inc);
                    float returned_y = -y_ratio / ((float)_settings.force_ratio - (float)force_ratio_inc);

                    x_value = _settings.c_d3Width + _settings.d3_Rect.Left + returned_x;
                    y_value = _settings.c_d3Height + _settings.d3_Rect.Top + returned_y;

                    if (!force_stop)
                    {
                        ForceMove(true);
                        if (!touch)
                        {
                            contacts[0] = MakePointerTouchInfo(PointerInputType.TOUCH, PointerFlags.NONE, (int)x_value,
                                (int)y_value, 1, 1, "Start");
                            touch = true;
                        }
                        else
                        {
                            contacts[0] = MakePointerTouchInfo(PointerInputType.TOUCH, PointerFlags.NONE, (int)x_value,
                                (int)y_value, 1, 1, "Hover");
                        }

                        TouchInjector.InjectTouchInput(1, contacts);
                    }
                    else
                    {
                        if (touch)
                        {
                            contacts[0] = MakePointerTouchInfo(PointerInputType.TOUCH, PointerFlags.NONE, (int)x_value,
                                (int)y_value, 1, 1, "End");
                            TouchInjector.InjectTouchInput(1, contacts);
                            touch = false;
                        }
                        MoveMouseTo(x_value, y_value);
                    }
                }
                else
                {
                    if (stick1)
                    {
                        if (touch)
                        {
                            contacts[0] = MakePointerTouchInfo(PointerInputType.TOUCH, PointerFlags.NONE, (int)x_value,
                                (int)y_value, 1, 1, "End");
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

                    var gamepad_x2 = Convert.ToInt16(rightStick.X * _settings.stick_speed2);
                    var gamepad_y2 = Convert.ToInt16(rightStick.Y * _settings.stick_speed2);
                    var returned_x2 = gamepad_x2;
                    var returned_y2 = -gamepad_y2;

                    if(cursor_x + returned_x2 < _settings.d3_Rect.Left + _settings.d3Width)
                        if (cursor_x + returned_x2 > _settings.d3_Rect.Left)
                            cursor_x += returned_x2;

                    if (cursor_y + returned_y2 > _settings.d3_Rect.Top)
                        if (cursor_y + returned_y2 < _settings.d3_Rect.Bottom)
                            cursor_y += returned_y2;

                    if (!stick1)
                        MoveMouseTo(cursor_x, cursor_y);
                }
                else if (stick2)
                {
                    stick2 = false;
                }
            }
        }

        public void ForceMove(bool f)
        {
            if (f)
            {
                force_move = true;
                _inputSimulator.Keyboard.KeyDown(VKC_ForceMove);
            }
            else
            {
                force_move = false;
                _inputSimulator.Keyboard.KeyUp(VKC_ForceMove);
            }
        }

        public void ForceStop(bool f)
        {
            if (f)
            {
                force_stop = true;
                _inputSimulator.Keyboard.KeyDown(VKC_ForceStop);
            }
            else
            {
                force_stop = false;
                _inputSimulator.Keyboard.KeyUp(VKC_ForceStop);
            }
        }

        private static Vector Normalize(short rawX, short rawY, short threshold)
        {
            Vector value = new Vector(rawX, rawY);
            double magnitude = value.Length;
            double vector = (magnitude > 0 ? magnitude : 1);
            var direction = value / vector;

            var normalizedMagnitude = 0.0;
            if (magnitude - threshold > 0)
            {
                normalizedMagnitude = Math.Min((magnitude - threshold) / (short.MaxValue - threshold), 1);
                normalizedMagnitude = normalizedMagnitude < 0.2 ? 0.2 : normalizedMagnitude;

                return direction * normalizedMagnitude;
            }

            return new Vector(0,0);
        }

        private void SendVibration(Vibration vb)
        {
            if (vibration)
                _controller.SetVibration(vb);
        }

        public bool IsConnected()
        {
            return _controller.IsConnected;
        }

        private void MoveMouseTo(double x, double y)
        {
            var normalizedCoordinatesMagicNumber = 65535;
            var realX = x * normalizedCoordinatesMagicNumber / _settings.screenWidth;
            var realY = y * normalizedCoordinatesMagicNumber / _settings.screenHeight;
            _inputSimulator.Mouse.MoveMouseTo(realX, realY);
        }

    }
}