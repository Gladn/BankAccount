using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Data;

namespace BankAccount.Error
{
    public class ErrorListConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is IEnumerable<string> errors)
            {
                return string.Join(Environment.NewLine, errors);
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
