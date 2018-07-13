using Fleck;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FleckWebsocketRecorder
{
    class Program
    {
        static ManualResetEvent _quitEvent = new ManualResetEvent(false);
        static void Main(string[] args)
        {
            var server = new WebSocketServer("ws://0.0.0.0:8181");
            server.RestartAfterListenError = true;
            server.ListenerSocket.NoDelay = true;

            Console.WriteLine("Path to save files: ");
            string path = Console.ReadLine();

            if (Directory.Exists(path))
            {
                server.Start(socket =>
                {
                    Console.WriteLine("started!");
                    socket.OnOpen = () =>
                    {
                        Console.WriteLine("Open!");
                    };

                    socket.OnClose = () =>
                    {
                        Console.WriteLine("Close!");
                        var s = new RawSourceWaveStream(File.OpenRead($"{path}\\MyFile.raw"), new WaveFormat(16000, 1));
                        var outpath = $"{path}\\myFile.wav";
                        WaveFileWriter.CreateWaveFile(outpath, s);
                    };
                    socket.OnMessage = message => { Console.WriteLine(message); };
                    socket.OnBinary = binary =>
                    {
                        using (var fs = new FileStream($"{path}\\MyFile.raw", FileMode.Append, FileAccess.Write))
                        {
                           fs.Write(binary, 0, binary.Length);
                        }
                     };
                });
            }
            else
            {
                Console.WriteLine("Path not found");
            }

            _quitEvent.WaitOne();
        }
    }
    
}
