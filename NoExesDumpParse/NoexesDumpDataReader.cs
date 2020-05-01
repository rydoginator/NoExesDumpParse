﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace NoExesDumpParse
{
    class NoexsDumpIndex
    {
        public long address;
        public long pos;
        public long size;
        public NoexsDumpIndex(long addrValue, long posValue, long sizeValue)
        {
            address = addrValue;
            pos = posValue;
            size = sizeValue;
        }
    }
    class NoexsMemoryInfo
    {
        public long address;
        public long size;
        public NoexsMemoryType type;
        public Int32 perm;
        public NoexsMemoryInfo(long addrValue, long sizeValue, Int32 typeValue, Int32 permValue)
        {
            address = addrValue;
            size = sizeValue;
            type = (NoexsMemoryType)Enum.ToObject(typeof(NoexsMemoryType), typeValue);
            perm = permValue;
        }
        public bool IsReadable()
        {
            return (perm & 1) != 0;
        }

        public bool IsWriteable()
        {
            return (perm & 2) != 0;
        }

        public bool IsExecutable()
        {
            return (perm & 4) != 0;
        }

        public bool Contains(long addr)
        {
            return (address <= addr) && (address + size > addr);
        }
    }
    class NoexsDumpDataReader : IDumpDataReader
    {

        private BinaryReader fileStream;
        private long mainStartAddress;
        private long mainEndAddress;
        private long heapStartAddress;
        private long heapEndAddress;
        private byte[] buffer = new byte[8];
        private List<NoexsDumpIndex> indices;
        private List<NoexsMemoryInfo> infos;
        private Dictionary<long, long> readData;    //key:address,value:data
        public NoexsDumpDataReader(String path, long mainStart, long mainEnd, long heapStart, long heapEnd)
        {
            fileStream = new BinaryReader(new FileStream(path, FileMode.Open, FileAccess.Read));
            mainStartAddress = mainStart;
            mainEndAddress = mainEnd;
            heapStartAddress = heapStart;
            heapEndAddress = heapEnd;
            buffer = new byte[8];
            indices = null;
            infos = new List<NoexsMemoryInfo>();
            readData = new Dictionary<long, long>();
        }
        ~NoexsDumpDataReader()
        {
            if (fileStream != null)
            {
                fileStream.Close();
            }
        }
        private void ReadData(int length)
        {
            fileStream.Read(buffer, 0, length);
        }
        private void ReverseEndian(int length)
        {
            Array.Reverse(buffer, 0, length);
        }
        private Int32 ReadBigEndianInt32()
        {
            ReadData(4);
            ReverseEndian(4);
            return BitConverter.ToInt32(buffer, 0);
        }
        private Int64 ReadBigEndianInt64()
        {
            ReadData(8);
            ReverseEndian(8);
            return BitConverter.ToInt64(buffer, 0);
        }
        private Int64 ReadLittleEndianInt64()
        {
            //ReadData(8);
            return fileStream.ReadInt64();//BitConverter.ToInt64(buffer, 0);
        }
        Int64 IDumpDataReader.ReadLittleEndianInt64(long address)
        {
            foreach (NoexsDumpIndex x in indices)
            {
                if ((x.address <= address) && (address + 7 <= x.address + x.size))
                {
                    fileStream.BaseStream.Seek(x.pos + address - x.address, SeekOrigin.Begin);
                    return fileStream.ReadInt64();
                }
            }
            return 0;
        }

        long IDumpDataReader.GetMain()
        {
            return mainStartAddress;
        }

        long IDumpDataReader.GetHeap()
        {
            return heapStartAddress;
        }

        private void ReadIndicate()
        {
            if (indices != null)
            {
                //if already read indices,skip reading
                return;
            }
            indices = new List<NoexsDumpIndex>();
            fileStream.BaseStream.Seek(0, SeekOrigin.Begin);

            if (ReadBigEndianInt32() != 0x4E444D50)
            {
                fileStream.Close();
                throw new Exception("illegal file format");
            }

            long tid = ReadBigEndianInt64(); // TID

            int infoCount = ReadBigEndianInt32();
            long infoPtr = ReadBigEndianInt64();

            int idxCount = ReadBigEndianInt32();
            long idxPtr = ReadBigEndianInt64();
            long dataPtr = fileStream.BaseStream.Position;
            fileStream.BaseStream.Seek(idxPtr, SeekOrigin.Begin);
            for (int i = 0; i < idxCount; i++)
            {
                long addr = ReadBigEndianInt64();
                long pos = ReadBigEndianInt64();
                long size = ReadBigEndianInt64();
                indices.Add(new NoexsDumpIndex(addr, pos, size));
            }
        }
        List<NoexsDumpIndex> IDumpDataReader.Read()
        {
            ReadIndicate();
            return indices;
        }
        /*
        long IDumpDataReader.TryToParseAbs(List<IReverseOrderPath> path)
        {
            ReadIndicate();

            long address = mainStartAddress;
            for (int i = path.Count - 1; i >= 0; i--)
            {
                long data;
                if (readData.ContainsKey(address))
                {
                    data = readData[address];
                }
                else
                {
                    data = ReadLittleEndianInt64(address);
                    readData.Add(address, data);
                }
                address = path[i].ParseAddress(address, data);
                if ((address == 0) || !IsMainHeapAddress(address))
                {
                    return 0;
                }
            }
            return address;
        }

        Address IDumpDataReader.TryToParseRel(List<IReverseOrderPath> path)
        {
            long address = ((IDumpDataReader)this).TryToParseAbs(path);
            MemoryType type = GetMemoryType(address);
            return new Address(type, address - GetStartAddress(type));
        }
        */
        bool IDumpDataReader.IsHeap(long address)
        {
            return IsHeapAddress(address);
        }
        private bool IsMainHeapAddress(long evalAddress)
        {
            if ((mainStartAddress <= evalAddress) && (evalAddress < mainEndAddress))
            {
                return true;
            }
            if ((heapStartAddress <= evalAddress) && (evalAddress < heapEndAddress))
            {
                return true;
            }
            return false;
        }
        private bool IsHeapAddress(long evalAddress)
        {
            if ((heapStartAddress <= evalAddress) && (evalAddress < heapEndAddress))
            {
                return true;
            }
            return false;
        }
        private MemoryType GetMemoryType(long address)
        {
            if ((mainStartAddress <= address) && (address < mainEndAddress))
            {
                return MemoryType.MAIN;
            }
            return MemoryType.HEAP;
        }
        private long GetStartAddress(MemoryType type)
        {
            if (type == MemoryType.MAIN)
            {
                return mainStartAddress;
            }
            return heapStartAddress;
        }
    }
}
