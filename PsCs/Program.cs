using System;
using System.IO;
using PsCs.Hardware;

namespace PsCs
{
    class Program
    {
        static void Main(string[] args)
        {
            var bios = File.ReadAllBytes(args[0]);
            var bus = new Bus(bios);
            var cpu = new Cpu();

            while (true)
            {
                cpu.Cycle(bus);
            }
        }
    }
}