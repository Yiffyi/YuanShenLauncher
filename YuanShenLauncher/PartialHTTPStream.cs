using System;
using System.IO;
using System.Net;

namespace Launcher
{
    internal class PartialHTTPStream : Stream
    {
        private readonly long cacheLength = 1024;
        private const int noDataAvaiable = 0;
        private MemoryStream stream = null;
        private long currentChunkNumber = -1;
        private long? length;
        private bool isDisposed = false;

        public PartialHTTPStream(string url)
            : this(url, 1024) { }

        public PartialHTTPStream(string url, long cacheLength)
        {
            if (cacheLength > 0) { this.cacheLength = cacheLength; }
            Url = url;
        }

        public string Url { get; private set; }

        public override bool CanRead
        {
            get
            {
                EnsureNotDisposed();
                return true;
            }
        }

        public override bool CanWrite
        {
            get
            {
                EnsureNotDisposed();
                return false;
            }
        }

        public override bool CanSeek
        {
            get
            {
                EnsureNotDisposed();
                return true;
            }
        }

        public override long Length
        {
            get
            {
                EnsureNotDisposed();
                if (length == null)
                {
                    HttpWebRequest request = WebRequest.CreateHttp(Url);
                    request.Method = "HEAD";
                    length = request.GetResponse().ContentLength;
                }
                return length.Value;
            }
        }

        public override long Position
        {
            get
            {
                EnsureNotDisposed();
                long streamPosition = (stream != null) ? stream.Position : 0;
                long position = (currentChunkNumber != -1) ? currentChunkNumber * cacheLength : 0;

                return position + streamPosition;
            }
            set
            {
                EnsureNotDisposed();
                EnsurePositiv(value, "Position");
                Seek(value);
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            EnsureNotDisposed();
            switch (origin)
            {
                case SeekOrigin.Begin:
                    break;
                case SeekOrigin.Current:
                    offset = Position + offset;
                    break;
                case SeekOrigin.End:
                    offset = Length + offset;
                    break;
                default:
                    break;
            }

            return Seek(offset);
        }

        private long Seek(long offset)
        {
            long chunkNumber = offset / cacheLength;

            if (currentChunkNumber != chunkNumber)
            {
                ReadChunk(chunkNumber);
                currentChunkNumber = chunkNumber;
            }

            offset -= currentChunkNumber * cacheLength;

            stream.Seek(offset, SeekOrigin.Begin);

            return Position;
        }

        private void ReadNextChunk()
        {
            currentChunkNumber += 1;
            ReadChunk(currentChunkNumber);
        }

        private void ReadChunk(long chunkNumberToRead)
        {
            long rangeStart = chunkNumberToRead * cacheLength;

            if (rangeStart > Length) { return; }

            long rangeEnd = rangeStart + cacheLength - 1;
            if (rangeStart + cacheLength > Length)
            {
                rangeEnd = Length - 1;
            }

            if (stream != null) { stream.Close(); }
            stream = new MemoryStream((int)cacheLength);

            HttpWebRequest request = WebRequest.CreateHttp(Url);
            request.AddRange(rangeStart, rangeEnd);

            using (WebResponse response = request.GetResponse())
            {
                response.GetResponseStream().CopyTo(stream);
            }

            stream.Position = 0;
        }

        public override void Close()
        {
            EnsureNotDisposed();

            base.Close();
            if (stream != null) { stream.Close(); }
            isDisposed = true;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            EnsureNotDisposed();

            EnsureNotNull(buffer, "buffer");
            EnsurePositiv(offset, "offset");
            EnsurePositiv(count, "count");

            if (buffer.Length - offset < count) { throw new ArgumentException("count"); }

            if (stream == null) { ReadNextChunk(); }

            if (Position >= Length) { return noDataAvaiable; }

            if (Position + count > Length)
            {
                count = (int)(Length - Position);
            }

            int bytesRead = stream.Read(buffer, offset, count);
            int totalBytesRead = bytesRead;
            count -= bytesRead;

            while (count > noDataAvaiable)
            {
                ReadNextChunk();
                offset += bytesRead;
                bytesRead = stream.Read(buffer, offset, count);
                count -= bytesRead;
                totalBytesRead += bytesRead;
            }

            return totalBytesRead;

        }

        public override void SetLength(long value)
        {
            EnsureNotDisposed();
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            EnsureNotDisposed();
            throw new NotImplementedException();
        }

        public override void Flush()
        {
            EnsureNotDisposed();
        }

        private void EnsureNotNull(object obj, string name)
        {
            if (obj != null) { return; }
            throw new ArgumentNullException(name);
        }
        private void EnsureNotDisposed()
        {
            if (!isDisposed) { return; }
            throw new ObjectDisposedException("PartialHTTPStream");
        }
        private void EnsurePositiv(int value, string name)
        {
            if (value > -1) { return; }
            throw new ArgumentOutOfRangeException(name);
        }
        private void EnsurePositiv(long value, string name)
        {
            if (value > -1) { return; }
            throw new ArgumentOutOfRangeException(name);
        }
        private void EnsureNegativ(long value, string name)
        {
            if (value < 0) { return; }
            throw new ArgumentOutOfRangeException(name);
        }
    }
}
