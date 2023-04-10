using APP.Controller;
using System;
using System.Drawing.Printing;
using System.Drawing;
using System.IO;
using System.Runtime.Versioning;
using PdfiumViewer;
using static API.PrinterController;
using Newtonsoft.Json.Linq;
using System.Windows.Documents;

namespace PrinterModule
{
    [SupportedOSPlatform("windows")]
    public class PrintManager
    {
        private Guid ModuleId;
        private StreamReader streamToPrint;
        private static string message = "{2} al agregar el documento {0} a la cola de impresión de {1}. {3}";       

        public PrintManager(Guid ModuleId)
        {
            this.ModuleId = ModuleId;
        }

        /// <summary>
        /// Funcion principal para imprimir un documento
        /// </summary>
        /// <param name="fileNameToPrint">Nombre del documento en la salida</param>
        /// <param name="base64">Base64 de documento a imprimir</param>
        /// <param name="pageSettings">Configuracion de pagina y impresora</param>
        /// <returns></returns>
        public void Print(string fileNameToPrint, string base64, PageSettings pageSettings)
        {
            if(pageSettings == null)
            {
                throw new ArgumentNullException(nameof(pageSettings),"Valor nulo de PageSettings");
            }
            if(String.IsNullOrEmpty(fileNameToPrint))
            {
                throw new ArgumentNullException(nameof(fileNameToPrint), "Valor nulo de fichero a imprimir");
            }
            if (String.IsNullOrEmpty(base64))
            {
                throw new ArgumentNullException(nameof(base64),"Valor nulo de base64");
            }

            if (!IsBase64String(base64))
            {
                throw new ArgumentException(nameof(base64), "Formato base64 es erroneo");
            }

            Byte[] bytes = Convert.FromBase64String(base64);
            MemoryStream memoryStream = new MemoryStream(bytes);

            try
            {
                Enqueue(memoryStream, fileNameToPrint, pageSettings);

                new ActivityLogController().Post(ModuleId, ActivityLogController.ActivityLog.Status.OK, String.Format(message, fileNameToPrint, pageSettings.PrinterSettings.PrinterName, "Éxito", String.Empty));
            }
            catch(Exception ex)
            {
                new ActivityLogController().Post(ModuleId, ActivityLogController.ActivityLog.Status.ERROR,String.Format(message, fileNameToPrint, pageSettings.PrinterSettings.PrinterName, "Error", ex.Message), ex.StackTrace);
                throw;
            }
        }

        /// <summary>
        /// Procedimiento para agregar a la cola de impresion un documento
        /// </summary>        
        private void Enqueue(MemoryStream fileStr, string fileName, PageSettings pageSettings)
        {
            switch (Path.GetExtension(fileName)) {
                case ".pdf":
                    PrintPDF(fileStr, fileName, pageSettings);
                    break;
                case ".txt":
                    PrintTxt(fileStr, fileName, pageSettings);
                    break;
                default:
                    throw new ArgumentException($"Formato {Path.GetExtension(fileName)} no soportado",nameof(fileName));
            }
        }

        /// <summary>
        /// Construye configuracion de la impresora
        /// </summary>
        /// <param name="printerName">Nombre de la impresora</param>
        /// <param name="copies">Copias a imprimir</param>
        /// <param name="duplex">Doble cara</param>
        /// <returns></returns>
        public static PrinterSettings GetPrinterSettings(string printerName, short copies, Duplex duplex)
        {
            PrinterSettings printerSettings = new PrinterSettings();
            printerSettings.PrinterName = printerName;
            if (printerSettings.IsValid)
            {
                printerSettings.Copies = copies;
                printerSettings.Duplex = duplex;
            }

            return printerSettings;
        }

