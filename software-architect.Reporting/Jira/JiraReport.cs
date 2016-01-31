using System;
using System.IO;
using System.Xml;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;
using PdfSharp.Fonts;
using PdfSharp.Pdf;

namespace software_architect.Reporting.Jira
{
    public class JiraReport
    {
        static JiraReport()
        {
            GlobalFontSettings.FontResolver = new PdfFontResolver();
        }

        public byte[] Create(string content)
        {
            // http://pdfsharp.net/wiki/PrivateFonts-sample.ashx
            // http://stackoverflow.com/questions/31568349/error-accessing-fonts-in-azure-web-app-when-using-pdfsharp
            var familyName = "Verdana";

            using (PdfDocument document = new PdfDocument())
            {
                document.Info.Title = "Jira Agile Cards";

                XmlDocument xml = new XmlDocument();
                xml.LoadXml(content);

                if (xml.DocumentElement == null)
                    throw new NotSupportedException("Xml document is invalid!");

                var nodes = xml.DocumentElement.SelectNodes("//rss/channel/item");
                if (nodes == null)
                    throw new NotSupportedException("Xml document is invalid!");

                foreach (XmlNode node in nodes)
                {
                    var summary = GetNodeValue(node, "summary");
                    var issueNo = GetNodeValue(node, "key");
                    var parentNo = GetNodeValue(node, "parent");
                    var priority = GetNodeValue(node, "priority");
                    var taskType = GetNodeValue(node, "type");

                    var taskEstimate = GetNodeValue(node, "timeoriginalestimate");

                    if (taskType == "Story")
                    {
                        taskEstimate = GetNodeValue(node, "aggregatetimeoriginalestimate");
                        var customFields = node.SelectNodes("customfields/customfield");
                        if (customFields != null)
                        {
                            foreach (XmlNode customField in customFields)
                            {
                                var name = GetNodeValue(customField, "customfieldname");
                                if (name == "Story Points")
                                {
                                    var values = customField.SelectSingleNode("customfieldvalues");
                                    if (values != null)
                                    {
                                        var value = GetNodeValue(values, "customfieldvalue");
                                        if (!string.IsNullOrEmpty(value))
                                        {
                                            taskEstimate = value;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    var page = document.AddPage();
                    page.Size = PageSize.A4;
                    page.Orientation = PageOrientation.Landscape;

                    XGraphics gfx = XGraphics.FromPdfPage(page);

                    gfx.DrawLine(XPens.Black, new XPoint(10, 450), new XPoint(610, 450));

                    XTextFormatter tf = new XTextFormatter(gfx) {Alignment = XParagraphAlignment.Left};

                    var summaryRectangle = new XRect(10, 10, 600, 450);
                    XFont summaryFont = new XFont(familyName, 48, XFontStyle.Regular);
                    tf.DrawString(summary, summaryFont, XBrushes.Black, summaryRectangle, XStringFormats.TopLeft);

                    XFont issueNoFont = new XFont(familyName, 36, XFontStyle.Regular);
                    var issueNoRectangle = new XRect(10, 460, 290, 50);
                    tf.Alignment = XParagraphAlignment.Left;
                    tf.DrawString(issueNo, issueNoFont, XBrushes.Black, issueNoRectangle);

                    XFont parentNoFont = new XFont(familyName, 26, XFontStyle.Regular);
                    var parentNoRectangle = new XRect(320, 460, 290, 50);
                    tf.Alignment = XParagraphAlignment.Right;
                    tf.DrawString(parentNo, parentNoFont, XBrushes.DimGray, parentNoRectangle);

                    XFont priorityFont = new XFont(familyName, 24, XFontStyle.Italic);
                    var priorityRectangle = new XRect(10, 510, 290, 50);
                    tf.Alignment = XParagraphAlignment.Left;
                    tf.DrawString(priority, priorityFont, XBrushes.Black, priorityRectangle);

                    XFont taskTypeFont = new XFont(familyName, 24, XFontStyle.Italic);
                    var taskTypeRectangle = new XRect(320, 510, 290, 50);
                    tf.Alignment = XParagraphAlignment.Right;
                    tf.DrawString(taskType, taskTypeFont, XBrushes.Black, taskTypeRectangle);

                    var p1 = new XPoint(650, 10);
                    var p2 = new XPoint(800, 10);
                    var p3 = new XPoint(800, 160);
                    var p4 = new XPoint(650, 160);

                    gfx.DrawLines(XPens.Black, new[] {p1, p2, p3, p4, p1});

                    XFont estimateFont = new XFont(familyName, 48, XFontStyle.Regular);
                    var estimateRectangle = new XRect(650, 50, 150, 100);
                    tf.Alignment = XParagraphAlignment.Center;
                    tf.DrawString(taskEstimate, estimateFont, XBrushes.Black, estimateRectangle);
                }

                using (var exportStream = new MemoryStream())
                {
                    document.Save(exportStream);
                    var array = exportStream.ToArray();
                    return array;
                }
            }
        }

        private static string GetNodeValue(XmlNode node, string nodeName)
        {
            var summary = string.Empty;

            var summaryNode = node.SelectSingleNode(nodeName);
            if (summaryNode != null)
                summary = summaryNode.InnerText;
            return summary;
        }
    }
}
