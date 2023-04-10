using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;
using System.Text;
using APP;
using APP.Controller;
using PrinterModule;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using System.Drawing;
using static API.PrinterController;
using System.Windows.Forms;
using System.Runtime.Versioning;
using System.Net.Http;
using System.Collections;

namespace API
{
    [SupportedOSPlatform("windows")]
    [ApiController]
    public class PrinterController : ControllerBase
    {
        [Route("/api/printer/list")]
        [HttpPost]
        public IActionResult Get()
        {
            LocalPrinters lclPrinters = new LocalPrinters();
            foreach (var item in PrinterSettings.InstalledPrinters)
            {
                Printer printer = new Printer();
                printer.printerName = item.ToString();
                PrinterSettings printSt = new PrinterSettings();
                printSt.PrinterName = printer.printerName;
                printer.color = printSt.SupportsColor;
                printer.duplex = printSt.CanDuplex;
                printer.trays = printSt.PaperSources;
                printer.margins = printSt.DefaultPageSettings.Margins;
                printer.landScape = printSt.DefaultPageSettings.Landscape;
                printer.paperSize = printSt.DefaultPageSettings.PaperSize;
                lclPrinters.printers.Add(printer);
            }
            
            return Ok(lclPrinters);
        }

        [Route("/api/printer")]
        [HttpPost]
        public IActionResult Post([FromBody] PrinterModel value)
        {            
            Duplex duplex = Duplex.Default;
            if (Enum.IsDefined(typeof(Duplex), value.duplex)){                    
                duplex = Enum.Parse<Duplex>(value.duplex);
            }
                       
            PrinterSettings printerSettings = PrintManager.GetPrinterSettings(value.printerName, value.defaultcopies, duplex);

            PageSettings pageSettings = PrintManager.GetPageSettings(printerSettings, value.paperkind, value.margins, value.customMargins, value.height, value.width, value.landscape, value.papertray, value.color);
            
            Module module = (Module) APP.Module.StoreModules.GetModule(value.moduleId);            
            module.GetPrintManager().Print(value.fileName, value.base64, pageSettings);
            
            return Ok(
                new {
                    value.fileName,
                    value.printerName,
                    inQueue = true,
                }
            );          
        }


        public class PrinterModel
        {
            public Guid moduleId;
            public string printerName;
            public string fileName;
            public string base64;
            public string duplex;
            public bool color;
            public short defaultcopies;
            public string papertray;
            public string paperkind;
            public int height;
            public int width;
            public bool landscape;
            public Margins margins;
            public bool customMargins;
        }
        public class Printer
        {
            public string printerName;
            public bool color;
            public bool duplex;
            public bool landScape;
            public PaperSize paperSize;
            public Margins margins;
            public PrinterSettings.PaperSourceCollection trays;
        }        
    }

    [SupportedOSPlatform("windows")]
    public class LocalPrinters
    {
        public List<Printer> printers = new();
        
    }
}
