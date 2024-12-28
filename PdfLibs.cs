using iTextSharp.text.pdf;
using iTextSharp.text;
using Font = iTextSharp.text.Font;
using System.IO;
using System;

namespace NETCORE3
{
    public class PdfLibs
    {
        public static PdfPTable CreateTable(int column)
        {
            PdfPTable Table = new PdfPTable(column);
            Table.WidthPercentage = 100;
            float width = 100f / column;
            float[] columnWidth = new float[column];
            for (int i = 0; i < column; i++)
            {
                columnWidth[i] = width;
            }

            Table.SetWidths(columnWidth);
            return Table;
        }
        public static PdfPCell CreateCell(string _content, string _BIU, float _fontSize, bool _border, string _candoc, string _canngang, int _colspan) // BIU: "B", "BI", "IU", "bui", "ub", ...
        {
            string fontPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads/Font/times.ttf");
            BaseFont titleFont = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            int type = 0;
            if (_BIU.IndexOf('B', StringComparison.OrdinalIgnoreCase) >= 0) type += 1;
            if (_BIU.IndexOf('I', StringComparison.OrdinalIgnoreCase) >= 0) type += 2;
            if (_BIU.IndexOf('U', StringComparison.OrdinalIgnoreCase) >= 0) type += 4;
            PdfPCell Cell = new PdfPCell(new Phrase(_content, new Font(titleFont, _fontSize, type, BaseColor.BLACK)));
            if (!_border) Cell.Border = PdfPCell.NO_BORDER;
            switch (_canngang)
            {
                case "L":
                    Cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    break;
                case "R":
                    Cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                    break;
                default:
                    Cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    break;
            }
            switch (_candoc)
            {
                case "T":
                    Cell.VerticalAlignment = Element.ALIGN_TOP;
                    break;
                case "B":
                    Cell.VerticalAlignment = Element.ALIGN_BOTTOM;
                    break;
                default:
                    Cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    break;
            }
            Cell.Colspan = _colspan;
            return Cell;
        }
        public static PdfPCell CreateCell(string[] _content, string[] _BIU, float _fontSize, bool _border, string _candoc, string _canngang, int _colspan)
        {
            string fontPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads/Font/times.ttf");
            BaseFont titleFont = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            Phrase phrase = new Phrase();
            for (int i = 0; i < _content.Length; ++i)
            {
                int type = 0;
                if (_BIU[i].IndexOf('B', StringComparison.OrdinalIgnoreCase) >= 0) type += 1;
                if (_BIU[i].IndexOf('I', StringComparison.OrdinalIgnoreCase) >= 0) type += 2;
                if (_BIU[i].IndexOf('U', StringComparison.OrdinalIgnoreCase) >= 0) type += 4;
                phrase.Add(new Chunk(_content[i], new Font(titleFont, _fontSize, type, BaseColor.BLACK)));
            }
            PdfPCell Cell = new PdfPCell(phrase);
            if (!_border) Cell.Border = PdfPCell.NO_BORDER;
            switch (_canngang)
            {
                case "L":
                    Cell.HorizontalAlignment = Element.ALIGN_LEFT;
                    break;
                case "R":
                    Cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                    break;
                default:
                    Cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    break;
            }
            switch (_candoc)
            {
                case "T":
                    Cell.VerticalAlignment = Element.ALIGN_TOP;
                    break;
                case "B":
                    Cell.VerticalAlignment = Element.ALIGN_BOTTOM;
                    break;
                default:
                    Cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    break;
            }
            Cell.Colspan = _colspan;
            return Cell;
        }
        public static PdfPCell CreateImageCell(string path, float _width, float _height, bool _border, int _colspan)
        {
            string PathImage = Path.Combine(Directory.GetCurrentDirectory(), path);
            var img = Image.GetInstance(PathImage);
            img.ScaleAbsolute(_width, _height);
            PdfPCell imgCell = new PdfPCell(img);
            imgCell.HorizontalAlignment = Element.ALIGN_CENTER;
            imgCell.VerticalAlignment = Element.ALIGN_MIDDLE;
            imgCell.Colspan = _colspan;
            if (!_border)
            {
                imgCell.Border = PdfPCell.NO_BORDER;
            }
            return imgCell;
        }
    }
}