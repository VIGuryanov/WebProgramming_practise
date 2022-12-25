using System.Net.Sockets;
using System.Text.Json;
using TCPClient;
using XProtocol;
using XProtocol.Serializator;
using XProtocol.Packets;

namespace PaintOnlineClient
{
    public partial class Form1 : Form
    {
        XClient client;
        public string UserName { get; set; }
        public Color Color { get; set; }

        Graphics g;
        Pen p = new Pen(Color.Black, 5);

        public Form1()
        {
            InitializeComponent();
            g = groupBox1.CreateGraphics();
        }

        private void button1_ClickAsync(object sender, EventArgs e)
        {
            var button = sender as Button;
            button.Enabled = false;
            button.Visible = false;

            groupBox1.Click += SendPoint;

            UserName = playerName.Text;

            try
            {
                client = new ClientProcess(this).Client;

                client.QueuePacketSend(XPacketConverter.Serialize(XPacketType.User,
                    new UserPacket() { NickName = NickNamePacket.EncodeString(playerName.Text), Color =0 }).ToPacket());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void AddPlayer(string name, int color)
        {
            playersList.Invoke(() =>
            {
                var label = new Label();
                label.Text = name;
                label.ForeColor = Color.FromArgb(color);
                playersList.Controls.Add(label);
            });
        }

        private async void button2_ClickAsync(object sender, EventArgs e)
        {
            
        }

        public void Draw(Point point, Color color)
        {
            var p = new Pen(color, 5);

            Rectangle r = new Rectangle();
            r.Width = 4;
            r.Height = 4;
            r.Location = new Point(point.X - r.Width / 2, point.Y - r.Height / 2);
            g.DrawEllipse(p, r);
        }

        private async void SendPoint(object sender, EventArgs e)
        {
            MouseEventArgs mouseArgs = e as MouseEventArgs;

            var loc = mouseArgs.Location;

            client.QueuePacketSend(XPacketConverter.Serialize(XPacketType.ColoredPoint, new ColoredPoint { Color =0 , X = loc.X, Y = loc.Y }).ToPacket());
        }
    }
}