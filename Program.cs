using System;
using System.Threading.Tasks;

namespace LeagueLobby
{
    internal static class Program
    {
        [STAThread]
        static async Task Main()
        {
            TeammateFinder finder = TeammateFinder.Instance;
            Console.WriteLine("\nPress Enter to see who is in your lobby...");

            while (true)
            {
                if (Console.KeyAvailable)
                {
                    switch (Console.ReadKey().Key)
                    {
                        case ConsoleKey.Enter:
                        {
                            string teammates = await finder.GetTeammatesNames();
                            Console.WriteLine(teammates);
                            Console.WriteLine("Copy the names to OP.gg to see your teammates stats");
                            break;
                        }
                        case ConsoleKey.Escape:
                            Console.WriteLine("\nExiting the program...");
                            return;
                    }
                }

                System.Threading.Thread.Sleep(1000);
            }
        }
    }
}