using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SbslFileTransformer.Infrastructure.Helpers
{
    public static class ReaderHelper
    {
        public static bool TryGetValue(this IExcelDataReader reader, int ordinal, out object value)
        {
            value = null;

            try
            {
                value = reader.GetValue(ordinal);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
