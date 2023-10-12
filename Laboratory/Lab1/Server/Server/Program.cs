class PipeServer
{
    static void Main()
    {
        // Открытие каналов
        using NamedPipeServerStream pipeServer = new("channel", PipeDirection.InOut);
        pipeServer.WaitForConnection();

        int ID = 1;
        for (double X = 0.5; X < 100.0; X ++)
        {
            // Создание сообщения, преобразование в byte
            DataRequest msg = new()
            {
                Id = ID++,
                X = X
            };

            byte[] bytes = new byte[Unsafe.SizeOf<DataRequest>()];
            Unsafe.As<byte, DataRequest>(ref bytes[0]) = msg;
            pipeServer.Write(bytes, 0, bytes.Length);

            Console.WriteLine($"Server(SEND). Send request: Id = {msg.Id}, X = {msg.X}");

            // Получение обновленных данных от клиента
            byte[] received_bytes = new byte[Unsafe.SizeOf<DataResponse>()];
            pipeServer.Read(received_bytes, 0, received_bytes.Length);

            DataResponse received_data = Unsafe.As<byte, DataResponse>(ref received_bytes[0]);
            Console.WriteLine($"Server(GET): Id = {received_data.Id}, X = {received_data.X}, Result = {received_data.Result}\n");
            Thread.Sleep(2000);
        }
    }
}
