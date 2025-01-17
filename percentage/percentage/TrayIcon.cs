﻿using System;
using System.Drawing;
using System.Drawing.Text;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using percentage.Properties;

namespace percentage
{
    class TrayIcon
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern bool DestroyIcon(IntPtr handle);

        private const int iconFontSize = 12;

        private string batteryPercentage;
        private NotifyIcon notifyIcon;

        private PrivateFontCollection pfc = new PrivateFontCollection();

        public TrayIcon()
        {
            LoadEmbeddedFont();

            ContextMenu contextMenu = new ContextMenu();

            notifyIcon = new NotifyIcon();

            // initialize contextMenu
            contextMenu.MenuItems.AddRange(new[]
            {
                new MenuItem("&AutoStart", autoStartMenuItem_Click)
                {
                    Checked = SystemConfig.CheckIsAutoStart()
                },
                new MenuItem("E&xit", exitMenuItem_Click),
            });

            notifyIcon.ContextMenu = contextMenu;

            batteryPercentage = "?";

            notifyIcon.Visible = true;

            Timer timer = new Timer();
            timer.Tick += timer_Tick;
            timer.Interval = 1000; // in milliseconds
            timer.Start();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            PowerStatus powerStatus = SystemInformation.PowerStatus;
            batteryPercentage = (powerStatus.BatteryLifePercent * 100).ToString(CultureInfo.InvariantCulture);

            using (Bitmap bitmap = new Bitmap(DrawText(batteryPercentage, new Font(pfc.Families[0], iconFontSize), Color.White, Color.Transparent)))
            {
                IntPtr intPtr = bitmap.GetHicon();
                try
                {
                    using (Icon icon = Icon.FromHandle(intPtr))
                    {
                        notifyIcon.Icon = icon;
                        notifyIcon.Text = batteryPercentage + @"%";
                    }
                }
                finally
                {
                    DestroyIcon(intPtr);
                }
            }
        }

        private void exitMenuItem_Click(object sender, EventArgs e)
        {
            notifyIcon.Visible = false;
            notifyIcon.Dispose();
            Application.Exit();
        }

        private void autoStartMenuItem_Click(object sender, EventArgs e)
        {
            var menu = (MenuItem)sender;
            menu.Checked = !menu.Checked;
            SystemConfig.SetAutoStart(menu.Checked);
        }

        private Image DrawText(string text, Font font, Color textColor, Color backColor)
        {
            var textSize = GetImageSize(text, font);
            Image image = new Bitmap((int)textSize.Width, (int)textSize.Height);
            using (Graphics graphics = Graphics.FromImage(image))
            {
                // paint the background
                graphics.Clear(backColor);

                // create a brush for the text
                using (Brush textBrush = new SolidBrush(textColor))
                {
                    graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                    graphics.DrawString(text, font, textBrush, 0, 0);
                    graphics.Save();
                }
            }

            return image;
        }

        private void LoadEmbeddedFont()
        {
            var fontBytes = Resources.PANNETJE;
            var fontData = Marshal.AllocCoTaskMem(fontBytes.Length);
            Marshal.Copy(fontBytes, 0, fontData, fontBytes.Length);
            pfc.AddMemoryFont(fontData, fontBytes.Length);
            Marshal.FreeCoTaskMem(fontData);
        }

        private static SizeF GetImageSize(string text, Font font)
        {
            using (Image image = new Bitmap(1, 1))
            using (Graphics graphics = Graphics.FromImage(image))
                return graphics.MeasureString(text, font);
        }
    }
}
