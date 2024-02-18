namespace FSMViewAvalonia2.UEP;
internal static class UEPConnect
{
    public static ClientWebSocket client;

    public static async void Init(int port)
    {
        if (client != null)
        {
            await client.CloseAsync(WebSocketCloseStatus.Empty, null, CancellationToken.None);
        }

        client = new();
        await client.ConnectAsync(new Uri("ws://127.0.0.1:" + port + "/fsmviewer"), CancellationToken.None);
        while (client.State == WebSocketState.Open)
        {
            List<byte> buffer = [];

            Memory<byte> readBuffer = new(new byte[4096]);
            ValueWebSocketReceiveResult info;
            try
            {
                info = await client.ReceiveAsync(readBuffer, CancellationToken.None);
            } catch (WebSocketException)
            {
                continue;
            }

            while (!info.EndOfMessage)
            {
                buffer.AddRange(readBuffer[..info.Count].ToArray());
                info = await client.ReceiveAsync(readBuffer, CancellationToken.None);
            }

            buffer.AddRange(readBuffer[..info.Count].ToArray());

            string text = Encoding.UTF8.GetString(buffer.ToArray());
            string[] cmds = text.Split('\n');
            Program.ParseArgs(cmds);
        }
    }
    public static void Send(string text)
    {
        if (client?.State == WebSocketState.Open)
        {
            _ = (client?.SendAsync(Encoding.UTF8.GetBytes(text), WebSocketMessageType.Text, true, CancellationToken.None));
        }
    }
    public static bool UEPConnected => client?.State == WebSocketState.Open;
}
