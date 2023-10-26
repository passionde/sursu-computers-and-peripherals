class Program
{
    static Task Main()
    {
        PipeServer pipeServer = new PipeServer();
        return pipeServer.Start();
    }
}

class PipeServer
{
    private PriorityQueue<DataRequest, int> dataQueue = new PriorityQueue<DataRequest, int>();
    StreamWriter file = new StreamWriter("output.txt");
    private Mutex mutQueue = new Mutex();
    private Mutex mutFile = new Mutex();

    public async Task Start()
    {
        CancellationTokenSource cts = new CancellationTokenSource();

        Task getArgsTasksWorker = GetArgsTasksWorker(cts.Token);
        Task hangleTaskWorker = HandleTaskWorker(cts.Token);

        // Обработка Ctrl + C
        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

        await Task.WhenAll(getArgsTasksWorker, hangleTaskWorker);
        file.Close();
    }

    private Task GetArgsTasksWorker(CancellationToken cancellationToken)
    {
        int ID = 1;

        return Task.Run(() =>
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                Console.Write("\n\nEnter A, B (double) and priority (int) (Ctrl+C to stop): ");

                if (TryParseInput(Console.ReadLine(), out double A, out double B, out int priority))
                {
                    DataRequest msg = new DataRequest()
                    {
                        Id = ID++,
                        A = A,
                        B = B
                    };

                    mutQueue.WaitOne();
                    dataQueue.Enqueue(msg, priority);
                    mutQueue.ReleaseMutex();

                    Console.WriteLine($"\nSend request: Id = {msg.Id}, A = {msg.A}, B = {msg.B}, Priority = {priority}");
                }
                else
                {
                    Console.WriteLine("\nInvalid input. Please enter a valid double A, B and int priority.");
                }
            }
        });
    }

    private Task HandleTaskWorker(CancellationToken cancellationToken)
    {
        return Task.Run(() =>
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                bool result;
                DataRequest msg;

                mutQueue.WaitOne();
                result = dataQueue.TryDequeue(out msg, out _);
                mutQueue.ReleaseMutex();

                if (!result)
                {
                    continue;
                }

                HandleTaskAsync(msg, cancellationToken);
            }
        });
    }

    private async void HandleTaskAsync(DataRequest msg, CancellationToken cancellationToken)
    {
        try
        {
            // Запуск процесса
            Process myProcess = new Process();
            myProcess.StartInfo.FileName = "C:\\Develop\\rmlater\\sursu-computers-and-peripherals\\Laboratory\\Lab3\\Client\\Client\\bin\\Debug\\net7.0\\Client.exe";
            myProcess.StartInfo.Arguments = $"channel{msg.Id}";
            myProcess.StartInfo.UseShellExecute = false;
            myProcess.StartInfo.CreateNoWindow = true;
            myProcess.Start();

            await using NamedPipeServerStream pipeServer = new NamedPipeServerStream($"channel{msg.Id}", PipeDirection.InOut);

            await pipeServer.WaitForConnectionAsync(cancellationToken);

            byte[] bytes = new byte[Unsafe.SizeOf<DataRequest>()];
            Unsafe.As<byte, DataRequest>(ref bytes[0]) = msg;
            await pipeServer.WriteAsync(bytes, cancellationToken);

            byte[] received_bytes = new byte[Unsafe.SizeOf<DataResponse>()];
            await pipeServer.ReadAsync(received_bytes, cancellationToken);

            DataResponse received_data = Unsafe.As<byte, DataResponse>(ref received_bytes[0]);

            mutFile.WaitOne();
            file.WriteLine($"Id: {received_data.Id}, A: {msg.A}, B: {msg.B}, Result: {received_data.Result}");
            mutFile.ReleaseMutex();

            await myProcess.WaitForExitAsync(cancellationToken);
        }
        catch (Exception) { }
    }

    private bool TryParseInput(string? input, out double A, out double B, out int priority)
    {
        A = 0.0;
        B = 0.0;
        priority = 0;

        if (input == null)
        {
            return false;
        }

        string[] parts = input.Split(' ');
        if (parts.Length >= 2 && double.TryParse(parts[0], out A) && double.TryParse(parts[1], out B))
        {
            if (parts.Length >= 3 && int.TryParse(parts[2], out priority))
            {
                return true;
            }

            return true;
        }
        return false;
    }
}