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
        Console.WriteLine("   7 - 2 - 2024 */");
        if (!Directory.Exists(@"C:\Users\Public\Documents\LFSR\Received"))
            Directory.CreateDirectory(@"C:\Users\Public\Documents\LFSR\Received");
        try
        {
        PORT_ENTER:
            Console.Write("enter port number: ");
            var port = Int32.Parse(Console.ReadLine());
            Console.Write("enter ip: ");
            IPAddress ip = IPAddress.Parse(Console.ReadLine());
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
                            Console.WriteLine("reading size...");
                            client.Receive(_sdata);
                            size = BitConverter.ToInt32(_sdata);
                            Console.WriteLine(size + " bytes");
                            tracker++;
                            client.Send(awaiting);
                            client.Close();
                            return;

                        case 1:
                            Console.WriteLine("reading filename...");
                            client.Receive(_ndata);
                            filename = Encoding.UTF8.GetString(_ndata).Replace("\0", "");
                            Console.WriteLine(filename);
                            tracker++;
                            client.Send(awaiting);
                            client.Close();
                            return;

                        case 2:
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

            listener.Stop();
        }
        catch (Exception e)
        {
            Console.WriteLine("error: " + e.StackTrace);
            Console.ReadLine();
        }
    }
}