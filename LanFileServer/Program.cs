using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

public class ServerApp
{
    private static string SecureRandomString(int length)
    {
        using (var rng = new RNGCryptoServiceProvider())
        {
            var bytes = new byte[length];
            rng.GetBytes(bytes);
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(bytes.Select(b => chars[b % chars.Length]).ToArray());
        }
    }

    public static void Main()
    {
        Console.WriteLine("/* LanFileSenRec - Server");
        Console.WriteLine("   domer/PeripheralVisionPD2");
        Console.WriteLine("   7 - 5 - 2024 */");
        if (!Directory.Exists(@"C:\Users\Public\Documents\LFSR\Received"))
            Directory.CreateDirectory(@"C:\Users\Public\Documents\LFSR\Received");
        try
        {
        PORT_ENTER:
            Console.Write("enter port number: ");
            var port = Int32.Parse(Console.ReadLine());
            Console.Write("enter ip: ");
            IPAddress ip = IPAddress.Parse(Console.ReadLine());

            Console.Write("password [leave blank for none]: ");
            string password = Console.ReadLine();
            Console.WriteLine(password);
            if (port > 99999)
            {
                Console.WriteLine("invalid port");
                Thread.Sleep(2000);
                Console.Clear();
                goto PORT_ENTER;
            }
            TcpListener listener = new TcpListener(ip, port);
            listener.Start();
            Console.WriteLine("server is listening on " + listener.LocalEndpoint);

            int tracker = 0;
            int size = 0;
            string filename = "";
            byte[] _sdata = new byte[1024];
            byte[] _pdata = new byte[1024];
            byte[] _ndata = new byte[1024];
            byte[] awaiting = new byte[1];
            awaiting[0] = (byte)0x1;

            while (true)
            {
                Socket client = listener.AcceptSocket();
                Console.WriteLine("[packet accepted]");

                var childSocketThread = new Thread(() =>
                {
                    switch (tracker)
                    {
                        case 0:
                            int bufsize = client.Receive(_pdata);
                            string pass = Encoding.UTF8.GetString(_pdata, 0, bufsize);
                            if (password != pass)
                            {
                                Console.WriteLine("invalid password...");
                                tracker = 0;
                                filename = "";
                                awaiting[0] = 0x0;
                                client.Send(awaiting);
                                client.Close();
                                return;
                            }

                            tracker++;
                            client.Send(awaiting);
                            client.Close();
                            return;

                        case 1:
                            Console.WriteLine("reading size...");
                            client.Receive(_sdata);
                            size = BitConverter.ToInt32(_sdata);
                            Console.WriteLine(size + " bytes");
                            tracker++;
                            client.Send(awaiting);
                            client.Close();
                            return;

                        case 2:
                            Console.WriteLine("reading filename...");
                            int reqsize = client.Receive(_ndata);
                            filename = Encoding.UTF8.GetString(_ndata, 0, reqsize);
                            Console.WriteLine(filename);
                            tracker++;
                            client.Send(awaiting);
                            client.Close();
                            return;

                        case 3:
                            Console.WriteLine($"writing C:\\Users\\Public\\Documents\\LFSR\\Received\\{filename} bytes...");
                            byte[] _rdata = new byte[size];
                            client.Receive(_rdata);
                            File.WriteAllBytes($"C:\\Users\\Public\\Documents\\LFSR\\Received\\{filename}", _rdata);
                            Console.WriteLine($"done writing {filename}");
                            tracker = 0;
                            filename = "";
                            client.Send(awaiting);
                            client.Close();

                            return;
                    }
                });

                childSocketThread.Start();
            }
        }
        catch (Exception e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[error]: " + e.StackTrace);
            Console.ForegroundColor = ConsoleColor.White;
            Console.ReadLine();
        }
    }
}