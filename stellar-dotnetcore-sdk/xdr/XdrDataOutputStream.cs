﻿using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Text;

namespace stellar_dotnetcore_sdk.xdr
{
    public class XdrDataOutputStream
    {
        private readonly List<byte> _bytes;

        private readonly byte[][] _tails = {
            null,
            new byte[] { 0x00},
            new byte[] { 0x00, 0x00},
            new byte[] { 0x00, 0x00, 0x00}
        };

        public XdrDataOutputStream()
        {
            _bytes = new List<byte>();
        }

        public void Write(byte b)
        {
            _bytes.Add(b);
        }

        public void Write(byte[] bytes)
        {
            _bytes.AddRange(bytes);
        }

        public void Write(byte[] bytes, int offset, int count)
        {
            var newBytes = new byte[count];
            Array.Copy(bytes, offset, newBytes, 0, count);

            _bytes.AddRange(newBytes);
        }

        public void WriteString(string str)
        {
            WriteVarOpaque((uint)str.Length, Encoding.UTF8.GetBytes(str));
        }

        public void WriteIntArray(int[] a)
        {
            WriteInt(a.Length);
            WriteIntArray(a, a.Length);
        }

        private void WriteIntArray(int[] a, int l)
        {
            for (var i = 0; i < l; i++)
                WriteInt(a[i]);
        }

        public void WriteLong(long v)
        {
            Write((byte)((v >> 56) & 0xff));
            Write((byte)((v >> 48) & 0xff));
            Write((byte)((v >> 40) & 0xff));
            Write((byte)((v >> 32) & 0xff));
            Write((byte)((v >> 24) & 0xff));
            Write((byte)((v >> 16) & 0xff));
            Write((byte)((v >> 8) & 0xff));
            Write((byte)(v & 0xff));
        }

        public void WriteInt(int i)
        {
            Write((byte)((i >> 0x18) & 0xff));
            Write((byte)((i >> 0x10) & 0xff));
            Write((byte)((i >> 8) & 0xff));
            Write((byte)(i & 0xff));

        }

        public void WriteUInt(uint i)
        {
            Write((byte)((i >> 0x18) & 0xff));
            Write((byte)((i >> 0x10) & 0xff));
            Write((byte)((i >> 8) & 0xff));
            Write((byte)(i & 0xff));
        }

        private unsafe void WriteSingle(float v)
        {
            WriteInt(*(int*)(&v));
        }

        public void WriteSingleArray(float[] a)
        {
            WriteInt(a.Length);
            WriteSingleArray(a, a.Length);
        }

        private void WriteSingleArray(float[] a, int l)
        {
            for (var i = 0; i < l; i++)
                WriteSingle(a[i]);
        }

        private unsafe void WriteDouble(double v)
        {
            WriteLong(*(long*)(&v));
        }

        public void WriteDoubleArray(double[] a)
        {
            WriteInt(a.Length);
            WriteDoubleArray(a, a.Length);
        }

        private void WriteDoubleArray(double[] a, int l)
        {
            for (var i = 0; i < l; i++)
                WriteDouble(a[i]);

        }

        public byte[] ToArray()
        {
            return _bytes.ToArray();
        }

        public void WriteVarOpaque(uint max, byte[] v)
        {
            uint len = (uint)v.LongLength;
            if (len > max)
                throw new FormatException("unexpected length: " + len.ToString());

            try
            {
                WriteUInt(len);
            }
            catch (SystemException ex)
            {
                throw new FormatException("can't write length", ex);
            }
            NoCheckWriteFixOpaque(len, v);
        }



        private void NoCheckWriteFixOpaque(uint len, byte[] v)
        {
            try
            {
                Write(v);
                uint tail = len % 4u;
                if (tail != 0)
                    Write(_tails[4u - tail]);
            }
            catch (SystemException ex)
            {
                throw new FormatException("can't write byte array", ex);
            }
        }
    }

}