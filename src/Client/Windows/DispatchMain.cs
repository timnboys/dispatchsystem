﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;

using MaterialSkin;
using MaterialSkin.Controls;

using System.Net.Sockets;
using System.Net;
using System.Runtime.InteropServices;

using DispatchSystem.Common.DataHolders;
using DispatchSystem.Common.DataHolders.Storage;
using DispatchSystem.Common.NetCode;

namespace DispatchSystem.cl.Windows
{
    public partial class DispatchMain : MaterialForm
    {
        Socket usrSocket;

        BoloView boloWindow = null;

        public DispatchMain()
        {
            this.Icon = Icon.ExtractAssociatedIcon("icon.ico");
            InitializeComponent();

            SkinManager.Theme = MaterialSkinManager.Themes.LIGHT;
            SkinManager.ColorScheme = new ColorScheme(Primary.DeepPurple500, Primary.DeepPurple700, Primary.DeepPurple400, Accent.DeepPurple400, TextShade.WHITE);
        }

        public async void OnViewCivClick(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(firstName.Text) || string.IsNullOrWhiteSpace(lastName.Text))
                return;

            usrSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try { usrSocket.Connect(Config.IP, Config.Port); }
            catch (SocketException) { MessageBox.Show("Connection Refused or failed!\nPlease contact the owner of your server", "DispatchSystem", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }

            NetRequestHandler handle = new NetRequestHandler(usrSocket);

            Tuple<NetRequestResult, Civilian> result = await handle.TryTriggerNetFunction<Civilian>("GetCivilian", firstName.Text, lastName.Text);
            usrSocket.Shutdown(SocketShutdown.Both);
            usrSocket.Close();

            if (result.Item2 != null)
            {
                if (!(string.IsNullOrEmpty(result.Item2?.First) || string.IsNullOrEmpty(result.Item2?.Last))) // Checking if the civilian is empty bc for some reason == and .Equals are not working for this situation
                {
                    Invoke((MethodInvoker)delegate
                    {
                        new CivView(result.Item2).Show();
                    });
                }
                else
                    MessageBox.Show("That name doesn't exist in the system!", "DispatchSystem", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else
                MessageBox.Show("Invalid request", "DispatchSystem", MessageBoxButtons.OK, MessageBoxIcon.Error);

            firstName.ResetText();
            lastName.ResetText();
        }

        private async void OnViewCivVehClick(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(plate.Text))
                return;

            usrSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try { usrSocket.Connect(Config.IP, Config.Port); }
            catch (SocketException) { MessageBox.Show("Connection Refused or failed!\nPlease contact the owner of your server", "DispatchSystem", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }

            NetRequestHandler handle = new NetRequestHandler(usrSocket);

            Tuple<NetRequestResult, CivilianVeh> result = await handle.TryTriggerNetFunction<CivilianVeh>("GetCivilianVeh", plate.Text);
            usrSocket.Shutdown(SocketShutdown.Both);
            usrSocket.Close();

            if (result.Item2 != null)
            {
                if (!(string.IsNullOrEmpty(result.Item2.Plate)))
                {
                    Invoke((MethodInvoker)delegate
                    {
                        new CivVehView(result.Item2).Show();
                    });
                }
                else
                    MessageBox.Show("That plate doesn't exist in the system!", "DispatchSystem", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else
                MessageBox.Show("Invalid Request", "DispatchSystem", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

            plate.ResetText();
        }

        private async void OnViewBolosClick(object sender, EventArgs e)
        {
            if (boloWindow != null)
            {
                MessageBox.Show("You cannot have 2 instances of the Bolos window open at the same time!\nTry pressing the Resync button inside your bolo window.", "DispatchSystem", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }

            usrSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try { usrSocket.Connect(Config.IP, Config.Port); }
            catch (SocketException) { MessageBox.Show("Connection Refused or failed!\nPlease contact the owner of your server", "DispatchSystem", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }

            NetRequestHandler handle = new NetRequestHandler(usrSocket);

            Tuple<NetRequestResult, StorageManager<Bolo>> result = await handle.TryTriggerNetFunction<StorageManager<Bolo>>("GetBolos");
            usrSocket.Shutdown(SocketShutdown.Both);
            usrSocket.Close();

            if (result.Item2 != null)
            {
                Invoke((MethodInvoker)delegate
                {
                    (boloWindow = new BoloView(result.Item2)).Show();
                    boloWindow.FormClosed += delegate { boloWindow = null; };
                });
            }
            else
                MessageBox.Show("FATAL: Invalid", "DispatchSystem", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void OnRemoveBoloClick(object sender, EventArgs e)
        {
            Invoke((MethodInvoker)delegate
            {
                new AddRemoveView(AddRemoveView.Type.RemoveBolo).Show();
            });
        }

        private void OnAddBoloClick(object sender, EventArgs e)
        {
            Invoke((MethodInvoker)delegate
            {
                new AddRemoveView(AddRemoveView.Type.AddBolo).Show();
            });
        }

        private void OnFirstNameKeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsLetter(e.KeyChar))
                e.Handled = true;
            if (char.IsLetter(e.KeyChar))
                e.KeyChar = char.ToUpper(e.KeyChar);
        }

        private void OnLastNameKeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsLetter(e.KeyChar))
                e.Handled = true;
            if (char.IsLetter(e.KeyChar))
                e.KeyChar = char.ToUpper(e.KeyChar);
        }

        private void OnPlateKeyPress(object sender, KeyPressEventArgs e)
        {
            if ((!char.IsControl(e.KeyChar) && !char.IsLetterOrDigit(e.KeyChar) && e.KeyChar != ' ') || (plate.Text.Length >= 8 && !e.KeyChar.Equals('\b')))
            {
                e.Handled = true;
                
            }
            if (char.IsLetter(e.KeyChar))
                e.KeyChar = char.ToUpper(e.KeyChar);
        }
    }
}
