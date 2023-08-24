using System.ComponentModel;
using System.Net;
using System.IO.Compression;
using System.Diagnostics;
using AngleSharp.Html.Parser;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using System.Text.RegularExpressions;
using System.Configuration;
using System.Management;

namespace s3s_viewer
{
    /// <summary>
    /// MainFormクラス
    /// </summary>
    public partial class MainForm : Form
    {
        /// <summary>
        /// ID
        /// </summary>
        private string m_strUserId = string.Empty;

        /// <summary>
        /// ユーザー名
        /// </summary>
        private string m_strPassword = string.Empty;

        /// <summary>
        /// ネットワーククライアント
        /// </summary>
        private readonly HttpClient m_hpClient = new();

        /// <summary>
        /// ログインしているかどうか
        /// </summary>
        private bool m_bLogin = false;

        /// <summary>
        /// ポーリング状況
        /// </summary>
        private bool m_bPolling = false;

        /// <summary>
        /// ポーリング用のプロセス
        /// </summary>
        private Process? m_pPolling = null;

        /// <summary>
        /// ポーリング用のタスク
        /// </summary>
        private Task? m_tPolling = null;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// セットアップボタン押下
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSetup_Click(object sender, EventArgs e)
        {
            // メッセージ表示
            if (CustomMessageBox.Show(this, "s3sのセットアップを実行します。よろしいですか?", "注意", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            {
                return;
            }

            // 更新間隔チェック
            if (int.TryParse(edtFreq.Text, out int nFreq) == false)
            {
                CustomMessageBox.Show(this, "更新頻度の値が不正です。");
                return;
            }

            if (nFreq <= 0)
            {
                CustomMessageBox.Show(this, "更新頻度の値が不正です。");
                return;
            }

            // 名前とパスワードを設定
            m_strUserId = edtID.Text;
            m_strPassword = edtPassword.Text;

            // イベント設定
            WaitForm objWaitForm = new(WaitForm_DoWork);

            // 待機
            objWaitForm.ShowDialog();
        }

        /// <summary>
        /// ファイルダウンロード
        /// </summary>
        /// <param name="strUrl"></param>
        /// <param name="strDownloadPath"></param>
        /// <returns></returns>
        private static async Task DownloadImgAsync(string strUrl, string strDownloadPath)
        {
            using var hpClient = new HttpClient();
            
            // ダウンロード
            var hrmResponse = await hpClient.GetAsync(strUrl);
            if (hrmResponse.StatusCode != HttpStatusCode.OK)
            {
                return;
            }

            // ファイル作成
            using var sResponse = await hrmResponse.Content.ReadAsStreamAsync();
            using var fsStream = File.Create(strDownloadPath);
            sResponse.CopyTo(fsStream);
        }

        /// <summary>
        /// イベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WaitForm_DoWork(object? sender, DoWorkEventArgs e)
        {
            try
            {
                // ディレクトリ削除
                if (Directory.Exists(".\\s3s-master") == true)
                {
                    Directory.Delete(".\\s3s-master", true);
                }

                // ファイルダウンロード
                var tDownload = DownloadImgAsync("https://github.com/frozenpandaman/s3s/archive/refs/heads/master.zip", "s3s.zip");

                // ダウンロード待機
                tDownload.Wait();

                // zip解凍
                ZipFile.ExtractToDirectory("s3s.zip", ".\\");

                // 不要ファイル削除
                File.Delete("s3s.zip");

                // ソースファイルを読み込む
                string strData = File.ReadAllText(".\\s3s-master\\s3s.py");

                // 時間部分を変更
                strData = strData.Replace("elif secs < 60:", "elif secs < " + edtFreq.Text.ToString() + ":");

                // ファイル書込み
                File.WriteAllText(".\\s3s-master\\s3s.py", strData);

                // ソースファイルを読み込む
                strData = File.ReadAllText(".\\s3s-master\\iksm.py");

                // 時間部分を変更
                strData = strData.Replace("USE_OLD_NSOAPP_VER    = False", "USE_OLD_NSOAPP_VER    = True");

                // ファイル書込み
                File.WriteAllText(".\\s3s-master\\iksm.py", strData);

                // プロセス情報設定
                ProcessStartInfo psInfo = new()
                {
                    FileName = "cmd",
                    Arguments = "/c " + "pip3 install -r requirements.txt",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    WorkingDirectory = ".\\s3s-master"
                };

                // プロセス実行
                using (Process? pProc = Process.Start(psInfo))
                {
                    pProc?.WaitForExit();
                }

                // stat.inkにログインする

                // パーサー作成
                HtmlParser hpParse = new();

                strData = string.Empty;
                IHtmlDocument? dParse = null;

                if (m_bLogin == false)
                {
                    // ログインページ
                    strData = GetData("https://stat.ink/login");

                    // パース
                    dParse = hpParse.ParseDocument(strData);

                    // トークン取得
                    var strToken = dParse.QuerySelector("meta[name='csrf-token']")?.Attributes[1]?.Value;
                    if (strToken == null)
                    {
                        throw new Exception();
                    }

                    // ログイン実行
                    strData = PostData("https://stat.ink/login", new Dictionary<string, string>()
                    {
                        {"_csrf", strToken},
                        {"LoginForm[screen_name]", m_strUserId},
                        {"LoginForm[password]", m_strPassword},
                        {"LoginForm[remember_me]", "0"}
                    });

                    if (strData.Contains(m_strUserId) == false)
                    {
                        throw new Exception();
                    }

                    m_bLogin = true;
                }

                // プロフィールページ
                strData = GetData("https://stat.ink/profile");

                // パース
                dParse = hpParse.ParseDocument(strData);

                // APIキー取得
                var eApiKey = dParse.QuerySelector("div[id='apikey']");
                if (eApiKey == null)
                {
                    throw new Exception();
                }

                // APIキー取得
                var strApiKey = eApiKey.QuerySelector("input[class='form-control']")?.Attributes[2]?.Value;

                // s3sの設定を行う

                // プロセス情報設定
                psInfo = new ProcessStartInfo
                {
                    FileName = "cmd",
                    Arguments = "/c " + "python s3s.py -r",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    WorkingDirectory = ".\\s3s-master",
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true
                };

                string strUrl = string.Empty;
                bool bSuccess = false;

                // プロセス実行
                using (Process? pProc = Process.Start(psInfo))
                using (var swIn = pProc?.StandardInput)
                using (var swOut = pProc?.StandardOutput)
                {
                    // 入力を行う
                    swIn?.WriteLine(strApiKey);
                    swIn?.WriteLine();

                    // ニンテンドーオンラインのURLを取得する

                    while (true)
                    {
                        // 標準入力から1行読み込む
                        var strOut = swOut?.ReadLine();

                        if (strOut == null)
                        {
                            break;
                        }

                        Debug.WriteLine(strOut);

                        var mResult = Regex.Match(strOut, @"state=.*");
                        if (mResult.Success == true)
                        {
                            // URLを取得
                            strUrl = "https://accounts.nintendo.com/connect/1.0.0/authorize?" + mResult.Value;

                            // 入力画面表示
                            InputForm inputForm = new(strUrl);
                            inputForm.ShowDialog();

                            if (string.IsNullOrEmpty(inputForm.Result) == true)
                            {
                                return;
                            }

                            // APIトークン入力
                            swIn?.WriteLine(inputForm.Result);
                        }

                        if (strOut.Contains("Validating your tokens... done.") == true)
                        {
                            bSuccess = true;
                            break;
                        }
                    }

                    // 強制的に終わらせる
                    if (pProc != null)
                    {
                        KillProcessAndChildren(pProc.Id);
                    }
                }

                if (bSuccess == true)
                {
                    // stat.inkからデータを取得する
                    strData = GetData("https://stat.ink/@" + m_strUserId + "/spl3");

                    // パース
                    dParse = hpParse.ParseDocument(strData);

                    // バトル数が含まれた要素を取得する
                    var eCount = dParse.QuerySelector("div[class='user-number']");
                    if (eCount == null)
                    {
                        throw new Exception();
                    }

                    // 戦績の一番名を取得
                    var eBattle = dParse.QuerySelector("tr[class='battle-row']");
                    if (eBattle == null)
                    {
                        throw new Exception();
                    }

                    var eTemp = eBattle.QuerySelector("a");
                    if (eTemp == null)
                    {
                        throw new Exception();
                    }

                    // IDを取得
                    string? strBattleId = eTemp.Attributes[1]?.Value;

                    // ページ遷移
                    Invoke(() =>
                    {
                        // ページ遷移
                        wv2View.Source = new Uri("https://stat.ink" + strBattleId);
                    });

                    Invoke(() =>
                    {
                        CustomMessageBox.Show(this, "セットアップに成功しました。");
                    });
                }
                else
                {
                    throw new Exception();
                }
            }
            catch
            {
                Invoke(() =>
                {
                    CustomMessageBox.Show(this, "セットアップ中に異常が発生しました。\nシステム管理者にお問い合わせください。");
                });
            }
        }

        /// <summary>
        /// HTTP-GET実行
        /// </summary>
        /// <param name="strUrl"></param>
        /// <returns></returns>
        private string GetData(string strUrl)
        {
            HttpResponseMessage response = m_hpClient.GetAsync(strUrl).Result;

            // 正常終了した場合
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return response.Content.ReadAsStringAsync().Result;
            }

            return string.Empty;
        }

        /// <summary>
        /// HTTP-POST実行
        /// </summary>
        /// <param name="strUrl"></param>
        /// <param name="dicData"></param>
        /// <returns></returns>
        private string PostData(string strUrl, Dictionary<string, string> dicData)
        {
            MultipartFormDataContent mContent = new();

            foreach (var kvPair in dicData)
            {
                mContent.Add(new StringContent(kvPair.Value), kvPair.Key);
            }

            HttpResponseMessage response = m_hpClient.PostAsync(strUrl, mContent).Result;
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return response.Content.ReadAsStringAsync().Result;
            }

            return string.Empty;
        }

