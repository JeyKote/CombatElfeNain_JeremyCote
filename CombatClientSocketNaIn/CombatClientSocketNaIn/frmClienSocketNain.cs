using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CombatClientSocketNaIn.Classes;

namespace CombatClientSocketNaIn
{
    public partial class frmClienSocketNain : Form
    {
        Random m_r;
        Elfe m_elfe;
        Nain m_nain;
        public frmClienSocketNain()
        {
            InitializeComponent();
            m_r = new Random();
            Reset();
            btnReset.Enabled = false;
            
            Control.CheckForIllegalCrossThreadCalls = false;
        }   
        void Reset()
        {
            //reset les stats du nain et de l'elfe
            m_nain = new Nain(m_r.Next(10, 20), m_r.Next(2, 6), m_r.Next(0, 3));
            picNain.Image = m_nain.Avatar;
            lblVieNain.Text = "Vie: " + m_nain.Vie.ToString(); ;
            lblForceNain.Text = "Force: " + m_nain.Force.ToString();
            lblArmeNain.Text = "Arme: " + m_nain.Arme;

            m_elfe = new Elfe(1, 0, 0);
            picElfe.Image = m_elfe.Avatar;
            lblVieElfe.Text = "Vie: " + m_elfe.Vie.ToString();
            lblForceElfe.Text = "Force: " + m_elfe.Force.ToString();
            lblSortElfe.Text = "Sort: " + m_elfe.Sort.ToString();
        }
        void AfficheStatNain()
        {
            lblVieNain.Text = "Vie: " + m_nain.Vie.ToString();
            lblForceNain.Text = "Force: " + m_nain.Force.ToString();
            lblArmeNain.Text = "Arme: " + m_nain.Arme.ToString();


            this.Update(); // pour s'assurer de l'affichage via le thread
        }
        void AfficheStatElfe()
        {
            lblVieElfe.Text = "Vie: " + m_elfe.Vie.ToString();
            lblForceElfe.Text = "Force: " + m_elfe.Force.ToString();
            lblSortElfe.Text = "Sort: " + m_elfe.Sort.ToString();


            this.Update(); // pour s'assurer de l'affichage via le thread
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            btnFrappe.Enabled = true;
            btnReset.Enabled = false;
            Reset();
        }

        private void btnFrappe_Click(object sender, EventArgs e)
        {
            string receptionClient = "rien";
            int nbOctetReception;
            string[] ts;
            byte[] tByteReception = new byte[50];
            Socket client;
            try
            { 
                //se connecte au serveur
                client = new Socket(SocketType.Stream, ProtocolType.Tcp);
                client.Connect(IPAddress.Parse("127.0.0.1"), 8888);
                MessageBox.Show("assurez-vous que le serveur est en attente d'un client");

                if (client.Connected)
                {
                    //envoie des donnés du nain au serveur
                    string envoie = m_nain.Vie.ToString() + ";" + m_nain.Force.ToString() + ";" + m_nain.Arme.ToString() + ";";
                    ASCIIEncoding textByte = new ASCIIEncoding();
                    Byte[] tEnvoie = textByte.GetBytes(envoie);
                    client.Send(tEnvoie);
                    //reception des données du nain et de l'elfe
                    nbOctetReception = client.Receive(tByteReception);
                    receptionClient = Encoding.ASCII.GetString(tByteReception);

                    ts = receptionClient.Split(';');

                    
                    m_nain.Vie = Convert.ToInt32(ts[0]);
                    m_nain.Force = Convert.ToInt32(ts[1]);
                    m_nain.Arme = ts[2];
                    m_elfe.Vie = Convert.ToInt32(ts[3]);
                    m_elfe.Force = Convert.ToInt32(ts[4]);
                    m_elfe.Sort = Convert.ToInt32(ts[5]);
                    AfficheStatNain();
                    AfficheStatElfe();
                    //détermine le vainqueur
                    if (m_nain.Vie <= 0)
                    {
                        btnFrappe.Enabled = false; btnReset.Enabled = true;
                        picNain.Image = m_elfe.Avatar;
                        MessageBox.Show("L'elfe a gagne");
                    }
                    if (m_elfe.Vie <= 0)
                    {
                        btnFrappe.Enabled = false; btnReset.Enabled = true;
                        picElfe.Image = m_nain.Avatar;
                        MessageBox.Show("Le nain a gagne");
                    }
                }
                client.Close();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
