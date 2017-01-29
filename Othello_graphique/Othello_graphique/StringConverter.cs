using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Othello_graphique
{
    class StringConverter : IValueConverter
    {
        /// <summary>
        /// Will Convert a value to a special string
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {

                if (value is bool)
                {
                    if ((bool)value)
                    {
                        return "Resume";
                    }
                    else
                    {
                        return "Pause";
                    }

                }


                int number = (int)value;

                string TexteTest = number.ToString();
                string TexteSortie;
                switch (TexteTest)
                {
                    case "1":
                        TexteSortie = "Black";
                        break;
                    case "0":
                        TexteSortie = "End";
                        break;
                    default:
                        TexteSortie = "White";
                        break;
                }
                return TexteSortie;
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
