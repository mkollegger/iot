using System.Diagnostics;

namespace hellopi
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
            Console.Write ("Warte auf Debugger ...");

            // Warten bis Debugger angehängt ist
            int count = 0;
            while (!Debugger.IsAttached)
            {
                Thread.Sleep(100);
                count++;
                if (count % 10 == 0)
                {
                    Console.Write(".");
                }

                if (count == 600)
                {
                    Console.WriteLine("\nZeitüberschreitung beim Warten auf Debugger.");
                    break;
                }
            }
            Console.WriteLine();


            if (Debugger.IsAttached)
            {
                Console.WriteLine("Debugger angehängt. Fortfahren...");
                Debugger.Break();
            }

            // Hier können Sie Ihren Code hinzufügen, der debuggt werden soll

            Console.WriteLine("Programm beendet. Drücken Sie eine Taste zum Beenden.");
        }
    }
}
