using System;
using System.Configuration;
using System.Drawing;
using System.Windows;
using SharpDX.XInput;

namespace d3gamepad
{
    public class ControllerSettings
    {
        public int character_ratio { get; }
        public int deadzone { get; }
        public float dpi_fix { get; }
        public int force_ratio { get; }
        public int max { get; }
        public int max_stick { get; }
        public int stick_speed { get; }
        public int stick2_ratio { get; }
        public int vb_stick_value { get; }
        public bool vibration { get; }
        public bool moveattack { get; set; }

        // SCREEN VARIABLES
        public int screenWidth { get; set; }
        public int screenWidth_round { get; set; }
        public int screenHeight { get; set; }
        public int c_screenWidth { get; set; }
        public int c_screenHeight { get; set; }
        public double refresh_rate { get; set; }
        public int stick_speed2 { get; set;  }


        public GamepadButtonFlags GamepadButtonFlagsA { get; }
        public GamepadButtonFlags GamepadButtonFlagsB { get; }
        public GamepadButtonFlags GamepadButtonFlagsBack { get; }
        public GamepadButtonFlags GamepadButtonFlagsDPadDown { get; }
        public GamepadButtonFlags GamepadButtonFlagsDPadLeft { get; }
        public GamepadButtonFlags GamepadButtonFlagsDPadRight { get; }
        public GamepadButtonFlags GamepadButtonFlagsDPadUp { get; }
        public GamepadButtonFlags GamepadButtonFlagsLeftShoulder { get; }
        public GamepadButtonFlags GamepadButtonFlagsLeftThumb { get; }
        public GamepadButtonFlags GamepadButtonFlagsRightShoulder { get; }
        public GamepadButtonFlags GamepadButtonFlagsRightThumb { get; }
        public GamepadButtonFlags GamepadButtonFlagsStart { get; }
        public GamepadButtonFlags GamepadButtonFlagsX { get; }
        public GamepadButtonFlags GamepadButtonFlagsY { get; }

        public ControllerSettings()
        {
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["force_ratio"]))
                force_ratio = Convert.ToInt16(ConfigurationManager.AppSettings["force_ratio"]);
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["stick2_ratio"]))
                stick2_ratio = Convert.ToInt16(ConfigurationManager.AppSettings["stick2_ratio"]);
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["character_ratio"]))
                character_ratio = Convert.ToInt16(ConfigurationManager.AppSettings["character_ratio"]);
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["max"]))
                max = Convert.ToInt32(ConfigurationManager.AppSettings["max"]);
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["max_stick"]))
                max_stick = Convert.ToInt16(ConfigurationManager.AppSettings["max_stick"]);
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["deadzone"]))
                deadzone = Convert.ToInt16(ConfigurationManager.AppSettings["deadzone"]);
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["stick_speed"]))
                stick_speed = Convert.ToInt16(ConfigurationManager.AppSettings["stick_speed"]);
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["moveattack"]))
                moveattack = Convert.ToBoolean(ConfigurationManager.AppSettings["moveattack"]);
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["vibration"]))
                vibration = Convert.ToBoolean(ConfigurationManager.AppSettings["vibration"]);
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["vb_stick"]))
                vb_stick_value = Convert.ToInt16(ConfigurationManager.AppSettings["vb_stick"]);
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["dpi_fix"]))
                dpi_fix = (float) Convert.ToDouble(ConfigurationManager.AppSettings["dpi_fix"]);


            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["refresh_rate"]))
                refresh_rate = Convert.ToDouble(ConfigurationManager.AppSettings["refresh_rate"]);

            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["GamepadButtonFlagsX"]))
                GamepadButtonFlagsX = ReadKeyString(ConfigurationManager.AppSettings["GamepadButtonFlagsX"]);
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["GamepadButtonFlagsA"]))
                GamepadButtonFlagsA = ReadKeyString(ConfigurationManager.AppSettings["GamepadButtonFlagsA"]);
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["GamepadButtonFlagsB"]))
                GamepadButtonFlagsB = ReadKeyString(ConfigurationManager.AppSettings["GamepadButtonFlagsB"]);
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["GamepadButtonFlagsY"]))
                GamepadButtonFlagsY = ReadKeyString(ConfigurationManager.AppSettings["GamepadButtonFlagsY"]);
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["GamepadButtonFlagsRightShoulder"]))
                GamepadButtonFlagsRightShoulder = ReadKeyString(ConfigurationManager.AppSettings["GamepadButtonFlagsRightShoulder"]);
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["GamepadButtonFlagsLeftShoulder"]))
                GamepadButtonFlagsLeftShoulder = ReadKeyString(ConfigurationManager.AppSettings["GamepadButtonFlagsLeftShoulder"]);
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["GamepadButtonFlagsStart"]))
                GamepadButtonFlagsStart = ReadKeyString(ConfigurationManager.AppSettings["GamepadButtonFlagsStart"]);
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["GamepadButtonFlagsBack"]))
                GamepadButtonFlagsBack = ReadKeyString(ConfigurationManager.AppSettings["GamepadButtonFlagsBack"]);
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["GamepadButtonFlagsDPadUp"]))
                GamepadButtonFlagsDPadUp = ReadKeyString(ConfigurationManager.AppSettings["GamepadButtonFlagsDPadUp"]);
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["GamepadButtonFlagsDPadRight"]))
                GamepadButtonFlagsDPadRight = ReadKeyString(ConfigurationManager.AppSettings["GamepadButtonFlagsDPadRight"]);
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["GamepadButtonFlagsDPadLeft"]))
                GamepadButtonFlagsDPadLeft = ReadKeyString(ConfigurationManager.AppSettings["GamepadButtonFlagsDPadLeft"]);
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["GamepadButtonFlagsDPadDown"]))
                GamepadButtonFlagsDPadDown = ReadKeyString(ConfigurationManager.AppSettings["GamepadButtonFlagsDPadDown"]);
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["GamepadButtonFlagsLeftThumb"]))
                GamepadButtonFlagsLeftThumb = ReadKeyString(ConfigurationManager.AppSettings["GamepadButtonFlagsLeftThumb"]);
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["GamepadButtonFlagsRightThumb"]))
                GamepadButtonFlagsRightThumb = ReadKeyString(ConfigurationManager.AppSettings["GamepadButtonFlagsRightThumb"]);
        }

        public GamepadButtonFlags ReadKeyString(string key)
        {
            switch (key)
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
                default:
                    return GamepadButtonFlags.None;
            }
        }

        public void UpdateScreenValues()
        {
            // DPI SETTINGS
            var graphics = Graphics.FromHwnd(IntPtr.Zero);
            var dpiX = (graphics.DpiX + dpi_fix) / 100;
            var dpiY = (graphics.DpiY + dpi_fix) / 100;

            // SCREEN SETTINGS
            screenWidth = Convert.ToInt16(SystemParameters.PrimaryScreenWidth * dpiX);
            screenHeight = Convert.ToInt16(SystemParameters.PrimaryScreenHeight * dpiY);

            screenWidth_round = screenHeight; // CIRCLE SCREEN
            c_screenWidth = screenWidth / 2;
            c_screenHeight = (screenHeight -
                              screenHeight / character_ratio) / 2;
            stick_speed2 = c_screenWidth / stick2_ratio;
        }
    }
}