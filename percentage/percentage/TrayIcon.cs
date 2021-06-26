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
        private const string font = "Comic Sans";

        private NotifyIcon notifyIcon;

        public TrayIcon()
        {
            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = new MenuItem();

            notifyIcon = new NotifyIcon();

            contextMenu.MenuItems.AddRange(new MenuItem[] { menuItem });

            menuItem.Click += new System.EventHandler(MenuItemClick);
            menuItem.Index = 0;
            menuItem.Text = "E&xit";

            notifyIcon.ContextMenu = contextMenu;
            notifyIcon.Visible = true;


            TimerTick(null, null);

            Timer timer = new Timer();
            timer.Interval = 60*1000;
            timer.Tick += new EventHandler(TimerTick);
            timer.Start();
        }

        private static SizeF GetStringImageSize(string text, Font font)
        {
            using (Image image = new Bitmap(1, 1))
            using (Graphics graphics = Graphics.FromImage(image))
                return graphics.MeasureString(text, font);
        }

        private void MenuItemClick(object sender, EventArgs e)
        {
            notifyIcon.Visible = false;
            notifyIcon.Dispose();
            Application.Exit();
        }
        private void TimerTick(object sender, EventArgs e)
        {
            PowerStatus powerStatus = SystemInformation.PowerStatus;
            String percentage = ((int)(powerStatus.BatteryLifePercent * 100)).ToString();
            bool isCharging = SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Online;
            // The "+" part will only be visible when percentage is single digit
            String bitmapText = isCharging ? percentage + "+" : percentage;

            // Redone this part to make the font more readable
            Font fontToUse = new Font(font, fontSize, FontStyle.Regular, GraphicsUnit.Pixel);
            Brush brushToUse = new SolidBrush(Color.White);
            Bitmap bitmap = new Bitmap(16, 16);
            Graphics g = System.Drawing.Graphics.FromImage(bitmap);

            g.Clear(Color.Transparent);
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
            g.DrawString(bitmapText, fontToUse, brushToUse, -4, -2);
            IntPtr hIcon = bitmap.GetHicon();

            notifyIcon.Icon = Icon.FromHandle(hIcon);

            DestroyIcon(hIcon);

            String toolTipText = percentage + "%" + (isCharging ? " Charging" : "");
            notifyIcon.Text = toolTipText;
        }
    }
}
