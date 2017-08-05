﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using static mV2RayConfig.Config;
using Newtonsoft.Json.Linq;

namespace mV2RayConfig
{
    public partial class Form1 : Form
    {


        public Form1()
        {
            InitializeComponent();
        }

        ServerInfo serverInfo = new ServerInfo();
        Outbound outbound = new Outbound();
        Inbound inbound = new Inbound();
        InBoundSetting inBoundSetting = new InBoundSetting();
        VmessClients clients = new VmessClients();
        Log log = new Log();

        private void Form1_Load(object sender, EventArgs e)
        {
            log.access = "/var/log/v2ray/access.log";
            log.error = "/var/log/v2ray/error.log";
            log.loglevel = "warning";
            serverInfo.log = log;

            inbound.port = 2332;
            inbound.protocol = "vmess";
            clients.id = Guid.NewGuid().ToString();
            clients.alterId = 100;
            JArray centArray = new JArray();
            centArray.Add(JObject.FromObject(clients));
            inBoundSetting.clients = centArray;
            inbound.settings = inBoundSetting;
            serverInfo.inbound = inbound;

            outbound.protocol = "freedom";
            outbound.settings = new Setting();
            serverInfo.outbound = outbound;

            richTextBox1.Text = JsonConvert.SerializeObject(serverInfo);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form1_Load(null, null);
        }
    }
}