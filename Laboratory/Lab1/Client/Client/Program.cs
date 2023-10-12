class PipeClient
{
    static void Main()
    {
        using NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", "channel", PipeDirection.InOut);
        pipeClient.Connect();

        while (pipeClient.IsConnected)
        {
            // Получение данных от сервера
            byte[] bytes = new byte[Unsafe.SizeOf<DataRequest>()];
            try 
            {
                pipeClient.Read(bytes, 0, bytes.Length);
            }
            catch (IOException) { break; }

            DataRequest received_data = Unsafe.As<byte, DataRequest>(ref bytes[0]);
            Console.WriteLine($"Client(GET). Get request: ID = {received_data.Id}, X = {received_data.X}");

            // Изменение флага
            DataResponse response_data = new()
            {
                Id = received_data.Id,
                X = received_data.X,
                Result = received_data.X * received_data.X
            };

            // Отправка обновленных данных обратно на сервер
            byte[] response_bytes = new byte[Unsafe.SizeOf<DataResponse>()];
            Unsafe.As<byte, DataResponse>(ref response_bytes[0]) = response_data;

            try
            {
                pipeClient.Write(response_bytes, 0, response_bytes.Length);
            }
            catch (IOException) { break; }
            
            Console.WriteLine($"Client(SEND). Send response: ID = {response_data.Id}, X = {response_data.X}, Result = {response_data.Result}\n");
        }
        Console.WriteLine("Disconnected from the server.");
    }
}
