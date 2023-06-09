using System.ComponentModel;

namespace s3s_viewer
{
    /// <summary>
    /// 待機フォームクラス
    /// </summary>
    public partial class WaitForm : Form
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public WaitForm(DoWorkEventHandler dwWork)
        {
            InitializeComponent();

            // イベント設定
            bwWorker.DoWork += dwWork;
        }

        /// <summary>
        /// バックグラウンド処理終了時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bwWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// 画面表示時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WaitForm_Shown(object sender, EventArgs e)
        {
            bwWorker.RunWorkerAsync();
        }
    }
}
