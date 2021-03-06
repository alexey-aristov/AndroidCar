﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Aristov.Common;
using Aristov.Common.Windows;
using Aristov.Communication.RT;
using Aristov.Communication.RT.Queued;
using Timer = System.Timers.Timer;

namespace TcpCatcher {
	public partial class Form1 : Form
	{

		VideoClient _videoClient;
		ILogger Logger;
		
		public Form1(){
			LoggerFactory.Setup ( typeof ( DiagnostigsLogger ) );
			Logger = LoggerFactory.Create ();
			InitializeComponent();
			
		}

		void _videoClient_NewFrame ( object sender , NewFrameEventArgs e )
		{
			Image img = Image.FromStream(e.Frame.GetStream());
			pictureBox1.Image = img;
		}

		int countF = 0;
		void _client_NewFrameEventHandler ( object sender , NewFrameEventArgsOLd e ) {
			//countF++;
			Console.WriteLine("Recived {0} with hash {1},Length: {2}",e.Type,e.FrameBytes.GetHashCode(),e.FrameBytes.Length);
			string file = Path.Combine(Environment.CurrentDirectory, "photos",string.Format("img{0}.jpeg",countF));
			var img = Image.FromStream(new MemoryStream(e.FrameBytes));
			
			
			setPic(img);
			//var f = new BinaryWriter(new FileStream(file, FileMode.Create));
			//f.Write(e.FrameBytes);
			//f.Close();
			//f.Dispose();

		}

		Thread thread;
		List<Thread> clientsThreads;
		TcpListener listener;
		Timer timer;

		private void button1_Click(object sender, EventArgs e)
		{
			_videoClient = new VideoClient ( textBox1.Text , 9999 );
			_videoClient.NewFrame += _videoClient_NewFrame;
			//_client.NewFrameEventHandler += _client_NewFrameEventHandler;
			//thread = new Thread(listen);
			//thread.Start();
			//timer = new Timer(100);
			//ConcurrentQueue<Image> _imagesBag = new ConcurrentQueue<Image> ();
			//timer.Elapsed += (o, args) =>
			//{
			//	if (Image!=null)
			//	{
			//		//var image = _imagesBag.Last ();
			//		//pictureBox1.Image = Image;
			//		//SetText( DateTime.Now.TimeOfDay+"|"+Image.GetHashCode()+ Environment.NewLine);
			//	}
				
			//};
			//timer.Start();

		}
		

		public void ClientF(TcpClient client)
		{
			var stream = client.GetStream ();
			
			while (stream.CanRead)
			{
				
			//	byte[] buffer =new byte[client.re];
				//var bytesRead =stream.Read(buffer, 0, 6000);
				//byte[] newd = buffer.ToArray().Take(bytesRead).ToArray();

				//var ms = new MemoryStream(newd,0,bytesRead-1);
				MemoryStream c =new MemoryStream();
				
				Image im = Image.FromStream(stream);
				//textBox2.Text =GetString(buffer);
				//SetText(TextEncoder.GetString(buffer));
				//TypeConverter tc = TypeDescriptor.GetConverter ( typeof ( Bitmap ) );
				//Bitmap bitmap1 = (Bitmap) tc.ConvertFrom ( newd);
				setPic ( im );
			}
		}
		delegate void SetTextCallback ( string text );

		delegate void SetPicCallback(Image img);

		int frames = 0;
		private void setPic(Image img)
		{
			if (pictureBox1.InvokeRequired)
			{
				SetPicCallback c = new SetPicCallback(setPic);
				this.Invoke(c, new object[] {img});
			}
			else
			{
				pictureBox1.Image = img;

			}
		}

		private void SetText ( string text ) {
			// InvokeRequired required compares the thread ID of the
			// calling thread to the thread ID of the creating thread.
			// If these threads are different, it returns true.
			if ( this.textBox1.InvokeRequired ) {
				SetTextCallback d = new SetTextCallback ( SetText );
				this.Invoke ( d , new object[] { text } );
			}
			else {
				this.textBox2.Text += text;
			}
		}
		public void listen()
		{
			listener = new TcpListener ( IPAddress.Parse ( this.textBox1.Text ) , 123 );
			listener.Start ();
			acceptClient(listener);
			int co = 0;
            
			//while (true)
			//{
				
				//var client = listener.AcceptTcpClient();
				//var stream =client.GetStream();
				//Image im = Image.FromStream ( stream );
				//setPic(im);
				//client.Close();
				co++;
			//}
			//clientsThreads = new List<Thread>();
			//while (true)
			//{
				
			//	var client = listener.AcceptTcpClient();
				
			//	var thread1 = new Thread(() => ClientF(client));
			//		clientsThreads.Add (thread1);
			//	thread1.Start();

			//}
		}

		Image _image;
		Image Image {
			get { return _image; }
			set
			{
				SetText( "imgSet" );
			//	textBox2.Text +=;
				_image = value;
			}
		}
		int count = 0;
		void acceptClient(TcpListener listener)
		{
			listener.BeginAcceptTcpClient ( ar => {
					var listenerTcp = (TcpListener) ar.AsyncState;
					var client = listenerTcp.EndAcceptTcpClient (ar);
                    
					acceptClient(listener);
					var stream = client.GetStream ();
					Image im = Image.FromStream ( stream );
				    frames++;
				//SetText("adding..."+im.GetHashCode()+Environment.NewLine);
					//_imagesBag.Add(im);
				    setPic( im);
				    //SetText(count.ToString());
				    //setPic ( im );
			} , listener );
		}

		ConcurrentBag<Image> _imagesBag;
		TcpListener server_socket;
		Socket client_socket;
		Thread video_thread;
		NetworkStream ns;
		private void startVideoConferencing () {
			try {
				server_socket = new TcpListener ( System.Net.IPAddress.Parse ( "192.168.0.105" ) , 123 );
				server_socket.Start ();
				client_socket = server_socket.AcceptSocket ();
				ns = new NetworkStream ( client_socket );
				pictureBox1.Image = Image.FromStream ( ns );
				server_socket.Stop ();

				if ( client_socket.Connected == true ) {
					while ( true ) {
						startVideoConferencing ();
					}
					ns.Flush ();
				}
			}
			catch ( Exception ex ) {
				button1.Enabled = true;
				video_thread.Abort ();
			}
		}

		private void buttonV_Click ( object sender , EventArgs e ) {
			button1.Enabled = false;
			video_thread = new Thread ( new ThreadStart ( startVideoConferencing ) );
			video_thread.Start ();
		}

		VideoServer s;
		private void Form1_Load ( object sender , EventArgs e ) {
			Timer timer = new Timer(1000);
			timer.Elapsed += timer_Elapsed;
			VideoServer server = new VideoServer("127.0.0.1",9991);
			
		}

		void timer_Elapsed ( object sender , System.Timers.ElapsedEventArgs e )
		{
			byte[] arr = new[] {new byte(), new byte()};
			s.NewFrame(arr);
		}
	}
}
