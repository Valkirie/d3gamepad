using System;
using System.Configuration;
using System.Drawing;
using System.Windows;
using SharpDX.XInput;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;

namespace d3gamepad
{
    public class ControllerSettings
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindow(string strClassName, string strWindowName);

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hwnd, ref Rect rectangle);

        public struct Rect
        {
            public int Left { get; set; }
            public int Top { get; set; }
            public int Right { get; set; }
            public int Bottom { get; set; }
        }

        [DllImport("gdi32.dll")]
        static extern int GetDeviceCaps(IntPtr hdc, int nIndex);
        public enum DeviceCap
        {
            VERTRES = 10,
            DESKTOPVERTRES = 117,
        }

        public int character_ratio { get; }
        public int deadzone { get; }
        public int force_ratio { get; }
        public int max { get; }
        public int max_stick { get; }
        public int stick_speed { get; }
        public int stick2_ratio { get; }
        public int vb_stick_value { get; }
        public bool vibration { get; }
        public bool dpi_check { get; }
        public bool moveattack { get; set; }
        public float ScreenScalingFactor { get; set; }

        // VKC
        public string VKC_ForceStop { get; set; }
        public string VKC_ForceMove { get; set; }
        public string VKC_Skill3 { get; set; }
        public string VKC_Skill4 { get; set; }
        public string VKC_Skill5 { get; set; }
        public string VKC_Skill6 { get; set; }
        public string VKC_Potion { get; set; }
        public string VKC_Character { get; set; }
        public string VKC_TownPortal { get; set; }
        public string VKC_Skill { get; set; }

        public string VKC_Start { get; set; }
        public string VKC_Inventory { get; set; }
        public string VKC_MAP { get; set; }

        public string VKC_HINTS { get; set; }
        public string VKC_SKIP { get; set; }

        // SCREEN VARIABLES
        public int d3Width { get; set; }
        public int d3Height { get; set; }
        public int UIWidth { get; set; }
        public int UIHeight { get; set; }
        public int c_d3Width { get; set; }
        public int c_d3Height { get; set; }

        public int screenWidth { get; set; }
        public int screenHeight { get; set; }

        public Rect d3_Rect = new Rect();
        public bool hasChanged = false;
        private string D3Prefs;

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
            // VKC
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["VKC_ForceMove"]))
                VKC_ForceMove = Convert.ToString(ConfigurationManager.AppSettings["VKC_ForceMove"]);
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["VKC_ForceStop"]))
                VKC_ForceStop = Convert.ToString(ConfigurationManager.AppSettings["VKC_ForceStop"]);
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["VKC_Skill3"]))
                VKC_Skill3 = Convert.ToString(ConfigurationManager.AppSettings["VKC_Skill3"]);
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["VKC_Skill4"]))
                VKC_Skill4 = Convert.ToString(ConfigurationManager.AppSettings["VKC_Skill4"]);
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["VKC_Skill5"]))
                VKC_Skill5 = Convert.ToString(ConfigurationManager.AppSettings["VKC_Skill5"]);
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["VKC_Skill6"]))
                VKC_Skill6 = Convert.ToString(ConfigurationManager.AppSettings["VKC_Skill6"]);
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["VKC_Potion"]))
                VKC_Potion = Convert.ToString(ConfigurationManager.AppSettings["VKC_Potion"]);
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["VKC_Character"]))
                VKC_Character = Convert.ToString(ConfigurationManager.AppSettings["VKC_Character"]);
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["VKC_TownPortal"]))
                VKC_TownPortal = Convert.ToString(ConfigurationManager.AppSettings["VKC_TownPortal"]);
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["VKC_Skill"]))
                VKC_Skill = Convert.ToString(ConfigurationManager.AppSettings["VKC_Skill"]);

            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["VKC_Start"]))
                VKC_Start = Convert.ToString(ConfigurationManager.AppSettings["VKC_Start"]);
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["VKC_Inventory"]))
                VKC_Inventory = Convert.ToString(ConfigurationManager.AppSettings["VKC_Inventory"]);
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["VKC_MAP"]))
                VKC_MAP = Convert.ToString(ConfigurationManager.AppSettings["VKC_MAP"]);

            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["VKC_HINTS"]))
                VKC_HINTS = Convert.ToString(ConfigurationManager.AppSettings["VKC_HINTS"]);
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["VKC_SKIP"]))
                VKC_SKIP = Convert.ToString(ConfigurationManager.AppSettings["VKC_SKIP"]);

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
                stick_speed = Convert.ToInt32(ConfigurationManager.AppSettings["stick_speed"]);
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["moveattack"]))
                moveattack = Convert.ToBoolean(ConfigurationManager.AppSettings["moveattack"]);
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["vibration"]))
                vibration = Convert.ToBoolean(ConfigurationManager.AppSettings["vibration"]);
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["vb_stick"]))
                vb_stick_value = Convert.ToInt16(ConfigurationManager.AppSettings["vb_stick"]);
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["dpi_check"]))
                dpi_check = Convert.ToBoolean(ConfigurationManager.AppSettings["dpi_check"]);

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

        private string Between(ref string src, string start, string ended, bool del)
        {
            string ret = "";
            int idxStart = src.IndexOf(start);
            if (idxStart != -1)
            {
                idxStart = idxStart + start.Length;
                int idxEnd = src.IndexOf(ended, idxStart);
                if (idxEnd != -1)
                {
                    ret = src.Substring(idxStart, idxEnd - idxStart);
                    if (del == true)
                        src = src.Replace(start + ret + ended, "");
                }
            }
            return ret;
        }

        public int DisplayModeWindowMode;
        string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Documents", "Diablo III", "D3Prefs.txt");
        public bool UpdateScreenValues()
        {
            // GET WINDOW
            hasChanged = false;

            if (File.Exists(path))
            {
                try
                {
                    using (FileStream stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            string readText = reader.ReadToEnd();
                            if (readText != D3Prefs)
                            {
                                DisplayModeWindowMode = Int32.Parse(Between(ref readText, "DisplayModeWindowMode \"", "\"", false));
                                Graphics g = Graphics.FromHwnd(IntPtr.Zero);
                                IntPtr desktop = g.GetHdc();
                                int LogicalScreenHeight = GetDeviceCaps(desktop, (int)DeviceCap.VERTRES);
                                int PhysicalScreenHeight = GetDeviceCaps(desktop, (int)DeviceCap.DESKTOPVERTRES);
                                ScreenScalingFactor = dpi_check ? (float)PhysicalScreenHeight / (float)LogicalScreenHeight : 1;

                                screenWidth = Convert.ToInt16(SystemParameters.PrimaryScreenWidth * ScreenScalingFactor);
                                screenHeight = Convert.ToInt16(SystemParameters.PrimaryScreenHeight * ScreenScalingFactor);

                                if (DisplayModeWindowMode == 1)
                                {
                                    // Windowed
                                    d3Width = Int32.Parse(Between(ref readText, "DisplayModeWinWidth \"", "\"", false));
                                    d3Height = Int32.Parse(Between(ref readText, "DisplayModeWinHeight \"", "\"", false));

                                    UIWidth = Int32.Parse(Between(ref readText, "DisplayModeWidth \"", "\"", false));
                                    UIHeight = Int32.Parse(Between(ref readText, "DisplayModeHeight \"", "\"", false));

                                    d3_Rect.Left = Int32.Parse(Between(ref readText, "DisplayModeWinLeft \"", "\"", false));
                                    d3_Rect.Top = Int32.Parse(Between(ref readText, "DisplayModeWinTop \"", "\"", false));
                                    d3_Rect.Right = d3_Rect.Left + d3Width;
                                    d3_Rect.Bottom = d3_Rect.Top + d3Height;

                                    // Windows bar
                                    d3_Rect.Top += 30;
                                }
                                else
                                {
                                    // Fullscreen
                                    UIWidth = d3Width = screenWidth;
                                    UIHeight = d3Height = screenHeight;

                                    d3_Rect.Left = 0;
                                    d3_Rect.Top = 0;
                                    d3_Rect.Right = UIWidth;
                                    d3_Rect.Bottom = UIHeight;
                                }

                                Trace.WriteLine("DisplayModeWindowMode:" + DisplayModeWindowMode + "\nPrimaryScreenWidth:" + UIWidth + "\nPrimaryScreenHeight:" + UIHeight);
                                c_d3Width = d3Width / 2;
                                c_d3Height = (d3Height - d3Height / character_ratio) / 2;
                                stick_speed2 = c_d3Width / stick2_ratio;

                                D3Prefs = readText;
                                hasChanged = true;
                            }
                        }
                    }
                }catch(Exception ex)
                {
                    // File is being read/write by D3
                    Trace.WriteLine("File is being read/write by D3");
                }
            }
            return hasChanged;
        }
    }
}