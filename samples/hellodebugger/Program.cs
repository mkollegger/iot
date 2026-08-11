#region License

// MIT License
// 
// Copyright (C) 2026 Michael Kollegger
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#endregion

using System.Diagnostics;

namespace hellopi
{
    /// <summary>
    ///     
    /// </summary>
    internal static class Program
    {
        /// <summary>
        ///     Haupteinstiegspunkt des Programms.
        ///     Unterstützt einen optionalen --debug Parameter für Remote-Debugging.
        /// </summary>
        /// <param name="args">Kommandozeilenargumente.</param>
        static async Task Main(string[] args)
        {
            var dbg = args.Any(a => a.Equals("--debug", StringComparison.OrdinalIgnoreCase));
            Console.WriteLine("Hello, World@Remote Debugger with C#!");

            if (!dbg)
            {
                Console.WriteLine("Starte ohne Debugger.");
                Console.WriteLine("Programm beendet.");
                return;
            }

            Console.Write("Warte auf Debugger ...");

            // Warten bis Debugger angehängt ist (wichtig für Remote-Debugging via SSH)
            int count = 0;
            while (!Debugger.IsAttached)
            {
                Thread.Sleep(100);
                count++;
                if (count % 10 == 0)
                {
                    Console.Write(".");
                }

                if (count == 1200)
                {
                    Console.WriteLine("\nZeitüberschreitung beim Warten auf Debugger.");
                    break;
                }
            }

            Console.WriteLine();

            if (Debugger.IsAttached)
            {
                Console.WriteLine("Debugger angehängt.");
                Debugger.Break();
            }

            Console.WriteLine("Programm beendet.");
        }

       
    }
}