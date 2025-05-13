namespace Client
{
    using System.Net.Sockets;
    using System.Net;
    using System.Text;
    using Spectre.Console;

    public static class Logger
    {
        public static void Log(string message)
        {
            string time = DateTime.Now.ToString("HH:mm:ss");
            Console.WriteLine($"[{time}] {message}");
            // AnsiConsole.MarkupInterpolated($"[[{time}]] {message} \n");
        }

        public static void LogInfo(string message)
        {
            string time = DateTime.Now.ToString("HH:mm:ss");
            AnsiConsole.MarkupInterpolated($"[[{time}]] [skyblue3]INFO[/]: {message} \n");
            // Console.ForegroundColor = ConsoleColor.Cyan;
            // Log(message);
            Console.ResetColor();
        }

        public static void LogNotice(string message)
        {
            string time = DateTime.Now.ToString("HH:mm:ss");
            AnsiConsole.MarkupInterpolated($"[[{time}]] [yellow3]NOTICE[/]: {message} \n");
            // Console.ForegroundColor = ConsoleColor.Cyan;
            // Log(message);
            Console.ResetColor();
        }

        public static void LogSuccess(string message)
        {
            string time = DateTime.Now.ToString("HH:mm:ss");
            AnsiConsole.MarkupInterpolated($"[[{time}]] [lime]SUCCESS[/]: {message} \n");
            // Console.ForegroundColor = ConsoleColor.Yellow;
            // Log("WARNING: " + message);
            Console.ResetColor();
        }

        public static void LogOK(string message)
        {
            LogSuccess(message);
        }

        public static void LogWarning(string message)
        {
            string time = DateTime.Now.ToString("HH:mm:ss");
            AnsiConsole.MarkupInterpolated($"[[{time}]] [maroon]WARNING[/]: {message} \n");
            // Console.ForegroundColor = ConsoleColor.Yellow;
            // Log("WARNING: " + message);
            Console.ResetColor();
        }

        public static void LogError(string message)
        {
            string time = DateTime.Now.ToString("HH:mm:ss");
            AnsiConsole.MarkupInterpolated($"[[{time}]] [red]ERROR[/]: {message} \n");
            // Console.ForegroundColor = ConsoleColor.Red;
            // Log("ERROR: " + message);
            Console.ResetColor();
        }
    }

    internal class Program
    {
        private const int DEFAULT_BUFLEN = 512;
        private const string DEFAULT_PORT = "27015";

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8; // кириллица
            Console.Title = "CLIENT SIDE";
            try
            {
                var ipAddress = IPAddress.Loopback; // IP-адрес локального хоста (127.0.0.1), который используется для подключения к серверу на текущем устройстве
                var remoteEndPoint = new IPEndPoint(ipAddress, int.Parse(DEFAULT_PORT));

                var clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                clientSocket.Connect(remoteEndPoint); // инициирует подключение клиента к серверу по указанному конечному пункту (IP-адрес и порт)
                Logger.LogOK("Подключение к серверу установлено.");

                Logger.LogNotice("Введите число!");
                var user_input = Console.ReadLine();
                string message = "";
                if (user_input != null) message = user_input;
                byte[] messageBytes = Encoding.UTF8.GetBytes(message);
                clientSocket.Send(messageBytes);
                Logger.Log($"Сообщение отправлено: {message}");

                var buffer = new byte[DEFAULT_BUFLEN];
                int bytesReceived = clientSocket.Receive(buffer);
                string response = Encoding.UTF8.GetString(buffer, 0, bytesReceived);
                Logger.Log($"Ответ от сервера: {response}");

                clientSocket.Shutdown(SocketShutdown.Send);
                clientSocket.Close();
                Logger.LogNotice("Соединение с сервером закрыто.");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Произошла ошибка: {ex.Message}");
            }
        }
    }
}