        /// <summary>
        /// コントロールの状態を変更する
        /// </summary>
        /// <param name="bPolling"></param>
        private void ChangeControlEnable(bool bPolling)
        {
            Invoke(() =>
            {
                if (bPolling == true)
                {
                    btnSetup.Enabled = false;
                    btnConnect.Enabled = false;
                    btnDisconnect.Enabled = true;
                    edtID.Enabled = false;
                    edtPassword.Enabled = false;
                    edtFreq.Enabled = false;
                }
                else
                {
                    btnSetup.Enabled = true;
                    btnConnect.Enabled = true;
                    btnDisconnect.Enabled = false;
                    edtID.Enabled = true;
                    edtPassword.Enabled = true;
                    edtFreq.Enabled = true;
                }
            });
        }

        /// <summary>
        /// 接続ボタン押下時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnConnect_Click(object sender, EventArgs e)
        {
            // セットアップ済みか確認
            if (File.Exists(".\\s3s-master\\config.txt") == false)
            {
                MessageBox.Show("セットアップが実行されていません。\n初回はセットアップを行ってください。");
                return;
            }

            // 名前とパスワードを設定
            m_strUserId = edtID.Text;
            m_strPassword = edtPassword.Text;

            m_bPolling = true;

            // ボタンの状態を変更する
            ChangeControlEnable(true);

            m_tPolling = Task.Run(() =>
            {
                try
                {
                    // パーサー作成
                    HtmlParser hpParse = new();

                    if (m_bLogin == false)
                    {
                        // ログインページ
                        var strData = GetData("https://stat.ink/login");

                        // パース
                        var dParse = hpParse.ParseDocument(strData);

                        // トークン取得
                        var strToken = dParse.QuerySelector("meta[name='csrf-token']")?.Attributes[1]?.Value;
                        if (strToken == null)
                        {
                            throw new Exception();
                        }

                        // ログイン実行
                        strData = PostData("https://stat.ink/login", new Dictionary<string, string>()
                        {
                            {"_csrf", strToken},
                            {"LoginForm[screen_name]", m_strUserId},
                            {"LoginForm[password]", m_strPassword},
                            {"LoginForm[remember_me]", "0"}
                        });

                        if (strData.Contains(m_strUserId) == false)
                        {
                            throw new Exception();
                        }

                        m_bLogin = true;
                    }

                    Invoke(() =>
                    {
                        // ページ遷移
                        wv2View.Source = new Uri("https://stat.ink/@" + m_strUserId + "/spl3");
                    });

                    // s3sを実行して画面に反映していく

                    // プロセス情報設定
                    var psInfo = new ProcessStartInfo
                    {
                        FileName = "cmd",
                        Arguments = "/c " + "python s3s.py -M 5 -r",
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        WorkingDirectory = ".\\s3s-master",
                        RedirectStandardOutput = true
                    };

                    m_pPolling = Process.Start(psInfo);

                    using (var swOut = m_pPolling?.StandardOutput)
                    {
                        // stat.inkからデータを取得する
                        var strData = GetData("https://stat.ink/@" + m_strUserId + "/spl3");

                        // パース
                        var dParse = hpParse.ParseDocument(strData);

                        // バトル数が含まれた要素を取得する
                        var eCount = dParse.QuerySelector("div[class='user-number']");
                        if (eCount == null)
                        {
                            throw new Exception();
                        }

                        // 戦績の一番名を取得
                        var eBattle = dParse.QuerySelector("tr[class='battle-row']");
                        if (eBattle == null)
                        {
                            throw new Exception();
                        }

                        var eTemp = eBattle.QuerySelector("a");
                        if (eTemp == null)
                        {
                            throw new Exception();
                        }

                        // IDを取得
                        string? strBattleId = eTemp.Attributes[1]?.Value;

                        // ページ遷移
                        Invoke(() =>
                        {
                            // ページ遷移
                            wv2View.Source = new Uri("https://stat.ink" + strBattleId);
                        });

                        int nPreviousCount = 0;

                        while (true)
                        {
                            try
                            {
                                // 停止要求が来た場合
                                if (m_bPolling == false)
                                {
                                    return;
                                }

                                // 標準入力から1行読み込む
                                var strOut = swOut?.ReadLine();

                                if (strOut == null)
                                {
                                    break;
                                }

                                Debug.WriteLine(strOut);

                                // stat.inkからデータを取得する
                                strData = GetData("https://stat.ink/@" + m_strUserId + "/spl3");

                                // パース
                                dParse = hpParse.ParseDocument(strData);

                                // バトル数が含まれた要素を取得する
                                eCount = dParse.QuerySelector("div[class='user-number']");
                                if (eCount == null)
                                {
                                    throw new Exception();
                                }

                                // バトル数を取得する
                                var nCount = int.Parse(eCount.TextContent.Trim().Replace(",", ""));
                                if (nPreviousCount < nCount)
                                {
                                    // ページ更新

                                    // 戦績の一番名を取得
                                    eBattle = dParse.QuerySelector("tr[class='battle-row']");
                                    if (eBattle == null)
                                    {
                                        throw new Exception();
                                    }

                                    eTemp = eBattle.QuerySelector("a");
                                    if (eTemp == null)
                                    {
                                        throw new Exception();
                                    }

                                    // IDを取得
                                    strBattleId = eTemp.Attributes[1]?.Value;

                                    // ページ遷移
                                    Invoke(() =>
                                    {
                                        // ページ遷移
                                        wv2View.Source = new Uri("https://stat.ink" + strBattleId);
                                    });

                                    nPreviousCount = nCount;
                                }
                            }
                            catch
                            {
                                // エラーが発生したら再度s3s実行
                                if (m_pPolling != null)
                                {
                                    KillProcessAndChildren(m_pPolling.Id);
                                }

                                m_pPolling = Process.Start(psInfo);
                            }

                            Thread.Sleep(1000);
                        }
                    }
                }
                catch
                {
                    Invoke(() =>
                    {
                        CustomMessageBox.Show(this, "s3s実行中に異常が発生しました。\nシステム管理者にお問い合わせください。");
                    });
                }
            });
        }

