﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SniffExplorer.Core.Packets.Types;

namespace SniffExplorer.Core.Packets.Parsing
{
    public sealed class PacketReader : BinaryReader
    {
        private int _dataSize;

        private byte _bitpos = 8;
        private byte _curbitval;

        public PacketReader(Stream baseStream, int dataSize) : base(baseStream, Encoding.UTF8, false)
        {
            _dataSize = dataSize;
        }

        private void CheckValid(int size)
        {
            if (_dataSize < size)
                throw new InvalidOperationException(nameof(size));

            if (size < 0)
                throw new ArgumentOutOfRangeException(nameof(size));

            _dataSize -= size;
        }

        #region Readers
        public new long ReadInt64()
        {
            ResetBitReader();
            CheckValid(sizeof(long));
            return base.ReadInt64();
        }

        public new ulong ReadUInt64()
        {
            ResetBitReader();
            CheckValid(sizeof(ulong));
            return base.ReadUInt64();
        }

        public new int ReadInt32()
        {
            ResetBitReader();
            CheckValid(sizeof(int));
            return base.ReadInt32();
        }

        public new uint ReadUInt32()
        {
            ResetBitReader();
            CheckValid(sizeof(uint));
            return base.ReadUInt32();
        }

        public new short ReadInt16()
        {
            CheckValid(sizeof(short));
            return base.ReadInt16();
        }

        public new ushort ReadUInt16()
        {
            ResetBitReader();
            CheckValid(sizeof(ushort));
            return base.ReadUInt16();
        }

        public new byte ReadByte()
        {
            ResetBitReader();
            CheckValid(sizeof(byte));
            return base.ReadByte();
        }

        public new sbyte ReadSByte()
        {
            ResetBitReader();
            CheckValid(sizeof(sbyte));
            return base.ReadSByte();
        }

        public new float ReadSingle()
        {
            ResetBitReader();
            CheckValid(sizeof(float));
            return base.ReadSingle();
        }

        public new double ReadDouble()
        {
            ResetBitReader();
            CheckValid(sizeof(double));
            return base.ReadDouble();
        }

        public new char ReadChar()
        {
            ResetBitReader();
            CheckValid(1);
            return (char) base.ReadByte();
        }

        public void ResetBitReader()
        {
            _bitpos = 8;
        }

        public bool ReadBit()
        {
            ++_bitpos;

            if (_bitpos > 7)
            {
                _bitpos = 0;
                CheckValid(sizeof(byte));
                _curbitval = base.ReadByte();
            }

            return ((_curbitval >> (7 - _bitpos)) & 1) != 0;
        }

        public int ReadBits(int bits)
        {
            var value = 0;
            for (var i = bits - 1; i >= 0; --i)
                if (ReadBit())
                    value |= (int)(1 << i);

            return value;
        }

        public DateTime ReadTime() => new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(ReadInt32());

        public DateTime ReadPackedTime()
        {
            var packedDate = ReadInt32();
            return new DateTime(2000, 1, 1)
                .AddYears((packedDate >> 24) & 0x1F)
                .AddMonths((packedDate >> 20) & 0xF)
                .AddDays((packedDate >> 14) & 0x3F)
                .AddHours((packedDate >> 6) & 0x1F)
                .AddMinutes(packedDate & 0x3F);
        }

        public string ReadCString()
        {
            var bytes = new List<byte>();

            byte b;
            while ((b = ReadByte()) != 0)  // CDataStore::GetCString calls CanRead too
                bytes.Add(b);

            return Encoding.UTF8.GetString(bytes.ToArray());
        }

        public string ReadString(int stringSize)
        {
            CheckValid(stringSize);
            var stringValue = Encoding.UTF8.GetString(ReadBytes(stringSize));
            return stringValue;
        }

        public ulong ReadPackedUInt64()
        {
            return ReadPackedUInt64(ReadByte());
        }

        public ulong ReadPackedUInt64(byte mask)
        {
            if (mask == 0)
                return 0;

            ulong res = 0;

            var i = 0;
            while (i < 8)
            {
                if ((mask & 1 << i) != 0)
                    res += (ulong)ReadByte() << (i * 8);

                i++;
            }

            return res;
        }

        public T ReadGUID<T>() where T : IObjectGuid, new()
        {
            var guid = new T();
            guid.Read(this);
            return guid;
        }

        public T ReadPackedGUID<T>() where T : IObjectGuid, new()
        {
            var guid = new T();
            guid.ReadPacked(this);
            return guid;
        }
        #endregion
    }
}