        /// <summary>
        /// Construye configuracion de la hoja
        /// </summary>
        /// <param name="printerSettings">Configuración de la impresora</param>
        /// <param name="paperkind">Formato de papel (A3,A4,etc.)</param>
        /// <param name="margins">Margenes a aplicar</param>
        /// <param name="customMargins">Debe aplicar margenes personalizados</param>
        /// <param name="height">Altura del documento</param>
        /// <param name="width">Anchura del documento</param>
        /// <param name="landscape">Formato apaisado</param>
        /// <param name="papertray">Bandeja de la salida</param>
        /// <param name="color">Impresion en color</param>
        /// <returns></returns>
        public static PageSettings GetPageSettings(PrinterSettings printerSettings, string paperkind, Margins margins, bool customMargins, int height, int width, bool landscape, string papertray, bool color)
        {
            //definir el page settings
            PageSettings pageSettings = new PageSettings(printerSettings);
            pageSettings.Landscape = landscape;
            
            PaperSize paperSize = new PaperSize();
            paperSize.RawKind = (int) Enum.Parse<PaperKind>("A4");

            //si no tengo paperkind definido dejo default
            if (!String.IsNullOrEmpty(paperkind))
            {
                if (!paperkind.Equals("Custom"))
                {
                    if (Enum.IsDefined(typeof(PaperKind), paperkind))
                    {
                        PaperKind paperKind = Enum.Parse<PaperKind>(paperkind);
                        paperSize.RawKind = (int) paperKind;
                    }

                }
                else
                {
                    paperSize = new PaperSize(paperkind, width, height);
                }

                //si no tengo bandeja definida dejo default
                if (!String.IsNullOrEmpty(papertray))
                {
                    for (int i = 0; i < printerSettings.PaperSources.Count; i++)
                    {
                        if (printerSettings.PaperSources[i].SourceName.Equals(papertray))
                        {
                            pageSettings.PaperSource = printerSettings.PaperSources[i];
                        }
                    }
                }

                //defino los margenes
                if (customMargins)
                {
                    if(margins == null)
                    {
                        throw new ArgumentException($"Margins no debe ser nulo");
                    }
                    //comprobar minimos
                    float xMarg = pageSettings.HardMarginX;
                    float yMarg = pageSettings.HardMarginY;

                    pageSettings.Margins.Left = (int) (margins.Left < xMarg ? xMarg : margins.Left);
                    pageSettings.Margins.Right = (int) (margins.Right < xMarg ? xMarg : margins.Right);
                    pageSettings.Margins.Top = (int) (margins.Top < yMarg ? yMarg : margins.Top);
                    pageSettings.Margins.Bottom = (int) (margins.Bottom < yMarg ? yMarg : margins.Bottom);
                }
            }

            pageSettings.PaperSize = paperSize;
            pageSettings.Color = color;

            if (!printerSettings.SupportsColor)
            {
                pageSettings.Color = false;
            }

            return pageSettings;           
        }

        /// <summary>
        /// Agrega a la cola de impresión documento PDF a partir de un stream con datos
        /// </summary>
        /// <param name="stream">Stream con documento</param>
        /// <param name="fileNameToPrint">Indica nombre del documento en la cola de impresión</param>
        /// <param name="pageSettings">Configuración de hoja a imprimir</param>
        private void PrintPDF(Stream stream, string fileNameToPrint, PageSettings pageSettings)
        {              
            using (var document = PdfDocument.Load(stream))
            {
                using (var printDocument = document.CreatePrintDocument())
                {                       
                    printDocument.DocumentName= fileNameToPrint;
                    printDocument.DefaultPageSettings = pageSettings;
                    printDocument.PrinterSettings = (PrinterSettings)pageSettings.PrinterSettings.Clone();
                    printDocument.Print();
                }
            }
        }

        /// <summary>
        /// Agrega a la cola de impresión documento TXT a partir de un stream con datos
        /// </summary>
        /// <param name="stream">Stream con documento</param>
        /// <param name="fileNameToPrint">Indica nombre del documento en la cola de impresión</param>
        /// <param name="pageSettings">Configuración de hoja a imprimir</param>
        private void PrintTxt(Stream stream, string fileNameToPrint, PageSettings pageSettings)
        {            
            streamToPrint = new StreamReader(stream);
            try
            {
                PrintDocument printDocument = new PrintDocument();
                printDocument.DocumentName = fileNameToPrint;
                printDocument.PrinterSettings = pageSettings.PrinterSettings;
                printDocument.PrintPage += new PrintPageEventHandler
                    (this.pd_PrintPage);
                printDocument.Print();                   
            }
            catch (Exception)
            {
                streamToPrint.Close();
                throw;
            }
            streamToPrint.Close();
        }

        /// <summary>
        /// Procedimiento que permite generar un documento para imprimir de texto plano
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ev"></param>
        private void pd_PrintPage(object sender, PrintPageEventArgs ev)
        {            
            Font printFont = new Font("Arial", 10);
            float linesPerPage = 0;
            float yPos = 0;
            int count = 0;
            float leftMargin = ev.MarginBounds.Left;
            float topMargin = ev.MarginBounds.Top;
            string line = null;

            // Calcula la cantidad de lineas por pagina.
            linesPerPage = ev.MarginBounds.Height /
               printFont.GetHeight(ev.Graphics);

            // Imprime linea por linea del documento
            while (count < linesPerPage &&
               ((line = streamToPrint.ReadLine()) != null))
            {
                yPos = topMargin + (count *
                   printFont.GetHeight(ev.Graphics));
                ev.Graphics.DrawString(line, printFont, System.Drawing.Brushes.Black,
                   leftMargin, yPos, new StringFormat());
                count++;
            }

            // Si existen mas lineas, imprime otra pagina
            if (line != null)
                ev.HasMorePages = true;
            else
                ev.HasMorePages = false;
        }

        /// <summary>
        /// Indica si base64 es correcto
        /// </summary>
        /// <param name="base64"></param>
        /// <returns></returns>
        public static bool IsBase64String(string base64)
        {
            Span<byte> buffer = new Span<byte>(new byte[base64.Length]);
            return Convert.TryFromBase64String(base64, buffer, out int bytesParsed);
        }

    }
}