        /// <summary>
        /// 子プロセス含めてキルする
        /// </summary>
        /// <param name="pid"></param>
        private void KillProcessAndChildren(int pid)
        {
            // Cannot close 'system idle process'.
            if (pid == 0)
            {
                return;
            }

            ManagementObjectSearcher searcher = new ("Select * From Win32_Process Where ParentProcessID=" + pid);
            ManagementObjectCollection moc = searcher.Get();
            foreach (ManagementObject mo in moc.Cast<ManagementObject>())
            {
                KillProcessAndChildren(Convert.ToInt32(mo["ProcessID"]));
            }
            try
            {
                Process proc = Process.GetProcessById(pid);
                proc.Kill();
            }
            catch (ArgumentException)
            {
                // Process already exited.
            }
            catch (Win32Exception)
            {
                // Access denied
            }
        }

        /// <summary>
        /// 停止ボタン押下時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnDisconnect_Click(object sender, EventArgs e)
        {
            m_bPolling = false;

            // プロセスが終了するまで待機
            if (m_pPolling != null)
            {
                KillProcessAndChildren(m_pPolling.Id);
            }

            try
            {
                // タスクが完了するまで待機
                m_tPolling?.Dispose();
            }
            catch
            {
                // 無視
            }

            // ボタンの状態を変更
            ChangeControlEnable(m_bPolling);
        }

