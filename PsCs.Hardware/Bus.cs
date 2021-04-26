using System;

namespace PsCs.Hardware
{
    internal class MemoryRegion
    {
        public readonly uint Start;
        public readonly uint Size;

        public MemoryRegion(uint start, uint size)
        {
            Start = start;
            Size = size;
        }

        public bool Contains(uint address)
        {
            return address >= Start && address < Start + Size;
        }

        public int Map(uint address)
        {
            return (int) (address - Start);
        }
    }

    internal static class MemoryMap
    {
        public static readonly MemoryRegion Ram = new MemoryRegion(0x00000000, 2048 * 1024);
        public static readonly MemoryRegion Scratchpad = new MemoryRegion(0x1F800000, 1 * 1024);
        public static readonly MemoryRegion Bios = new MemoryRegion(0x1FC00000, 512 * 1024);
        public static readonly MemoryRegion IoPorts = new MemoryRegion(0x1F801000, 8 * 1024);
        public static readonly MemoryRegion CacheControl = new MemoryRegion(0xFFFE0000, 512);
        public static readonly MemoryRegion Expansion1 = new MemoryRegion(0x1F000000, 8192 * 1024);
        public static readonly MemoryRegion Expansion2 = new MemoryRegion(0x1F802000, 8 * 1024);
        public static readonly MemoryRegion Expansion3 = new MemoryRegion(0x1FA00000, 2048 * 1024);
    }

    public class Bus
    {
        private readonly byte[] _ram;
        private readonly byte[] _scratchpad;
        private readonly byte[] _ioPorts;
        private readonly byte[] _bios;
        private readonly byte[] _cacheControl;
        private readonly byte[] _expansion1;
        private readonly byte[] _expansion2;
        private readonly byte[] _expansion3;

        public Bus(byte[] bios)
        {
            _bios = bios;
            _ram = new byte[MemoryMap.Ram.Size];
            _scratchpad = new byte[MemoryMap.Scratchpad.Size];
            _ioPorts = new byte[MemoryMap.IoPorts.Size];
            _cacheControl = new byte[MemoryMap.CacheControl.Size];
            _expansion1 = new byte[MemoryMap.Expansion1.Size];
            _expansion2 = new byte[MemoryMap.Expansion2.Size];
            _expansion3 = new byte[MemoryMap.Expansion3.Size];
        }

        private Span<byte> Map(uint address)
        {
            if (MemoryMap.CacheControl.Contains(address))
            {
                return _cacheControl.AsSpan(MemoryMap.CacheControl.Map(address));
            }

            address = address & 0x1FFFFFFF;

            if (MemoryMap.Ram.Contains(address))
                return _ram.AsSpan(MemoryMap.Ram.Map(address));
            if (MemoryMap.Scratchpad.Contains(address))
                return _scratchpad.AsSpan(MemoryMap.Scratchpad.Map(address));
            if (MemoryMap.IoPorts.Contains(address))
                return _ioPorts.AsSpan(MemoryMap.IoPorts.Map(address));
            if (MemoryMap.Bios.Contains(address))
                return _bios.AsSpan(MemoryMap.Bios.Map(address));
            if (MemoryMap.Expansion1.Contains(address))
                return _expansion1.AsSpan(MemoryMap.Expansion1.Map(address));
            if (MemoryMap.Expansion2.Contains(address))
                return _expansion2.AsSpan(MemoryMap.Expansion2.Map(address));
            if (MemoryMap.Expansion3.Contains(address))
                return _expansion3.AsSpan(MemoryMap.Expansion3.Map(address));

            throw new InvalidCodeException($"Unknown memory region at {address:X8}");
        }

        public byte LoadByte(uint address)
        {
            var data = Map(address);
            return data[0];
        }

        public ushort LoadHalfword(uint address)
        {
            var data = Map(address);
            ushort a = data[1];
            ushort b = data[0];
            return (ushort) ((a << 8) | b);
        }

        public uint LoadWord(uint address)
        {
            var data = Map(address);
            uint a = data[3];
            uint b = data[2];
            uint c = data[1];
            uint d = data[0];
            return (a << 24) | (b << 16) | (c << 8) | d;
        }

        public void StoreByte(uint address, byte value)
        {
            var data = Map(address);
            data[0] = value;
        }

        public void StoreHalfword(uint address, ushort value)
        {
            var data = Map(address);
            data[1] = (byte) (value >> 8);
            data[0] = (byte) (value);
        }

        public void StoreWord(uint address, uint value)
        {
            var data = Map(address);

            data[3] = (byte) (value >> 24);
            data[2] = (byte) (value >> 16);
            data[1] = (byte) (value >> 8);
            data[0] = (byte) (value);
        }
    }
}