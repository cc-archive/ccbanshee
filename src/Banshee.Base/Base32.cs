/***************************************************************************
 *  Base32.cs
 *
 *  Copyright (C) 2006 Luke Hoersten
 *  Written by Luke Hoersten <luke.hoersten@gmail.com>
 ****************************************************************************/
 
using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace Banshee.Base
{
    public class Base32
    {
        private const int IN_BYTE_SIZE = 8;
        private const int OUT_BYTE_SIZE = 5;
        private static char[] alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567".ToCharArray();
        private string base32_string;        
        
        public Base32(byte[] data)
        {
            base32_string = Encode(data);
        }
        
        /* Based on Java code by Robert Kaye and Gordon Mohr
         * (PD) 2006 The Bitzi Corporation (http://bitzi.com/publicdomain)
         * (RFC http://www.faqs.org/rfcs/rfc3548.html)
         */
        static public string Encode(byte[] data) {
            int i = 0, index = 0, digit = 0;
            int currByte, nextByte;
            StringBuilder result = new StringBuilder((data.Length + 7) * IN_BYTE_SIZE / OUT_BYTE_SIZE);
            while(i < data.Length) {
                currByte = (data[i] >= 0) ? data[i] : (data[i] + 256); // Is unsigning needed?

                /* Is the current digit going to span a byte boundary? */
                if(index > (IN_BYTE_SIZE - OUT_BYTE_SIZE)) {
                    if ((i + 1) < data.Length) {
                        nextByte = (data[i + 1] >= 0) ? data[i + 1] : (data[i + 1] + 256);
                    } else {
                        nextByte = 0;
                    }

                    digit = currByte & (0xFF >> index);
                    index = (index + OUT_BYTE_SIZE) % IN_BYTE_SIZE;
                    digit <<= index;
                    digit |= nextByte >> (IN_BYTE_SIZE - index);
                    i++;
                } else {
                    digit = (currByte >> (IN_BYTE_SIZE - (index + OUT_BYTE_SIZE))) & 0x1F;
                    index = (index + OUT_BYTE_SIZE) % IN_BYTE_SIZE;
                    if (index == 0)
                        i++;
                }
                result.Append(alphabet[digit]);
            }

            return result.ToString();
        }
        
        public override string ToString() {
            return base32_string;
        }
        
        public static void Main(string[] args) {
            SHA1Managed hasher = new SHA1Managed();
            Base32 base32 = new Base32(hasher.ComputeHash(File.OpenRead(args[0])));
            Console.WriteLine("Hash: \"{0}\"", base32.ToString());            
        }
    }
}
