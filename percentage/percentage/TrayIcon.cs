using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace percentage
{
    class TrayIcon
    {
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        static extern bool DestroyIcon(IntPtr handle);

        private const int fontSize = 16;
        // private const string font = "Comic Sans";
        private const string font = "Segoe UI";
        private const float smallFontSise = fontSize * 2 / 3;
        private const string smallFont = "Comic Sans";

        private string prevPercentage;
        private bool prevIsCharging;

        private NotifyIcon notifyIcon;

        public TrayIcon()
        {
            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = new MenuItem();
            MenuItem menuItem2 = new MenuItem();

            notifyIcon = new NotifyIcon();

            contextMenu.MenuItems.AddRange(new MenuItem[] { menuItem, menuItem2 });

            menuItem.Click += new System.EventHandler(MenuItemClick);
            menuItem.Index = 1;
            menuItem.Text = "E&xit";
            menuItem2.Click += new System.EventHandler(MenuItem2Click);
            menuItem2.Index = 0;
            menuItem2.Text = "R&efresh";

            notifyIcon.ContextMenu = contextMenu;
            notifyIcon.Visible = true;

            Timer timer = new Timer();
            timer.Interval = 1000;
            timer.Tick += new EventHandler(TimerTick);
            timer.Start();
        }

        private void MenuItemClick(object sender, EventArgs e)
        {
            notifyIcon.Visible = false;
            notifyIcon.Dispose();
            Application.Exit();
        }
        private void MenuItem2Click(object sender, EventArgs e)
        {
            prevPercentage+=1;
        }
        private void TimerTick(object sender, EventArgs e)
        {
            String percentage = (SystemInformation.PowerStatus.BatteryLifePercent * 100).ToString();
            bool isCharging = SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Online;
            // skip update if the text did not change
            if (prevPercentage == percentage && prevIsCharging == isCharging) {
                return;
            }
            prevPercentage = percentage;
            prevIsCharging = isCharging;
            // Redone this part to make the font more readable
            Font fontToUse;
            if (percentage.Length < 3)
                fontToUse = new Font(font, fontSize, FontStyle.Regular, GraphicsUnit.Pixel);
            else // For 100% state, Comic Sans is easier to display with small amount of pixels
                fontToUse = new Font(smallFont, smallFontSise, FontStyle.Regular, GraphicsUnit.Pixel);
            Brush brushToUse = new SolidBrush(Color.White);
            Bitmap bitmap = new Bitmap(fontSize, fontSize);
            Graphics g = System.Drawing.Graphics.FromImage(bitmap);
            StringFormat format = new StringFormat();
            format.Alignment = StringAlignment.Center;
            format.LineAlignment = StringAlignment.Center;
            // g.Clear(Color.Transparent);
            // g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias; 
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
            // center the text
            g.DrawString(percentage, fontToUse, brushToUse, fontSize/2, fontSize/2, format);

            System.IntPtr intPtr = bitmap.GetHicon();
            try
            {
                using (Icon icon = Icon.FromHandle(intPtr))
                {
                    notifyIcon.Icon = icon;
                    String toolTipText = percentage + "%" + (isCharging ? " Charging" : "");
                    notifyIcon.Text = toolTipText;
                }
            }
            finally
            {
                DestroyIcon(intPtr);
            }
        }
    }
}
