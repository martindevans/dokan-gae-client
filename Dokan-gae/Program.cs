using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Dokan;

namespace Dokan_gae
{
    static class Program
    {
        static void Main(string[] args)
        {
            GaeFs fs;

            Thread t = new Thread(() =>
            {
                int error = DokanNet.DokanMain(new DokanOptions()
                {
                    DebugMode = true,
                    DriveLetter = 'g',
                    NetworkDrive = false,
                    UseStdErr = true,
                    VolumeLabel = "GAE-FS",
                    ThreadCount = 1,
                }, fs = new GaeFs("http://localhost:8086"));

                Console.WriteLine("Dokan code = " + error);
            });
            t.Start();

            Console.WriteLine("Press any key to unmount and exit");

            Console.ReadLine();

            while (!t.Join(500))
            {
                Console.WriteLine("Waiting for Dokan to terminate");
            }
        }

        public static DateTime AsUnixTime(this long timestamp)
        {
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return epoch.AddSeconds(timestamp);
        }
    }
}
