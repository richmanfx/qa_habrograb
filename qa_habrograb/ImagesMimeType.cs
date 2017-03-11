using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace qa_habrograb
{	
    /// Тип графичекого файла
    class ImagesMimeType
    {
	// Первый байты определённых графических файлов
        private static readonly byte[] BMP = { 66, 77 };
        private static readonly byte[] GIF = { 71, 73, 70, 56 };
        private static readonly byte[] ICO = { 0, 0, 1, 0 };
        private static readonly byte[] JPG = { 255, 216, 255 };
        private static readonly byte[] PNG = { 137, 80, 78, 71, 13, 10, 26, 10, 0, 0, 0, 13, 73, 72, 68, 82 };
        private static readonly byte[] TIFF = { 73, 73, 42, 0 };

	/// Определяет mime-type графического файла по первым байтам
        public static string GetMimeType(byte[] file_as_byte)
        {
            string mime = "application/octet-stream";       // Неизвестный MIME-тип по умолчания

            if (file_as_byte.Take(2).SequenceEqual(BMP))
                mime = "image/bmp";
            else if (file_as_byte.Take(4).SequenceEqual(GIF))
                mime = "image/gif";
            else if (file_as_byte.Take(4).SequenceEqual(ICO))
                mime = "image/x-icon";
            else if (file_as_byte.Take(3).SequenceEqual(JPG))
                mime = "image/jpeg";
            else if (file_as_byte.Take(16).SequenceEqual(PNG))
                mime = "image/png";
            else if (file_as_byte.Take(4).SequenceEqual(TIFF))
                mime = "image/tiff";

            return mime;
        }
    }
}
