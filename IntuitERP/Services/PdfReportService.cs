using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Colors = QuestPDF.Helpers.Colors;
using IContainer = QuestPDF.Infrastructure.IContainer;

namespace IntuitERP.Services
{
    internal class PdfReportService
    {
        public async Task<byte[]> GeneratePdfReport<T>(string title, string[] headers, List<T> data) where T : class
        {
            return await Task.Run(() =>
            {
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(12));

                        page.Header()
                            .Text(title)
                            .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                        page.Content()
                            .PaddingVertical(1, Unit.Centimetre)
                            .Column(x =>
                            {
                                x.Spacing(20);
                                x.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        for (int i = 0; i < headers.Length; i++)
                                        {
                                            columns.RelativeColumn();
                                        }
                                    });

                                    table.Header(header =>
                                    {
                                        foreach (var head in headers)
                                        {
                                            header.Cell().Element(CellStyle).Text(head);
                                        }

                                        static QuestPDF.Infrastructure.IContainer CellStyle(QuestPDF.Infrastructure.IContainer container)
                                        {
                                            return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);
                                        }
                                    });

                                    foreach (var item in data)
                                    {
                                        // Use reflection to get property values in the order of the headers
                                        var properties = typeof(T).GetProperties();
                                        foreach (var prop in properties)
                                        {
                                            table.Cell().Element(CellStyle).Text(prop.GetValue(item)?.ToString() ?? "");
                                        }

                                        static IContainer CellStyle(IContainer container)
                                        {
                                            return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
                                        }
                                    }
                                });
                            });

                        page.Footer()
                            .AlignCenter()
                            .Text(x =>
                            {
                                x.Span("Page ");
                                x.CurrentPageNumber();
                            });
                    });
                });

                return document.GeneratePdf();
            });
        }
    }
}