using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    /// <summary>
    /// Fallback HTML stranice koje preusmjeravaju u mobilnu app (ebooking://).
    /// Korisno ako SuccessUrl još uvijek pokazuje na http://localhost:8080/payment-return.
    /// </summary>
    [AllowAnonymous]
    [ApiController]
    public class PaymentReturnController : ControllerBase
    {
        [HttpGet("/payment-return")]
        public ContentResult PaymentReturn()
        {
            return Content(BuildRedirectHtml("payment-return", "Plaćanje uspješno"), "text/html; charset=utf-8");
        }

        [HttpGet("/payment-cancel")]
        public ContentResult PaymentCancel()
        {
            return Content(BuildRedirectHtml("payment-cancel", "Plaćanje otkazano"), "text/html; charset=utf-8");
        }

        private static string BuildRedirectHtml(string host, string title)
        {
            var safeTitle = System.Net.WebUtility.HtmlEncode(title);
            return "<!DOCTYPE html>\n"
                + "<html lang=\"bs\">\n"
                + "<head>\n"
                + "  <meta charset=\"utf-8\" />\n"
                + "  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1\" />\n"
                + "  <title>" + safeTitle + "</title>\n"
                + "  <style>\n"
                + "    body { font-family: system-ui, sans-serif; max-width: 420px; margin: 48px auto; padding: 0 16px; text-align: center; }\n"
                + "    a { color: #1565c0; }\n"
                + "  </style>\n"
                + "</head>\n"
                + "<body>\n"
                + "  <h1>" + safeTitle + "</h1>\n"
                + "  <p>Vraćamo vas u aplikaciju…</p>\n"
                + "  <p><a id=\"open-app\" href=\"#\">Otvori aplikaciju</a></p>\n"
                + "  <script>\n"
                + "    var target = \"ebooking://" + host + "\" + window.location.search;\n"
                + "    document.getElementById(\"open-app\").href = target;\n"
                + "    window.location.replace(target);\n"
                + "  </script>\n"
                + "</body>\n"
                + "</html>";
        }
    }
}
