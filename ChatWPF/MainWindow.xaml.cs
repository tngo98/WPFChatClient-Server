using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net.Sockets;

namespace ChatWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //TCP Connection
        private Int32 port = 13000;
        private string server = "2607:fea8:1e60:eee:94e8:7dbd:c9c4:b63";
        private TcpClient client;
        private NetworkStream stream;
        private Thread serverListener;
        //Data members
        private string[] userIdDelimiter = new string[] { "/[+UserId+]/" };
        private bool MouseInLogs = false;
        private bool Scrolling = false;
        private bool continueListen = true;

        //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        //
        // METHOD		:	MainWindow()
        // DESCRIPTION	:	This method initializes the main window, starts the server 
        //                  and connects to the client
        // PARAMETERS   :   none
        // RETURNS		:	none
        //
        public MainWindow()
        {
            InitializeComponent();
            Closing += MainWindow_Close;

            //Assigns the method to the thread
            serverListener = new Thread(new ParameterizedThreadStart(ListenToServer));
            //Set thread working flag to true
            continueListen = true;
            //Start the thread
            serverListener.Start(stream);

            try
            {
                //Connect TCP client to server
                client = new TcpClient(server, port);
                //Setup service stream with client
                stream = client.GetStream();
            }
            catch (ArgumentNullException Ae)
            {
                MessageBox.Show(Ae.ToString());
            }
            catch (SocketException Se)
            {
                MessageBox.Show(Se.ToString());
            }
        }
        //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++



        //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        //
        // METHOD		:	MainWindow_Close()
        // DESCRIPTION	:	This method shuts down all the running functions and threads when the 
        //                  user exits the program
        // PARAMETERS   :   object sender       :   object that called the funtion
        //                  CancelEventArgs e   :   any arguments if an event is canceled
        // RETURNS		:	void
        //
        private void MainWindow_Close(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                //Set thread listener flag to false
                continueListen = false;
                //Close Stream 
                stream.Close();
                //Close Client
                client.Close();
            }
            catch (ArgumentNullException ARe)
            {
                MessageBox.Show(ARe.ToString());
            }
            catch (SocketException SKe)
            {
                MessageBox.Show(SKe.ToString());
            }
        }
        //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++



        //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        //
        // METHOD		:	Post_Message()
        // DESCRIPTION	:	This method connects the client to the server and displays the client's 
        //                  message in the chatbox
        // PARAMETERS   :   string message  :   message sent by the client
        //                  string userID   :   holds the user id 
        // RETURNS		:	void
        //
        private void Post_Message(string message, string userID)
        {
            try
            {
                // Translate the passed message into ASCII and store it as a Byte array.
                Byte[] data = System.Text.Encoding.ASCII.GetBytes(message);

                // Send the message to the connected TcpServer. 
                if (stream != null)
                {
                    stream.Write(data, 0, data.Length);
                }
                else
                {
                    MessageBox.Show("SERVER NOT ONLINE");
                }

                //Add the message to the chatlogs at the same time
                AddToChatLog(message, userID);
            }
            catch (ArgumentNullException ANe)
            {
                MessageBox.Show(ANe.ToString());
            }
            catch (SocketException SCKe)
            {
                MessageBox.Show(SCKe.ToString());
            }
        }
        //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++



        //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        //
        // METHOD		:	ListenToServer(object o)
        // DESCRIPTION	:	This method waits for readible data from the stream and converts it to 
        //                  a string and passes this to AddToChatLog function
        // PARAMETERS   :   object o    :   object that called the function 
        // RETURNS		:	void
        //
        private void ListenToServer(object o)
        {
            //Variables
            Int32 bytes;
            Byte[] data = new Byte[256];
            String responseData = String.Empty;

            while (continueListen)
            {
                //Reset variable values
                responseData = string.Empty;
                data = new byte[256];

                if (stream != null)
                {
                    if (stream.CanRead == true && stream.DataAvailable == true)
                    {
                        bytes = stream.Read(data, 0, data.Length);
                        //Convert bytes to string
                        responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                        //Parse the userID
                        string[] serverResponse = responseData.Split(userIdDelimiter, StringSplitOptions.None);
                        //Add the message to the chatbox
                        AddToChatLog(serverResponse[1], serverResponse[0]);
                    }
                }
            }
        }
        //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++



        //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        //
        // METHOD		:	AddToChatLog()
        // DESCRIPTION	:	This method initializes the main window, starts the server 
        //                  and connects to the client
        // PARAMETERS   :   string inputMessage  :  message to be added to the chat box
        //                  string senderID      :  the ID of the message sender
        // RETURNS		:	void
        //
        private void AddToChatLog(string inputMessage, string senderID)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                TextBlock newTxtBlck = new TextBlock();
                newTxtBlck.Text = inputMessage;

                //Adds message block to the chatbox
                chatBox.Items.Add(newTxtBlck);

                //Scroll chatbox keep latest messages in view
                if (!(MouseInLogs == true && Scrolling == true))
                {
                    chatBoxScroll.ScrollToEnd();
                }
            }));
        }
        //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++



        //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        //
        // METHOD		:	Return_Check()
        // DESCRIPTION	:	This method checks if the user has pressed the enter button, then passes 
        //                  the object to the appropriate event handler
        // PARAMETERS   :   object sender   :   object that called the function 
        //                  KeyEventArgs e  :   any arguments of key events
        // RETURNS		:	void
        //

        private void messageText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.sendMessageButton_Click(sender, e);
            }
        }
        //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++



        //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        //
        // METHOD		:	Send_Message()
        // DESCRIPTION	:	This method checks if the message is empty then adds it to the WPF 
        //                  chatbox and passes the massage to the appropriate event handler
        // PARAMETERS   :   object sender       :   object that called the function
        //                  RoutedEventArgs e   :   any arguments of routed events
        // RETURNS		:	void
        //
        private void sendMessageButton_Click(object sender, RoutedEventArgs e)
        {
            if (messageText.Text != "")
            {
                string message = messageText.Text;
                Post_Message(message, "You");
                messageText.Text = "";
            }
        }
        //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    }
}
