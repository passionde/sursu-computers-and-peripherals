public delegate double Function(double x);

public class TrapezoidalRule
{
    public static double Integrate(Function f, double a, double b, double epsilon)
    {
        int n = 1;
        double previousResult, currentResult;

        currentResult = (b - a) * (f(a) + f(b)) / 2;

        do
        {
            n *= 2;
            double h = (b - a) / n;
            previousResult = currentResult;
            currentResult = 0.5 * (f(a) + f(b));

            for (int i = 1; i < n; i++)
            {
                currentResult += f(a + i * h);
            }

            currentResult *= h;

        } while (Math.Abs(currentResult - previousResult) > epsilon);

        return currentResult;
    }
}


class PipeClient
{
    static void Main(string[] args)
    {
        if (args.Length < 1) { return; }

        Function f = x => 2 * x * x;
        using NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", args[0], PipeDirection.InOut);
        pipeClient.Connect();

        // Получение данных от сервера
        byte[] bytes = new byte[Unsafe.SizeOf<DataRequest>()];
        try 
        {
            pipeClient.Read(bytes);
        }
        catch (IOException) { return; }

        DataRequest received_data = Unsafe.As<byte, DataRequest>(ref bytes[0]);

        DataResponse response_data = new()
        {
            Id = received_data.Id,
            Result = TrapezoidalRule.Integrate(f, received_data.A, received_data.B, 0.0000001)
        };

        // Отправка обновленных данных обратно на сервер
        byte[] response_bytes = new byte[Unsafe.SizeOf<DataResponse>()];
        Unsafe.As<byte, DataResponse>(ref response_bytes[0]) = response_data;

        try
        {
            pipeClient.Write(response_bytes);
        }
        catch (IOException) { return; }
    }
}
