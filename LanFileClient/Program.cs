using System.Net.Sockets;
using System.Text;

var client = new TcpClient();
Console.WriteLine("/* LanFileSenRec - Client");
Console.WriteLine("   domer/PeripheralVisionPD2");
Console.WriteLine("   7 - 5 - 2024 */");
PORT_ENTER:
Console.Write("enter port number: ");
var port = Int32.Parse(Console.ReadLine());
if (port > 99999)
{
    Console.WriteLine("invalid port");
    Thread.Sleep(2000);
    Console.Clear();
    goto PORT_ENTER;
}
Console.Write("enter local ip of server: ");
var ip = Console.ReadLine();
Console.Write("enter pass of server: ");

string pass = Console.ReadLine();
List<string> sent = new List<string>();
foreach (string filename in Directory.GetFiles(@"C:\Users\Public\Documents\LFSR\Send"))
{
    byte[] magic = new byte[1];
    magic[0] = (byte)0x0;
    client = new TcpClient();
    client.Connect(ip, port);
    client.Client.Send(Encoding.UTF8.GetBytes(pass));
    client.Client.Receive(magic);
    if (magic[0] != (byte)0x1)
        return;
    client.Client.Disconnect(false);
    client = new TcpClient();

    if (sent.Contains(filename))
        break;
    sent.Add(filename);
    client = new TcpClient();
    client.Connect(ip, port);

    byte[] _file = File.ReadAllBytes(filename);
    byte[] _size = BitConverter.GetBytes(_file.Length);

    Console.WriteLine($"size of {filename} " + BitConverter.ToInt32(_size) + "bytes");
    if (client.Client.Send(_size) == 0)
    {
        Console.WriteLine("error sending file size to server");
        Console.ReadLine();
        return;
    }

    client.Client.Receive(magic);
    if (magic[0] != (byte)0x1)
        return;
    Console.WriteLine("[server reply]");
    client.Client.Disconnect(false);
    client = new TcpClient();
    client.Connect(ip, port);

    string _filename = filename.Replace("C:\\Users\\Public\\Documents\\LFSR\\Send\\", "");
    Console.WriteLine($"sending {_filename} to server");
    if (client.Client.Send(Encoding.UTF8.GetBytes(_filename)) == 0)
    {
        Console.WriteLine("error sending file name to server");
        Console.ReadLine();
        return;
    }
    client.Client.Receive(magic);
    if (magic[0] != (byte)0x1)
        return;
    Console.WriteLine("[server reply]");
    client.Client.Disconnect(false);
    client = new TcpClient();
    client.Connect(ip, port);

    if (client.Client.Send(_file) == 0)
        Console.WriteLine("error sending file");
    client.Client.Receive(magic);
    Console.WriteLine("[server reply]");
    if (magic[0] != (byte)0x1)
        return;
    client.Client.Disconnect(false);
}
Console.WriteLine("done. press enter to exit");
Console.ReadLine();
return;