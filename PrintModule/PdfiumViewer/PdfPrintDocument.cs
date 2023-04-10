
using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Runtime.Versioning;

namespace PdfiumViewer
{
    [SupportedOSPlatform("windows")]
    internal class PdfPrintDocument : PrintDocument
    {
        private enum Orientation
        {
            Portrait,
            Landscape
        }

        private readonly IPdfDocument _document;

        private readonly PdfPrintSettings _settings;

        private int _currentPage;

        public event QueryPageSettingsEventHandler BeforeQueryPageSettings;

        public event PrintPageEventHandler BeforePrintPage;

        protected virtual void OnBeforeQueryPageSettings(QueryPageSettingsEventArgs e)
        {
            this.BeforeQueryPageSettings?.Invoke(this, e);
        }

        protected virtual void OnBeforePrintPage(PrintPageEventArgs e)
        {
            this.BeforePrintPage?.Invoke(this, e);
        }

        public PdfPrintDocument(IPdfDocument document, PdfPrintSettings settings)
        {
            if (document == null)
            {
                throw new ArgumentNullException("document");
            }

            _document = document;
            _settings = settings;
        }

        protected override void OnBeginPrint(PrintEventArgs e)
        {
            _currentPage = ((base.PrinterSettings.FromPage != 0) ? (base.PrinterSettings.FromPage - 1) : 0);
            base.OnBeginPrint(e);
        }

        protected override void OnQueryPageSettings(QueryPageSettingsEventArgs e)
        {
            OnBeforeQueryPageSettings(e);
            bool flag = e.PageSettings.Bounds.Width > e.PageSettings.Bounds.Height != e.PageSettings.Landscape;
            if (_settings.MultiplePages == null && _currentPage < _document.PageCount)
            {
                bool flag2 = GetOrientation(_document.PageSizes[_currentPage]) == Orientation.Landscape;
                if (flag)
                {
                    flag2 = !flag2;
                }

                e.PageSettings.Landscape = flag2;
            }

            base.OnQueryPageSettings(e);
        }

        protected override void OnPrintPage(PrintPageEventArgs e)
        {
            OnBeforePrintPage(e);
            if (_settings.MultiplePages != null)
            {
                PrintMultiplePages(e);
            }
            else
            {
                PrintSinglePage(e);
            }

            base.OnPrintPage(e);
        }

        private void PrintMultiplePages(PrintPageEventArgs e)
        {
            PdfPrintMultiplePages multiplePages = _settings.MultiplePages;
            int num = multiplePages.Horizontal * multiplePages.Vertical;
            int num2 = (_document.PageCount - 1) / num + 1;
            if (_currentPage < num2)
            {
                double num3 = (float)e.PageBounds.Width - e.PageSettings.HardMarginX * 2f;
                double num4 = (float)e.PageBounds.Height - e.PageSettings.HardMarginY * 2f;
                double num5 = (num3 - (double)((float)(multiplePages.Horizontal - 1) * multiplePages.Margin)) / (double)multiplePages.Horizontal;
                double num6 = (num4 - (double)((float)(multiplePages.Vertical - 1) * multiplePages.Margin)) / (double)multiplePages.Vertical;
                for (int i = 0; i < multiplePages.Horizontal; i++)
                {
                    for (int j = 0; j < multiplePages.Vertical; j++)
                    {
                        int num7 = _currentPage * num;
                        num7 = ((multiplePages.Orientation != 0) ? (num7 + (i * multiplePages.Horizontal + j)) : (num7 + (j * multiplePages.Vertical + i)));
                        if (num7 < _document.PageCount)
                        {
                            double left = (num5 + (double)multiplePages.Margin) * (double)i;
                            double top = (num6 + (double)multiplePages.Margin) * (double)j;
                            RenderPage(e, num7, left, top, num5, num6);
                        }
                    }
                }

                _currentPage++;
            }

            if (base.PrinterSettings.ToPage > 0)
            {
                num2 = Math.Min(base.PrinterSettings.ToPage, num2);
            }

            e.HasMorePages = _currentPage < num2;
        }

        private void PrintSinglePage(PrintPageEventArgs e)
        {
            if (_currentPage < _document.PageCount)
            {
                Orientation orientation = GetOrientation(_document.PageSizes[_currentPage]);
                Orientation orientation2 = GetOrientation(e.PageBounds.Size);
                e.PageSettings.Landscape = orientation == Orientation.Landscape;
                double a;
                double b;
                double b2;
                double a2;
                if (_settings.Mode == PdfPrintMode.ShrinkToMargin)
                {
                    a = 0.0;
                    b = 0.0;
                    b2 = (float)e.PageBounds.Width - e.PageSettings.HardMarginX * 2f;
                    a2 = (float)e.PageBounds.Height - e.PageSettings.HardMarginY * 2f;
                }
                else
                {
                    a = 0f - e.PageSettings.HardMarginX;
                    b = 0f - e.PageSettings.HardMarginY;
                    b2 = e.PageBounds.Width;
                    a2 = e.PageBounds.Height;
                }

                if (orientation != orientation2)
                {
                    Swap(ref a2, ref b2);
                    Swap(ref a, ref b);
                }

                RenderPage(e, _currentPage, a, b, b2, a2);
                _currentPage++;
            }

            int num = ((base.PrinterSettings.ToPage == 0) ? _document.PageCount : Math.Min(base.PrinterSettings.ToPage, _document.PageCount));
            e.HasMorePages = _currentPage < num;
        }

        private void RenderPage(PrintPageEventArgs e, int page, double left, double top, double width, double height)
        {
            SizeF sizeF = _document.PageSizes[page];
            double num = sizeF.Height / sizeF.Width;
            double num2 = height / width;
            double num3 = width;
            double num4 = height;
            if (num > num2)
            {
                num3 = width * (num2 / num);
            }
            else
            {
                num4 = height * (num / num2);
            }

            left += (width - num3) / 2.0;
            top += (height - num4) / 2.0;
            _document.Render(page, e.Graphics, e.Graphics!.DpiX, e.Graphics!.DpiY, new Rectangle(AdjustDpi(e.Graphics!.DpiX, left), AdjustDpi(e.Graphics!.DpiY, top), AdjustDpi(e.Graphics!.DpiX, num3), AdjustDpi(e.Graphics!.DpiY, num4)), PdfRenderFlags.ForPrinting | PdfRenderFlags.Annotations);
        }

        private static void Swap(ref double a, ref double b)
        {
            double num = a;
            a = b;
            b = num;
        }

        private static int AdjustDpi(double value, double dpi)
        {
            return (int)(value / 100.0 * dpi);
        }

        private Orientation GetOrientation(SizeF pageSize)
        {
            if (pageSize.Height > pageSize.Width)
            {
                return Orientation.Portrait;
            }

            return Orientation.Landscape;
        }
    }
}
