namespace Server
{
    using System.Net; // основное пространство имён для работы с сетевыми адресами и протоколами
    using System.Net.Sockets; // пространство имён для работы с сокетами
    using System.Text; // пространство имён для работы с кодировками
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
        private const int DEFAULT_BUFLEN = 512; // задаёт размер буфера для получения данных
                                                // если нужно работать с большим количеством данных, рекомендуется использовать буферы от 4 КБ до 64 КБ (размер, с которым обычно работают сетевые приложения)
                                                // если данные небольшие и ожидается, что они будут приходить в небольших объёмах, можно использовать буфер 512 байт или даже меньше
        private const string DEFAULT_PORT = "27015"; // указывает порт, на котором сервер будет прослушивать подключения
        private const int PAUSE = 0; // задаёт паузу в миллисекундах для красоты и удобства вывода сообщений (можно смело убрать)

        static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8; // кириллица
            Console.Title = "SERVER SIDE";
            Logger.LogInfo("Процесс сервера запущен!");
            Thread.Sleep(PAUSE);

            try
            {
                var ipAddress = IPAddress.Any; // получает любой доступный IP-адрес для прослушивания (означает, что сервер будет слушать на всех интерфейсах, например, Wi-Fi, Ethernet
                var localEndPoint = new IPEndPoint(ipAddress, int.Parse(DEFAULT_PORT)); // создаёт конечную точку (адрес и порт), к которой сервер будет привязан

                var listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); // создаёт сокет для использования TCP-соединения (потоковый сокет)
                listener.Bind(localEndPoint); // привязывает сокет к указанному адресу и порту

                Logger.LogSuccess("Получение адреса и порта сервера прошло успешно!");
                Thread.Sleep(PAUSE);

                listener.Listen(10); // начинает прослушивание входящих соединений, устанавливая максимальное количество ожидающих соединений (10), то есть сервер может иметь до 10 клиентов (соединений) в очереди на подключение
                Logger.LogNotice("Начинается прослушивание информации от клиента.\nПожалуйста, запустите клиентскую программу!");

                var clientSocket = listener.Accept(); // ожидает подключение клиента и принимает его, возвращая сокет для общения с клиентом. есть AcceptAsync(), чтоб не блокировать поток
                Logger.LogSuccess("Подключение с клиентской программой установлено успешно!");

                listener.Close(); // закрывает сокет слушателя, так как соединение с клиентом уже установлено
                                  // соединение с клиентом теперь управляется отдельным сокетом, полученным от метода Accept(), и слушающий сокет больше не нужен
                while (true)
                {
                    var buffer = new byte[DEFAULT_BUFLEN]; // создаёт буфер для хранения полученных данных
                    int bytesReceived = clientSocket.Receive(buffer); // получает данные от клиента и сохраняет их в буфер

                    if (bytesReceived > 0) // если данные были получены
                    {
                        string message = Encoding.UTF8.GetString(buffer, 0, bytesReceived);
                        Logger.Log($"Процесс клиента отправил сообщение: {message}"); // выводит полученное сообщение
                        Thread.Sleep(PAUSE); // делает паузу
                        int client_number = 0;
                        int.TryParse(message, out client_number);

                        string response = $"{client_number + 1}"; // формирует ответ для клиента
                        byte[] responseBytes = Encoding.UTF8.GetBytes(response); // преобразует ответ в массив байтов
                        clientSocket.Send(responseBytes); // отправляет ответ клиенту
                        Logger.Log($"Процесс сервера отправляет ответ: {response}");
                        Thread.Sleep(PAUSE);
                    }
                    else if (bytesReceived == 0) // если клиент закрыл соединение (получен 0 байтов)
                    {
                        Logger.LogWarning("Соединение закрывается..."); // информирует о том, что соединение будет закрыто
                        break;
                    }
                    else
                    {
                        Logger.LogError("Ошибка при получении данных.");
                        break;
                    }
                }

                clientSocket.Shutdown(SocketShutdown.Send); // закрывает сокет для отправки данных (клиент завершил отправку)
                clientSocket.Close(); // закрывает сокет для общения с клиентом
                Logger.LogNotice("Процесс сервера завершает свою работу!");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Произошла ошибка: {ex.Message}");
            }
        }
    }
}
