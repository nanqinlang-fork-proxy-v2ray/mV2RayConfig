﻿using System;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static mV2RayConfig.Config;
using mV2RayConfig.Forms;

namespace mV2RayConfig
{
    public partial class MainForm : Form
    {
        private ServerInfo serverInfo = new ServerInfo();
        private Outbound outbound = new Outbound();
        private Inbound inbound = new Inbound();
        private InBoundSetting inBoundSetting = new InBoundSetting();
        private VmessClients clients = new VmessClients();
        private Log log = new Log();

        public MainForm()
        {
            InitializeComponent();
            comboBoxLogLevel.SelectedIndex = 2;
            comboBoxProtocol.SelectedIndex = 0;
            comboBoxFakeKCP.SelectedIndex = 0;
            textBoxUUID.Text = uuidGen();
            buttonUserConfig.Hide();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            richTextBoxConfig.Text = configGen();
        }

        private void buttonNewGen_Click(object sender, EventArgs e)
        {
            richTextBoxConfig.Text = configGen();
            if (!(checkBoxHttpFake.Checked||checkBoxKCP.Checked || checkBoxWS.Checked || checkBoxTLS.Checked))
            {
                buttonUserConfig.Show();
            }
        }

        private void labelPort_Click(object sender, EventArgs e)
        {
            upDownPort.Value = new Random(DateTime.Now.Second).Next(2000, 7000);
        }

        private void buttonUUID_Click(object sender, EventArgs e)
        {
            textBoxUUID.Text = uuidGen();
        }

        private void checkBoxManyUser_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxManyUser.Checked)
            {
                listBoxManyUser.Enabled = true;
                buttonAdd.Enabled = true;
                buttonDel.Enabled = true;
                buttonUserConfig.Enabled = false;
            }
            else
            {
                listBoxManyUser.Enabled = false;
                buttonAdd.Enabled = false;
                buttonDel.Enabled = false;
                buttonUserConfig.Enabled = true;
            }
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            clients.id = textBoxUUID.Text;
            clients.alterId = Convert.ToInt32(upDownAlterID.Value);
            if (checkBoxUserLevel.Checked)
            {
                clients.level = 1;
            }
            else
            {
                clients.level = 0;
            }
            listBoxManyUser.Items.Add(JsonConvert.SerializeObject(clients));
            textBoxUUID.Text = uuidGen();
        }

        private void buttonDel_Click(object sender, EventArgs e)
        {
            listBoxManyUser.Items.Remove(listBoxManyUser.SelectedItem);
        }

        private void listBoxManyUser_DoubleClick(object sender, EventArgs e)
        {
            if (listBoxManyUser.SelectedItem != null)
            {
                MessageBox.Show(listBoxManyUser.SelectedItem.ToString());
            }
        }

        private string configGen()
        {
            log.access = textBoxAccessLog.Text;
            log.error = textBoxErrorLog.Text;
            log.loglevel = comboBoxLogLevel.Text;
            serverInfo.log = log;

            inbound.port = Convert.ToInt32(upDownPort.Value);
            inbound.protocol = comboBoxProtocol.Text;
            JArray centArray = new JArray();

            if (!checkBoxManyUser.Checked)
            {
                clients.id = textBoxUUID.Text;
                clients.alterId = Convert.ToInt32(upDownAlterID.Value);
                if (checkBoxUserLevel.Checked)
                {
                    clients.level = 1;
                }
                else
                {
                    clients.level = 0;
                }
                centArray.Add(JObject.FromObject(clients));
            }
            else
            {
                foreach (var item in listBoxManyUser.Items)
                {
                    centArray.Add(JObject.Parse(item.ToString()));
                }
            }

            inBoundSetting.clients = centArray;
            inbound.settings = inBoundSetting;
            serverInfo.inbound = inbound;

            outbound.protocol = "freedom";
            outbound.settings = new JObject();
            serverInfo.outbound = outbound;

            JObject configJson = JObject.FromObject(serverInfo);
            JObject stream = new JObject();

            configJson["outboundDetour"] = JArray.Parse(OutboundDetourStr);
            if (checkBoxRouting.Checked)
            {
                configJson["routing"] = JObject.Parse(RoutingStr);
            }

            if (checkBoxHttpFake.Checked)
            {
                stream["network"] = "tcp";
                stream["tcpSettings"] = JObject.Parse(HttpFakeStr);
                configJson["streamSettings"] = stream;
            }

            if (checkBoxKCP.Checked)
            {
                stream["network"] = "kcp";
                JObject KCPSet = JObject.Parse(MKcpStr);
                JObject fakeType = new JObject();
                fakeType["type"] = comboBoxFakeKCP.Text;
                KCPSet["header"] = fakeType;
                stream["kcpSettings"] = KCPSet;
                configJson["streamSettings"] = stream;
            }

            if (checkBoxTLS.Checked)
            {
                if (!checkBoxWS.Checked)
                {
                    stream["network"] = "tcp";
                }
                stream["security"] = "tls";
                JObject tlsSet = new JObject();
                JObject certSet = new JObject();
                certSet["certificateFile"] = CertificateFile;
                certSet["keyFile"] = KeyFile;
                JArray certificatesArray = new JArray();
                certificatesArray.Add(certSet);
                tlsSet["serverName"] = ServerName;
                tlsSet["certificates"] = certificatesArray;
                stream["tlsSettings"] = tlsSet;
                configJson["streamSettings"] = stream;
            }

            if (checkBoxWS.Checked)
            {
                stream["network"] = "ws";
                configJson["streamSettings"] = stream;
            }

            return MyJson.FormatJsonString(configJson.ToString());
        }

        private string uuidGen()
        {
            return Guid.NewGuid().ToString();
        }

        private void resetEnabled()
        {
            checkBoxHttpFake.Enabled = true;
            checkBoxWS.Enabled = true;
            checkBoxKCP.Enabled = true;
            checkBoxTLS.Enabled = true;
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "V2Ray配置文件Json|config.json";
            saveFileDialog.FileName = "config.json";
            saveFileDialog.Title = "保存V2Ray配置文件";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllText(saveFileDialog.FileName, richTextBoxConfig.Text);
            }
        }

        private void editLink_Click(object sender, EventArgs e)
        {
            new EditForm().ShowDialog();
        }

        private void checkBoxHttpFake_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxHttpFake.Checked)
            {
                checkBoxWS.Enabled = false;
                checkBoxKCP.Enabled = false;
            }
            else
            {
                resetEnabled();
            }
        }

        private void checkBoxKCP_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxKCP.Checked)
            {
                checkBoxWS.Enabled = false;
                checkBoxHttpFake.Enabled = false;
                checkBoxTLS.Enabled = false;
            }
            else
            {
                resetEnabled();
            }
        }

        private void checkBoxWS_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxWS.Checked)
            {
                checkBoxHttpFake.Enabled = false;
                checkBoxKCP.Enabled = false;
            }
            else
            {
                resetEnabled();
            }
        }

        private void checkBoxTLS_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxTLS.Checked)
            {
                checkBoxKCP.Enabled = false;
                new TLSForm().ShowDialog();
            }
            else
            {
                resetEnabled();
            }
        }

        private void buttonUserConfig_Click(object sender, EventArgs e)
        {
            string uuid = clients.id;
            int id = clients.alterId;
            int port = inbound.port;
            new ClientConfigForm(uuid, id, port).Show();
        }

        private void panel1_DoubleClick(object sender, EventArgs e)
        {
            string uuid = clients.id;
            int id = clients.alterId;
            int port = inbound.port;
            new ClientConfigForm(uuid, id, port).Show();
        }
    }
}
