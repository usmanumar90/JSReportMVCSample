using jsreport.Binary;
using jsreport.Local;
using jsreport.MVC;
using jsreport.Types;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using TestingExport.Models;

namespace TestingExport.Controllers
{
    public class HomeController : Controller
    {
        ILocalUtilityReportingService rs;
        public HomeController() 
        {
            rs = new LocalReporting()
                       .KillRunningJsReportProcesses()
                       .UseBinary(JsReportBinary.GetBinary())
                       .AsUtility()
                       .Create();
        }

        public async Task<FileResult> PDFAsync()
        {
            string filename = "myReport.pdf";
            var invoiceModel = InvoiceModel.Example();
            var htmlContent = MvcStringHelper.RenderViewToString(this.ControllerContext, "/Views/Home/Invoice.cshtml", invoiceModel);
            (var contentType, var generatedFile) = await GeneratePDFAsync(htmlContent);
            Response.Headers["Content-Disposition"] = $"attachment; filename=" + filename;
            generatedFile.Seek(0, SeekOrigin.Begin);

            return File(generatedFile.ToArray(), "application/pdf", filename);
        }

        public ActionResult Index()
        {
            return View();
        }

        public async Task<(string ContentType, MemoryStream GeneratedFileStream)> GeneratePDFAsync(string htmlContent)
        {            

            IJsReportFeature feature = new JsReportFeature(HttpContext);
            feature.Recipe(Recipe.ChromePdf);
            feature.RenderRequest.Template.Content = htmlContent;
            var report = await rs.RenderAsync(feature.RenderRequest);
            var contentType = report.Meta.ContentType;
            MemoryStream ms = new MemoryStream();
            report.Content.CopyTo(ms);
            return (contentType, ms);
        }

        //Without using JSReportAttibute registered in GlobalActionFilter
        public async Task<ActionResult> InvoiceDownloadAsync()
        {
            try
            {
                string filename = "myReport.pdf";
                var invoiceModel = InvoiceModel.Example();
                var htmlContent = MvcStringHelper.RenderViewToString(this.ControllerContext, "/Views/Home/Invoice.cshtml", invoiceModel);
                (var contentType, var generatedFile) = await GeneratePDFAsync(htmlContent);
                Response.Headers["Content-Disposition"] = $"attachment; filename=\"TestingApp.pdf\"";

                // You may save your file here
                //using (var fileStream = new FileStream(Path.Combine(folder, fileName), FileMode.Create))
                //{
                //    await generatedFile.CopyToAsync(fileStream);
                //}
                // You may need this for re-use of the stream
                generatedFile.Seek(0, SeekOrigin.Begin);
                return File(generatedFile.ToArray(), "application/pdf", filename);
            }
            catch (Exception ex)
            {
                var test = ex;
                return null;
            }
        }

        //Using JSReportAttibute registered in GlobalActionFilter
        [EnableJsReport()]
        public ActionResult InvoiceDownloadTest()
        {
            try
            {
                string filename = "myReport";
                var invoiceModel = InvoiceModel.Example();
                var chromePDF = HttpContext.JsReportFeature().Recipe(Recipe.ChromePdf);
                chromePDF.OnAfterRender((r) => HttpContext.Response.Headers["Content-Disposition"] = "attachment; filename=\"myReport.pdf\"");
                return View("Invoice", invoiceModel);

            }
            catch (Exception ex)
            {
                var test = ex;
                return null;
            }
        }


        [EnableJsReport()]
        public ActionResult InvoiceDownload()
        {
            try
            {
                HttpContext.JsReportFeature().Recipe(Recipe.ChromePdf)
               .OnAfterRender((r) => HttpContext.Response.Headers["Content-Disposition"] = "attachment; filename=\"myReport.pdf\"");

            }
            catch (Exception ex)
            {
                var test = ex;
            }
            var invoiceModel = InvoiceModel.Example();
            return View(invoiceModel);
        }

        [EnableJsReport()]
        public ActionResult Items()
        {
            HttpContext.JsReportFeature()
                .Recipe(Recipe.HtmlToXlsx)
                .Configure((r) => r.Template.HtmlToXlsx = new HtmlToXlsx() { HtmlEngine = "chrome" });

            return View(InvoiceModel.Example());
        }

        [EnableJsReport()]
        public ActionResult ItemsExcelOnline()
        {
            HttpContext.JsReportFeature()
                .Configure(req => req.Options.Preview = true)
                .Recipe(Recipe.HtmlToXlsx)
                .Configure((r) => r.Template.HtmlToXlsx = new HtmlToXlsx() { HtmlEngine = "chrome" });

            return View("Items", InvoiceModel.Example());
        }

        [EnableJsReport()]
        public ActionResult InvoiceDebugLogs()
        {
            HttpContext.JsReportFeature()
                .DebugLogsToResponse()
                .Recipe(Recipe.ChromePdf);

            return View("Invoice", InvoiceModel.Example());
        }

        [EnableJsReport()]
        public ActionResult InvoiceWithHeader()
        {
            try
            {
                HttpContext.JsReportFeature()
                    .Recipe(Recipe.ChromePdf)
                    .Configure((r) => r.Template.Chrome = new Chrome
                    {
                        HeaderTemplate = this.RenderViewToString("Header", new { }),
                        DisplayHeaderFooter = true,
                        FooterTemplate = this.RenderViewToString("Footer", new { }),
                        MarginTop = "1cm",
                        MarginLeft = "1cm",
                        MarginBottom = "1cm",
                        MarginRight = "1cm"
                    });
            }
            catch (Exception ex)
            {
                var test = ex;
            }

            return View("Invoice", InvoiceModel.Example());
        }

        [EnableJsReport()]
        public ActionResult InvoiceWithHeaderDownload()
        {
            try
            {
                HttpContext.JsReportFeature()
                    .Recipe(Recipe.ChromePdf)
                    .Configure((r) => r.Template.Chrome = new Chrome
                    {
                        HeaderTemplate = this.RenderViewToString("Header", new { }),
                        DisplayHeaderFooter = true,
                        FooterTemplate = this.RenderViewToString("Footer", new { }),
                        MarginTop = "1cm",
                        MarginLeft = "1cm",
                        MarginBottom = "1cm",
                        MarginRight = "1cm"
                    }).OnAfterRender((r) => HttpContext.Response.Headers["Content-Disposition"] = "attachment; filename=\"myReport.pdf\"");
            }
            catch (Exception ex)
            {
                var test = ex;
            }

            return View("Invoice", InvoiceModel.Example());
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}