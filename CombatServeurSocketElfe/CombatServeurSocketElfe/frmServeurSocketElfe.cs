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
using CombatServeurSocketElfe.Classes;

namespace CombatServeurSocketElfe
{
    public partial class frmServeurSocketElfe : Form
    {
        Random m_r;
        Nain m_nain;
        Elfe m_elfe;
        TcpListener m_ServerListener;
        Socket m_client;
        Thread m_thCombat;

        public frmServeurSocketElfe()
        {
            InitializeComponent();
            m_ServerListener = new TcpListener(IPAddress.Parse("127.0.0.1"), 8888);
            m_ServerListener.Start();
            m_r = new Random();
            Reset();
            btnReset.Enabled = false;
            //Démarre un serveur de socket (TcpListener)
            
            lstReception.Items.Add("Serveur démarré !");
            lstReception.Items.Add("PRESSER : << attendre un client >>");
            lstReception.Update();
            Control.CheckForIllegalCrossThreadCalls = false;
        }
        void Reset()
        {
            m_nain = new Nain(1, 0, 0);
            picNain.Image = m_nain.Avatar;
            AfficheStatNain();

            m_elfe = new Elfe(m_r.Next(10, 20), m_r.Next(2, 6), m_r.Next(2, 6));
            picElfe.Image = m_elfe.Avatar;
            AfficheStatElfe();
 
            lstReception.Items.Clear();
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
            btnReset.Enabled = false;
            Reset();
        }     

        private void btnAttente_Click(object sender, EventArgs e)
        {
            ThreadStart code = new ThreadStart(Combat);
            m_thCombat = new Thread(code);
            m_thCombat.Start();
            // Combat par un thread
            
        }
        public void Combat() 
        {
            // déclarations de variables locales 
            string receptionClient = "rien";
            int nbOctetReception;
            string[] ts;
            byte[] tByteReception = new byte[50];
            ASCIIEncoding textByte = new ASCIIEncoding();

            try
            {
                // tous le code de traitement
                while (m_nain.Vie >= 0 || m_elfe.Vie >= 0)
                {
                    m_client = m_ServerListener.AcceptSocket();
                    lstReception.Items.Add("client branché");
                    lstReception.Update();
                    Thread.Sleep(500);
                    //recois données du nain
                    nbOctetReception = m_client.Receive(tByteReception);
                    receptionClient = Encoding.ASCII.GetString(tByteReception);

                    lstReception.Items.Add("du client: " + receptionClient);
                    lstReception.Update();

                    ts = receptionClient.Split(';');

                    m_nain.Vie = Convert.ToInt32(ts[0]);
                    m_nain.Force = Convert.ToInt32(ts[1]);
                    m_nain.Arme = ts[2];
                    AfficheStatNain();
                    MessageBox.Show("serveur: frapper l'elfe");
                    m_nain.Frapper(m_elfe);
                    m_elfe.LancerSort(m_nain);
                    AfficheStatNain();
                    AfficheStatElfe();
                    if(m_nain.Vie < 1||m_elfe.Vie < 1)
                    {
                        btnReset.Enabled = true;
                    }
                    //envoie des données du nain et de l'elfes au client
                    string envoie = m_nain.Vie.ToString() + ";" + m_nain.Force.ToString() + ";" + m_nain.Arme.ToString() + ";" + m_elfe.Vie.ToString() + ";" + m_elfe.Force.ToString() + ";" + m_elfe.Sort.ToString() + ";";
                    ASCIIEncoding textByteenvoie = new ASCIIEncoding();
                    Byte[] tEnvoie = textByteenvoie.GetBytes(envoie);
                    m_client.Send(tEnvoie);
                    m_client.Close();
                }
               
            }

            catch (Exception ex)
            {
                MessageBox.Show("Exception: " + ex.Message);
            }

        }

        private void btnFermer_Click(object sender, EventArgs e)
        {
            // il faut avoir un objet elfe et un objet nain instanciés
            m_elfe.Vie = 0;
            m_nain.Vie = 0;
            try
            {
                
                Thread.Sleep(1000);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception: " + ex.Message);
            }
        }

        private void frmServeurSocketElfe_FormClosing(object sender, FormClosingEventArgs e)
        {
            btnFermer_Click(sender,e);
            try
            {
                // il faut avoir un objet TCPListener existant
                m_ServerListener.Stop();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception: " + ex.Message);
            }
        }
    }
}
