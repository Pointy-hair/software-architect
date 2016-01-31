using System;
using System.IO;
using System.Reflection;
using PdfSharp.Fonts;

namespace software_architect.Reporting.Jira
{
    public class PdfFontResolver : IFontResolver
    {
        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            if (isBold)
            {
                return new FontResolverInfo("verdanab", true, false);
            }
            if (isItalic)
            {
                return new FontResolverInfo("verdanai", false, true);
            }
            return new FontResolverInfo("verdana", false, false);
        }

        public byte[] GetFont(string faceName)
        {
            var fontName = $"software_architect.Reporting.Jira.Fonts.{faceName}.ttf";
            return LoadFontData(fontName);
        }

        static byte[] LoadFontData(string name)
        {
            Assembly assembly = typeof(PdfFontResolver).Assembly;
            using (Stream stream = assembly.GetManifestResourceStream(name))
            {
                if (stream == null)
                    throw new ArgumentException("No resource with name " + name);

                var count = (int)stream.Length;
                var data = new byte[count];
                stream.Read(data, 0, count);
                return data;
            }
        }
    }
}
