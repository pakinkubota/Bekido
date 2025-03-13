using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FGL;

namespace UpdataDataHour
{
    class Script
    {
        static List<Task> tasks;

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetStdHandle(int nStdHandle);

        const int STD_INPUT_HANDLE = -10;
        const uint ENABLE_QUICK_EDIT_MODE = 0x0040;
        const uint ENABLE_INSERT_MODE = 0x0020;
        const uint ENABLE_EXTENDED_FLAGS = 0x0080;

        static void DisableQuickEditMode()
        {
            IntPtr consoleHandle = GetStdHandle(STD_INPUT_HANDLE);

            // get current console mode
            uint consoleMode;
            if (!GetConsoleMode(consoleHandle, out consoleMode))
            {
                // error getting the console mode. Exit.
                return;
            }

            // Clear the quick edit bit in the mode flags
            consoleMode &= ~ENABLE_QUICK_EDIT_MODE;
            consoleMode |= ENABLE_EXTENDED_FLAGS;

            // set the new mode
            SetConsoleMode(consoleHandle, consoleMode);
        }

        static Task Main(string[] args)
        {
            DisableQuickEditMode();

            int[] scheduledLineCodeArray = { 990000, 990001 };
            int[] scheduledLineCodeArrayCell = { 920602, 920604, 920606, 920608 };
            String[] pickingLampStaionArrayCell = { "920L01", "920L02", "920L03", "920L04" };
            TimeSpan timeFinish = DateTime.Parse("21:30:00").TimeOfDay;
            TimeSpan timeStart = DateTime.Parse("07:20:00").TimeOfDay;
            while (true)
            {
                TimeSpan now = DateTime.Now.TimeOfDay;
                if (now < timeFinish && now > timeStart)
                {
                    DateTime start = DateTime.Now;
                    Console.WriteLine($"Update at {start}");
                    tasks = new List<Task>();
                    foreach (int i in scheduledLineCodeArray)
                        tasks.Add(UpdataDataBekido(i, "n"));
                    for (int i = 0; i < scheduledLineCodeArrayCell.Length; i++)
                        tasks.Add(UpdataDataBekido(scheduledLineCodeArrayCell[i], "c", pickingLampStaionArrayCell[i]));
                    Task.WaitAll(tasks.ToArray());
                    Console.WriteLine($"Take {(DateTime.Now - start).TotalSeconds.ToString("0.0000")} Sec");
                    Console.WriteLine($"+++++++++++++++++++++++++++++++++++");

                }
                Thread.Sleep(10000);
            }
        }

        static async Task UpdataDataBekido(int scheduledLineCode, string mode, string pickingLampStaion = "-")
        {
            await Task.Run(() =>
            {
                try
                {
                    DateTime start = DateTime.Now;
                    DataHour dataHour = new DataHour(scheduledLineCode, mode, pickingLampStaion);
                    dataHour.UpdateData();
                    Console.WriteLine($"{scheduledLineCode} finished ({(DateTime.Now - start).TotalSeconds.ToString("0.0000")} sec)");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            });

        }
    }
}
