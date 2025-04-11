using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DDW_PDV_WPF.Modelo;
using System.Windows.Data;
using System.Xml.Serialization;

namespace DDW_PDV_WPF
{
    public class XmlToSummaryConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                return "N/A";

            try
            {
                var serializer = new XmlSerializer(typeof(ArticuloDTO));
                using (var reader = new StringReader(value.ToString()))
                {
                    var articulo = (ArticuloDTO)serializer.Deserialize(reader);
                    return $"{articulo.Descripcion} (Stock: {articulo.Stock})";
                }
            }
            catch
            {
                return "Ver detalles";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
