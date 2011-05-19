using System.IO;
using System.Text;
using System.Web;

namespace CompositeC1Contrib.Web
{
    public abstract class BaseResponseFilter : Stream
    {
        private Stream _innerStream;

        protected HttpContext Context { get; private set; }

        private long _position;
        public override long Position
        {
            get { return this._position; }
            set { this._position = value; }
        }

        public override bool CanRead
        {
            get { return _innerStream.CanRead; }
        }

        public override bool CanSeek
        {
            get { return _innerStream.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return _innerStream.CanWrite; }
        }

        public override long Length
        {
            get { return _innerStream.Length; }
        }

        public BaseResponseFilter(Stream responseStream, HttpContext ctx)
        {
            _innerStream = responseStream;
            Context = ctx;
        }

        public override void Close()
        {
            _innerStream.Close();
        }

        public override void Flush()
        {
            _innerStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _innerStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin direction)
        {
            return _innerStream.Seek(offset, direction);
        }

        public override void SetLength(long length)
        {
            _innerStream.SetLength(length);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            var encoding = Encoding.UTF8;

            var s = encoding.GetString(buffer, offset, count);
            s = Process(s);

            buffer = encoding.GetBytes(s);

            _innerStream.Write(buffer, 0, buffer.Length);
        }

        public abstract string Process(string s);
    }
}