        /// <summary>
        /// 閉じるボタン押下時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// 画面ロード時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_Load(object sender, EventArgs e)
        {
            edtID.Text = ConfigurationManager.AppSettings["ID"];
            edtPassword.Text = ConfigurationManager.AppSettings["PASSWORD"];
            edtFreq.Text = ConfigurationManager.AppSettings["FREQ"];
        }

        /// <summary>
        /// 終了時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            m_bPolling = false;

            // プロセスが終了するまで待機
            if(m_pPolling != null)
            {
                KillProcessAndChildren(m_pPolling.Id);
            }

            try
            {
                // タスクが完了するまで待機
                m_tPolling?.Dispose();
            }
            catch
            {
                // 無視
            }

            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            if (config.AppSettings.Settings.AllKeys.Contains("ID") == false)
            {
                config.AppSettings.Settings.Add(new KeyValueConfigurationElement("ID", edtID.Text));
            }
            else
            {
                config.AppSettings.Settings["ID"].Value = edtID.Text;
            }

            if (config.AppSettings.Settings.AllKeys.Contains("PASSWORD") == false)
            {
                config.AppSettings.Settings.Add(new KeyValueConfigurationElement("PASSWORD", edtPassword.Text));
            }
            else
            {
                config.AppSettings.Settings["PASSWORD"].Value = edtPassword.Text;
            }

            if (config.AppSettings.Settings.AllKeys.Contains("FREQ") == false)
            {
                config.AppSettings.Settings.Add(new KeyValueConfigurationElement("FREQ", edtFreq.Text));
            }
            else
            {
                config.AppSettings.Settings["FREQ"].Value = edtFreq.Text;
            }

            config.Save();
        }

        /// <summary>
        /// キーが押下されたときの処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EdtFreq_KeyPress(object sender, KeyPressEventArgs e)
        {
            //バックスペースが押された時は有効（Deleteキーも有効）
            if (e.KeyChar == '\b')
            {
                return;
            }

            //数値0～9以外が押された時はイベントをキャンセルする
            if ((e.KeyChar < '0' || '9' < e.KeyChar))
            {
                e.Handled = true;
            }
        }
    }
}