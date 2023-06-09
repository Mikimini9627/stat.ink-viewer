namespace s3s_viewer
{
    /// <summary>
    /// ウォーターマークを表示するテキストボックス
    /// </summary>
    public partial class WatermarkTextbox : TextBox
    {
        /// <summary>ウォーターマーク内部値</summary>
        private string _watermarkText = string.Empty;

        /// <summary>ウォーターマーク</summary>
        public string WatermarkText
        {
            get
            {
                return _watermarkText;
            }
            set
            {
                _watermarkText = value;
                Invalidate();
            }
        }

        /// <summary>
        /// ウォーターマーク設定ありの場合に描画します
        /// </summary>
        /// <param name="m"></param>
        protected override void WndProc(ref Message m)
        {
            const int WM_PAINT = 0x000F;
            const int WM_LBUTTONDOWN = 0x0201;
            base.WndProc(ref m);

            if ((m.Msg == WM_PAINT || m.Msg == WM_LBUTTONDOWN) &&
                Enabled &&
                string.IsNullOrEmpty(Text) &&
                !string.IsNullOrEmpty(WatermarkText)
                )
            {
                using (Graphics g = Graphics.FromHwnd(Handle))
                {
                    Rectangle rect = ClientRectangle;
                    rect.Offset(1, 1);
                    TextRenderer.DrawText(g,
                                          WatermarkText,
                                          Font,
                                          rect,
                                          Color.LightGray,
                                          TextFormatFlags.Top | TextFormatFlags.Left);

                }
            }
        }
    }
}
