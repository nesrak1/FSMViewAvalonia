

namespace FSMViewAvalonia2
{
    class Program
    {
        public const string PipeName = "FSMViewAvaloniaInstancePipe";
        public static Mutex mutex = new(false, "MutexLock." + PipeName);
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        public static void Main(string[] args)
        {
            if (!mutex.WaitOne(1))
            {
                using (NamedPipeClientStream pipe = new(PipeName))
                {
                    try
                    {
                        pipe.Connect(100);
                    }
                    catch (Exception)
                    {
                        Environment.Exit(0);
                    }
                    using (BinaryWriter bw = new(pipe))
                    {
                        if (args.Length > 0)
                        {
                            bw.Write((byte)args.Length);
                            foreach (var v in args)
                            {
                                bw.Write((byte)v.Length);
                                bw.Write(v.ToCharArray());
                            }
                        }
                        else
                        {
                            bw.Write((byte)0);
                        }
                        bw.Flush();
                        pipe.Flush();
                    }
                }
                Environment.Exit(0);
            }
            else
            {

                new Thread(() =>
                        {
                            while (App.mainWindow == null) Thread.Sleep(0);
                            new Thread(PipeThread)
                            {
                                IsBackground = true
                            }.Start();
                            ParseArgs(args);
                        })
                {
                    IsBackground = true
                }.Start();

                BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
            }
        }
        
        public static void PipeThread()
        {
            using(NamedPipeServerStream pipe = new(PipeName))
            {
                while(true)
                {
                    pipe.WaitForConnection();
                    Dispatcher.UIThread.Post(() => {
                        App.mainWindow.Topmost = true;
                        Thread.Sleep(10);
                        App.mainWindow.Topmost = false;
                    });
                    using (BinaryReader bw = new(pipe, Encoding.UTF8, true))
                    {
                        int argc = 0;
                        while ((argc = bw.ReadByte()) == -1)
                        {
                            Thread.Sleep(0);
                        }
                        if (argc == 0) break;
                        string[] args = new string[argc];
                        for (int i = 0; i < argc; i++)
                        {
                            int strLength;
                            while ((strLength = bw.ReadByte()) == -1)
                            {
                                Thread.Sleep(0);
                            }
                            args[i] = new(bw.ReadChars(strLength));
                        }
                        ParseArgs(args);
                    }
                    pipe.Disconnect();
                }
            }
        }
        public static void ParseArgs(string[] args)
        {
            Dispatcher.UIThread.Post(async () =>
            {
                if (args.Length < 2) return;
                var filename = args[0];
                if(!File.Exists(filename))
                {
                    filename = System.IO.Path.GetFileName(filename);
                    string gamePath = await GameFileHelper.FindHollowKnightPath(App.mainWindow);
                    if (gamePath == null)
                        return;

                    filename = GameFileHelper.FindGameFilePath(gamePath, filename);
                }
                if (!File.Exists(filename)) return;
                await App.mainWindow.LoadFsm(filename, args[1], args.Length != 2 && bool.Parse(args[2]));
            });
        }
        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace()
                ;
    }
}
