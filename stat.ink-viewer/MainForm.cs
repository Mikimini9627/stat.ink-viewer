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
    /// MainForm�N���X
    /// </summary>
    public partial class MainForm : Form
    {
        /// <summary>
        /// ID
        /// </summary>
        private string m_strUserId = string.Empty;

        /// <summary>
        /// ���[�U�[��
        /// </summary>
        private string m_strPassword = string.Empty;

        /// <summary>
        /// �l�b�g���[�N�N���C�A���g
        /// </summary>
        private readonly HttpClient m_hpClient = new();

        /// <summary>
        /// ���O�C�����Ă��邩�ǂ���
        /// </summary>
        private bool m_bLogin = false;

        /// <summary>
        /// �|�[�����O��
        /// </summary>
        private bool m_bPolling = false;

        /// <summary>
        /// �|�[�����O�p�̃v���Z�X
        /// </summary>
        private Process? m_pPolling = null;

        /// <summary>
        /// �|�[�����O�p�̃^�X�N
        /// </summary>
        private Task? m_tPolling = null;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// �Z�b�g�A�b�v�{�^������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSetup_Click(object sender, EventArgs e)
        {
            // ���b�Z�[�W�\��
            if (CustomMessageBox.Show(this, "s3s�̃Z�b�g�A�b�v�����s���܂��B��낵���ł���?", "����", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            {
                return;
            }

            // �X�V�Ԋu�`�F�b�N
            if (int.TryParse(edtFreq.Text, out int nFreq) == false)
            {
                CustomMessageBox.Show(this, "�X�V�p�x�̒l���s���ł��B");
                return;
            }

            if (nFreq <= 0)
            {
                CustomMessageBox.Show(this, "�X�V�p�x�̒l���s���ł��B");
                return;
            }

            // ���O�ƃp�X���[�h��ݒ�
            m_strUserId = edtID.Text;
            m_strPassword = edtPassword.Text;

            // �C�x���g�ݒ�
            WaitForm objWaitForm = new(WaitForm_DoWork);

            // �ҋ@
            objWaitForm.ShowDialog();
        }

        /// <summary>
        /// �t�@�C���_�E�����[�h
        /// </summary>
        /// <param name="strUrl"></param>
        /// <param name="strDownloadPath"></param>
        /// <returns></returns>
        private static async Task DownloadImgAsync(string strUrl, string strDownloadPath)
        {
            using var hpClient = new HttpClient();
            
            // �_�E�����[�h
            var hrmResponse = await hpClient.GetAsync(strUrl);
            if (hrmResponse.StatusCode != HttpStatusCode.OK)
            {
                return;
            }

            // �t�@�C���쐬
            using var sResponse = await hrmResponse.Content.ReadAsStreamAsync();
            using var fsStream = File.Create(strDownloadPath);
            sResponse.CopyTo(fsStream);
        }

        /// <summary>
        /// �C�x���g����
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WaitForm_DoWork(object? sender, DoWorkEventArgs e)
        {
            try
            {
                // �f�B���N�g���폜
                if (Directory.Exists(".\\s3s-master") == true)
                {
                    Directory.Delete(".\\s3s-master", true);
                }

                // �t�@�C���_�E�����[�h
                var tDownload = DownloadImgAsync("https://github.com/frozenpandaman/s3s/archive/refs/heads/master.zip", "s3s.zip");

                // �_�E�����[�h�ҋ@
                tDownload.Wait();

                // zip��
                ZipFile.ExtractToDirectory("s3s.zip", ".\\");

                // �s�v�t�@�C���폜
                File.Delete("s3s.zip");

                // �\�[�X�t�@�C����ǂݍ���
                string strData = File.ReadAllText(".\\s3s-master\\s3s.py");

                // ���ԕ�����ύX
                strData = strData.Replace("elif secs < 60:", "elif secs < " + edtFreq.Text.ToString() + ":");

                // �t�@�C��������
                File.WriteAllText(".\\s3s-master\\s3s.py", strData);

                // �\�[�X�t�@�C����ǂݍ���
                strData = File.ReadAllText(".\\s3s-master\\iksm.py");

                // ���ԕ�����ύX
                strData = strData.Replace("USE_OLD_NSOAPP_VER    = False", "USE_OLD_NSOAPP_VER    = True");

                // �t�@�C��������
                File.WriteAllText(".\\s3s-master\\iksm.py", strData);

                // �v���Z�X���ݒ�
                ProcessStartInfo psInfo = new()
                {
                    FileName = "cmd",
                    Arguments = "/c " + "pip3 install -r requirements.txt",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    WorkingDirectory = ".\\s3s-master"
                };

                // �v���Z�X���s
                using (Process? pProc = Process.Start(psInfo))
                {
                    pProc?.WaitForExit();
                }

                // stat.ink�Ƀ��O�C������

                // �p�[�T�[�쐬
                HtmlParser hpParse = new();

                strData = string.Empty;
                IHtmlDocument? dParse = null;

                if (m_bLogin == false)
                {
                    // ���O�C���y�[�W
                    strData = GetData("https://stat.ink/login");

                    // �p�[�X
                    dParse = hpParse.ParseDocument(strData);

                    // �g�[�N���擾
                    var strToken = dParse.QuerySelector("meta[name='csrf-token']")?.Attributes[1]?.Value;
                    if (strToken == null)
                    {
                        throw new Exception();
                    }

                    // ���O�C�����s
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

                // �v���t�B�[���y�[�W
                strData = GetData("https://stat.ink/profile");

                // �p�[�X
                dParse = hpParse.ParseDocument(strData);

                // API�L�[�擾
                var eApiKey = dParse.QuerySelector("div[id='apikey']");
                if (eApiKey == null)
                {
                    throw new Exception();
                }

                // API�L�[�擾
                var strApiKey = eApiKey.QuerySelector("input[class='form-control']")?.Attributes[2]?.Value;

                // s3s�̐ݒ���s��

                // �v���Z�X���ݒ�
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

                // �v���Z�X���s
                using (Process? pProc = Process.Start(psInfo))
                using (var swIn = pProc?.StandardInput)
                using (var swOut = pProc?.StandardOutput)
                {
                    // ���͂��s��
                    swIn?.WriteLine(strApiKey);
                    swIn?.WriteLine();

                    // �j���e���h�[�I�����C����URL���擾����

                    while (true)
                    {
                        // �W�����͂���1�s�ǂݍ���
                        var strOut = swOut?.ReadLine();

                        if (strOut == null)
                        {
                            break;
                        }

                        Debug.WriteLine(strOut);

                        var mResult = Regex.Match(strOut, @"state=.*");
                        if (mResult.Success == true)
                        {
                            // URL���擾
                            strUrl = "https://accounts.nintendo.com/connect/1.0.0/authorize?" + mResult.Value;

                            // ���͉�ʕ\��
                            InputForm inputForm = new(strUrl);
                            inputForm.ShowDialog();

                            if (string.IsNullOrEmpty(inputForm.Result) == true)
                            {
                                return;
                            }

                            // API�g�[�N������
                            swIn?.WriteLine(inputForm.Result);
                        }

                        if (strOut.Contains("Validating your tokens... done.") == true)
                        {
                            bSuccess = true;
                            break;
                        }
                    }

                    // �����I�ɏI��点��
                    if (pProc != null)
                    {
                        KillProcessAndChildren(pProc.Id);
                    }
                }

                if (bSuccess == true)
                {
                    // stat.ink����f�[�^���擾����
                    strData = GetData("https://stat.ink/@" + m_strUserId + "/spl3");

                    // �p�[�X
                    dParse = hpParse.ParseDocument(strData);

                    // �o�g�������܂܂ꂽ�v�f���擾����
                    var eCount = dParse.QuerySelector("div[class='user-number']");
                    if (eCount == null)
                    {
                        throw new Exception();
                    }

                    // ��т̈�Ԗ����擾
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

                    // ID���擾
                    string? strBattleId = eTemp.Attributes[1]?.Value;

                    // �y�[�W�J��
                    Invoke(() =>
                    {
                        // �y�[�W�J��
                        wv2View.Source = new Uri("https://stat.ink" + strBattleId);
                    });

                    Invoke(() =>
                    {
                        CustomMessageBox.Show(this, "�Z�b�g�A�b�v�ɐ������܂����B");
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
                    CustomMessageBox.Show(this, "�Z�b�g�A�b�v���Ɉُ킪�������܂����B\n�V�X�e���Ǘ��҂ɂ��₢���킹���������B");
                });
            }
        }

        /// <summary>
        /// HTTP-GET���s
        /// </summary>
        /// <param name="strUrl"></param>
        /// <returns></returns>
        private string GetData(string strUrl)
        {
            HttpResponseMessage response = m_hpClient.GetAsync(strUrl).Result;

            // ����I�������ꍇ
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return response.Content.ReadAsStringAsync().Result;
            }

            return string.Empty;
        }

        /// <summary>
        /// HTTP-POST���s
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
        /// �R���g���[���̏�Ԃ�ύX����
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
        /// �ڑ��{�^���������̏���
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnConnect_Click(object sender, EventArgs e)
        {
            // �Z�b�g�A�b�v�ς݂��m�F
            if (File.Exists(".\\s3s-master\\config.txt") == false)
            {
                MessageBox.Show("�Z�b�g�A�b�v�����s����Ă��܂���B\n����̓Z�b�g�A�b�v���s���Ă��������B");
                return;
            }

            // ���O�ƃp�X���[�h��ݒ�
            m_strUserId = edtID.Text;
            m_strPassword = edtPassword.Text;

            m_bPolling = true;

            // �{�^���̏�Ԃ�ύX����
            ChangeControlEnable(true);

            m_tPolling = Task.Run(() =>
            {
                try
                {
                    // �p�[�T�[�쐬
                    HtmlParser hpParse = new();

                    if (m_bLogin == false)
                    {
                        // ���O�C���y�[�W
                        var strData = GetData("https://stat.ink/login");

                        // �p�[�X
                        var dParse = hpParse.ParseDocument(strData);

                        // �g�[�N���擾
                        var strToken = dParse.QuerySelector("meta[name='csrf-token']")?.Attributes[1]?.Value;
                        if (strToken == null)
                        {
                            throw new Exception();
                        }

                        // ���O�C�����s
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
                        // �y�[�W�J��
                        wv2View.Source = new Uri("https://stat.ink/@" + m_strUserId + "/spl3");
                    });

                    // s3s�����s���ĉ�ʂɔ��f���Ă���

                    // �v���Z�X���ݒ�
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
                        // stat.ink����f�[�^���擾����
                        var strData = GetData("https://stat.ink/@" + m_strUserId + "/spl3");

                        // �p�[�X
                        var dParse = hpParse.ParseDocument(strData);

                        // �o�g�������܂܂ꂽ�v�f���擾����
                        var eCount = dParse.QuerySelector("div[class='user-number']");
                        if (eCount == null)
                        {
                            throw new Exception();
                        }

                        // ��т̈�Ԗ����擾
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

                        // ID���擾
                        string? strBattleId = eTemp.Attributes[1]?.Value;

                        // �y�[�W�J��
                        Invoke(() =>
                        {
                            // �y�[�W�J��
                            wv2View.Source = new Uri("https://stat.ink" + strBattleId);
                        });

                        int nPreviousCount = 0;

                        while (true)
                        {
                            try
                            {
                                // ��~�v���������ꍇ
                                if (m_bPolling == false)
                                {
                                    return;
                                }

                                // �W�����͂���1�s�ǂݍ���
                                var strOut = swOut?.ReadLine();

                                if (strOut == null)
                                {
                                    break;
                                }

                                Debug.WriteLine(strOut);

                                // stat.ink����f�[�^���擾����
                                strData = GetData("https://stat.ink/@" + m_strUserId + "/spl3");

                                // �p�[�X
                                dParse = hpParse.ParseDocument(strData);

                                // �o�g�������܂܂ꂽ�v�f���擾����
                                eCount = dParse.QuerySelector("div[class='user-number']");
                                if (eCount == null)
                                {
                                    throw new Exception();
                                }

                                // �o�g�������擾����
                                var nCount = int.Parse(eCount.TextContent.Trim().Replace(",", ""));
                                if (nPreviousCount < nCount)
                                {
                                    // �y�[�W�X�V

                                    // ��т̈�Ԗ����擾
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

                                    // ID���擾
                                    strBattleId = eTemp.Attributes[1]?.Value;

                                    // �y�[�W�J��
                                    Invoke(() =>
                                    {
                                        // �y�[�W�J��
                                        wv2View.Source = new Uri("https://stat.ink" + strBattleId);
                                    });

                                    nPreviousCount = nCount;
                                }
                            }
                            catch
                            {
                                // �G���[������������ēxs3s���s
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
                        CustomMessageBox.Show(this, "s3s���s���Ɉُ킪�������܂����B\n�V�X�e���Ǘ��҂ɂ��₢���킹���������B");
                    });
                }
            });
        }

        /// <summary>
        /// �q�v���Z�X�܂߂ăL������
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
        /// ��~�{�^���������̏���
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnDisconnect_Click(object sender, EventArgs e)
        {
            m_bPolling = false;

            // �v���Z�X���I������܂őҋ@
            if (m_pPolling != null)
            {
                KillProcessAndChildren(m_pPolling.Id);
            }

            try
            {
                // �^�X�N����������܂őҋ@
                m_tPolling?.Dispose();
            }
            catch
            {
                // ����
            }

            // �{�^���̏�Ԃ�ύX
            ChangeControlEnable(m_bPolling);
        }

        /// <summary>
        /// ����{�^���������̏���
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// ��ʃ��[�h���̏���
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
        /// �I�����̏���
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            m_bPolling = false;

            // �v���Z�X���I������܂őҋ@
            if(m_pPolling != null)
            {
                KillProcessAndChildren(m_pPolling.Id);
            }

            try
            {
                // �^�X�N����������܂őҋ@
                m_tPolling?.Dispose();
            }
            catch
            {
                // ����
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
        /// �L�[���������ꂽ�Ƃ��̏���
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EdtFreq_KeyPress(object sender, KeyPressEventArgs e)
        {
            //�o�b�N�X�y�[�X�������ꂽ���͗L���iDelete�L�[���L���j
            if (e.KeyChar == '\b')
            {
                return;
            }

            //���l0�`9�ȊO�������ꂽ���̓C�x���g���L�����Z������
            if ((e.KeyChar < '0' || '9' < e.KeyChar))
            {
                e.Handled = true;
            }
        }
    }
}