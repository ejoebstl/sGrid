// This source file is part of BoincGuiRpc.Net
//
// Author: 	    Emanuel Jöbstl <emi@eex-dev.net>
// Weblink: 	http://boincguirpc.codeplex.com
//
// Licensed under the MIT License
//
// (c) 2012-2013

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Boinc
{
    /// <summary>
    /// This classs provides a wrapper stream 
    /// which checks for the ASCII char 0x03 (End-Of-Text) in all read bytes and
    /// replaces them with space chars. 
    /// Also, all spaces before the closing slashes of XML tags are stripped. 
    /// </summary>
    class FilterStream : Stream
    {
        private Stream baseStream;
        private const byte toReplace = 0x03;
        private const byte replaceWith = (byte)' ';

        /// <summary>
        /// Creates a new instance of this class. 
        /// </summary>
        /// <param name="baseStream">The stream to invoke all operations on. </param>
        public FilterStream(Stream baseStream)
        {
            this.baseStream = baseStream;
        }

        /// <summary>
        /// Reads the given amount of bytes from the baseStream object and 
        /// replaces all occuring End-Of-Text chars with spaces. 
        /// </summary>
        /// <param name="buffer">The buffer to read into.</param>
        /// <param name="offset">The offset where copying into the buffer starts.</param>
        /// <param name="count">The count of bytes to read. </param>
        /// <returns>An int indicating how much bytes were read into the buffer.</returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            int value = baseStream.Read(buffer, offset, count);

            for (int i = offset; i < offset + count; i++)
            {
                if (buffer[i] == toReplace)
                    buffer[i] = replaceWith;
            }

            return value;
        }

        /// <summary>
        /// Strips all spaces before the closing slash of any html tag and then calls
        /// the Write method of the base stream.
        /// <remarks>
        /// The given data has to be convertible into an ASCII string. 
        /// If count is smaller than three, the data is passed to the base stream 
        /// without modifications. 
        /// </remarks>
        /// </summary>
        /// <param name="buffer">The buffer to write.</param>
        /// <param name="offset">The offset where copying from the buffer starts.</param>
        /// <param name="count">The count of bytes to write.</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (count > 3)
            {
                string data = Encoding.ASCII.GetString(buffer, offset, count);

                data = data.Replace(" />", "/>");
                buffer = Encoding.ASCII.GetBytes(data);

                baseStream.Write(buffer, 0, buffer.Length);
            }
            else
            {
                baseStream.Write(buffer, offset, count);
            }
        }


        // Notice: Comments are intentionally omitted, since
        // all methods below only call the same method with the same parameters on the
        // baseStream object. All Documentation is inherited. 

        /// <inheritdoc />
        public override bool CanRead
        {
            get { return baseStream.CanRead; }
        }

        /// <inheritdoc />
        public override bool CanSeek
        {
            get { return baseStream.CanSeek; }
        }

        /// <inheritdoc />
        public override bool CanWrite
        {
            get { return baseStream.CanWrite; }
        }

        /// <inheritdoc />
        public override void Flush()
        {
            baseStream.Flush();
        }

        /// <inheritdoc />
        public override long Length
        {
            get { return baseStream.Length; }
        }

        /// <inheritdoc />
        public override long Position
        {
            get
            {
                return baseStream.Position;
            }
            set
            {
                baseStream.Position = value;
            }
        }

        /// <inheritdoc />
        public override long Seek(long offset, SeekOrigin origin)
        {
            return baseStream.Seek(offset, origin);
        }

        /// <inheritdoc />
        public override void SetLength(long value)
        {
            baseStream.SetLength(value);
        }
    }
}
