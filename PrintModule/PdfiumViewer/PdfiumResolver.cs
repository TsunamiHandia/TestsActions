

namespace PdfiumViewer
{
    public class PdfiumResolver
    {
        public static event PdfiumResolveEventHandler Resolve;

        private static void OnResolve(PdfiumResolveEventArgs e)
        {
            PdfiumResolver.Resolve?.Invoke(null, e);
        }

        internal static string GetPdfiumFileName()
        {
            PdfiumResolveEventArgs pdfiumResolveEventArgs = new PdfiumResolveEventArgs();
            OnResolve(pdfiumResolveEventArgs);
            return pdfiumResolveEventArgs.PdfiumFileName;
        }
    }
}
