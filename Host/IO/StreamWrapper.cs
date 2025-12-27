using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Chameleon.Emulator.Core.IO;

namespace Chameleon.Host.IO
{
    class StreamWrapper : IStream
    {
        public StreamWrapper(Stream stream)
        {
            Stream = stream;
        }
        public int Read(byte[] buffer, int offset, uint count)
        {
            return Stream.Read(buffer, offset, (int)count);
        }

        private Stream Stream;

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Stream.Dispose();
                }

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}
