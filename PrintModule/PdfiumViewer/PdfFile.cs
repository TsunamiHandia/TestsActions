

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace PdfiumViewer
{
    internal class PdfFile : IDisposable
    {
        private class PageData : IDisposable
        {
            private readonly IntPtr _form;

            private bool _disposed;

            public IntPtr Page { get; private set; }

            public IntPtr TextPage { get; private set; }

            public double Width { get; private set; }

            public double Height { get; private set; }

            public PageData(IntPtr document, IntPtr form, int pageNumber)
            {
                _form = form;
                Page = NativeMethods.FPDF_LoadPage(document, pageNumber);
                TextPage = NativeMethods.FPDFText_LoadPage(Page);
                NativeMethods.FORM_OnAfterLoadPage(Page, form);
                NativeMethods.FORM_DoPageAAction(Page, form, NativeMethods.FPDFPAGE_AACTION.OPEN);
                Width = NativeMethods.FPDF_GetPageWidth(Page);
                Height = NativeMethods.FPDF_GetPageHeight(Page);
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    NativeMethods.FORM_DoPageAAction(Page, _form, NativeMethods.FPDFPAGE_AACTION.CLOSE);
                    NativeMethods.FORM_OnBeforeClosePage(Page, _form);
                    NativeMethods.FPDFText_ClosePage(TextPage);
                    NativeMethods.FPDF_ClosePage(Page);
                    _disposed = true;
                }
            }
        }

        private static readonly Encoding FPDFEncoding = new UnicodeEncoding(bigEndian: false, byteOrderMark: false, throwOnInvalidBytes: false);

        private IntPtr _document;

        private IntPtr _form;

        private bool _disposed;

        private NativeMethods.FPDF_FORMFILLINFO _formCallbacks;

        private GCHandle _formCallbacksHandle;

        private readonly int _id;

        private Stream _stream;

       // public PdfBookmarkCollection Bookmarks { get; private set; }

        public PdfFile(Stream stream, string password)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            PdfLibrary.EnsureLoaded();
            _stream = stream;
            _id = StreamManager.Register(stream);
            IntPtr intPtr = NativeMethods.FPDF_LoadCustomDocument(stream, password, _id);
            if (intPtr == IntPtr.Zero)
            {
                throw new PdfException((PdfError)NativeMethods.FPDF_GetLastError());
            }

            LoadDocument(intPtr);
        }

        public bool RenderPDFPageToDC(int pageNumber, IntPtr dc, int dpiX, int dpiY, int boundsOriginX, int boundsOriginY, int boundsWidth, int boundsHeight, NativeMethods.FPDF flags)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }

            using (PageData pageData = new PageData(_document, _form, pageNumber))
            {
                NativeMethods.FPDF_RenderPage(dc, pageData.Page, boundsOriginX, boundsOriginY, boundsWidth, boundsHeight, 0, flags);
            }

            return true;
        }

        public bool RenderPDFPageToBitmap(int pageNumber, IntPtr bitmapHandle, int dpiX, int dpiY, int boundsOriginX, int boundsOriginY, int boundsWidth, int boundsHeight, int rotate, NativeMethods.FPDF flags, bool renderFormFill)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }

            using (PageData pageData = new PageData(_document, _form, pageNumber))
            {
                if (renderFormFill)
                {
                    flags &= ~NativeMethods.FPDF.ANNOT;
                }

                NativeMethods.FPDF_RenderPageBitmap(bitmapHandle, pageData.Page, boundsOriginX, boundsOriginY, boundsWidth, boundsHeight, rotate, flags);
                if (renderFormFill)
                {
                    NativeMethods.FPDF_FFLDraw(_form, bitmapHandle, pageData.Page, boundsOriginX, boundsOriginY, boundsWidth, boundsHeight, rotate, flags);
                }
            }

            return true;
        }

        
        public List<SizeF> GetPDFDocInfo()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }

            int num = NativeMethods.FPDF_GetPageCount(_document);
            List<SizeF> list = new List<SizeF>(num);
            for (int i = 0; i < num; i++)
            {
                list.Add(GetPDFDocInfo(i));
            }

            return list;
        }

        public SizeF GetPDFDocInfo(int pageNumber)
        {
            NativeMethods.FPDF_GetPageSizeByIndex(_document, pageNumber, out var width, out var height);
            return new SizeF((float)width, (float)height);
        }

        public void Save(Stream stream)
        {
            NativeMethods.FPDF_SaveAsCopy(_document, stream, NativeMethods.FPDF_SAVE_FLAGS.FPDF_NO_INCREMENTAL);
        }

        protected void LoadDocument(IntPtr document)
        {
            _document = document;
            NativeMethods.FPDF_GetDocPermissions(_document);
            _formCallbacks = new NativeMethods.FPDF_FORMFILLINFO();
            _formCallbacksHandle = GCHandle.Alloc(_formCallbacks, GCHandleType.Pinned);
            for (int i = 1; i <= 2; i++)
            {
                _formCallbacks.version = i;
                _form = NativeMethods.FPDFDOC_InitFormFillEnvironment(_document, _formCallbacks);
                if (_form != IntPtr.Zero)
                {
                    break;
                }
            }

            NativeMethods.FPDF_SetFormFieldHighlightColor(_form, 0, 16770269u);
            NativeMethods.FPDF_SetFormFieldHighlightAlpha(_form, 100);
            NativeMethods.FORM_DoDocumentJSAction(_form);
            NativeMethods.FORM_DoDocumentOpenAction(_form);            
        }


        public IList<PdfRectangle> GetTextBounds(PdfTextSpan textSpan)
        {
            using PageData pageData = new PageData(_document, _form, textSpan.Page);
            return GetTextBounds(pageData.TextPage, textSpan.Page, textSpan.Offset, textSpan.Length);
        }

        public Point PointFromPdf(int page, PointF point)
        {
            using PageData pageData = new PageData(_document, _form, page);
            NativeMethods.FPDF_PageToDevice(pageData.Page, 0, 0, (int)pageData.Width, (int)pageData.Height, 0, point.X, point.Y, out var device_x, out var device_y);
            return new Point(device_x, device_y);
        }

        public Rectangle RectangleFromPdf(int page, RectangleF rect)
        {
            using PageData pageData = new PageData(_document, _form, page);
            NativeMethods.FPDF_PageToDevice(pageData.Page, 0, 0, (int)pageData.Width, (int)pageData.Height, 0, rect.Left, rect.Top, out var device_x, out var device_y);
            NativeMethods.FPDF_PageToDevice(pageData.Page, 0, 0, (int)pageData.Width, (int)pageData.Height, 0, rect.Right, rect.Bottom, out var device_x2, out var device_y2);
            return new Rectangle(device_x, device_y, device_x2 - device_x, device_y2 - device_y);
        }

        public PointF PointToPdf(int page, Point point)
        {
            using PageData pageData = new PageData(_document, _form, page);
            NativeMethods.FPDF_DeviceToPage(pageData.Page, 0, 0, (int)pageData.Width, (int)pageData.Height, 0, point.X, point.Y, out var page_x, out var page_y);
            return new PointF((float)page_x, (float)page_y);
        }

        public RectangleF RectangleToPdf(int page, Rectangle rect)
        {
            using PageData pageData = new PageData(_document, _form, page);
            NativeMethods.FPDF_DeviceToPage(pageData.Page, 0, 0, (int)pageData.Width, (int)pageData.Height, 0, rect.Left, rect.Top, out var page_x, out var page_y);
            NativeMethods.FPDF_DeviceToPage(pageData.Page, 0, 0, (int)pageData.Width, (int)pageData.Height, 0, rect.Right, rect.Bottom, out var page_x2, out var page_y2);
            return new RectangleF((float)page_x, (float)page_y, (float)(page_x2 - page_x), (float)(page_y2 - page_y));
        }

        private IList<PdfRectangle> GetTextBounds(IntPtr textPage, int page, int index, int matchLength)
        {
            List<PdfRectangle> list = new List<PdfRectangle>();
            RectangleF? rectangleF = null;
            for (int i = 0; i < matchLength; i++)
            {
                RectangleF bounds = GetBounds(textPage, index + i);
                if (bounds.Width != 0f && bounds.Height != 0f)
                {
                    if (rectangleF.HasValue && AreClose(rectangleF.Value.Right, bounds.Left) && AreClose(rectangleF.Value.Top, bounds.Top) && AreClose(rectangleF.Value.Bottom, bounds.Bottom))
                    {
                        float num = Math.Max(rectangleF.Value.Top, bounds.Top);
                        float num2 = Math.Min(rectangleF.Value.Bottom, bounds.Bottom);
                        rectangleF = new RectangleF(rectangleF.Value.Left, num, bounds.Right - rectangleF.Value.Left, num2 - num);
                        list[list.Count - 1] = new PdfRectangle(page, rectangleF.Value);
                    }
                    else
                    {
                        rectangleF = bounds;
                        list.Add(new PdfRectangle(page, bounds));
                    }
                }
            }

            return list;
        }

        private bool AreClose(float p1, float p2)
        {
            return Math.Abs(p1 - p2) < 4f;
        }

        private RectangleF GetBounds(IntPtr textPage, int index)
        {
            NativeMethods.FPDFText_GetCharBox(textPage, index, out var left, out var right, out var bottom, out var top);
            return new RectangleF((float)left, (float)top, (float)(right - left), (float)(bottom - top));
        }

        public string GetPdfText(int page)
        {
            using PageData pageData = new PageData(_document, _form, page);
            int length = NativeMethods.FPDFText_CountChars(pageData.TextPage);
            return GetPdfText(pageData, new PdfTextSpan(page, 0, length));
        }

        public string GetPdfText(PdfTextSpan textSpan)
        {
            using PageData pageData = new PageData(_document, _form, textSpan.Page);
            return GetPdfText(pageData, textSpan);
        }

        private string GetPdfText(PageData pageData, PdfTextSpan textSpan)
        {
            byte[] array = new byte[(textSpan.Length + 1) * 2];
            NativeMethods.FPDFText_GetText(pageData.TextPage, textSpan.Offset, textSpan.Length, array);
            return FPDFEncoding.GetString(array, 0, textSpan.Length * 2);
        }

        public void DeletePage(int pageNumber)
        {
            NativeMethods.FPDFPage_Delete(_document, pageNumber);
        }

        private string GetMetaText(string tag)
        {
            uint num = NativeMethods.FPDF_GetMetaText(_document, tag, null, 0u);
            if (num <= 2)
            {
                return string.Empty;
            }

            byte[] array = new byte[num];
            NativeMethods.FPDF_GetMetaText(_document, tag, array, num);
            return Encoding.Unicode.GetString(array, 0, (int)(num - 2));
        }

        public DateTime? GetMetaTextAsDate(string tag)
        {
            string metaText = GetMetaText(tag);
            if (string.IsNullOrEmpty(metaText))
            {
                return null;
            }

            Match match = new Regex("(?:D:)(?<year>\\d\\d\\d\\d)(?<month>\\d\\d)(?<day>\\d\\d)(?<hour>\\d\\d)(?<minute>\\d\\d)(?<second>\\d\\d)(?<tz_offset>[+-zZ])?(?<tz_hour>\\d\\d)?'?(?<tz_minute>\\d\\d)?'?").Match(metaText);
            if (match.Success)
            {
                string value = match.Groups["year"].Value;
                string value2 = match.Groups["month"].Value;
                string value3 = match.Groups["day"].Value;
                string value4 = match.Groups["hour"].Value;
                string value5 = match.Groups["minute"].Value;
                string value6 = match.Groups["second"].Value;
                string text = match.Groups["tz_offset"]?.Value;
                string arg = match.Groups["tz_hour"]?.Value;
                string arg2 = match.Groups["tz_minute"]?.Value;
                string text2 = $"{value}-{value2}-{value3}T{value4}:{value5}:{value6}.0000000";
                if (!string.IsNullOrEmpty(text))
                {
                    switch (text)
                    {
                        case "Z":
                        case "z":
                            text2 += "+0";
                            break;
                        case "+":
                        case "-":
                            text2 += $"{text}{arg}:{arg2}";
                            break;
                    }
                }

                try
                {
                    return DateTime.Parse(text2);
                }
                catch (FormatException)
                {
                    return null;
                }
            }

            return null;
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                StreamManager.Unregister(_id);
                if (_form != IntPtr.Zero)
                {
                    NativeMethods.FORM_DoDocumentAAction(_form, NativeMethods.FPDFDOC_AACTION.WC);
                    NativeMethods.FPDFDOC_ExitFormFillEnvironment(_form);
                    _form = IntPtr.Zero;
                }

                if (_document != IntPtr.Zero)
                {
                    NativeMethods.FPDF_CloseDocument(_document);
                    _document = IntPtr.Zero;
                }

                if (_formCallbacksHandle.IsAllocated)
                {
                    _formCallbacksHandle.Free();
                }

                if (_stream != null)
                {
                    _stream.Dispose();
                    _stream = null;
                }

                _disposed = true;
            }
        }
    }
}
