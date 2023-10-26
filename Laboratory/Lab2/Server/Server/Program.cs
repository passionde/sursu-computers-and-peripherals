class Program
{
    static Task Main()
    {
        PipeServer pipeServer = new PipeServer("channel");
        return pipeServer.Start();
    }
}

class PipeServer
{
    private NamedPipeServerStream pipeServer;
    private PriorityQueue<DataRequest, int> dataQueue = new PriorityQueue<DataRequest, int>();

    private Mutex mutQueue = new Mutex();
    private Mutex mutFile = new Mutex();

    public PipeServer(string pipeName)
    {
        pipeServer = new NamedPipeServerStream(pipeName, PipeDirection.InOut);
    }

    public async Task Start()
    {
        CancellationTokenSource cts = new CancellationTokenSource();

        Console.WriteLine("Waiting for connection...");
        pipeServer.WaitForConnection();

        Task getArgsTasksWorker = GetArgsTasksWorker(cts.Token);
        Task hangleTaskWorker = HandleTaskWorker(cts.Token);

        // Обработка Ctrl + C
        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

        await Task.WhenAll(getArgsTasksWorker, hangleTaskWorker);

        await pipeServer.DisposeAsync();
    }

    private Task GetArgsTasksWorker(CancellationToken cancellationToken)
    {
        int ID = 1;

        return Task.Run(() =>
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                Console.Write("\n\nEnter X (double) and priority (int) (Ctrl+C to stop): ");

                if (TryParseInput(Console.ReadLine(), out double X, out int priority))
                {
                    DataRequest msg = new DataRequest()
                    {
                        Id = ID++,
                        X = X
                    };

                    mutQueue.WaitOne();
                    dataQueue.Enqueue(msg, priority);
                    mutQueue.ReleaseMutex();

                    Console.WriteLine($"\nSend request: Id = {msg.Id}, X = {msg.X}, Priority = {priority}");
                }
                else
                {
                    Console.WriteLine("\nInvalid input. Please enter a valid double X and int priority.");
                }
            }
        });
    }

    private async Task HandleTaskWorker(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested || dataQueue.Count > 0)
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

            byte[] bytes = new byte[Unsafe.SizeOf<DataRequest>()];
            Unsafe.As<byte, DataRequest>(ref bytes[0]) = msg;
            await pipeServer.WriteAsync(bytes);

            byte[] received_bytes = new byte[Unsafe.SizeOf<DataResponse>()];
            await pipeServer.ReadAsync(received_bytes, 0, received_bytes.Length);

            DataResponse received_data = Unsafe.As<byte, DataResponse>(ref received_bytes[0]);

            mutFile.WaitOne();
            File.AppendAllText("output.txt", $"Id: {received_data.Id}, X: {received_data.X}, Result: {received_data.Result}\n");
            mutFile.ReleaseMutex();
        }
    }

    private bool TryParseInput(string input, out double X, out int priority)
    {
        X = 0.0;
        priority = 0;

        if (input == null)
        {
            return false;
        }

        string[] parts = input.Split(' ');
        if (parts.Length >= 1 && double.TryParse(parts[0], out X))
        {
            if (parts.Length >= 2 && int.TryParse(parts[1], out priority))
            {
                return true;
            }

            return true;
        }
        return false;
    }
}