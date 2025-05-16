using Microsoft.AspNetCore.Mvc;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Text;

namespace DocxUpdater.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocxUpdateController : ControllerBase
    {
        [HttpPost("update")]
        public IActionResult UpdateDocx([FromBody] UpdateRequest request)
        {
            byte[] fileBytes = Convert.FromBase64String(request.Base64Docx);

            using var inputStream = new MemoryStream(fileBytes);
            using var wordDoc = WordprocessingDocument.Open(inputStream, true);

            var headers = wordDoc.MainDocumentPart.HeaderParts;
            foreach (var header in headers)
            {
                foreach (var text in header.RootElement.Descendants<Text>())
                {
                    if (text.Text.Contains("Date Effective"))
                        text.Text = $"Date Effective : {request.ReviewDate}";
                    else if (text.Text.Contains("Next Review"))
                        text.Text = $"Next Review : {request.ExpiryDate}";
                }
            }

            wordDoc.Close();

            string updatedBase64 = Convert.ToBase64String(inputStream.ToArray());
            return Ok(new { updatedBase64 });
        }

        public class UpdateRequest
        {
            public string Base64Docx { get; set; }
            public string ReviewDate { get; set; }
            public string ExpiryDate { get; set; }
        }
    }
}
