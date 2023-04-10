

using System;

namespace PdfiumViewer
{
    internal class PdfLibrary : IDisposable
    {
        private static readonly object _syncRoot = new object();

        private static PdfLibrary _library;

        private bool _disposed;

        public static void EnsureLoaded()
        {
            lock (_syncRoot)
            {
                if (_library == null)
                {
                    _library = new PdfLibrary();
                }
            }
        }

        private PdfLibrary()
        {
            NativeMethods.FPDF_AddRef();
        }

        ~PdfLibrary()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                NativeMethods.FPDF_Release();
                _disposed = true;
            }
        }
    }
}
