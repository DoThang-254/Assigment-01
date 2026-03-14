using CsvHelper;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using System.Globalization;
using System.Text;
using DataAccess.Models.Dto;

namespace Slot8_9_7_CsvHelper
{
    public class CsvOutputFormatter : TextOutputFormatter
    {
        public CsvOutputFormatter()
        {
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("text/csv"));
            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.Unicode);
        }

        protected override bool CanWriteType(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return typeof(IEnumerable<ReportDTO>).IsAssignableFrom(type);
        }

        public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            var response = context.HttpContext.Response;
            response.ContentType = "text/csv";

            await using (var writer = new StreamWriter(response.Body, selectedEncoding))
            await using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                var products = (IEnumerable<ReportDTO>)context.Object;
                await csv.WriteRecordsAsync(products);
                await writer.FlushAsync();  // Đảm bảo tất cả dữ liệu được ghi ra stream
            }
        }
    }
}
