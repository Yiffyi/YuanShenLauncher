using System;
using System.IO;
using System.Net;

namespace Launcher
{
    internal class SeekableHTTPStream : Stream
    {
        private const int noDataAvaiable = 0;
        private bool isDisposed = false;
        private long position;
        private Lazy<long> length;
        private WebResponse baseResponse;

        public SeekableHTTPStream(string url)
        {
            Url = url;
            length = new Lazy<long>(() =>
            {
                HttpWebRequest request = WebRequest.CreateHttp(Url);
                request.Method = "HEAD";
                return request.GetResponse().ContentLength;
            });

            issueRequest(0);
        }

        public void issueRequest(long begin)
        {
            if (baseResponse != null)
            {
                baseResponse.Close();
                baseResponse = null;
            }
            HttpWebRequest baseRequest = WebRequest.CreateHttp(Url);
            if (begin > 0) baseRequest.AddRange(begin);
            baseResponse = baseRequest.GetResponse();
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
                return length.Value;
            }
        }

        public override long Position
        {
            get
            {
                EnsureNotDisposed();
                return position;
            }
            set
            {
                EnsureNotDisposed();
                EnsurePositiv(value, "Position");
                position = value;
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
            issueRequest(offset);
            return Position = offset;
        }

        public override void Close()
        {
            // EnsureNotDisposed();

            base.Close();
            if (baseResponse != null)
            {
                baseResponse.Close();
                baseResponse = null;
            }
            isDisposed = true;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            EnsureNotDisposed();

            EnsureNotNull(buffer, "buffer");
            EnsurePositiv(offset, "offset");
            EnsurePositiv(count, "count");

            if (buffer.Length - offset < count) { throw new ArgumentException("count"); }

            if (Position >= Length) { return noDataAvaiable; }

            if (Position + count > Length)
            {
                count = (int)(Length - Position);
            }

            int bytesRead = baseResponse.GetResponseStream().Read(buffer, offset, count);

            Position += bytesRead;

            return bytesRead;

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
