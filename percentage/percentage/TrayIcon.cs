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

        private const int fontSize = 64;
        // private const string font = "Comic Sans";
        private const string font = "Segoe UI";

        private string prevPercentage;
        private bool prevIsCharging;

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
            Font fontToUse = new Font(font, fontSize, FontStyle.Regular, GraphicsUnit.Pixel);
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
