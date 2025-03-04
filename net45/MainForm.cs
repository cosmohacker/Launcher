using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Contra
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
            InitializeComponent();
            Application.ApplicationExit += new EventHandler(OnApplicationExit);
            LaunchBtn.TabStop = false;
            ExitBtn.TabStop = false;
            ReadMeBtn.TabStop = false;
            ModDBBtn.TabStop = false;
            WBBtn.TabStop = false;
            MinBtnSm.TabStop = false;
            ExitBtnSm.TabStop = false;
            DiscordBtn.TabStop = false;
            HelpBtn.TabStop = false;
            SiteBtn.TabStop = false;
            RadioLocQuotes.TabStop = false;
            RadioEN.TabStop = false;
            MNew.TabStop = false;
            DefaultPics.TabStop = false;
            QSCheckBox.TabStop = false;
            WinCheckBox.TabStop = false;
            RadioFlag_GB.TabStop = false;
            RadioFlag_RU.TabStop = false;
            RadioFlag_UA.TabStop = false;
            RadioFlag_BG.TabStop = false;
            RadioFlag_DE.TabStop = false;
            DonateBtn.TabStop = false;
            DonateBtn.FlatAppearance.MouseOverBackColor = Color.Transparent;
            DonateBtn.FlatAppearance.MouseDownBackColor = Color.Transparent;

            // Determine OS bitness
            if (IntPtr.Size == 8)
            {
                Globals.userOS = "64";
            }
            else
            {
                Globals.userOS = "32";
            }

            // Get "Command and Conquer Generals Zero Hour Data" path:
            // Try to get path the hard-coded way
            if (Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Command and Conquer Generals Zero Hour Data\"))
            {
                Globals.myDocPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Command and Conquer Generals Zero Hour Data\";
            }
            // If above fails, search in Registry
            else
            {
                var ourVar = string.Empty;
                if (Globals.userOS == "32")
                {
                    var userDataRegistryPath = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Electronic Arts\EA Games\Command and Conquer Generals Zero Hour");
                    if (userDataRegistryPath != null)
                    {
                        ourVar = userDataRegistryPath.GetValue("UserDataLeafName") as string;
                    }
                }
                else if (Globals.userOS == "64")
                {
                    var userDataRegistryPath = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Electronic Arts\EA Games\Command and Conquer Generals Zero Hour");
                    if (userDataRegistryPath != null)
                    {
                        ourVar = userDataRegistryPath.GetValue("UserDataLeafName") as string;
                    }
                }
                if (ourVar != null)
                {
                    Globals.myDocPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\" + ourVar + @"\";
                }
            }

            DelTmpChunk();
        }

        //**********DRAG FORM CODE START**********
        const int WM_NCLBUTTONDBLCLK = 0xA3;
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_NCLBUTTONDBLCLK)
                return;

            base.WndProc(ref m);
            switch (m.Msg)
            {
                case 0x84:
                    base.WndProc(ref m);
                    if ((int)m.Result == 0x1)
                        m.Result = (IntPtr)0x2;
                    return;
            }
            base.WndProc(ref m);
        }
        //**********DRAG FORM CODE END**********

        //**********MINIMIZE FORM CODE START**********
        const int WS_MINIMIZEBOX = 0x20000;
        const int CS_DBLCLKS = 0x8;
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.Style |= WS_MINIMIZEBOX;
                cp.ClassStyle |= CS_DBLCLKS;
                return cp;
            }
        }
        //**********MINIMIZE FORM CODE END**********

        string currentFileLabel;
        string newVersion, genToolFileName = "";
        string versions_url = "https://raw.githubusercontent.com/ContraMod/Launcher/master/Versions.txt";
        string launcher_url = "https://github.com/ContraMod/Launcher/releases/download/";
        string patch_url = "http://contra.cncguild.net/Downloads/";
        static string launcherExecutingPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        bool applyNewLauncher = false;

        [DllImport("version.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetFileVersionInfoSize(string lptstrFilename, out int lpdwHandle);
        [DllImport("version.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool GetFileVersionInfo(string lptstrFilename, int dwHandle, int dwLen, byte[] lpData);
        [DllImport("version.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool VerQueryValue(byte[] pBlock, string lpSubBlock, out IntPtr lplpBuffer, out int puLen);

        public static readonly CancellationTokenSource httpCancellationToken = new CancellationTokenSource();

        public async void GetLauncherUpdate(string versionsTXT, string launcher_url)
        {
            string launcher_ver = versionsTXT.Substring(versionsTXT.LastIndexOf("Launcher: ") + 10);
            newVersion = launcher_ver.Substring(0, launcher_ver.IndexOf("$"));
            string zip_url = launcher_url + launcher_ver.Substring(0, launcher_ver.IndexOf("$")) + @"/Contra_Launcher.zip";
            string zip_path = zip_url.Split('/').Last();

            // If there is a new launcher version, call the DownloadUpdate method
            if (newVersion != Application.ProductVersion)
            {
                try
                {
                    var updatePendingText = new Dictionary<Tuple<string, string>, bool>
                    {
                        { Tuple.Create($"Contra Launcher version {newVersion} is available! Click OK to update and restart!", "Update Available"), Globals.GB_Checked},
                        { Tuple.Create($"Версия Contra Launcher {newVersion} доступна! Нажмите «ОК», чтобы обновить и перезапустить!", "Доступно обновление"), Globals.RU_Checked},
                        { Tuple.Create($"Версія Contra Launcher {newVersion} доступна! Натисніть кнопку ОК, щоб оновити та перезапустити!", "Доступне оновлення"), Globals.UA_Checked},
                        { Tuple.Create($"Contra Launcher версия {newVersion} е достъпна! Щракнете OK, за да обновите и рестартирате!", "Достъпна е актуализация"), Globals.BG_Checked},
                        { Tuple.Create($"Contra Launcher version {newVersion} ist verfьgbar! Klicke OK zum aktualisieren und neu starten!", "Aktualisierung verfьgbar"), Globals.DE_Checked},
                    }.Single(l => l.Value).Key;
                    MessageBox.Show(new Form { TopMost = true }, updatePendingText.Item1, updatePendingText.Item2, MessageBoxButtons.OK, MessageBoxIcon.Information);

                    await DownloadFile(zip_url, zip_path, TimeSpan.FromMinutes(5), httpCancellationToken.Token);

                    using (ZipArchive archive = await Task.Run(() => ZipFile.OpenRead(zip_path)))
                    {
                        foreach (ZipArchiveEntry entry in archive.Entries)
                        {
                            if (entry.Name == "Contra_Launcher.exe") continue;
                            await Task.Run(() => entry.ExtractToFile(entry.Name, true));
                        }
                    }

                    File.Delete(zip_path);
                    applyNewLauncher = true;

                    // Show a message when the launcher download has completed
                    var updateDoneText = new Dictionary<Tuple<string, string>, bool>
                    {
                        { Tuple.Create("Your application is now up-to-date!\n\nThe application will now restart!", "Update Complete"), Globals.GB_Checked},
                        { Tuple.Create("Ваше приложение теперь обновлено!\n\nПриложение будет перезагружено!", "Обновление завершено"), Globals.RU_Checked},
                        { Tuple.Create("Ваша готова до оновлення!\n\nПрограма буде перезавантажена!", "Оновлення завершено"), Globals.UA_Checked},
                        { Tuple.Create("Приложението е вече обновено!\n\nСега ще се рестартира!", "Обновяването е завършено"), Globals.BG_Checked},
                        { Tuple.Create("Ihr Programm ist jetzt auf dem neuesten Stand!\n\nDas Programm wird sich jetzt neu starten!", "Aktualisierung abgeschlossen"), Globals.DE_Checked},
                    }.Single(l => l.Value).Key;
                    MessageBox.Show(new Form { TopMost = true }, updateDoneText.Item1, updateDoneText.Item2, MessageBoxButtons.OK, MessageBoxIcon.Information);

                    //this.Close();
                    Application.Restart();
                    Environment.Exit(0);
                }
                catch (OperationCanceledException)
                {
                    applyNewLauncher = false;
                    File.Delete(zip_path); // Clean-up partial download
                    PatchDLPanel.Hide();
                }
                catch (Exception ex)
                {
                    applyNewLauncher = false;
                    File.Delete(zip_path); // Clean-up partial download
                    PatchDLPanel.Hide();
                    MessageBox.Show(ex.ToString());
                }
            }
        }

        private void UpdateLogic()
        {
            string versionsTXT = (new WebClient { Encoding = Encoding.UTF8 }).DownloadString(versions_url);

            // Update launcher
            GetLauncherUpdate(versionsTXT, launcher_url);

            // Update patch
            string launcher_ver = versionsTXT.Substring(versionsTXT.LastIndexOf("Launcher: ") + 10);
            newVersion = launcher_ver.Substring(0, launcher_ver.IndexOf("$"));

            // If launcher is up to date, P3 exists and P3 Hotfixes are missing, update the mod
            if ((newVersion == Application.ProductVersion) && (File.Exists("!!!!Contra009Final_Patch3.ctr") || File.Exists("!!!!Contra009Final_Patch3.big")))
            {
                if
                (!File.Exists("!!!!!Contra009Final_Patch3_Hotfix.ctr") && !File.Exists("!!!!!Contra009Final_Patch3_Hotfix.big") ||
                !File.Exists("!!!!!!Contra009Final_Patch3_Hotfix2.ctr") && !File.Exists("!!!!!!Contra009Final_Patch3_Hotfix2.big") ||
                !File.Exists("!!!!!!!Contra009Final_Patch3_Hotfix3.ctr") && !File.Exists("!!!!!!!Contra009Final_Patch3_Hotfix3.big") ||
                !File.Exists("!!!!!!!Contra009Final_Patch3_Hotfix3_AI.ctr") && !File.Exists("!!!!!!!Contra009Final_Patch3_Hotfix3_AI.big") ||
                !File.Exists("!!!!!!!!Contra009Final_Patch3_Hotfix4.ctr") && !File.Exists("!!!!!!!!Contra009Final_Patch3_Hotfix4.big"))
                {
                    GetModUpdate(versionsTXT, patch_url);
                }
            }

            ////Load MOTD
            //new Thread(() => ThreadProcSafeMOTD(versionsTXT)) { IsBackground = true }.Start();
        }

        private void RetrieveMOTD()
        {
            string versionsTXT = (new WebClient { Encoding = Encoding.UTF8 }).DownloadString(versions_url);

            //Load MOTD
            new Thread(() => ThreadProcSafeMOTD(versionsTXT)) { IsBackground = true }.Start();
        }

        private void applyResources(ComponentResourceManager resources, Control.ControlCollection ctls)
        {
            foreach (Control ctl in ctls)
            {
                resources.ApplyResources(ctl, ctl.Name);
                applyResources(resources, ctl.Controls);
            }
        }

        private void RadioFlag_GB_CheckedChanged(object sender, EventArgs e)
        {
            if (RadioFlag_GB.Checked)
            {
                Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-US");
                ComponentResourceManager resources = new ComponentResourceManager(typeof(MainForm));
                resources.ApplyResources(this, "$this");
                applyResources(resources, Controls);
                Globals.BG_Checked = false;
                Globals.RU_Checked = false;
                Globals.UA_Checked = false;
                Globals.DE_Checked = false;
                Globals.GB_Checked = true;
                Globals.currentLanguage = "EN";
                toolTip1.SetToolTip(RadioLocQuotes, "Units of all three factions will speak English.");
                toolTip1.SetToolTip(RadioOrigQuotes, "Each faction's units will speak their native language.");
                toolTip1.SetToolTip(RadioEN, "English in-game language.");
                toolTip1.SetToolTip(RadioRU, "Russian in-game language.");
                toolTip1.SetToolTip(MNew, "Use new soundtracks.");
                toolTip1.SetToolTip(MStandard, "Use standard Zero Hour soundtracks.");
                toolTip1.SetToolTip(DefaultPics, "Use default general portraits.");
                toolTip1.SetToolTip(GoofyPics, "Use funny general portraits.");
                toolTip1.SetToolTip(WinCheckBox, "Starts Contra in a window instead of full screen.");
                toolTip1.SetToolTip(QSCheckBox, "Disables intro and shellmap (game starts up faster).");
                toolTip1.SetToolTip(DonateBtn, "Make a donation.");
                currentFileLabel = "File: ";
                ModDLLabel.Text = "Download progress: ";
                CancelModDLBtn.Text = "Cancel";
                string verString, yearString;
                if (File.Exists("!!!!!!!!Contra009Final_Patch3_Hotfix4.big") || File.Exists("!!!!!!!!Contra009Final_Patch3_Hotfix4.ctr") && (File.Exists("!!!!!!!Contra009Final_Patch3_Hotfix3.big") || File.Exists("!!!!!!!Contra009Final_Patch3_Hotfix3.ctr") && (File.Exists("!!!!!!Contra009Final_Patch3_Hotfix2.big") || File.Exists("!!!!!!Contra009Final_Patch3_Hotfix2.ctr") && (File.Exists("!!!!!Contra009Final_Patch3_Hotfix.big") || File.Exists("!!!!!Contra009Final_Patch3_Hotfix.ctr") && (File.Exists("!!!!Contra009Final_Patch3.big") || File.Exists("!!!!Contra009Final_Patch3.ctr") && (File.Exists("!!!Contra009Final_Patch2.big") || File.Exists("!!!Contra009Final_Patch2.ctr") && (File.Exists("!!Contra009Final_Patch1.big") || File.Exists("!!Contra009Final_Patch1.ctr")) && (File.Exists("!Contra009Final.big") || File.Exists("!Contra009Final.ctr"))))))))
                {
                    verString = "009 Final Patch 3 Hotfix 4";
                    yearString = "2022";
                }
                else if (File.Exists("!!!!!!!Contra009Final_Patch3_Hotfix3.big") || File.Exists("!!!!!!!Contra009Final_Patch3_Hotfix3.ctr") && (File.Exists("!!!!!!Contra009Final_Patch3_Hotfix2.big") || File.Exists("!!!!!!Contra009Final_Patch3_Hotfix2.ctr") && (File.Exists("!!!!!Contra009Final_Patch3_Hotfix.big") || File.Exists("!!!!!Contra009Final_Patch3_Hotfix.ctr") && (File.Exists("!!!!Contra009Final_Patch3.big") || File.Exists("!!!!Contra009Final_Patch3.ctr") && (File.Exists("!!!Contra009Final_Patch2.big") || File.Exists("!!!Contra009Final_Patch2.ctr") && (File.Exists("!!Contra009Final_Patch1.big") || File.Exists("!!Contra009Final_Patch1.ctr")) && (File.Exists("!Contra009Final.big") || File.Exists("!Contra009Final.ctr")))))))
                {
                    verString = "009 Final Patch 3 Hotfix 3";
                    yearString = "2022";
                }
                else if (File.Exists("!!!!!!Contra009Final_Patch3_Hotfix2.big") || File.Exists("!!!!!!Contra009Final_Patch3_Hotfix2.ctr") && (File.Exists("!!!!!Contra009Final_Patch3_Hotfix.big") || File.Exists("!!!!!Contra009Final_Patch3_Hotfix.ctr") && (File.Exists("!!!!Contra009Final_Patch3.big") || File.Exists("!!!!Contra009Final_Patch3.ctr") && (File.Exists("!!!Contra009Final_Patch2.big") || File.Exists("!!!Contra009Final_Patch2.ctr") && (File.Exists("!!Contra009Final_Patch1.big") || File.Exists("!!Contra009Final_Patch1.ctr")) && (File.Exists("!Contra009Final.big") || File.Exists("!Contra009Final.ctr"))))))
                {
                    verString = "009 Final Patch 3 Hotfix 2";
                    yearString = "2021";
                }
                else if (File.Exists("!!!!!Contra009Final_Patch3_Hotfix.big") || File.Exists("!!!!!Contra009Final_Patch3_Hotfix.ctr") && (File.Exists("!!!!Contra009Final_Patch3.big") || File.Exists("!!!!Contra009Final_Patch3.ctr") && (File.Exists("!!!Contra009Final_Patch2.big") || File.Exists("!!!Contra009Final_Patch2.ctr") && (File.Exists("!!Contra009Final_Patch1.big") || File.Exists("!!Contra009Final_Patch1.ctr")) && (File.Exists("!Contra009Final.big") || File.Exists("!Contra009Final.ctr")))))
                {
                    verString = "009 Final Patch 3 Hotfix 1";
                    yearString = "2021";
                }
                else if (File.Exists("!!!!Contra009Final_Patch3.big") || File.Exists("!!!!Contra009Final_Patch3.ctr") && (File.Exists("!!!Contra009Final_Patch2.big") || File.Exists("!!!Contra009Final_Patch2.ctr") && (File.Exists("!!Contra009Final_Patch1.big") || File.Exists("!!Contra009Final_Patch1.ctr")) && (File.Exists("!Contra009Final.big") || File.Exists("!Contra009Final.ctr"))))
                {
                    verString = "009 Final Patch 3";
                    yearString = "2020";
                }
                else if (File.Exists("!!!Contra009Final_Patch2.big") || File.Exists("!!!Contra009Final_Patch2.ctr") && (File.Exists("!!Contra009Final_Patch1.big") || File.Exists("!!Contra009Final_Patch1.ctr")) && (File.Exists("!Contra009Final.big") || File.Exists("!Contra009Final.ctr")))
                {
                    verString = "009 Final Patch 2";
                    yearString = "2019";
                }
                else if (File.Exists("!!Contra009Final_Patch1.big") || File.Exists("!!Contra009Final_Patch1.ctr") && (File.Exists("!Contra009Final.big") || File.Exists("!Contra009Final.ctr")))
                {
                    verString = "009 Final Patch 1";
                    yearString = "2019";
                }
                else if (File.Exists("!Contra009Final.big") || File.Exists("!Contra009Final.ctr"))
                {
                    verString = "009 Final";
                    yearString = "2018";
                }
                else
                {
                    verString = "???";
                    yearString = "2018";
                }
                versionLabel.Text = "Contra Project Team " + yearString + " - Version " + verString + " - Launcher: " + Application.ProductVersion;

                // Temporary hack so update runs on main thread, versionsTXT should be rewritten to be async if possible
                try
                {
                    RetrieveMOTD();
                }
                catch { }
            }
        }

        private void RadioFlag_RU_CheckedChanged(object sender, EventArgs e)
        {
            if (RadioFlag_RU.Checked)
            {
                Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("ru-RU");
                ComponentResourceManager resources = new ComponentResourceManager(typeof(MainForm));
                resources.ApplyResources(this, "$this");
                applyResources(resources, Controls);
                Globals.GB_Checked = false;
                Globals.BG_Checked = false;
                Globals.UA_Checked = false;
                Globals.DE_Checked = false;
                Globals.RU_Checked = true;
                Globals.currentLanguage = "RU";
                toolTip1.SetToolTip(RadioLocQuotes, "Юниты всех трех фракций будут разговаривать на английском.");
                toolTip1.SetToolTip(RadioOrigQuotes, "Юниты каждой фракции будут разговаривать на их родном языке.");
                toolTip1.SetToolTip(RadioEN, "Английский язык.");
                toolTip1.SetToolTip(RadioRU, "Русский язык.");
                toolTip1.SetToolTip(MNew, "Включить новые саундтреки.");
                toolTip1.SetToolTip(MStandard, "Включить стандартные саундтреки Zero Hour.");
                toolTip1.SetToolTip(DefaultPics, "Включить портреты Генералов по умолчанию.");
                toolTip1.SetToolTip(GoofyPics, "Включить смешные портреты Генералов.");
                toolTip1.SetToolTip(WinCheckBox, "Запуск Contra в режиме окна вместо полноэкранного.");
                toolTip1.SetToolTip(QSCheckBox, "Отключает интро и шелмапу (игра запускается быстрее).");
                toolTip1.SetToolTip(DonateBtn, "Дарить команду проекта.");
                RadioLocQuotes.Text = "Англ.";
                RadioOrigQuotes.Text = "Родные";
                MNew.Text = "Новая";
                MStandard.Text = "ZH";
                WinCheckBox.Text = "Режим окна"; WinCheckBox.Left = 254;
                QSCheckBox.Text = "Быстр. старт"; QSCheckBox.Left = 254;
                RadioEN.Text = "Англ.";
                RadioRU.Text = "Русский";
                DefaultPics.Text = "По умолч.";
                GoofyPics.Text = "Смешные";
                moreOptions.Text = "Больше опций";
                currentFileLabel = "Файл: ";
                ModDLLabel.Text = "Прогресс загрузки: ";
                CancelModDLBtn.Text = "Отмена";
                onlineInstructionsLabel.Text = "Как играть онлайн?";
                replaysLabel.Text = "Повторы игр";
                customAddonsLabel.Text = "Карты и дополнения";
                supportLabel.Text = "У меня проблема";
                string verString, yearString;
                if (File.Exists("!!!!!!!!Contra009Final_Patch3_Hotfix4.big") || File.Exists("!!!!!!!!Contra009Final_Patch3_Hotfix4.ctr") && (File.Exists("!!!!!!!Contra009Final_Patch3_Hotfix3.big") || File.Exists("!!!!!!!Contra009Final_Patch3_Hotfix3.ctr") && (File.Exists("!!!!!!Contra009Final_Patch3_Hotfix2.big") || File.Exists("!!!!!!Contra009Final_Patch3_Hotfix2.ctr") && (File.Exists("!!!!!Contra009Final_Patch3_Hotfix.big") || File.Exists("!!!!!Contra009Final_Patch3_Hotfix.ctr") && (File.Exists("!!!!Contra009Final_Patch3.big") || File.Exists("!!!!Contra009Final_Patch3.ctr") && (File.Exists("!!!Contra009Final_Patch2.big") || File.Exists("!!!Contra009Final_Patch2.ctr") && (File.Exists("!!Contra009Final_Patch1.big") || File.Exists("!!Contra009Final_Patch1.ctr")) && (File.Exists("!Contra009Final.big") || File.Exists("!Contra009Final.ctr"))))))))
                {
                    verString = "009 Финал Патч 3 Хотфикс 4";
                    yearString = "2022";
                }
                else if (File.Exists("!!!!!!!Contra009Final_Patch3_Hotfix3.big") || File.Exists("!!!!!!!Contra009Final_Patch3_Hotfix3.ctr") && (File.Exists("!!!!!!Contra009Final_Patch3_Hotfix2.big") || File.Exists("!!!!!!Contra009Final_Patch3_Hotfix2.ctr") && (File.Exists("!!!!!Contra009Final_Patch3_Hotfix.big") || File.Exists("!!!!!Contra009Final_Patch3_Hotfix.ctr") && (File.Exists("!!!!Contra009Final_Patch3.big") || File.Exists("!!!!Contra009Final_Patch3.ctr") && (File.Exists("!!!Contra009Final_Patch2.big") || File.Exists("!!!Contra009Final_Patch2.ctr") && (File.Exists("!!Contra009Final_Patch1.big") || File.Exists("!!Contra009Final_Patch1.ctr")) && (File.Exists("!Contra009Final.big") || File.Exists("!Contra009Final.ctr")))))))
                {
                    verString = "009 Финал Патч 3 Хотфикс 3";
                    yearString = "2022";
                }
                else if (File.Exists("!!!!!!Contra009Final_Patch3_Hotfix2.big") || File.Exists("!!!!!!Contra009Final_Patch3_Hotfix2.ctr") && (File.Exists("!!!!!Contra009Final_Patch3_Hotfix.big") || File.Exists("!!!!!Contra009Final_Patch3_Hotfix.ctr") && (File.Exists("!!!!Contra009Final_Patch3.big") || File.Exists("!!!!Contra009Final_Patch3.ctr") && (File.Exists("!!!Contra009Final_Patch2.big") || File.Exists("!!!Contra009Final_Patch2.ctr") && (File.Exists("!!Contra009Final_Patch1.big") || File.Exists("!!Contra009Final_Patch1.ctr")) && (File.Exists("!Contra009Final.big") || File.Exists("!Contra009Final.ctr"))))))
                {
                    verString = "009 Финал Патч 3 Хотфикс 2";
                    yearString = "2021";
                }
                else if (File.Exists("!!!!!Contra009Final_Patch3_Hotfix.big") || File.Exists("!!!!!Contra009Final_Patch3_Hotfix.ctr") && (File.Exists("!!!!Contra009Final_Patch3.big") || File.Exists("!!!!Contra009Final_Patch3.ctr") && (File.Exists("!!!Contra009Final_Patch2.big") || File.Exists("!!!Contra009Final_Patch2.ctr") && (File.Exists("!!Contra009Final_Patch1.big") || File.Exists("!!Contra009Final_Patch1.ctr")) && (File.Exists("!Contra009Final.big") || File.Exists("!Contra009Final.ctr")))))
                {
                    verString = "009 Финал Патч 3 Хотфикс 1";
                    yearString = "2021";
                }
                else if (File.Exists("!!!!Contra009Final_Patch3.big") || File.Exists("!!!!Contra009Final_Patch3.ctr") && (File.Exists("!!!Contra009Final_Patch2.big") || File.Exists("!!!Contra009Final_Patch2.ctr") && (File.Exists("!!Contra009Final_Patch1.big") || File.Exists("!!Contra009Final_Patch1.ctr")) && (File.Exists("!Contra009Final.big") || File.Exists("!Contra009Final.ctr"))))
                {
                    verString = "009 Финал Патч 3";
                    yearString = "2020";
                }
                else if (File.Exists("!!!Contra009Final_Patch2.big") || File.Exists("!!!Contra009Final_Patch2.ctr") && (File.Exists("!!Contra009Final_Patch1.big") || File.Exists("!!Contra009Final_Patch1.ctr")) && (File.Exists("!Contra009Final.big") || File.Exists("!Contra009Final.ctr")))
                {
                    verString = "009 Финал Патч 2";
                    yearString = "2019";
                }
                else if (File.Exists("!!Contra009Final_Patch1.big") || File.Exists("!!Contra009Final_Patch1.ctr") && (File.Exists("!Contra009Final.big") || File.Exists("!Contra009Final.ctr")))
                {
                    verString = "009 Финал Патч 1";
                    yearString = "2019";
                }
                else if (File.Exists("!Contra009Final.big") || File.Exists("!Contra009Final.ctr"))
                {
                    verString = "009 Финал";
                    yearString = "2018";
                }
                else
                {
                    verString = "???";
                    yearString = "2018";
                }
                versionLabel.Text = "Contra Project Team " + yearString + " - Версия " + verString + " - Launcher: " + Application.ProductVersion;

                // Temporary hack so update runs on main thread, versionsTXT should be rewritten to be async if possible
                try
                {
                    RetrieveMOTD();
                }
                catch { }
            }
        }

        private void RadioFlag_UA_CheckedChanged(object sender, EventArgs e)
        {
            if (RadioFlag_UA.Checked)
            {
                Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("uk-UA");
                ComponentResourceManager resources = new ComponentResourceManager(typeof(MainForm));
                resources.ApplyResources(this, "$this");
                applyResources(resources, Controls);
                Globals.GB_Checked = false;
                Globals.RU_Checked = false;
                Globals.BG_Checked = false;
                Globals.DE_Checked = false;
                Globals.UA_Checked = true;
                Globals.currentLanguage = "UA";
                toolTip1.SetToolTip(RadioLocQuotes, "Юніти всіх трьох фракцій розмовлятимуть англійською.");
                toolTip1.SetToolTip(RadioOrigQuotes, "Юніти кожної фракції розмовлятимуть їхньою рідною мовою.");
                toolTip1.SetToolTip(RadioEN, "Англійська мова.");
                toolTip1.SetToolTip(RadioRU, "Російська мова.");
                toolTip1.SetToolTip(MNew, "Використовуйте нові саундтреки.");
                toolTip1.SetToolTip(MStandard, "Використовуйте стандартні саундтреки Zero Hour.");
                toolTip1.SetToolTip(DefaultPics, "Використовуйте портрети Генералів за замовчуванням.");
                toolTip1.SetToolTip(GoofyPics, "Використовуйте смішні портрети Генералів.");
                toolTip1.SetToolTip(WinCheckBox, "Запускає Contra у віконному режимі замість повноекранного.");
                toolTip1.SetToolTip(QSCheckBox, "Вимикає інтро і шелмапу (гра запускається швидше).");
                toolTip1.SetToolTip(DonateBtn, "Дарить команду проекту.");
                RadioLocQuotes.Text = "Англ.";
                RadioOrigQuotes.Text = "Рідні";
                MNew.Text = "Нова";
                MStandard.Text = "ZH";
                WinCheckBox.Text = "Віконний";
                QSCheckBox.Text = "Шв. старт";
                RadioEN.Text = "Англ.";
                RadioRU.Text = "Рос.";
                DefaultPics.Text = "За замовч.";
                GoofyPics.Text = "Смішні";
                moreOptions.Text = "Більше опцій";
                currentFileLabel = "Файл: ";
                ModDLLabel.Text = "Прогрес завантаження: ";
                CancelModDLBtn.Text = "Скасувати";
                onlineInstructionsLabel.Text = "Як грати онлайн?";
                replaysLabel.Text = "Повтори гри";
                customAddonsLabel.Text = "Карти та доповнення";
                supportLabel.Text = "У мене є проблема";
                string verString, yearString;
                if (File.Exists("!!!!!!!!Contra009Final_Patch3_Hotfix4.big") || File.Exists("!!!!!!!!Contra009Final_Patch3_Hotfix4.ctr") && (File.Exists("!!!!!!!Contra009Final_Patch3_Hotfix3.big") || File.Exists("!!!!!!!Contra009Final_Patch3_Hotfix3.ctr") && (File.Exists("!!!!!!Contra009Final_Patch3_Hotfix2.big") || File.Exists("!!!!!!Contra009Final_Patch3_Hotfix2.ctr") && (File.Exists("!!!!!Contra009Final_Patch3_Hotfix.big") || File.Exists("!!!!!Contra009Final_Patch3_Hotfix.ctr") && (File.Exists("!!!!Contra009Final_Patch3.big") || File.Exists("!!!!Contra009Final_Patch3.ctr") && (File.Exists("!!!Contra009Final_Patch2.big") || File.Exists("!!!Contra009Final_Patch2.ctr") && (File.Exists("!!Contra009Final_Patch1.big") || File.Exists("!!Contra009Final_Patch1.ctr")) && (File.Exists("!Contra009Final.big") || File.Exists("!Contra009Final.ctr"))))))))
                {
                    verString = "009 Фінал Патч 3 Хотфикс 4";
                    yearString = "2022";
                }
                else if (File.Exists("!!!!!!!Contra009Final_Patch3_Hotfix3.big") || File.Exists("!!!!!!!Contra009Final_Patch3_Hotfix3.ctr") && (File.Exists("!!!!!!Contra009Final_Patch3_Hotfix2.big") || File.Exists("!!!!!!Contra009Final_Patch3_Hotfix2.ctr") && (File.Exists("!!!!!Contra009Final_Patch3_Hotfix.big") || File.Exists("!!!!!Contra009Final_Patch3_Hotfix.ctr") && (File.Exists("!!!!Contra009Final_Patch3.big") || File.Exists("!!!!Contra009Final_Patch3.ctr") && (File.Exists("!!!Contra009Final_Patch2.big") || File.Exists("!!!Contra009Final_Patch2.ctr") && (File.Exists("!!Contra009Final_Patch1.big") || File.Exists("!!Contra009Final_Patch1.ctr")) && (File.Exists("!Contra009Final.big") || File.Exists("!Contra009Final.ctr")))))))
                {
                    verString = "009 Фінал Патч 3 Хотфикс 3";
                    yearString = "2022";
                }
                else if (File.Exists("!!!!!!Contra009Final_Patch3_Hotfix2.big") || File.Exists("!!!!!!Contra009Final_Patch3_Hotfix2.ctr") && (File.Exists("!!!!!Contra009Final_Patch3_Hotfix.big") || File.Exists("!!!!!Contra009Final_Patch3_Hotfix.ctr") && (File.Exists("!!!!Contra009Final_Patch3.big") || File.Exists("!!!!Contra009Final_Patch3.ctr") && (File.Exists("!!!Contra009Final_Patch2.big") || File.Exists("!!!Contra009Final_Patch2.ctr") && (File.Exists("!!Contra009Final_Patch1.big") || File.Exists("!!Contra009Final_Patch1.ctr")) && (File.Exists("!Contra009Final.big") || File.Exists("!Contra009Final.ctr"))))))
                {
                    verString = "009 Фінал Патч 3 Хотфикс 2";
                    yearString = "2021";
                }
                else if (File.Exists("!!!!!Contra009Final_Patch3_Hotfix.big") || File.Exists("!!!!!Contra009Final_Patch3_Hotfix.ctr") && (File.Exists("!!!!Contra009Final_Patch3.big") || File.Exists("!!!!Contra009Final_Patch3.ctr") && (File.Exists("!!!Contra009Final_Patch2.big") || File.Exists("!!!Contra009Final_Patch2.ctr") && (File.Exists("!!Contra009Final_Patch1.big") || File.Exists("!!Contra009Final_Patch1.ctr")) && (File.Exists("!Contra009Final.big") || File.Exists("!Contra009Final.ctr")))))
                {
                    verString = "009 Фінал Патч 3 Хотфикс 1";
                    yearString = "2021";
                }
                else if (File.Exists("!!!!Contra009Final_Patch3.big") || File.Exists("!!!!Contra009Final_Patch3.ctr") && (File.Exists("!!!Contra009Final_Patch2.big") || File.Exists("!!!Contra009Final_Patch2.ctr") && (File.Exists("!!Contra009Final_Patch1.big") || File.Exists("!!Contra009Final_Patch1.ctr")) && (File.Exists("!Contra009Final.big") || File.Exists("!Contra009Final.ctr"))))
                {
                    verString = "009 Фінал Патч 3";
                    yearString = "2020";
                }
                else if (File.Exists("!!!Contra009Final_Patch2.big") || File.Exists("!!!Contra009Final_Patch2.ctr") && (File.Exists("!!Contra009Final_Patch1.big") || File.Exists("!!Contra009Final_Patch1.ctr")) && (File.Exists("!Contra009Final.big") || File.Exists("!Contra009Final.ctr")))
                {
                    verString = "009 Фінал Патч 2";
                    yearString = "2019";
                }
                else if (File.Exists("!!Contra009Final_Patch1.big") || File.Exists("!!Contra009Final_Patch1.ctr") && (File.Exists("!Contra009Final.big") || File.Exists("!Contra009Final.ctr")))
                {
                    verString = "009 Фінал Патч 1";
                    yearString = "2019";
                }
                else if (File.Exists("!Contra009Final.big") || File.Exists("!Contra009Final.ctr"))
                {
                    verString = "009 Фінал";
                    yearString = "2018";
                }
                else
                {
                    verString = "???";
                    yearString = "2018";
                }
                versionLabel.Text = "Contra Project Team " + yearString + " - Версія " + verString + " - Launcher: " + Application.ProductVersion;

                // Temporary hack so update runs on main thread, versionsTXT should be rewritten to be async if possible
                try
                {
                    RetrieveMOTD();
                }
                catch { }
            }
        }

        private void RadioFlag_BG_CheckedChanged(object sender, EventArgs e)
        {
            if (RadioFlag_BG.Checked)
            {
                Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("bg-BG");
                ComponentResourceManager resources = new ComponentResourceManager(typeof(MainForm));
                resources.ApplyResources(this, "$this");
                applyResources(resources, Controls);
                Globals.GB_Checked = false;
                Globals.RU_Checked = false;
                Globals.UA_Checked = false;
                Globals.DE_Checked = false;
                Globals.BG_Checked = true;
                Globals.currentLanguage = "BG";
                toolTip1.SetToolTip(RadioLocQuotes, "Единиците на трите фракции ще говорят на английски.");
                toolTip1.SetToolTip(RadioOrigQuotes, "Единиците на трите фракции ще говорят на техния роден език.");
                toolTip1.SetToolTip(RadioEN, "Английски език в играта.");
                toolTip1.SetToolTip(RadioRU, "Руски език в играта.");
                toolTip1.SetToolTip(MNew, "Използвайте новата музика.");
                toolTip1.SetToolTip(MStandard, "Използвайте стандартната музика в Zero Hour.");
                toolTip1.SetToolTip(DefaultPics, "Използвайте оригиналните генералски портрети.");
                toolTip1.SetToolTip(GoofyPics, "Използвайте забавните генералски портрети.");
                toolTip1.SetToolTip(WinCheckBox, "Стартира Contra в нов прозорец вместо на цял екран.");
                toolTip1.SetToolTip(QSCheckBox, "Изключва интрото и анимираната карта (шелмапа). Играта стартира по-бързо.");
                toolTip1.SetToolTip(DonateBtn, "Направете дарение.");
                RadioLocQuotes.Text = "Англ.";
                RadioOrigQuotes.Text = "Родни";
                MNew.Text = "Нова";
                MStandard.Text = "ZH";
                WinCheckBox.Text = "В прозорец"; WinCheckBox.Left = 267;
                QSCheckBox.Text = "Бърз старт"; QSCheckBox.Left = 267;
                RadioEN.Text = "Англ.";
                RadioRU.Text = "Руски";
                DefaultPics.Text = "По подр.";
                GoofyPics.Text = "Забавни";
                moreOptions.Text = "Доп. Опции";
                currentFileLabel = "Файл: ";
                ModDLLabel.Text = "Прогрес на изтегляне: ";
                CancelModDLBtn.Text = "Отмени";
                onlineInstructionsLabel.Text = "Как да играя онлайн?";
                replaysLabel.Text = "Игрови повторения";
                customAddonsLabel.Text = "Карти и добавки";
                supportLabel.Text = "Имам проблем";
                string verString, yearString;
                if (File.Exists("!!!!!!!!Contra009Final_Patch3_Hotfix4.big") || File.Exists("!!!!!!!!Contra009Final_Patch3_Hotfix4.ctr") && (File.Exists("!!!!!!!Contra009Final_Patch3_Hotfix3.big") || File.Exists("!!!!!!!Contra009Final_Patch3_Hotfix3.ctr") && (File.Exists("!!!!!!Contra009Final_Patch3_Hotfix2.big") || File.Exists("!!!!!!Contra009Final_Patch3_Hotfix2.ctr") && (File.Exists("!!!!!Contra009Final_Patch3_Hotfix.big") || File.Exists("!!!!!Contra009Final_Patch3_Hotfix.ctr") && (File.Exists("!!!!Contra009Final_Patch3.big") || File.Exists("!!!!Contra009Final_Patch3.ctr") && (File.Exists("!!!Contra009Final_Patch2.big") || File.Exists("!!!Contra009Final_Patch2.ctr") && (File.Exists("!!Contra009Final_Patch1.big") || File.Exists("!!Contra009Final_Patch1.ctr")) && (File.Exists("!Contra009Final.big") || File.Exists("!Contra009Final.ctr"))))))))
                {
                    verString = "009 Final Пач 3 Hotfix 4";
                    yearString = "2022";
                }
                else if (File.Exists("!!!!!!!Contra009Final_Patch3_Hotfix3.big") || File.Exists("!!!!!!!Contra009Final_Patch3_Hotfix3.ctr") && (File.Exists("!!!!!!Contra009Final_Patch3_Hotfix2.big") || File.Exists("!!!!!!Contra009Final_Patch3_Hotfix2.ctr") && (File.Exists("!!!!!Contra009Final_Patch3_Hotfix.big") || File.Exists("!!!!!Contra009Final_Patch3_Hotfix.ctr") && (File.Exists("!!!!Contra009Final_Patch3.big") || File.Exists("!!!!Contra009Final_Patch3.ctr") && (File.Exists("!!!Contra009Final_Patch2.big") || File.Exists("!!!Contra009Final_Patch2.ctr") && (File.Exists("!!Contra009Final_Patch1.big") || File.Exists("!!Contra009Final_Patch1.ctr")) && (File.Exists("!Contra009Final.big") || File.Exists("!Contra009Final.ctr")))))))
                {
                    verString = "009 Final Пач 3 Hotfix 3";
                    yearString = "2022";
                }
                else if (File.Exists("!!!!!!Contra009Final_Patch3_Hotfix2.big") || File.Exists("!!!!!!Contra009Final_Patch3_Hotfix2.ctr") && (File.Exists("!!!!!Contra009Final_Patch3_Hotfix.big") || File.Exists("!!!!!Contra009Final_Patch3_Hotfix.ctr") && (File.Exists("!!!!Contra009Final_Patch3.big") || File.Exists("!!!!Contra009Final_Patch3.ctr") && (File.Exists("!!!Contra009Final_Patch2.big") || File.Exists("!!!Contra009Final_Patch2.ctr") && (File.Exists("!!Contra009Final_Patch1.big") || File.Exists("!!Contra009Final_Patch1.ctr")) && (File.Exists("!Contra009Final.big") || File.Exists("!Contra009Final.ctr"))))))
                {
                    verString = "009 Final Пач 3 Hotfix 2";
                    yearString = "2021";
                }
                else if (File.Exists("!!!!!Contra009Final_Patch3_Hotfix.big") || File.Exists("!!!!!Contra009Final_Patch3_Hotfix.ctr") && (File.Exists("!!!!Contra009Final_Patch3.big") || File.Exists("!!!!Contra009Final_Patch3.ctr") && (File.Exists("!!!Contra009Final_Patch2.big") || File.Exists("!!!Contra009Final_Patch2.ctr") && (File.Exists("!!Contra009Final_Patch1.big") || File.Exists("!!Contra009Final_Patch1.ctr")) && (File.Exists("!Contra009Final.big") || File.Exists("!Contra009Final.ctr")))))
                {
                    verString = "009 Final Пач 3 Hotfix 1";
                    yearString = "2021";
                }
                else if (File.Exists("!!!!Contra009Final_Patch3.big") || File.Exists("!!!!Contra009Final_Patch3.ctr") && (File.Exists("!!!Contra009Final_Patch2.big") || File.Exists("!!!Contra009Final_Patch2.ctr") && (File.Exists("!!Contra009Final_Patch1.big") || File.Exists("!!Contra009Final_Patch1.ctr")) && (File.Exists("!Contra009Final.big") || File.Exists("!Contra009Final.ctr"))))
                {
                    verString = "009 Final Пач 3";
                    yearString = "2020";
                }
                else if (File.Exists("!!!Contra009Final_Patch2.big") || File.Exists("!!!Contra009Final_Patch2.ctr") && (File.Exists("!!Contra009Final_Patch1.big") || File.Exists("!!Contra009Final_Patch1.ctr")) && (File.Exists("!Contra009Final.big") || File.Exists("!Contra009Final.ctr")))
                {
                    verString = "009 Final Пач 2";
                    yearString = "2019";
                }
                else if (File.Exists("!!Contra009Final_Patch1.big") || File.Exists("!!Contra009Final_Patch1.ctr") && (File.Exists("!Contra009Final.big") || File.Exists("!Contra009Final.ctr")))
                {
                    verString = "009 Final Пач 1";
                    yearString = "2019";
                }
                else if (File.Exists("!Contra009Final.big") || File.Exists("!Contra009Final.ctr"))
                {
                    verString = "009 Final";
                    yearString = "2018";
                }
                else
                {
                    verString = "???";
                    yearString = "2018";
                }
                versionLabel.Text = "Contra Екип " + yearString + " - Версия " + verString + " - Launcher: " + Application.ProductVersion;

                // Temporary hack so update runs on main thread, versionsTXT should be rewritten to be async if possible
                try
                {
                    RetrieveMOTD();
                }
                catch { }
            }
        }

        private void RadioFlag_DE_CheckedChanged(object sender, EventArgs e)
        {
            if (RadioFlag_DE.Checked)
            {
                System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("de-DE");
                ComponentResourceManager resources = new ComponentResourceManager(typeof(MainForm));
                resources.ApplyResources(this, "$this");
                applyResources(resources, Controls);
                Globals.GB_Checked = false;
                Globals.RU_Checked = false;
                Globals.UA_Checked = false;
                Globals.BG_Checked = false;
                Globals.DE_Checked = true;
                Globals.currentLanguage = "DE";
                toolTip1.SetToolTip(RadioLocQuotes, "Einheiten von allen drei Fraktionen werden Englisch sprechen.");
                toolTip1.SetToolTip(RadioOrigQuotes, "Die Einheiten jeder Fraktion sprechen ihre Muttersprache.");
                toolTip1.SetToolTip(RadioEN, "Englische in-game Sprache.");
                toolTip1.SetToolTip(RadioRU, "Russische in-game Sprache.");
                toolTip1.SetToolTip(MNew, "Verwende den neuen Soundtrack.");
                toolTip1.SetToolTip(MStandard, "Verwende den Standard Zero Hour Soundtrack.");
                toolTip1.SetToolTip(DefaultPics, "Verwende normale General Portraits.");
                toolTip1.SetToolTip(GoofyPics, "Verwende lustige General Portraits.");
                toolTip1.SetToolTip(WinCheckBox, "Startet Contra in einem Fenster anstatt im Vollbild.");
                toolTip1.SetToolTip(QSCheckBox, "Deaktiviert das Intro und die shellmap (Spiel startet schneller).");
                toolTip1.SetToolTip(DonateBtn, "Spende an das Contra-Team.");
                voicespanel.Left = 260;
                voicespanel.Size = new Size(95, 61);
                RadioLocQuotes.Text = "Englisch"; RadioLocQuotes.Left = 0;
                RadioOrigQuotes.Text = "Einheimisch"; RadioOrigQuotes.Left = 0;
                MNew.Text = "Neu";
                MStandard.Text = "Standard";
                WinCheckBox.Text = "Fenstermodus"; WinCheckBox.Left = 260;
                QSCheckBox.Text = "Schnellstart"; QSCheckBox.Left = 260;
                RadioEN.Text = "Englisch";
                RadioRU.Text = "Russisch";
                DefaultPics.Text = "Standard";
                GoofyPics.Text = "Lustig";
                moreOptions.Text = "Einstellungen";
                currentFileLabel = "Datei: ";
                ModDLLabel.Text = "Downloadfortschritt: ";
                CancelModDLBtn.Text = "Stornieren";
                onlineInstructionsLabel.Text = "Wie spiele ich online?";
                replaysLabel.Text = "Spielwiederholungen";
                customAddonsLabel.Text = "Karten und Addons";
                supportLabel.Text = "Ich habe ein Problem";
                string verString, yearString;
                if (File.Exists("!!!!!!!!Contra009Final_Patch3_Hotfix4.big") || File.Exists("!!!!!!!!Contra009Final_Patch3_Hotfix4.ctr") && (File.Exists("!!!!!!!Contra009Final_Patch3_Hotfix3.big") || File.Exists("!!!!!!!Contra009Final_Patch3_Hotfix3.ctr") && (File.Exists("!!!!!!Contra009Final_Patch3_Hotfix2.big") || File.Exists("!!!!!!Contra009Final_Patch3_Hotfix2.ctr") && (File.Exists("!!!!!Contra009Final_Patch3_Hotfix.big") || File.Exists("!!!!!Contra009Final_Patch3_Hotfix.ctr") && (File.Exists("!!!!Contra009Final_Patch3.big") || File.Exists("!!!!Contra009Final_Patch3.ctr") && (File.Exists("!!!Contra009Final_Patch2.big") || File.Exists("!!!Contra009Final_Patch2.ctr") && (File.Exists("!!Contra009Final_Patch1.big") || File.Exists("!!Contra009Final_Patch1.ctr")) && (File.Exists("!Contra009Final.big") || File.Exists("!Contra009Final.ctr"))))))))
                {
                    verString = "009 Final Patch 3 Hotfix 4";
                    yearString = "2022";
                }
                else if (File.Exists("!!!!!!!Contra009Final_Patch3_Hotfix3.big") || File.Exists("!!!!!!!Contra009Final_Patch3_Hotfix3.ctr") && (File.Exists("!!!!!!Contra009Final_Patch3_Hotfix2.big") || File.Exists("!!!!!!Contra009Final_Patch3_Hotfix2.ctr") && (File.Exists("!!!!!Contra009Final_Patch3_Hotfix.big") || File.Exists("!!!!!Contra009Final_Patch3_Hotfix.ctr") && (File.Exists("!!!!Contra009Final_Patch3.big") || File.Exists("!!!!Contra009Final_Patch3.ctr") && (File.Exists("!!!Contra009Final_Patch2.big") || File.Exists("!!!Contra009Final_Patch2.ctr") && (File.Exists("!!Contra009Final_Patch1.big") || File.Exists("!!Contra009Final_Patch1.ctr")) && (File.Exists("!Contra009Final.big") || File.Exists("!Contra009Final.ctr")))))))
                {
                    verString = "009 Final Patch 3 Hotfix 3";
                    yearString = "2022";
                }
                else if (File.Exists("!!!!!!Contra009Final_Patch3_Hotfix2.big") || File.Exists("!!!!!!Contra009Final_Patch3_Hotfix2.ctr") && (File.Exists("!!!!!Contra009Final_Patch3_Hotfix.big") || File.Exists("!!!!!Contra009Final_Patch3_Hotfix.ctr") && (File.Exists("!!!!Contra009Final_Patch3.big") || File.Exists("!!!!Contra009Final_Patch3.ctr") && (File.Exists("!!!Contra009Final_Patch2.big") || File.Exists("!!!Contra009Final_Patch2.ctr") && (File.Exists("!!Contra009Final_Patch1.big") || File.Exists("!!Contra009Final_Patch1.ctr")) && (File.Exists("!Contra009Final.big") || File.Exists("!Contra009Final.ctr"))))))
                {
                    verString = "009 Final Patch 3 Hotfix 2";
                    yearString = "2021";
                }
                else if (File.Exists("!!!!!Contra009Final_Patch3_Hotfix.big") || File.Exists("!!!!!Contra009Final_Patch3_Hotfix.ctr") && (File.Exists("!!!!Contra009Final_Patch3.big") || File.Exists("!!!!Contra009Final_Patch3.ctr") && (File.Exists("!!!Contra009Final_Patch2.big") || File.Exists("!!!Contra009Final_Patch2.ctr") && (File.Exists("!!Contra009Final_Patch1.big") || File.Exists("!!Contra009Final_Patch1.ctr")) && (File.Exists("!Contra009Final.big") || File.Exists("!Contra009Final.ctr")))))
                {
                    verString = "009 Final Patch 3 Hotfix 1";
                    yearString = "2021";
                }
                else if (File.Exists("!!!!Contra009Final_Patch3.big") || File.Exists("!!!!Contra009Final_Patch3.ctr") && (File.Exists("!!!Contra009Final_Patch2.big") || File.Exists("!!!Contra009Final_Patch2.ctr") && (File.Exists("!!Contra009Final_Patch1.big") || File.Exists("!!Contra009Final_Patch1.ctr")) && (File.Exists("!Contra009Final.big") || File.Exists("!Contra009Final.ctr"))))
                {
                    verString = "009 Final Patch 3";
                    yearString = "2020";
                }
                else if (File.Exists("!!!Contra009Final_Patch2.big") || File.Exists("!!!Contra009Final_Patch2.ctr") && (File.Exists("!!Contra009Final_Patch1.big") || File.Exists("!!Contra009Final_Patch1.ctr")) && (File.Exists("!Contra009Final.big") || File.Exists("!Contra009Final.ctr")))
                {
                    verString = "009 Final Patch 2";
                    yearString = "2019";
                }
                else if (File.Exists("!!Contra009Final_Patch1.big") || File.Exists("!!Contra009Final_Patch1.ctr") && (File.Exists("!Contra009Final.big") || File.Exists("!Contra009Final.ctr")))
                {
                    verString = "009 Final Patch 1";
                    yearString = "2019";
                }
                else if (File.Exists("!Contra009Final.big") || File.Exists("!Contra009Final.ctr"))
                {
                    verString = "009 Final";
                    yearString = "2018";
                }
                else
                {
                    verString = "???";
                    yearString = "2018";
                }
                versionLabel.Text = "Contra Projekt Team " + yearString + " - Version " + verString + " - Launcher: " + Application.ProductVersion;

                // Temporary hack so update runs on main thread, versionsTXT should be rewritten to be async if possible
                try
                {
                    RetrieveMOTD();
                }
                catch { }
            }
        }

        public async void GetModUpdate(string versionsTXT, string patch_url)
        {
            string zip_url = null;
            string modVersionText = null;

            if (!File.Exists("!!!!!Contra009Final_Patch3_Hotfix.ctr") && !File.Exists("!!!!!Contra009Final_Patch3_Hotfix.big"))
            {
                zip_url = patch_url + @"/Contra009FinalPatch3Hotfix.zip";
                modVersionText = "009 Final Patch 3 Hotfix";
            }
            else if (!File.Exists("!!!!!!Contra009Final_Patch3_Hotfix2.ctr") && !File.Exists("!!!!!!Contra009Final_Patch3_Hotfix2.big"))
            {
                zip_url = patch_url + @"/Contra009FinalPatch3Hotfix2.zip";
                modVersionText = "009 Final Patch 3 Hotfix 2";
            }
            else if (!File.Exists("!!!!!!!Contra009Final_Patch3_Hotfix3.ctr") && !File.Exists("!!!!!!!Contra009Final_Patch3_Hotfix3.big"))
            {
                zip_url = patch_url + @"/Contra009FinalPatch3Hotfix3.zip";
                modVersionText = "009 Final Patch 3 Hotfix 3";
            }
            else if (!File.Exists("!!!!!!!Contra009Final_Patch3_Hotfix3_AI.ctr") && !File.Exists("!!!!!!!Contra009Final_Patch3_Hotfix3_AI.big"))
            {
                zip_url = patch_url + @"/Contra009FinalPatch3Hotfix3_AI.zip";
                modVersionText = "009 Final Patch 3 Hotfix 3";
            }
            else if (!File.Exists("!!!!!!!!Contra009Final_Patch3_Hotfix4.ctr") && !File.Exists("!!!!!!!!Contra009Final_Patch3_Hotfix4.big"))
            {
                zip_url = patch_url + @"/Contra009FinalPatch3Hotfix4.zip";
                modVersionText = "009 Final Patch 3 Hotfix 4";
            }
            string zip_path = zip_url.Split('/').Last();

            try
            {
                var updatePendingText = new Dictionary<Tuple<string, string>, bool>
                    {
                        { Tuple.Create($"Contra version {modVersionText} is available!\n\nNote: If you play online, you should download the new version at all costs, otherwise the game will be interrupted by mismatch error!\n\nWould you like to download and update now?", "Update Available"), Globals.GB_Checked},
                        { Tuple.Create($"Версия Contra {modVersionText} доступна!\n\nПримечание: Если вы играете онлайн, вам следует загрузить новую версию любой ценой, иначе игра выдаст ошибку несоответствия!\n\nХотите скачать и обновить сейчас?", "Доступно обновление"), Globals.RU_Checked},
                        { Tuple.Create($"Версія Contra {modVersionText} доступна!\n\nПримітка: Якщо ви граєте в Інтернеті, вам слід завантажити нову версію за будь-яку ціну, інакше гра викличе помилку невідповідності!\n\nХочете завантажити та оновити зараз?", "Доступне оновлення"), Globals.UA_Checked},
                        { Tuple.Create($"Contra версия {modVersionText} е достъпна!\n\nЗабележка: Ако играете онлайн, трябва да изтеглите новата версия на всяка цена, в противен случай играта ще прекъсва с грешка за несъответствие!\n\nИскате ли да изтеглите и актуализирате сега?", "Достъпна е актуализация"), Globals.BG_Checked},
                        { Tuple.Create($"Contra version {modVersionText} ist verfьgbar!\n\nHinweis: Wenn Sie online spielen, sollten Sie die neue Version unbedingt herunterladen, da sonst ein Fehlanpassungsfehler auftritt!\n\nMöchten Sie jetzt herunterladen und aktualisieren?", "Aktualisierung verfьgbar"), Globals.DE_Checked},
                    }.Single(l => l.Value).Key;
                DialogResult dialogResult = MessageBox.Show(new Form { TopMost = true }, updatePendingText.Item1, updatePendingText.Item2, MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                if (dialogResult == DialogResult.Yes)
                {
                    await DownloadFile(zip_url, zip_path, TimeSpan.FromMinutes(5), httpCancellationToken.Token);

                    using (ZipArchive archive = await Task.Run(() => ZipFile.OpenRead(zip_path)))
                    {
                        foreach (ZipArchiveEntry entry in archive.Entries)
                        {
                            //if (entry.Name == "Contra_Launcher.exe") continue;
                            await Task.Run(() => entry.ExtractToFile(entry.Name, true));
                        }
                    }

                    File.Delete(zip_path);
                    //restartLauncher = true;

                    // Show a message when the patch download has completed
                    var updateDoneText = new Dictionary<Tuple<string, string>, bool>
                        {
                            { Tuple.Create($"The new version {modVersionText} was installed successfully! The launcher will now restart!", "Update Complete"), Globals.GB_Checked},
                            { Tuple.Create($"Новая версия {modVersionText} успешно установлена! Лаунчер перезапустится!", "Обновление завершено"), Globals.RU_Checked},
                            { Tuple.Create($"Нова версія {modVersionText} була успішно встановлена! Тепер лаунчер перезапуститься!", "Оновлення завершено"), Globals.UA_Checked},
                            { Tuple.Create($"Новата версия {modVersionText} беше инсталирана успешно! Launcher-а ще се рестартира!", "Обновяването е завършено"), Globals.BG_Checked},
                            { Tuple.Create($"Die neue Version {modVersionText} wurde erfolgreich installiert! Der Launcher wird jetzt neu gestartet!", "Aktualisierung abgeschlossen"), Globals.DE_Checked},
                        }.Single(l => l.Value).Key;
                    MessageBox.Show(new Form { TopMost = true }, updateDoneText.Item1, updateDoneText.Item2, MessageBoxButtons.OK, MessageBoxIcon.Information);

                    //this.Close();
                    Application.Restart();
                    Environment.Exit(0);
                }
                else
                { }
            }
            catch (OperationCanceledException)
            {
                //restartLauncher = false;
                File.Delete(zip_path); // Clean-up partial download
                PatchDLPanel.Hide();
            }
            catch (Exception ex)
            {
                //restartLauncher = false;
                File.Delete(zip_path); // Clean-up partial download
                PatchDLPanel.Hide();
                MessageBox.Show(ex.ToString());
            }
        }

        public static readonly HttpClient httpclient = new HttpClient();

        public static Tuple<double, string> ByteToSizeType(long value)
        {
            if (value == 0L) return Tuple.Create(0D, "Bytes"); // zero is plural
            IReadOnlyDictionary<long, string> thresholds = new Dictionary<long, string>()
                {
                    { 1, "Byte" },
                    { 2, "Bytes" },
                    { 1024, "KiB" },
                    { 1048576, "MiB" },
                    { 1073741824, "GiB" },
                    { 1099511627776, "TiB" },
                    { 1125899906842620, "PiB" },
                    { 1152921504606850000, "EiB" },
                };
            for (int t = thresholds.Count - 1; t > 0; t--)
            {
                if (value >= thresholds.ElementAt(t).Key) return Tuple.Create(Math.Round((double)value / thresholds.ElementAt(t).Key, 2), thresholds.ElementAt(t).Value);
            }
            // handle negative values if given
            var reValue = ByteToSizeType(-value);
            return Tuple.Create(-reValue.Item1, reValue.Item2);
        }

        public async Task DownloadFile(string url, string outPath, TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            httpclient.Timeout = timeout;
            var response = httpclient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead).Result;
            response.EnsureSuccessStatusCode();

            var contentLength = response.Content.Headers.ContentLength.GetValueOrDefault();
            var totalToDownload = ByteToSizeType(contentLength);
            var downloadSize = totalToDownload.Item1;
            var downloadUnit = totalToDownload.Item2;
            PatchDLPanel.Show();

            using (Stream contentStream = await response.Content.ReadAsStreamAsync(), fileStream = new FileStream(outPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
            {
                var data = new byte[8192];
                long totalBytesRead = 0L, readCount = 0L;
                bool bytesRemaining = true;

                do
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var bytesRead = await contentStream.ReadAsync(data, 0, data.Length, cancellationToken);

                    if (bytesRead == 0) bytesRemaining = false;
                    else
                    {
                        await fileStream.WriteAsync(data, 0, bytesRead);
                        totalBytesRead += bytesRead;
                        readCount += 1;
                        if (readCount % 100 == 0)
                        {
                            PatchDLProgressBar.Value = Convert.ToInt32((double)totalBytesRead / contentLength * 100);
                            DLPercentLabel.Text = $"{(double)totalBytesRead / contentLength * 100:F2}%";
                            ModDLCurrentFileLabel.Text = currentFileLabel + outPath;
                            ModDLFileSizeLabel.Text = $"{ByteToSizeType(totalBytesRead).Item1} / {downloadSize} {downloadUnit}";
                            //ModDLFileSizeLabel.Text = $"{BytesToSize(e.BytesReceived, SizeUnits.MiB)} MiB / {BytesToSize(e.TotalBytesToReceive, SizeUnits.MiB)} MiB";
                        }
                    }
                }
                while (bytesRemaining);
            }
            PatchDLPanel.Hide();
        }

        public static async Task DownloadFileSimple(string url, string outPath, TimeSpan timeout)
        {
            httpclient.Timeout = timeout;
            var response = httpclient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead).Result;
            response.EnsureSuccessStatusCode();

            using (var contentStream = new FileStream(outPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
            {
                await response.Content.CopyToAsync(contentStream);
            }
        }

        private void CheckInstallDir()
        {
            DialogResult dialogResult = MessageBox.Show(Messages.GenerateMessage("E_NotFound_GeneralsEXE", Globals.currentLanguage), Messages.GenerateMessage("Error", Globals.currentLanguage), MessageBoxButtons.YesNoCancel, MessageBoxIcon.Error);
            if (dialogResult == DialogResult.Yes)
            {
                Url_open("https://github.com/ContraMod/Launcher/blob/master/Online%20Instructions.md");
            }
        }

        private void DelTmpChunk()
        {
            try
            {
                File.Delete(Globals.myDocPath + "_tmpChunk.dat");
            }
            catch { }
        }

        private void LaunchBtn_MouseEnter(object sender, EventArgs e)
        {
            LaunchBtn.BackgroundImage = Properties.Resources._button_launch_text;
            LaunchBtn.ForeColor = SystemColors.ButtonHighlight;
            LaunchBtn.FlatAppearance.BorderColor = Color.FromArgb(0, 255, 255, 255);
        }
        private void LaunchBtn_MouseLeave(object sender, EventArgs e)
        {
            LaunchBtn.BackgroundImage = Properties.Resources._button_launch;
            LaunchBtn.ForeColor = SystemColors.ButtonHighlight;
            LaunchBtn.FlatAppearance.BorderColor = Color.FromArgb(0, 255, 255, 255);
        }
        private void LaunchBtn_MouseDown(object sender, MouseEventArgs e)
        {
            LaunchBtn.BackgroundImage = Properties.Resources._button_highlight;
            LaunchBtn.ForeColor = SystemColors.ButtonHighlight;
            LaunchBtn.FlatAppearance.BorderColor = Color.FromArgb(0, 255, 255, 255);
        }

        private void SiteBtn_MouseEnter(object sender, EventArgs e)
        {
            SiteBtn.BackgroundImage = Properties.Resources._button_website_text;
            SiteBtn.ForeColor = SystemColors.ButtonHighlight;
            SiteBtn.FlatAppearance.BorderColor = Color.FromArgb(0, 255, 255, 255);
        }
        private void SiteBtn_MouseLeave(object sender, EventArgs e)
        {
            SiteBtn.BackgroundImage = Properties.Resources._button_website;
            SiteBtn.ForeColor = SystemColors.ButtonHighlight;
            SiteBtn.FlatAppearance.BorderColor = Color.FromArgb(0, 255, 255, 255);
        }
        private void SiteBtn_MouseDown(object sender, MouseEventArgs e)
        {
            SiteBtn.BackgroundImage = Properties.Resources._button_highlight;
            SiteBtn.ForeColor = SystemColors.ButtonHighlight;
            SiteBtn.FlatAppearance.BorderColor = Color.FromArgb(0, 255, 255, 255);
        }
        private void SiteBtn_Click(object sender, EventArgs e)
        {
            Url_open("https://contra.cncguild.net/oldsite/Eng/index.php");
        }

        private void ModDBBtn_MouseEnter(object sender, EventArgs e)
        {
            ModDBBtn.BackgroundImage = Properties.Resources._button_moddb_text;
            ModDBBtn.ForeColor = SystemColors.ButtonHighlight;
            ModDBBtn.FlatAppearance.BorderColor = Color.FromArgb(0, 255, 255, 255);
        }
        private void ModDBBtn_MouseLeave(object sender, EventArgs e)
        {
            ModDBBtn.BackgroundImage = Properties.Resources._button_moddb;
            ModDBBtn.ForeColor = SystemColors.ButtonHighlight;
            ModDBBtn.FlatAppearance.BorderColor = Color.FromArgb(0, 255, 255, 255);
        }
        private void ModDBBtn_MouseDown(object sender, MouseEventArgs e)
        {
            ModDBBtn.BackgroundImage = Properties.Resources._button_highlight;
            ModDBBtn.ForeColor = SystemColors.ButtonHighlight;
            ModDBBtn.FlatAppearance.BorderColor = Color.FromArgb(0, 255, 255, 255);
        }
        private void ModDBBtn_Click(object sender, EventArgs e)
        {
            Url_open("https://www.moddb.com/mods/contra");
        }

        private void ReadMeBtn_MouseEnter(object sender, EventArgs e)
        {
            ReadMeBtn.BackgroundImage = Properties.Resources._button_readme_text;
            ReadMeBtn.ForeColor = SystemColors.ButtonHighlight;
            ReadMeBtn.FlatAppearance.BorderColor = Color.FromArgb(0, 255, 255, 255);
        }
        private void ReadMeBtn_MouseLeave(object sender, EventArgs e)
        {
            ReadMeBtn.BackgroundImage = Properties.Resources._button_readme;
            ReadMeBtn.ForeColor = SystemColors.ButtonHighlight;
            ReadMeBtn.FlatAppearance.BorderColor = Color.FromArgb(0, 255, 255, 255);
        }
        private void ReadMeBtn_MouseDown(object sender, MouseEventArgs e)
        {
            ReadMeBtn.BackgroundImage = Properties.Resources._button_highlight;
            ReadMeBtn.ForeColor = SystemColors.ButtonHighlight;
            ReadMeBtn.FlatAppearance.BorderColor = Color.FromArgb(0, 255, 255, 255);
        }
        private void ReadMeBtn_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start("Readme_Contra.txt");
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
        }

        private void WBBtn_MouseEnter(object sender, EventArgs e)
        {
            WBBtn.BackgroundImage = Properties.Resources._button_wb_text;
            WBBtn.ForeColor = SystemColors.ButtonHighlight;
            WBBtn.FlatAppearance.BorderColor = Color.FromArgb(0, 255, 255, 255);
        }
        private void WBBtn_MouseLeave(object sender, EventArgs e)
        {
            WBBtn.BackgroundImage = Properties.Resources._button_wb;
            WBBtn.ForeColor = SystemColors.ButtonHighlight;
            WBBtn.FlatAppearance.BorderColor = Color.FromArgb(0, 255, 255, 255);
        }
        private void WBBtn_MouseDown(object sender, MouseEventArgs e)
        {
            WBBtn.BackgroundImage = Properties.Resources._button_highlight;
            WBBtn.ForeColor = SystemColors.ButtonHighlight;
            WBBtn.FlatAppearance.BorderColor = Color.FromArgb(0, 255, 255, 255);
        }
        private void WBBtn_Enter(object sender, EventArgs e)
        {
            WBBtn.BackColor = System.Drawing.Color.Transparent;
        }
        private void WBBtn_Click(object sender, EventArgs e)
        {
            DeleteDuplicateFiles();
            RenameBigToCtr();

            try
            {
                List<string> ctrs = new List<string>
                {
                    "!Contra009Final.ctr",
                    "!!Contra009Final_Patch1.ctr",
                    "!!!Contra009Final_Patch2.ctr",
                    "!!!Contra009Final_Patch2_GameData.ctr",
                    "!!!!Contra009Final_Patch3.ctr",
                    "!!!!Contra009Final_Patch3_GameData.ctr",
                    "!!!!!Contra009Final_Patch3_Hotfix.ctr",
                    "!!!!!!Contra009Final_Patch3_Hotfix2.ctr",
                    "!!!!!!!Contra009Final_Patch3_Hotfix3.ctr",
                    "!!!!!!!Contra009Final_Patch3_Hotfix3_AI.ctr",
                    "!!!!!!!!Contra009Final_Patch3_Hotfix4.ctr",
                    "!Contra009Final_EN.ctr",
                    "!!Contra009Final_Patch1_EN.ctr",
                    "!!!Contra009Final_Patch2_EN.ctr",
                    "!!!!Contra009Final_Patch3_EN_Legacy.ctr",
                    "!Contra009Final_EngVO.ctr",
                    "!!Contra009Final_Patch1_EngVO.ctr",
                    "!!!Contra009Final_Patch2_EngVO.ctr",
                    "!!!!Contra009Final_Patch3_EngVO.ctr"
                };
                foreach (string ctr in ctrs)
                {
                    string big = ctr.Replace(".ctr", ".big");
                    try
                    {
                        File.Move(ctr, big);
                    }
                    catch { }
                }
            }
            catch { }

            if ((Properties.Settings.Default.WaterEffects == false) && (File.Exists("!!Contra009Final_WaterEffectsOff.ctr")))
            {
                File.Move("!!Contra009Final_WaterEffectsOff.ctr", "!!Contra009Final_WaterEffectsOff.big");
            }
            else if ((Properties.Settings.Default.WaterEffects == true) && (File.Exists("!!Contra009Final_WaterEffectsOff.big")))
            {
                File.Move("!!Contra009Final_WaterEffectsOff.big", "!!Contra009Final_WaterEffectsOff.ctr");
            }

            Process wb = new Process();
            wb.StartInfo.Verb = "runas";
            try
            {
                if (File.Exists("WorldBuilder_Ctr.exe"))
                {
                    wb.StartInfo.FileName = "WorldBuilder_Ctr.exe";
                    wb.StartInfo.WorkingDirectory = Path.GetDirectoryName("WorldBuilder_Ctr.exe");
                    wb.Start();
                }
                else if (File.Exists("WorldBuilder.exe"))
                {
                    wb.StartInfo.FileName = "WorldBuilder.exe";
                    wb.StartInfo.WorkingDirectory = Path.GetDirectoryName("WorldBuilder.exe");
                    wb.Start();
                }
                else
                {
                    Messages.GenerateMessageBox("E_NotFound_WB", Globals.currentLanguage);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
            return;
        }

        private void DiscordBtn_MouseDown(object sender, MouseEventArgs e)
        {
            DiscordBtn.BackgroundImage = Properties.Resources._button_highlight;
            DiscordBtn.ForeColor = SystemColors.ButtonHighlight;
            DiscordBtn.FlatAppearance.BorderColor = Color.FromArgb(0, 255, 255, 255);
        }
        private void DiscordBtn_MouseEnter(object sender, EventArgs e)
        {
            DiscordBtn.BackgroundImage = Properties.Resources._button_discord_text;
            DiscordBtn.ForeColor = SystemColors.ButtonHighlight;
            DiscordBtn.FlatAppearance.BorderColor = Color.FromArgb(0, 255, 255, 255);
        }
        private void DiscordBtn_MouseLeave(object sender, EventArgs e)
        {
            DiscordBtn.BackgroundImage = Properties.Resources._button_discord;
            DiscordBtn.ForeColor = SystemColors.ButtonHighlight;
            DiscordBtn.FlatAppearance.BorderColor = Color.FromArgb(0, 255, 255, 255);
        }
        private void DiscordBtn_Click(object sender, EventArgs e)
        {
            Url_open("https://discordapp.com/invite/015E6KXXHmdWFXCtt");
        }

        private void HelpBtn_MouseDown(object sender, MouseEventArgs e)
        {
            HelpBtn.BackgroundImage = Properties.Resources._button_highlight;
            HelpBtn.ForeColor = SystemColors.ButtonHighlight;
            HelpBtn.FlatAppearance.BorderColor = Color.FromArgb(0, 255, 255, 255);
        }
        private void HelpBtn_MouseEnter(object sender, EventArgs e)
        {
            HelpBtn.BackgroundImage = Properties.Resources._button_help_text;
            HelpBtn.ForeColor = SystemColors.ButtonHighlight;
            HelpBtn.FlatAppearance.BorderColor = Color.FromArgb(0, 255, 255, 255);
        }
        private void HelpBtn_MouseLeave(object sender, EventArgs e)
        {
            HelpBtn.BackgroundImage = Properties.Resources._button_help;
            HelpBtn.ForeColor = SystemColors.ButtonHighlight;
            HelpBtn.FlatAppearance.BorderColor = Color.FromArgb(0, 255, 255, 255);
        }
        private void HelpBtn_Click(object sender, EventArgs e)
        {
            Url_open("https://contra.cncguild.net/oldsite/Eng/trouble.php");
        }

        private void OptionsForm_MouseEnter(object sender, EventArgs e)
        {
            moreOptions.ForeColor = Color.FromArgb(255, 210, 100);
        }
        private void OptionsForm_MouseDown(object sender, MouseEventArgs e)
        {
            moreOptions.ForeColor = Color.FromArgb(255, 230, 160);
        }
        private void OptionsForm_MouseLeave(object sender, EventArgs e)
        {
            moreOptions.ForeColor = Color.FromArgb(255, 255, 255);
        }
        private void OptionsForm_Click(object sender, EventArgs e)
        {
            // Delete duplicate GameData if such exists
            if (File.Exists("!!!Contra009Final_Patch2_GameData.ctr") && File.Exists("!!!Contra009Final_Patch2_GameData.big"))
            {
                File.Delete("!!!Contra009Final_Patch2_GameData.big");
            }
            if (File.Exists("!!!!Contra009Final_Patch3_GameData.ctr") && File.Exists("!!!!Contra009Final_Patch3_GameData.big"))
            {
                File.Delete("!!!!Contra009Final_Patch3_GameData.big");
            }
            // Enable GameData so that we can show current camera height in Options
            if (File.Exists("!!!!Contra009Final_Patch3_GameData.ctr"))
            {
                File.Move("!!!!Contra009Final_Patch3_GameData.ctr", "!!!!Contra009Final_Patch3_GameData.big");
            }
            if (File.Exists("!!!Contra009Final_Patch2_GameData.ctr"))
            {
                File.Move("!!!Contra009Final_Patch2_GameData.ctr", "!!!Contra009Final_Patch2_GameData.big");
            }

            if (File.Exists(Globals.myDocPath + "Options.ini"))
            {
                foreach (Form OptionsForm in Application.OpenForms)
                {
                    if (OptionsForm is OptionsForm)
                    {
                        OptionsForm.Close();
                        new OptionsForm().Show();
                        return;
                    }
                }
                new OptionsForm().Show();
            }
            else
            {
                Messages.GenerateMessageBox("E_NotFound_OptionsIni", Globals.currentLanguage);
            }
        }

        private void ExitBtnSm_MouseEnter(object sender, EventArgs e)
        {
            ExitBtnSm.BackgroundImage = Properties.Resources._button_sm_exit_tr;
        }
        private void ExitBtnSm_MouseLeave(object sender, EventArgs e)
        {
            ExitBtnSm.BackgroundImage = Properties.Resources._button_sm_exit;
        }
        private void ExitBtnSm_Click(object sender, EventArgs e)
        {
            this.Close(); //Application.Exit(); //OnApplicationExit(sender, e);
        }

        private void MinBtnSm_MouseEnter(object sender, EventArgs e)
        {
            MinBtnSm.BackgroundImage = Properties.Resources._button_sm_min_tr;
        }
        private void MinBtnSm_MouseLeave(object sender, EventArgs e)
        {
            MinBtnSm.BackgroundImage = Properties.Resources._button_sm_min;
        }
        private void MinBtnSm_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        private void RadioFlag_GB_MouseEnter(object sender, EventArgs e)
        {
            RadioFlag_GB.BackgroundImage = Properties.Resources.flag_gb_tr;
        }
        private void RadioFlag_GB_MouseLeave(object sender, EventArgs e)
        {
            RadioFlag_GB.BackgroundImage = Properties.Resources.flag_gb;
        }

        private void RadioFlag_RU_MouseEnter(object sender, EventArgs e)
        {
            RadioFlag_RU.BackgroundImage = Properties.Resources.flag_ru_tr;
        }
        private void RadioFlag_RU_MouseLeave(object sender, EventArgs e)
        {
            RadioFlag_RU.BackgroundImage = Properties.Resources.flag_ru;
        }

        private void RadioFlag_UA_MouseEnter(object sender, EventArgs e)
        {
            RadioFlag_UA.BackgroundImage = Properties.Resources.flag_ua_tr;
        }
        private void RadioFlag_UA_MouseLeave(object sender, EventArgs e)
        {
            RadioFlag_UA.BackgroundImage = Properties.Resources.flag_ua;
        }

        private void RadioFlag_BG_MouseEnter(object sender, EventArgs e)
        {
            RadioFlag_BG.BackgroundImage = Properties.Resources.flag_bg_tr;
        }
        private void RadioFlag_BG_MouseLeave(object sender, EventArgs e)
        {
            RadioFlag_BG.BackgroundImage = Properties.Resources.flag_bg;
        }

        private void RadioFlag_DE_MouseEnter(object sender, EventArgs e)
        {
            RadioFlag_DE.BackgroundImage = Properties.Resources.flag_de_tr;
        }
        private void RadioFlag_DE_MouseLeave(object sender, EventArgs e)
        {
            RadioFlag_DE.BackgroundImage = Properties.Resources.flag_de;
        }

        private void ExitBtn_MouseEnter(object sender, EventArgs e)
        {
            ExitBtn.BackgroundImage = Properties.Resources._button_exit_text;
            ExitBtn.ForeColor = SystemColors.ButtonHighlight;
            ExitBtn.FlatAppearance.BorderColor = Color.FromArgb(0, 255, 255, 255);
        }
        private void ExitBtn_MouseLeave(object sender, EventArgs e)
        {
            ExitBtn.BackgroundImage = Properties.Resources._button_exit;
            ExitBtn.ForeColor = SystemColors.ButtonHighlight;
            ExitBtn.FlatAppearance.BorderColor = Color.FromArgb(0, 255, 255, 255);
        }
        private void ExitBtn_MouseDown(object sender, MouseEventArgs e)
        {
            ExitBtn.BackgroundImage = Properties.Resources._button_highlight;
            ExitBtn.ForeColor = SystemColors.ButtonHighlight;
            ExitBtn.FlatAppearance.BorderColor = Color.FromArgb(0, 255, 255, 255);
        }
        private void ExitBtn_Click(object sender, EventArgs e)
        {
            Process[] wbByName = Process.GetProcessesByName("worldbuilder_ctr");
            if (wbByName.Length > 0)
            {
                Messages.GenerateMessageBox("W_WBCouldNotUnloadMod", Globals.currentLanguage);
            }
            this.Close(); // Application.Exit(); //OnApplicationExit(sender, e);
        }
        private void DonateBtn_MouseDown(object sender, MouseEventArgs e)
        {
            DonateBtn.BackgroundImage = Properties.Resources._button_vpn_highlight;
            DonateBtn.ForeColor = SystemColors.ButtonHighlight;
            DonateBtn.FlatAppearance.BorderColor = Color.FromArgb(0, 255, 255, 255);
        }
        private void DonateBtn_MouseEnter(object sender, EventArgs e)
        {
            DonateBtn.BackgroundImage = Properties.Resources._button_donate_tr;
            DonateBtn.ForeColor = SystemColors.ButtonHighlight;
            DonateBtn.FlatAppearance.BorderColor = Color.FromArgb(0, 255, 255, 255);
        }
        private void DonateBtn_MouseLeave(object sender, EventArgs e)
        {
            DonateBtn.BackgroundImage = Properties.Resources._button_donate;
            DonateBtn.ForeColor = SystemColors.ButtonHighlight;
            DonateBtn.FlatAppearance.BorderColor = Color.FromArgb(0, 255, 255, 255);
        }
        private void DonateBtn_Click(object sender, EventArgs e)
        {
            Url_open("https://www.paypal.com/paypalme2/Contramod");
        }

        private void onlineInstructionsLabel_MouseEnter(object sender, EventArgs e)
        {
            onlineInstructionsLabel.ForeColor = Color.FromArgb(255, 210, 100);
        }

        private void onlineInstructionsLabel_MouseLeave(object sender, EventArgs e)
        {
            onlineInstructionsLabel.ForeColor = Color.FromArgb(255, 255, 255);
        }
        private void onlineInstructionsLabel_Click_1(object sender, EventArgs e)
        {
            if (Globals.GB_Checked == true || Globals.DE_Checked == true)
            {
                Url_open("https://github.com/ContraMod/Launcher/blob/master/Online%20Instructions.md#ENGLISH-");
            }
            else if (Globals.RU_Checked == true || Globals.UA_Checked == true)
            {
                Url_open("https://github.com/ContraMod/Launcher/blob/master/Online%20Instructions.md#РУССКИЙ-");
            }
            else if (Globals.BG_Checked == true)
            {
                Url_open("https://github.com/ContraMod/Launcher/blob/master/Online%20Instructions.md");
            }
        }

        private void replaysLabel_MouseEnter(object sender, EventArgs e)
        {
            replaysLabel.ForeColor = Color.FromArgb(255, 210, 100);
        }
        private void replaysLabel_MouseLeave(object sender, EventArgs e)
        {
            replaysLabel.ForeColor = Color.FromArgb(255, 255, 255);
        }
        private void replaysLabel_Click(object sender, EventArgs e)
        {
            Url_open("https://www.gamereplays.org/cnczerohourcontra/replays.php?game=101&tab=popular&show=index&tab_new=upcoming&display_mode=standard");
        }

        private void customAddonsLabel_MouseEnter(object sender, EventArgs e)
        {
            customAddonsLabel.ForeColor = Color.FromArgb(255, 210, 100);
        }
        private void customAddonsLabel_MouseLeave(object sender, EventArgs e)
        {
            customAddonsLabel.ForeColor = Color.FromArgb(255, 255, 255);
        }
        private void customAddonsLabel_Click(object sender, EventArgs e)
        {
            if (Globals.GB_Checked == true || Globals.DE_Checked == true)
            {
                Url_open("https://github.com/ContraMod/Launcher/blob/master/Mod%20Addons.md#ENGLISH-");
            }
            else if (Globals.RU_Checked == true || Globals.UA_Checked == true)
            {
                Url_open("https://github.com/ContraMod/Launcher/blob/master/Mod%20Addons.md#РУССКИЙ-");
            }
            else if (Globals.BG_Checked == true)
            {
                Url_open("https://github.com/ContraMod/Launcher/blob/master/Mod%20Addons.md");
            }
        }

        private void supportLabel_MouseEnter(object sender, EventArgs e)
        {
            supportLabel.ForeColor = Color.FromArgb(255, 210, 100);
        }
        private void supportLabel_MouseLeave(object sender, EventArgs e)
        {
            supportLabel.ForeColor = Color.FromArgb(255, 255, 255);
        }
        private void supportLabel_Click(object sender, EventArgs e)
        {
            if (Globals.GB_Checked == true || Globals.DE_Checked == true)
            {
                Url_open("https://github.com/ContraMod/Launcher/blob/master/Mod%20Support.md#ENGLISH-");
            }
            else if (Globals.RU_Checked == true || Globals.UA_Checked == true)
            {
                Url_open("https://github.com/ContraMod/Launcher/blob/master/Mod%20Support.md#РУССКИЙ-");
            }
            else if (Globals.BG_Checked == true)
            {
                Url_open("https://github.com/ContraMod/Launcher/blob/master/Mod%20Support.md");
            }
        }

        private void CancelModDLBtn_Click(object sender, EventArgs e)
        {
            httpCancellationToken.Cancel();
            //PatchDLPanel.Hide();
            //wcMod.CancelAsync();
        }

        private static void DeleteDuplicateFiles()
        {
            List<string> filenames = new List<string>
                {
                    "!!!!!!!!Contra009Final_Patch3_Hotfix4",
                    "!!!!!!!Contra009Final_Patch3_Hotfix3_AI",
                    "!!!!!!!Contra009Final_Patch3_Hotfix3",
                    "!!!!!!Contra009Final_Patch3_Hotfix2",
                    "!!!!!!Contra009Final_Patch3_Hotfix_FunnyGenPics",
                    "!!!!!Contra009Final_Patch3_Hotfix_NatVO",
                    "!!!!!Contra009Final_Patch3_Hotfix",
                    "!!!!Contra009Final_Patch3_GameData",
                    "!!!!Contra009Final_Patch3",
                    "!!!!Contra009Final_Patch3_RU_Leikeze",
                    "!!!!Contra009Final_Patch3_RU_Legacy",
                    "!!!!Contra009Final_Patch3_EN_Leikeze",
                    "!!!!Contra009Final_Patch3_EN_Legacy",
                    "!!!!Contra009Final_Patch3_NatVO",
                    "!!!!Contra009Final_Patch3_EngVO",
                    "!!!Contra009Final_Patch2_GameData",
                    "!!!Contra009Final_Patch2",
                    "!!!Contra009Final_Patch2_RU",
                    "!!!Contra009Final_Patch2_EN",
                    "!!!Contra009Final_Patch2_NatVO",
                    "!!!Contra009Final_Patch2_EngVO",
                    "!!Contra009Final_Patch1",
                    "!!Contra009Final_Patch1_RU",
                    "!!Contra009Final_Patch1_EN",
                    "!!Contra009Final_Patch1_EngVO",
                    "!!Contra009Final_FogOff",
                    "!!Contra009Final_WaterEffectsOff",
                    "!!Contra009Final_FunnyGenPics",
                    "!Contra009Final",
                    "!Contra009Final_NatVO",
                    "!Contra009Final_EngVO",
                    "!Contra009Final_NewMusic",
                    "!Contra009Final_EN",
                    "!Contra009Final_RU",
                };
            foreach (string filename in filenames)
            {
                try
                {
                    DeleteDuplicateFile(filename);
                    if (File.Exists("langdata.dat") && File.Exists("langdata1.dat"))
                    {
                        File.Delete("langdata1.dat");
                    }
                }
                catch { }
            }
        }

        private static void DeleteDuplicateFile(string filename)
        {
            if (File.Exists(filename + ".ctr") && File.Exists(filename + ".big"))
            {
                File.Delete(filename + ".big");
            }
        }

        private static void RenameBigToCtr()
        {
            try
            {
                List<string> bigs = new List<string>
                {
                    "!!!!!!!!Contra009Final_Patch3_Hotfix4.big",
                    "!!!!!!!Contra009Final_Patch3_Hotfix3_AI.big",
                    "!!!!!!!Contra009Final_Patch3_Hotfix3.big",
                    "!!!!!!Contra009Final_Patch3_Hotfix2.big",
                    "!!!!!!Contra009Final_Patch3_Hotfix_FunnyGenPics.big",
                    "!!!!!Contra009Final_Patch3_Hotfix_NatVO.big",
                    "!!!!!Contra009Final_Patch3_Hotfix.big",
                    "!!!!Contra009Final_Patch3_GameData.big",
                    "!!!!Contra009Final_Patch3.big",
                    "!!!!Contra009Final_Patch3_RU_Leikeze.big",
                    "!!!!Contra009Final_Patch3_RU_Legacy.big",
                    "!!!!Contra009Final_Patch3_EN_Leikeze.big",
                    "!!!!Contra009Final_Patch3_EN_Legacy.big",
                    "!!!!Contra009Final_Patch3_NatVO.big",
                    "!!!!Contra009Final_Patch3_EngVO.big",
                    "!!!Contra009Final_Patch2_GameData.big",
                    "!!!Contra009Final_Patch2.big",
                    "!!!Contra009Final_Patch2_RU.big",
                    "!!!Contra009Final_Patch2_EN.big",
                    "!!!Contra009Final_Patch2_NatVO.big",
                    "!!!Contra009Final_Patch2_EngVO.big",
                    "!!Contra009Final_Patch1.big",
                    "!!Contra009Final_Patch1_RU.big",
                    "!!Contra009Final_Patch1_EN.big",
                    "!!Contra009Final_Patch1_EngVO.big",
                    "!!Contra009Final_FogOff.big",
                    "!!Contra009Final_WaterEffectsOff.big",
                    "!!Contra009Final_FunnyGenPics.big",
                    "!Contra009Final.big",
                    "!Contra009Final_NatVO.big",
                    "!Contra009Final_EngVO.big",
                    "!Contra009Final_NewMusic.big",
                    "!Contra009Final_EN.big",
                    "!Contra009Final_RU.big",
                };
                foreach (string big in bigs)
                {
                    string ctr = big.Replace(".big", ".ctr");
                    try
                    {
                        File.Move(big, ctr);
                    }
                    catch { }
                }
                if (File.Exists("langdata1.dat"))
                {
                    File.Move("langdata1.dat", "langdata.dat");
                }
                if (Directory.Exists(@"Data\Scripts1"))
                {
                    Directory.Move(@"Data\Scripts1", @"Data\Scripts");
                }

                if (File.Exists("Install_Final_ZH.bmp"))
                {
                    try
                    {
                        File.SetAttributes("Install_Final.bmp", FileAttributes.Normal);
                        File.SetAttributes("Install_Final_ZH.bmp", FileAttributes.Normal);
                        File.SetAttributes("Install_Final_Contra.bmp", FileAttributes.Normal);
                        File.Copy("Install_Final_ZH.bmp", "Install_Final.bmp", true);
                    }
                    catch
                    { }
                }

                if (File.Exists("generals_zh.exe"))
                {
                    try
                    {
                        File.SetAttributes("generals.exe", FileAttributes.Normal);
                        File.SetAttributes("generals_zh.exe", FileAttributes.Normal);
                        File.SetAttributes("generals.ctr", FileAttributes.Normal);
                        File.Copy("generals_zh.exe", "generals.exe", true);
                    }
                    catch
                    { }
                }
            }
            catch
            { }
        }

        public bool wbRunningDialogResultYes()
        {
            Process[] wbByName = Process.GetProcessesByName("worldbuilder_ctr");
            if (wbByName.Length > 0)
            {
                DialogResult dialogResult = MessageBox.Show(Messages.GenerateMessage("W_PreferencesMayNotLoad", Globals.currentLanguage), Messages.GenerateMessage("Warning", Globals.currentLanguage), MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (dialogResult == DialogResult.No)
                {
                    return false;
                }
                else return true;
            }
            return true;
        }

        private void LaunchBtn_Click(object sender, EventArgs e)
        {
            DeleteDuplicateFiles();
            RenameBigToCtr();

            try
            {
                try
                {
                    List<string> ctrs = new List<string>
                    {
                        "!Contra009Final.ctr",
                        "!!Contra009Final_Patch1.ctr",
                        "!!!Contra009Final_Patch2.ctr",
                        "!!!Contra009Final_Patch2_GameData.ctr",
                        "!!!!Contra009Final_Patch3.ctr",
                        "!!!!Contra009Final_Patch3_GameData.ctr",
                        "!!!!!Contra009Final_Patch3_Hotfix.ctr",
                        "!!!!!!Contra009Final_Patch3_Hotfix2.ctr",
                        "!!!!!!!Contra009Final_Patch3_Hotfix3.ctr",
                        "!!!!!!!Contra009Final_Patch3_Hotfix3_AI.ctr",
                        "!!!!!!!!Contra009Final_Patch3_Hotfix4.ctr",
                    };
                    foreach (string ctr in ctrs)
                    {
                        string big = ctr.Replace(".ctr", ".big");
                        try
                        {
                            File.Move(ctr, big);
                        }
                        catch { }
                    }

                    // Remove dbghelp to fix DirectX error on game startup.
                    File.Delete("dbghelp.dll");
                    File.Delete("dbghelp.ctr");
                    File.Delete("dbghelp.backup");
                }
                catch { }

                // TODO: Find a place for this:
                //catch (FileNotFoundException ex)
                //{
                //    var text = new Dictionary<Tuple<string, string>, bool>
                //{
                //    { Tuple.Create(ex.Message + "\n\nThis means that you are launching the mod with missing files, or an older version of the mod, and there will be errors or mismatch issues in online games.\n\nWould you like to start the game anyway?", "Warning"), Globals.GB_Checked},
                //    { Tuple.Create(ex.Message + "\n\nЭто означает, что вы запускаете мод с отсутствующими файлами или более старую версию мода, и в онлайн-играх будут ошибки или проблемы с несоответствием.\n\nХотели бы вы начать игру?", "Предупреждение"), Globals.RU_Checked},
                //    { Tuple.Create(ex.Message + "\n\nЦе означає, що ви запускаєте мод з відсутніми файлами або старішою версією мода, і в онлайн-іграх будуть помилки або проблеми з невідповідністю.\n\nВи хочете все-таки почати гру?", "Попередження"), Globals.UA_Checked},
                //    { Tuple.Create(ex.Message + "\n\nТова означава, че стартирате мода с липсващи файлове или по-стара версия на мода и ще има грешки или несъответствие в онлайн игрите.\n\nЖелаете ли да стартирате играта въпреки това?", "Предупреждение"), Globals.BG_Checked},
                //    { Tuple.Create(ex.Message + "\n\nDas bedeutet, dass Sie den Mod mit fehlenden Dateien oder einer älteren Version des Mods starten und es in Online-Spielen zu Fehlern oder Nichtübereinstimmungsproblemen kommt.\n\nMöchten Sie das Spiel trotzdem starten?", "Warnung"), Globals.DE_Checked},
                //}.Single(l => l.Value).Key;

                //    DialogResult dialogResult = MessageBox.Show(new Form { TopMost = true }, text.Item1, text.Item2, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                //    if (dialogResult == DialogResult.Yes) { }
                //    else { return; }
                //}

                if ((RadioOrigQuotes.Checked) && (File.Exists("!Contra009Final_NatVO.ctr")))
                {
                    File.Move("!Contra009Final_NatVO.ctr", "!Contra009Final_NatVO.big");
                }
                if ((RadioLocQuotes.Checked) && (File.Exists("!Contra009Final_EngVO.ctr")))
                {
                    File.Move("!Contra009Final_EngVO.ctr", "!Contra009Final_EngVO.big");
                }
                if ((RadioLocQuotes.Checked) && (File.Exists("!!Contra009Final_Patch1_EngVO.ctr")))
                {
                    File.Move("!!Contra009Final_Patch1_EngVO.ctr", "!!Contra009Final_Patch1_EngVO.big");
                }
                if ((RadioOrigQuotes.Checked) && (File.Exists("!!!Contra009Final_Patch2_NatVO.ctr")))
                {
                    File.Move("!!!Contra009Final_Patch2_NatVO.ctr", "!!!Contra009Final_Patch2_NatVO.big");
                }
                if ((RadioLocQuotes.Checked) && (File.Exists("!!!Contra009Final_Patch2_EngVO.ctr")))
                {
                    File.Move("!!!Contra009Final_Patch2_EngVO.ctr", "!!!Contra009Final_Patch2_EngVO.big");
                }
                if ((RadioOrigQuotes.Checked) && (File.Exists("!!!!!Contra009Final_Patch3_Hotfix_NatVO.ctr")))
                {
                    File.Move("!!!!!Contra009Final_Patch3_Hotfix_NatVO.ctr", "!!!!!Contra009Final_Patch3_Hotfix_NatVO.big");
                }
                if ((RadioOrigQuotes.Checked) && (File.Exists("!!!!Contra009Final_Patch3_NatVO.ctr")))
                {
                    File.Move("!!!!Contra009Final_Patch3_NatVO.ctr", "!!!!Contra009Final_Patch3_NatVO.big");
                }
                if ((RadioLocQuotes.Checked) && (File.Exists("!!!!Contra009Final_Patch3_EngVO.ctr")))
                {
                    File.Move("!!!!Contra009Final_Patch3_EngVO.ctr", "!!!!Contra009Final_Patch3_EngVO.big");
                }
                if ((RadioEN.Checked) && (File.Exists("!Contra009Final_EN.ctr")))
                {
                    File.Move("!Contra009Final_EN.ctr", "!Contra009Final_EN.big");
                }
                if ((RadioEN.Checked) && (File.Exists("!!Contra009Final_Patch1_EN.ctr")))
                {
                    File.Move("!!Contra009Final_Patch1_EN.ctr", "!!Contra009Final_Patch1_EN.big");
                }
                if ((RadioEN.Checked) && (File.Exists("!!!Contra009Final_Patch2_EN.ctr")))
                {
                    File.Move("!!!Contra009Final_Patch2_EN.ctr", "!!!Contra009Final_Patch2_EN.big");
                }
                if ((RadioEN.Checked) && (Properties.Settings.Default.LeikezeHotkeys == true) && (File.Exists("!!!!Contra009Final_Patch3_EN_Leikeze.ctr")))
                {
                    File.Move("!!!!Contra009Final_Patch3_EN_Leikeze.ctr", "!!!!Contra009Final_Patch3_EN_Leikeze.big");
                }
                else if ((RadioEN.Checked) && (Properties.Settings.Default.LegacyHotkeys == true) && (File.Exists("!!!!Contra009Final_Patch3_EN_Legacy.ctr")))
                {
                    File.Move("!!!!Contra009Final_Patch3_EN_Legacy.ctr", "!!!!Contra009Final_Patch3_EN_Legacy.big");
                }
                if ((RadioRU.Checked) && (File.Exists("!Contra009Final_RU.ctr")))
                {
                    File.Move("!Contra009Final_RU.ctr", "!Contra009Final_RU.big");
                }
                if ((RadioRU.Checked) && (File.Exists("!!Contra009Final_Patch1_RU.ctr")))
                {
                    File.Move("!!Contra009Final_Patch1_RU.ctr", "!!Contra009Final_Patch1_RU.big");
                }
                if ((RadioRU.Checked) && (File.Exists("!!!Contra009Final_Patch2_RU.ctr")))
                {
                    File.Move("!!!Contra009Final_Patch2_RU.ctr", "!!!Contra009Final_Patch2_RU.big");
                }
                if ((RadioRU.Checked) && (Properties.Settings.Default.LeikezeHotkeys == true) && (File.Exists("!!!!Contra009Final_Patch3_RU_Leikeze.ctr")))
                {
                    File.Move("!!!!Contra009Final_Patch3_RU_Leikeze.ctr", "!!!!Contra009Final_Patch3_RU_Leikeze.big");
                }
                else if ((RadioRU.Checked) && (Properties.Settings.Default.LegacyHotkeys == true) && (File.Exists("!!!!Contra009Final_Patch3_RU_Legacy.ctr")))
                {
                    File.Move("!!!!Contra009Final_Patch3_RU_Legacy.ctr", "!!!!Contra009Final_Patch3_RU_Legacy.big");
                }
                if ((MNew.Checked) && (File.Exists("!Contra009Final_NewMusic.ctr")))
                {
                    File.Move("!Contra009Final_NewMusic.ctr", "!Contra009Final_NewMusic.big");
                }
                if ((Properties.Settings.Default.Fog == false) && (File.Exists("!!Contra009Final_FogOff.ctr")))
                {
                    File.Move("!!Contra009Final_FogOff.ctr", "!!Contra009Final_FogOff.big");
                }
                else if ((Properties.Settings.Default.Fog == true) && (File.Exists("!!Contra009Final_FogOff.big")))
                {
                    File.Move("!!Contra009Final_FogOff.big", "!!Contra009Final_FogOff.ctr");
                }
                if ((Properties.Settings.Default.WaterEffects == false) && (File.Exists("!!Contra009Final_WaterEffectsOff.ctr")))
                {
                    File.Move("!!Contra009Final_WaterEffectsOff.ctr", "!!Contra009Final_WaterEffectsOff.big");
                }
                else if ((Properties.Settings.Default.WaterEffects == true) && (File.Exists("!!Contra009Final_WaterEffectsOff.big")))
                {
                    File.Move("!!Contra009Final_WaterEffectsOff.big", "!!Contra009Final_WaterEffectsOff.ctr");
                }
                if ((GoofyPics.Checked) && (File.Exists("!!Contra009Final_FunnyGenPics.ctr")))
                {
                    File.Move("!!Contra009Final_FunnyGenPics.ctr", "!!Contra009Final_FunnyGenPics.big");
                }
                else if ((!GoofyPics.Checked) && (File.Exists("!!Contra009Final_FunnyGenPics.big")))
                {
                    File.Move("!!Contra009Final_FunnyGenPics.big", "!!Contra009Final_FunnyGenPics.ctr");
                }
                if ((GoofyPics.Checked) && (File.Exists("!!!!!!Contra009Final_Patch3_Hotfix_FunnyGenPics.ctr")))
                {
                    File.Move("!!!!!!Contra009Final_Patch3_Hotfix_FunnyGenPics.ctr", "!!!!!!Contra009Final_Patch3_Hotfix_FunnyGenPics.big");
                }
                else if ((!GoofyPics.Checked) && (File.Exists("!!!!!!Contra009Final_Patch3_Hotfix_FunnyGenPics.big")))
                {
                    File.Move("!!!!!!Contra009Final_Patch3_Hotfix_FunnyGenPics.big", "!!!!!!Contra009Final_Patch3_Hotfix_FunnyGenPics.ctr");
                }
                if ((Properties.Settings.Default.LangF == false) && (File.Exists("langdata.dat")))
                {
                    File.Move("langdata.dat", "langdata1.dat");
                }
                else if ((Properties.Settings.Default.LangF == true) && (File.Exists("langdata1.dat")))
                {
                    File.Move("langdata1.dat", "langdata.dat");
                }
                if (Directory.Exists(@"Data\Scripts"))
                {
                    int scripts = Directory.GetFiles(@"Data\Scripts").Length;
                    if (scripts == 0)
                    {
                        Directory.Delete(@"Data\Scripts");
                    }
                }
                if (Directory.Exists(@"Data\Scripts1"))
                {
                    int scripts1 = Directory.GetFiles(@"Data\Scripts1").Length;
                    if (scripts1 == 0)
                    {
                        Directory.Delete(@"Data\Scripts1");
                    }
                }
                if (Directory.Exists(@"Data\Scripts"))
                {
                    Directory.Move(@"Data\Scripts", @"Data\Scripts1");
                }
                if (File.Exists("Install_Final.bmp") && (File.Exists("Install_Final_Contra.bmp")))
                {
                    try
                    {
                        File.SetAttributes("Install_Final.bmp", FileAttributes.Normal);
                        if (File.Exists("Install_Final_ZH"))
                        {
                            File.SetAttributes("Install_Final_ZH.bmp", FileAttributes.Normal);
                        }
                        File.SetAttributes("Install_Final_Contra.bmp", FileAttributes.Normal);
                        File.Copy("Install_Final.bmp", "Install_Final_ZH.bmp", true);
                        File.Copy("Install_Final_Contra.bmp", "Install_Final.bmp", true);
                    }
                    catch
                    { }
                }

                // Make CTR Options.ini active
                try
                {
                    if (File.Exists(Globals.myDocPath + "Options_CTR.ini"))
                    {
                        File.SetAttributes(Globals.myDocPath + "Options.ini", FileAttributes.Normal);
                        File.SetAttributes(Globals.myDocPath + "Options_CTR.ini", FileAttributes.Normal);
                        File.SetAttributes(Globals.myDocPath + "Options_ZH.ini", FileAttributes.Normal);
                        File.Copy(Globals.myDocPath + "Options.ini", Globals.myDocPath + "Options_ZH.ini", true);
                        File.Copy(Globals.myDocPath + "Options_CTR.ini", Globals.myDocPath + "Options.ini", true);
                    }
                }
                catch
                { }

                // Disable cyrillic letters, enable German umlauts.
                if (File.Exists("GermanZH.big") && File.Exists("GenArial.ttf"))
                {
                    File.Move("GenArial.ttf", "GenArial_.ttf");
                }

                // Check for generals.ctr
                if (!File.Exists("generals.ctr") || CalculateMD5("generals.ctr") != "ee7d5e6c2d7fb66f5c27131f33da5fd3")
                {
                    DialogResult dialogResult = MessageBox.Show(Messages.GenerateMessage("W_NotFound_GeneralsCTR", Globals.currentLanguage), Messages.GenerateMessage("Warning", Globals.currentLanguage), MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (dialogResult == DialogResult.Yes)
                    {
                        StartGenerals();
                    }
                }
                else
                {
                    StartGenerals();
                }
            }
            catch
            { }
            return;
        }

        public void StartGenerals()
        {
            // Check for .dll files
            if (!File.Exists("binkw32.dll") || (!File.Exists("mss32.dll")))
            {
                DialogResult dialogResult = MessageBox.Show(Messages.GenerateMessage("E_NotFound_DLLs", Globals.currentLanguage), Messages.GenerateMessage("Error", Globals.currentLanguage), MessageBoxButtons.YesNoCancel, MessageBoxIcon.Error);
                if (dialogResult == DialogResult.Yes)
                {
                    Url_open("https://github.com/ContraMod/Launcher/blob/master/Online%20Instructions.md");
                }
                return;
            }

            // Rename generals.exes
            if (File.Exists("generals.exe") && (File.Exists("generals.ctr")))
            {
                try
                {
                    File.SetAttributes("generals.exe", FileAttributes.Normal);
                    if (File.Exists("generals_zh.exe"))
                    {
                        File.SetAttributes("generals_zh.exe", FileAttributes.Normal);
                    }
                    File.SetAttributes("generals.ctr", FileAttributes.Normal);
                    File.Copy("generals.exe", "generals_zh.exe", true);
                    File.Copy("generals.ctr", "generals.exe", true);
                }
                catch
                { }
            }

            if (File.Exists("generals.exe"))
            {
                if (wbRunningDialogResultYes() == true)
                {
                    Process generals = new Process();
                    generals.StartInfo.FileName = "generals.exe";

                    if (WinCheckBox.Checked == false && QSCheckBox.Checked == false)
                    {
                        //no start arguments
                    }
                    else if (QSCheckBox.Checked && WinCheckBox.Checked == false)
                    {
                        generals.StartInfo.Arguments = "-quickstart -nologo";
                    }
                    else if (WinCheckBox.Checked && QSCheckBox.Checked)
                    {
                        generals.StartInfo.Arguments = "-win -quickstart -nologo";
                    }
                    else //if (WinCheckBox.Checked && QSCheckBox.Checked == false)
                    {
                        generals.StartInfo.Arguments = "-win";
                    }

                    generals.EnableRaisingEvents = true;
                    generals.Exited += (sender1, e1) =>
                    {
                        WindowState = FormWindowState.Normal;
                    };
                    generals.StartInfo.WorkingDirectory = Path.GetDirectoryName("generals.exe");
                    WindowState = FormWindowState.Minimized;
                    try
                    {
                        generals.Start();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error");
                    }
                }
            }
            else
            {
                CheckInstallDir();
            }
        }

        internal static bool Url_open(string url)
        {
            try
            {
                Process.Start(url);
                return true;
            }
            catch
            {
                try
                {
                    Process.Start("IExplore.exe", url);
                    return true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Could not use your default browser to open URL:\n" + url + "\n\n" + ex.Message, "Opening link failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.UpgradeRequired)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpgradeRequired = false;
                Properties.Settings.Default.Save();
            }
            RadioEN.Checked = Properties.Settings.Default.LangEN;
            RadioRU.Checked = Properties.Settings.Default.LangRU;
            MNew.Checked = Properties.Settings.Default.MusicNew;
            MStandard.Checked = Properties.Settings.Default.MusicStandard;
            RadioOrigQuotes.Checked = Properties.Settings.Default.VoNew;
            RadioLocQuotes.Checked = Properties.Settings.Default.VoStandard;
            QSCheckBox.Checked = Properties.Settings.Default.Quickstart;
            WinCheckBox.Checked = Properties.Settings.Default.Windowed;
            DefaultPics.Checked = Properties.Settings.Default.GenPicDef;
            GoofyPics.Checked = Properties.Settings.Default.GenPicGoo;
            RadioFlag_GB.Checked = Properties.Settings.Default.Flag_GB;
            RadioFlag_RU.Checked = Properties.Settings.Default.Flag_RU;
            RadioFlag_UA.Checked = Properties.Settings.Default.Flag_UA;
            RadioFlag_BG.Checked = Properties.Settings.Default.Flag_BG;
            RadioFlag_DE.Checked = Properties.Settings.Default.Flag_DE;
            AutoScaleMode = AutoScaleMode.Dpi;
        }

        private void OnApplicationExit(object sender, EventArgs e)
        {
            DeleteDuplicateFiles();
            RenameBigToCtr();

            // Save CTR Options; Make ZH Options.ini active
            try
            {
                if (File.Exists(Globals.myDocPath + "Options_ZH.ini"))
                {
                    File.SetAttributes(Globals.myDocPath + "Options.ini", FileAttributes.Normal);
                    File.SetAttributes(Globals.myDocPath + "Options_CTR.ini", FileAttributes.Normal);
                    File.SetAttributes(Globals.myDocPath + "Options_ZH.ini", FileAttributes.Normal);
                    File.Copy(Globals.myDocPath + "Options.ini", Globals.myDocPath + "Options_CTR.ini", true);
                    File.Copy(Globals.myDocPath + "Options_ZH.ini", Globals.myDocPath + "Options.ini", true);
                }
            }
            catch { }

            Properties.Settings.Default.LangEN = RadioEN.Checked;
            Properties.Settings.Default.LangRU = RadioRU.Checked;
            Properties.Settings.Default.MusicNew = MNew.Checked;
            Properties.Settings.Default.MusicStandard = MStandard.Checked;
            Properties.Settings.Default.VoNew = RadioOrigQuotes.Checked;
            Properties.Settings.Default.VoStandard = RadioLocQuotes.Checked;
            Properties.Settings.Default.Quickstart = QSCheckBox.Checked;
            Properties.Settings.Default.Windowed = WinCheckBox.Checked;
            Properties.Settings.Default.GenPicDef = DefaultPics.Checked;
            Properties.Settings.Default.GenPicGoo = GoofyPics.Checked;
            Properties.Settings.Default.Flag_GB = RadioFlag_GB.Checked;
            Properties.Settings.Default.Flag_RU = RadioFlag_RU.Checked;
            Properties.Settings.Default.Flag_UA = RadioFlag_UA.Checked;
            Properties.Settings.Default.Flag_BG = RadioFlag_BG.Checked;
            Properties.Settings.Default.Flag_DE = RadioFlag_DE.Checked;
            Properties.Settings.Default.Save();

            DelTmpChunk();

            this.Close();
        }

        public static void SetFirewallExcemption(string exePath)
        {
            // Full path in rule name is ugly, let's only show filename instead
            string ExeWithoutPath = exePath;
            int idx = exePath.LastIndexOf(@"\");
            if (idx != -1) ExeWithoutPath = exePath.Substring(idx + 1);

            // Check if rule with same name exists
            var netsh = new Process();
            netsh.StartInfo.FileName = "netsh.exe";
            netsh.StartInfo.CreateNoWindow = true;
            netsh.StartInfo.UseShellExecute = false;
            netsh.StartInfo.Arguments = $"advfirewall firewall show rule name=\"Contra - \"{ExeWithoutPath}\"";
            netsh.Start();
            netsh.WaitForExit();

            // Add new firewall excemption rule if missing
            if (netsh.ExitCode != 0)
            {
                netsh.StartInfo.Arguments = $"advfirewall firewall add rule name=\"Contra - {ExeWithoutPath}\" dir=in action=allow program=\"{Environment.CurrentDirectory}\\{exePath}\" protocol=tcp profile=private,public edge=yes enable=yes";
                netsh.Start();

                Process netsh2 = new Process();
                netsh2.StartInfo = netsh.StartInfo;
                netsh2.StartInfo.Arguments = $"advfirewall firewall add rule name=\"Contra - {exePath}\" dir=in action=allow program=\"{Environment.CurrentDirectory}\\{exePath}\" protocol=udp profile=private,public edge=yes enable=yes";
                netsh2.Start();

                netsh.WaitForExit();
                netsh2.WaitForExit();
            }
        }

        public static void CheckFirewallExceptions()
        {
            // All executables which need listening ports open
            ReadOnlyCollection<string> exes = Array.AsReadOnly(new[] {
                "game.dat",
                "generals.exe",
            });

            // Check if all files exist first before attempting to add any rules
            bool allFilesExist = exes.All(file => File.Exists(Environment.CurrentDirectory + @"\" + file));
            if (allFilesExist) foreach (string exe in exes) SetFirewallExcemption(exe);
        }

        public string GetCurrentCulture()
        {
            var culture = CultureInfo.CurrentCulture;
            string cultureStr = culture.ToString();
            return cultureStr;
        }

        public static class ThreadHelperClass
        {
            delegate void SetTextCallback(Form f, Control ctrl, string text);
            /// <summary>
            /// Set text property of various controls
            /// </summary>
            /// <param name="form">The calling form</param>
            /// <param name="ctrl"></param>
            /// <param name="text"></param>
            public static void SetText(Form form, Control ctrl, string text)
            {
                // InvokeRequired required compares the thread ID of the 
                // calling thread to the thread ID of the creating thread. 
                // If these threads are different, it returns true. 
                if (ctrl.InvokeRequired)
                {
                    SetTextCallback d = new SetTextCallback(SetText);
                    form.Invoke(d, new object[] { form, ctrl, text });
                }
                else
                {
                    ctrl.Text = text;
                }
            }
        }

        bool downloadTextFile = false;
        bool seekForUpdate = true;

        // This method is executed on the worker thread and makes 
        // a thread-safe call on the TextBox control. 
        private void ThreadProcSafeMOTD(string versionsTXT)
        {
            try
            {
                {
                    if (downloadTextFile == false)
                    {
                        //Check for launcher update once per launch.
                        if (seekForUpdate == true)
                        {
                            seekForUpdate = false;
                            //GetLauncherUpdate(versionsTXT, launcher_url);
                            //GetModUpdate(versionsTXT, patch_url);
                        }
                        downloadTextFile = true;
                    }
                    void SetMOTD(string prefix)
                    {
                        string MOTDText = versionsTXT.Substring(versionsTXT.LastIndexOf(prefix) + 9);
                        string MOTDText2 = MOTDText.Substring(0, MOTDText.IndexOf("$"));
                        ThreadHelperClass.SetText(this, MOTD, MOTDText2);
                    }

                    var versionsTXT_lang = new Dictionary<string, bool>
                    {
                        {"MOTD-EN: ", Globals.GB_Checked},
                        {"MOTD-RU: ", Globals.RU_Checked},
                        {"MOTD-UA: ", Globals.UA_Checked},
                        {"MOTD-BG: ", Globals.BG_Checked},
                        {"MOTD-DE: ", Globals.DE_Checked},
                    };
                    SetMOTD(versionsTXT_lang.Single(l => l.Value).Key);
                }
            }
            catch { }
        }

        void gtwc_DownloadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            //Extract zip
            string zipPath = launcherExecutingPath + @"\" + genToolFileName;

            try //To prevent crash
            {
                using (ZipArchive archive = ZipFile.OpenRead(zipPath))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries.Where(a => a.FullName.Contains("d3d8.dll")))
                    {
                        entry.ExtractToFile(Path.Combine(launcherExecutingPath, entry.FullName), true);
                    }
                }
            }
            catch { }
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.ToString());
            //}
            try
            {
                File.Delete(genToolFileName);
            }
            catch { }

            //Show a message when the patch download has completed
            var gtLangText = new Dictionary<Tuple<string, string>, bool>
                {
                    { Tuple.Create("A new version of Gentool has been downloaded!", "Gentool update Complete"), Globals.GB_Checked},
                    { Tuple.Create("Новая версия GenTool был загружен!", "Gentool обновление завершено"), Globals.RU_Checked},
                    { Tuple.Create("Новий GenTool завантажено!", "Оновлення GenTool завершено"), Globals.UA_Checked},
                    { Tuple.Create("Нова версия на GenTool беше изтегленa!", "Обновяването на GenTool е завършено"), Globals.BG_Checked},
                    { Tuple.Create("Ein neuer GenTool wurde heruntergeladen!", "Aktualisierung GenTool abgeschlossen"), Globals.DE_Checked},
                }.Single(l => l.Value).Key;
            MessageBox.Show(new Form { TopMost = true }, gtLangText.Item1, gtLangText.Item2, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public void DownloadGentool(string url)
        {
            try
            {
                WebClient gtwc = new WebClient();
                gtwc.DownloadFileCompleted += new AsyncCompletedEventHandler(gtwc_DownloadCompleted);

                //CheckIfFileIsAvailable(url);
                //gtwc.OpenRead(url + genToolFileName);
                //bytes_total = Convert.ToInt64(gtwc.ResponseHeaders["Content-Length"]);

                gtwc.DownloadFileAsync(new Uri(url), launcherExecutingPath + @"\" + genToolFileName);
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
        }

        public static Tuple<int, int> getScreenResolution() => Tuple.Create(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
        readonly int ScreenResolutionX = getScreenResolution().Item1;
        readonly int ScreenResolutionY = getScreenResolution().Item2;

        public void CreateOptionsINI()
        {
            try
            {
                using (FileStream fs = File.Create(Globals.myDocPath + @"\Options.ini"))
                {
                    byte[] info = new UTF8Encoding(true).GetBytes(
                        "IdealStaticGameLOD = High" +
                        Environment.NewLine +
                        "Resolution = " + ScreenResolutionX + " " + ScreenResolutionY +
                        Environment.NewLine +
                        "BuildingOcclusion = Yes" +
                        Environment.NewLine +
                        "DynamicLOD = Yes" +
                        Environment.NewLine +
                        "ExtraAnimations = Yes" +
                        Environment.NewLine +
                        "HeatEffects = No" +
                        Environment.NewLine +
                        "ShowSoftWaterEdge = Yes" +
                        Environment.NewLine +
                        "ShowTrees = Yes" +
                        Environment.NewLine +
                        "StaticGameLOD = Custom" +
                        Environment.NewLine +
                        "TextureReduction = 0" +
                        Environment.NewLine +
                        "UseCloudMap = Yes" +
                        Environment.NewLine +
                        "UseLightMap = Yes" +
                        Environment.NewLine +
                        "UseShadowDecals = Yes" +
                        Environment.NewLine +
                        "UseShadowVolumes = Yes");
                    fs.Write(info, 0, info.Length);
                }
            }
            catch { }
        }

        static string CalculateMD5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        public string AspectRatio(int x, int y)
        {
            double value = (double)x / y;
            if (value > 1.7)
                return "16:9";
            else
                return "4:3";
        }

        private void ChangeCamHeight()
        {
            if (File.Exists("!!!!Contra009Final_Patch3.big") || File.Exists("!!!!Contra009Final_Patch3.ctr"))
            {
                if (File.Exists("!!!!Contra009Final_Patch3_GameData.big"))
                {
                    Encoding encoding = Encoding.GetEncoding("windows-1252");
                    var regex = Regex.Replace(File.ReadAllText("!!!!Contra009Final_Patch3_GameData.big", encoding), "  MaxCameraHeight = .*\r?\n", "  MaxCameraHeight = 282.0 ;350.0\r\n");
                    File.WriteAllText("!!!!Contra009Final_Patch3_GameData.big", regex, encoding);
                }
                else
                {
                    Messages.GenerateMessageBox("E_NotFound_GameDataP3", Globals.currentLanguage);
                }
            }
            else if (File.Exists("!!!Contra009Final_Patch2.big") || File.Exists("!!!Contra009Final_Patch2.ctr"))
            {
                if (File.Exists("!!!Contra009Final_Patch2_GameData.big"))
                {
                    Encoding encoding = Encoding.GetEncoding("windows-1252");
                    var regex = Regex.Replace(File.ReadAllText("!!!Contra009Final_Patch2_GameData.big"), "  MaxCameraHeight = .*\r?\n", "  MaxCameraHeight = 282.0 ;350.0\r\n");
                    string read = File.ReadAllText("!!!Contra009Final_Patch2_GameData.big", encoding);
                    File.WriteAllText("!!!Contra009Final_Patch2_GameData.big", regex, encoding);
                }
                else
                {
                    Messages.GenerateMessageBox("E_NotFound_GameDataP2", Globals.currentLanguage);
                }
            }
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            // Temporary hack so update runs on main thread, versionsTXT should be rewritten to be async if possible
            try
            {
                UpdateLogic();
            }
            catch { }

            string gtHash = null;
            try
            {
                gtHash = CalculateMD5("d3d8.dll");
            }
            catch { }

            if (isGentoolInstalled("d3d8.dll") && isGentoolOutdated("d3d8.dll", 79) || (gtHash == "70c28745f6e9a9a59cfa1be00df6836a" || gtHash == "13a13584d97922de92443631931d46c3"))
            {
                //try
                //{
                //    {
                //        System.Threading.Thread demoThread =
                //           new System.Threading.Thread(new System.Threading.ThreadStart(ThreadProcSafeGentool));
                //        demoThread.Start();
                //    }
                //}
                //catch (Exception ex) { MessageBox.Show(ex.ToString()); }

                try
                {
                    using (WebClient client = new WebClient())
                    {
                        genToolFileName = client.DownloadString("http://www.gentool.net/download/patch");
                        genToolFileName = genToolFileName.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)[1];

                        //MessageBox.Show(genToolFileName);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                    return;
                }

                string gtURL = "http://www.gentool.net/download/" + genToolFileName;
                DownloadGentool(gtURL);
            }

            // Cleanup old Launcher file after update
            if (File.Exists(launcherExecutingPath + @"\Contra_Launcher_ToDelete.exe"))
            {
                File.SetAttributes("Contra_Launcher_ToDelete.exe", FileAttributes.Normal);
                File.Delete(launcherExecutingPath + @"\Contra_Launcher_ToDelete.exe");
            }

            // Generate Options.ini if missing.
            if (!File.Exists(Globals.myDocPath + "Options.ini"))
            {
                CreateOptionsINI();
            }
            // If Options.ini is present but any of the graphical entries are missing, delete the current Options.ini and generate a new one.
            else
            {
                string OptionsText = File.ReadAllText(Globals.myDocPath + "Options.ini");
                if (!OptionsText.Contains("StaticGameLOD") ||
                    !OptionsText.Contains("UseShadowVolumes") ||
                    !OptionsText.Contains("UseShadowDecals") ||
                    !OptionsText.Contains("UseCloudMap") ||
                    !OptionsText.Contains("UseLightMap") ||
                    !OptionsText.Contains("ShowSoftWaterEdge") ||
                    !OptionsText.Contains("BuildingOcclusion") ||
                    !OptionsText.Contains("ShowTrees") ||
                    !OptionsText.Contains("ExtraAnimations") ||
                    !OptionsText.Contains("DynamicLOD") ||
                    !OptionsText.Contains("HeatEffects"))
                {
                    try
                    {
                        File.Delete(Globals.myDocPath + "Options.ini");
                        CreateOptionsINI();
                        File.SetAttributes(Globals.myDocPath + "Options.ini", FileAttributes.Normal);
                        File.SetAttributes(Globals.myDocPath + "Options_CTR.ini", FileAttributes.Normal);
                        File.Copy(Globals.myDocPath + "Options.ini", Globals.myDocPath + "Options_CTR.ini", true);
                    }
                    catch { }
                }
            }

            // Make 2 copies of Options.ini, name them Options_ZH.ini and Options_CTR.ini
            if (File.Exists(Globals.myDocPath + "Options.ini") && !File.Exists(Globals.myDocPath + "Options_ZH.ini") && !File.Exists(Globals.myDocPath + "Options_CTR.ini"))
            {
                File.SetAttributes(Globals.myDocPath + "Options.ini", FileAttributes.Normal);
                File.Copy(Globals.myDocPath + "Options.ini", Globals.myDocPath + "Options_ZH.ini", true);
                File.Copy(Globals.myDocPath + "Options.ini", Globals.myDocPath + "Options_CTR.ini", true);
            }

            // Make CTR Options.ini active
            try
            {
                if (File.Exists(Globals.myDocPath + "Options_CTR.ini"))
                {
                    File.SetAttributes(Globals.myDocPath + "Options.ini", FileAttributes.Normal);
                    File.SetAttributes(Globals.myDocPath + "Options_CTR.ini", FileAttributes.Normal);
                    File.SetAttributes(Globals.myDocPath + "Options_ZH.ini", FileAttributes.Normal);
                    File.Copy(Globals.myDocPath + "Options.ini", Globals.myDocPath + "Options_ZH.ini", true);
                    File.Copy(Globals.myDocPath + "Options_CTR.ini", Globals.myDocPath + "Options.ini", true);
                }
            }
            catch
            { }

            // Actions taken on first launcher run.
            if (Properties.Settings.Default.FirstRun)
            {
                try
                {
                    // Remove dbghelp to fix DirectX error on game startup.
                    File.Delete("dbghelp.dll");
                    File.Delete("dbghelp.ctr");
                    File.Delete("dbghelp.backup");
                }
                catch
                { }

                // Enable GameData
                if (File.Exists("!!!!Contra009Final_Patch3_GameData.ctr"))
                {
                    File.Move("!!!!Contra009Final_Patch3_GameData.ctr", "!!!!Contra009Final_Patch3_GameData.big");
                }
                else if (File.Exists("!!!Contra009Final_Patch2_GameData.ctr"))
                {
                    File.Move("!!!Contra009Final_Patch2_GameData.ctr", "!!!Contra009Final_Patch2_GameData.big");
                }
                // Set default cam height
                try
                {
                    if (AspectRatio(ScreenResolutionX, ScreenResolutionY) == "16:9" && isGentoolInstalled("d3d8.dll"))
                    {
                        ChangeCamHeight();
                    }
                }
                catch { }

                // Delete tinc vpn files
                try
                {
                    Directory.Delete(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Contra\vpnconfig\contravpn", true);
                    Directory.Delete(@"contra\vpn\32");
                    Directory.Delete(@"contra\vpn\64");
                    File.Delete(@"contra\vpn\tinc-adapters.cmd");
                    File.Delete(@"contra\vpn\tinc-add-tap-adapter.cmd");
                    File.Delete(@"contra\vpn\tinc-add-tap-adapter.ps1");
                    File.Delete(@"contra\vpn\tinc-config.cmd");
                    File.Delete(@"contra\vpn\tinc-console.cmd");
                    File.Delete(@"contra\vpn\tinc-license.txt");
                    File.Delete(@"contra\vpn\tinc-remove-tap-adapters.cmd");
                    File.Delete(@"contra\vpn\tinc-remove-tap-adapters.ps1");
                    File.Delete(@"contra\vpn\tinc-sources.url");
                    File.Delete(@"contra\vpn\tinc-start-daemon.cmd");
                    File.Delete(@"contra\vpn\tinc-up.cmd");
                }
                catch { }

                // If there are older Contra config folders, this means Contra Launcher has been
                // ran before on this PC, so in this case, we skip first run welcome message.
                int directoryCount = Directory.GetDirectories(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Contra").Length;

                // Zero Hour has a 'DeleteFile("Data\INI\INIZH.big");' line in GameEngine::init with no condition whatsoever (will always try to delete it if exists)
                // an identical copy of this file exists in root ZH folder so we can safely delete it before ZH runs to prevent unwanted crashes
                try
                {
                    File.Delete(@"Data\INI\INIZH.big");
                }
                catch { }

                // Show message on first run.
                if (GetCurrentCulture() == "en-US")
                {
                    RadioFlag_GB.Checked = true;
                    if (directoryCount <= 2)
                    {
                        MessageBox.Show("Welcome to Contra 009 Final! We highly recommend you to join our Discord community!", "Welcome!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else if (GetCurrentCulture() == "ru-RU")
                {
                    RadioFlag_RU.Checked = true;
                    if (directoryCount <= 2)
                    {
                        MessageBox.Show("Добро пожаловать в Contra 009 Final! Мы настоятельно рекомендуем Вам присоедениться к нашей группе Discord.", "Добро пожаловать!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        RadioRU.Checked = true;
                    }
                }
                else if (GetCurrentCulture() == "uk-UA")
                {
                    RadioFlag_UA.Checked = true;
                    if (directoryCount <= 2)
                    {
                        MessageBox.Show("Ласкаво просимо до Contra 009 Final! Ми максимально рекомендуємо Вам приєднатися до нашої спільноти Discord.", "Ласкаво просимо!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        RadioRU.Checked = true;
                    }
                }
                else if (GetCurrentCulture() == "bg-BG")
                {
                    RadioFlag_BG.Checked = true;
                    if (directoryCount <= 2)
                    {
                        MessageBox.Show("Добре дошли в Contra 009 Final! Силно препоръчваме да се присъедините към нашата Discord общност!", "Добре дошли!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else if (GetCurrentCulture() == "de-DE")
                {
                    RadioFlag_DE.Checked = true;
                    if (directoryCount <= 2)
                    {
                        MessageBox.Show("Wilkommen zu Contra 009 Final! Wir empfehlen dir unserem Discord Server beizutreten.", "Willkommen!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    RadioFlag_GB.Checked = true;
                    if (directoryCount <= 1)
                    {
                        MessageBox.Show("Welcome to Contra 009 Final! We highly recommend you to join our Discord community!", "Welcome!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }

                string optionsIniText = File.ReadAllText(Globals.myDocPath + "Options.ini");

                try
                {
                    // Switch Heat Effects off by default to prevent black screen issue of some users.
                    File.WriteAllText(Globals.myDocPath + "Options.ini", Regex.Replace(optionsIniText, "\r?\nHeatEffects = Yes", "\r\nHeatEffects = No", RegexOptions.IgnoreCase));
                }
                catch { }

                // Get CPU specs to determine default graphical settings
                ObjectQuery wql = new ObjectQuery("SELECT * FROM Win32_Processor");
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(wql);
                ManagementObjectCollection results = searcher.Get();
                foreach (ManagementObject result in results)
                {
                    Globals.cpuSpeed = Convert.ToInt32(result["MaxClockSpeed"]);

                    if (Globals.cpuSpeed < 3500) // We consider base clock less than 3500 MHz to be insufficient for stable FPS.
                                                 // If that's the case, we disable 3D shadows, water reflections and enable Dynamic LOD,
                                                 // as they are the most stressing graphical settings.
                    {
                        Messages.GenerateMessageBox("I_WeakCPU", Globals.currentLanguage);

                        // Switch Water Effects and 3D Shadows off, and Enable Dynamic LOD for better performance.
                        Properties.Settings.Default.WaterEffects = false;
                        try
                        {
                            File.WriteAllText(Globals.myDocPath + "Options.ini", Regex.Replace(optionsIniText, "\r?\nUseShadowVolumes = Yes", "\r\nUseShadowVolumes = No", RegexOptions.IgnoreCase));
                            File.WriteAllText(Globals.myDocPath + "Options.ini", Regex.Replace(optionsIniText, "\r?\nDynamicLOD = No", "\r\nDynamicLOD = Yes", RegexOptions.IgnoreCase));
                        }
                        catch { }
                    }
                }

                // Show tooltip on Options
                Point pt = new Point(0, 0);
                pt.Offset(moreOptions.Width - 30, moreOptions.Height - 55);

                optionsToolTip.Show(Messages.GenerateMessage("OptionsSuggestion", Globals.currentLanguage), moreOptions, pt, 10000);

                // Delete old Contra config folders
                DirectoryInfo di = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Contra");

                foreach (DirectoryInfo dir in di.EnumerateDirectories())
                {
                    if (dir.Name.Contains("vpnconfig") == true || dir.Name.Contains("Contra007Classic_Launcher_Url") == true) // Do not delete these folders
                    {
                        continue;
                    }
                    dir.Delete(true);
                }
                try
                {
                    // Enable Tournament Mode (limit super weapons and super units) on first run.
                    try
                    {
                        string skirmishIniText = File.ReadAllText(Globals.myDocPath + "Skirmish.ini");
                        {
                            File.WriteAllText(Globals.myDocPath + "Skirmish.ini", Regex.Replace(skirmishIniText, "\r?\nSuperweaponRestrict = No", "\r\nSuperweaponRestrict = Yes", RegexOptions.IgnoreCase));
                        }
                    }
                    catch { }
                }
                catch { }

                // Add Firewall exceptions.
                CheckFirewallExceptions();

                Properties.Settings.Default.FirstRun = false;
                Properties.Settings.Default.Save();
            }

            // Show warning if the base mod isn't found.
            if (!File.Exists("!Contra009Final.ctr") && !File.Exists("!Contra009Final.big") && Application.StartupPath.Contains(Environment.GetFolderPath(Environment.SpecialFolder.Desktop)))
            {
                Messages.GenerateMessageBox("W_NotFound_ContraOnDesktop", Globals.currentLanguage);
            }
            else if (!File.Exists("!Contra009Final.ctr") && !File.Exists("!Contra009Final.big"))
            {
                Messages.GenerateMessageBox("W_NotFound_Contra009Final", Globals.currentLanguage);
            }

            // Show warning if there are .ini files in "Data\INI" folder or its subfolders.
            try
            {
                if (Directory.GetFiles(Environment.CurrentDirectory + @"\Data\INI", "*.ini", SearchOption.AllDirectories).Length == 0)
                {
                    // no .ini files
                }
                else
                {
                    DialogResult dialogResult = MessageBox.Show(Messages.GenerateMessage("W_FoundIniFiles", Globals.currentLanguage), Messages.GenerateMessage("Warning", Globals.currentLanguage), MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                    if (dialogResult == DialogResult.Yes)
                    {
                        Process.Start(Environment.CurrentDirectory + @"\Data\INI");
                    }
                }
            }
            catch { }

            // Show warning if there are .wnd files in "Window" folder or its subfolders.
            try
            {
                if (Directory.GetFiles(Environment.CurrentDirectory + @"\Window", "*.wnd", SearchOption.AllDirectories).Length == 0)
                {
                    // no .wnd files
                }
                else
                {
                    DialogResult dialogResult = MessageBox.Show(Messages.GenerateMessage("W_FoundWndFiles", Globals.currentLanguage), Messages.GenerateMessage("Warning", Globals.currentLanguage), MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                    if (dialogResult == DialogResult.Yes)
                    {
                        Process.Start(Environment.CurrentDirectory + @"\Window");
                    }
                }
            }
            catch { }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            //If updating has failed, clear the 0KB file
            if (File.Exists($"{launcherExecutingPath}\\Contra_Launcher_{newVersion}.exe") && (applyNewLauncher == false))
            {
                File.Delete($"{launcherExecutingPath}\\Contra_Launcher_{newVersion}.exe");
            }
            //This renames the original file so any shortcut works and names it accordingly after the update
            if (File.Exists($"{launcherExecutingPath}\\Contra_Launcher_{newVersion}.exe") && (applyNewLauncher == true))
            {
                File.Move($"{launcherExecutingPath}\\Contra_Launcher.exe", $"{launcherExecutingPath}\\Contra_Launcher_ToDelete.exe");
                File.Move($"{launcherExecutingPath}\\Contra_Launcher_{newVersion}.exe", $"{launcherExecutingPath}\\Contra_Launcher.exe");
                //Process.Start(Path.Combine(launcherExecutingPath, "Contra_Launcher.exe"));
            }

            //Restart launcher after patching the mod
            //if (restartLauncher == true)
            //{
            //    Process.Start(Path.Combine(launcherExecutingPath, "Contra_Launcher.exe"));
            //}
        }

        public static bool isGentoolInstalled(string gentoolPath)
        {
            try
            {
                var size = GetFileVersionInfoSize(gentoolPath, out _);
                if (size == 0) { throw new Win32Exception(); };
                var bytes = new byte[size];
                bool success = GetFileVersionInfo(gentoolPath, 0, size, bytes);
                if (!success) { throw new Win32Exception(); }

                VerQueryValue(bytes, @"\StringFileInfo\040904E4\ProductName", out IntPtr ptr, out _);
                return Marshal.PtrToStringUni(ptr) == "GenTool";
            }
            catch //(Exception ex)
            {
                //Console.Error.WriteLine(ex);
                return false;
            }
        }

        public static bool isGentoolOutdated(string gentoolPath, int minVersion)
        {
            try
            {
                var size = GetFileVersionInfoSize(gentoolPath, out _);
                if (size == 0) { throw new Win32Exception(); };
                var bytes = new byte[size];
                bool success = GetFileVersionInfo(gentoolPath, 0, size, bytes);
                if (!success) { throw new Win32Exception(); }

                // 040904E4 US English + CP_USASCII
                VerQueryValue(bytes, @"\StringFileInfo\040904E4\ProductVersion", out IntPtr ptr, out _);
                return int.Parse(Marshal.PtrToStringUni(ptr)) < minVersion;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return false;
            }
        }
    }
}
