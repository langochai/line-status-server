using LineStatusServer.Common;
using LineStatusServer.DTOs;
using LineStatusServer.Models;
using Microsoft.Win32;
using NB_TestTruyenThong.Uc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LineStatusServer
{
    public partial class frmMain : Form
    {
        public BindingList<LineData> listLineData = new BindingList<LineData>();
        public string lastLineData = string.Empty;

        private CSocketServer tcpServer = null;
        private List<int> LstPrefix = new List<int>();
        private List<int> LstSuffix = new List<int>();
        private bool isRun;
        public bool IsRun
        {
            get { return isRun; }
            set { isRun = value; }
        }
        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Shown(object sender, EventArgs e)
        {
            InitializeCheckStartUp();
            var (serverName, dbName, userName, password, extra) = Settings.ReadSQLConnectionString();
            var (ip, port) = Settings.ReadUDPAddress();
            txtServerName.Text = serverName;
            txtDBName.Text = dbName;
            txtUserName.Text = userName;
            txtPassword.Text = password;
            txtOptional.Text = extra;
            txtIPAddress.Text = ip;
            txtPort.Text = port;
            grvMain.DataSource = listLineData;
            chkConnectWhenStart.Checked = Properties.Settings.Default.ConnectWhenStart;
            if (chkConnectWhenStart.Checked) btnConnect_Click(null, null);
        }

        /// <summary>
        /// Synchronize data in background thread
        /// </summary>
        /// <param name="JSONstring">Received data; JSON format</param>
        private void SynchData(string JSONstring)
        {
            try
            {
                if (lastLineData == JSONstring)
                    return;
                lastLineData = JSONstring;

                LineData lineData = JsonConvert.DeserializeObject<LineData>(JSONstring);
                lineData.Timestamp = SQLUtilities.GetDate();

                BeginInvoke(new Action(() =>
                {
                    listLineData.Insert(0, lineData);
                    if (listLineData.Count > 100) listLineData.RemoveAt(listLineData.Count - 1);
                }));
                var lineDataSQL = new Line_downtime_history
                {
                    line_code = lineData.LineCode,
                    timestamp = lineData.Timestamp,
                    product_count = lineData.ProductCount,
                    status = lineData.Status,
                };
                SQLHelper<Line_downtime_history>.Insert(lineDataSQL);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ErrorLogger.Write(ex);
            }
        }

        #region Minimize to tray

        private void btnMinimize_Click(object sender, EventArgs e)
        {
            this.Hide();
            notifyIcon.Visible = true;
        }

        private void notifyIcon_DoubleClick(object sender, EventArgs e)
        {
            ShowForm();
        }

        private void mnitemShow_Click(object sender, EventArgs e)
        {
            ShowForm();
        }

        private void mnitemExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ShowForm()
        {
            notifyIcon.Visible = false;
            this.Show();
            if (this.WindowState == FormWindowState.Minimized)
                this.WindowState = FormWindowState.Normal;
            this.Activate();
        }

        #endregion Minimize to tray

        #region Run on start up

        private void chkRunOnStartUp_CheckedChanged(object sender, EventArgs e)
        {
            if (chkRunOnStartUp.Checked) AddToStartup();
            else RemoveFromStartup();
        }

        public void InitializeCheckStartUp()
        {
            try
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", false);
                if (key != null)
                {
                    object value = key.GetValue("LineStatusServer");
                    chkRunOnStartUp.Checked = value != null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error checking startup registry: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void AddToStartup()
        {
            try
            {
                string appName = "LineStatusServer";
                string appPath = Application.ExecutablePath;

                RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                key.SetValue(appName, "\"" + appPath + "\"");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding to startup: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void RemoveFromStartup()
        {
            try
            {
                string appName = "LineStatusServer";

                RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                key.DeleteValue(appName, false);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error removing from startup: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion Run on start up

        private void btnSave_Click(object sender, EventArgs e)
        {
            Settings.WriteSQLConnectionString(
                txtServerName.Text,
                txtDBName.Text,
                txtUserName.Text,
                txtPassword.Text,
                txtOptional.Text
            );
            Settings.WriteUDPAddress(
                txtIPAddress.Text,
                txtPort.Text
            );
            btnSave.BeginInvoke(new Action(async () =>
            {
                btnSave.Text = "Đã lưu";
                btnSave.Padding = new Padding(45, 0, 35, 0);
                btnSave.Enabled = false;
                btnSave.FlatStyle = FlatStyle.Flat;
                await Task.Delay(2000);
                btnSave.Text = "Lưu";
                btnSave.Padding = new Padding(50, 0, 50, 0);
                btnSave.Enabled = true;
                btnSave.FlatStyle = FlatStyle.Standard;
            }));
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            //try
            //{
            //    if (btnConnect.Tag.ToString().ToLower() == "disconnected")
            //    {
            //        Task.Run(() => TCPServer.ResetConnection(SynchData)).Wait();
            //        lblCurrentStatus.Text = "Đã kết nối";
            //        lblCurrentStatus.ForeColor = Color.Green;
            //        btnConnect.Text = "Ngắt kết nối";
            //        btnConnect.Image = Properties.Resources.disconnect;
            //        btnConnect.Padding = new Padding(25, 0, 25, 0);
            //        btnConnect.ForeColor = Color.Red;
            //        btnConnect.Tag = "connected";
            //    }
            //    else
            //    {
            //        Task.Run(() => TCPServer.StopConnection()).Wait();
            //        lblCurrentStatus.Text = "Đã ngắt kết nối";
            //        lblCurrentStatus.ForeColor = Color.Red;
            //        btnConnect.Text = "Kết nối";
            //        btnConnect.Image = Properties.Resources.connect;
            //        btnConnect.Padding = new Padding(40, 0, 40, 0);
            //        btnConnect.ForeColor = Color.Green;
            //        btnConnect.Tag = "disconnected";
            //    }
            //}
            //catch
            //{
            //    lblCurrentStatus.Text = "Đã có lỗi xảy ra";
            //    lblCurrentStatus.ForeColor = Color.Red;
            //}

            if (!IsRun)
            {
                if (tcpServer != null) { tcpServer.Stop(); tcpServer = null; }
                tcpServer = new CSocketServer(Settings.UDPAddress.Port);
                tcpServer.Start();
                if (!tcpServer.IsConnected)
                {
                    lblCurrentStatus.Text = "Đã ngắt kết nối";
                    lblCurrentStatus.ForeColor = Color.Red;
                    btnConnect.Text = "Kết nối";
                    btnConnect.Image = Properties.Resources.connect;
                    btnConnect.Padding = new Padding(40, 0, 40, 0);
                    btnConnect.ForeColor = Color.Green;
                    btnConnect.Tag = "disconnected";
                    return;
                }
                else
                {
                    lblCurrentStatus.Text = "Đã kết nối";
                    lblCurrentStatus.ForeColor = Color.Green;
                    btnConnect.Text = "Ngắt kết nối";
                    btnConnect.Image = Properties.Resources.disconnect;
                    btnConnect.Padding = new Padding(25, 0, 25, 0);
                    btnConnect.ForeColor = Color.Red;
                    btnConnect.Tag = "connected";
                }
                // hàm lấy dữ liệu
                tcpServer.OnReceiveDataEvents_Server -= GetDataFromServer;
                tcpServer.OnReceiveDataEvents_Server += GetDataFromServer;
            }
            else
            {
                lblCurrentStatus.Text = "Đã ngắt kết nối";
                lblCurrentStatus.ForeColor = Color.Red;
                btnConnect.Text = "Kết nối";
                btnConnect.Image = Properties.Resources.connect;
                btnConnect.Padding = new Padding(40, 0, 40, 0);
                btnConnect.ForeColor = Color.Green;
                btnConnect.Tag = "disconnected";
                tcpServer.OnReceiveDataEvents_Server -= GetDataFromServer;
                if (tcpServer != null) { tcpServer.Stop(); tcpServer = null; }
            }
        }

        private void GetDataFromServer(string receive_Data)
        {
            try
            {
                if (lastLineData == receive_Data)
                    return;
                lastLineData = receive_Data;

                LineData lineData = JsonConvert.DeserializeObject<LineData>(receive_Data);
                lineData.Timestamp = SQLUtilities.GetDate();

                BeginInvoke(new Action(() =>
                {
                    listLineData.Insert(0, lineData);
                    if (listLineData.Count > 100) listLineData.RemoveAt(listLineData.Count - 1);
                }));
                var lineDataSQL = new Line_downtime_history
                {
                    line_code = lineData.LineCode,
                    timestamp = lineData.Timestamp,
                    product_count = lineData.ProductCount,
                    status = lineData.Status,
                };
                SQLHelper<Line_downtime_history>.Insert(lineDataSQL);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ErrorLogger.Write(ex);
            }
        }

        private void frmMain_Resize(object sender, EventArgs e)
        {
            int frmHeight = this.Height;
            grvMain.Height = frmHeight > 1000 ? (int)(frmHeight * 0.75) :
                            frmHeight > 850 ? (int)(frmHeight * 0.70) :
                            frmHeight > 700 ? (int)(frmHeight * 0.65) :
                            frmHeight > 600 ? (int)(frmHeight * 0.60) :
                            (int)(frmHeight * 0.50);
        }

        private void mnitemDelete_Click(object sender, EventArgs e)
        {
            listLineData.Clear();
        }

        private void chkConnectWhenStart_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.ConnectWhenStart = chkConnectWhenStart.Checked;
            Properties.Settings.Default.Save();
        }

    }
}