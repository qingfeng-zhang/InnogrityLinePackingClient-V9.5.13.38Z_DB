using System;
using System.Windows;
using System.Windows.Data;
using InnogrityLinePackingClient;
using System.Windows.Media;
using System.Diagnostics;

using System.Globalization;
using System.Text;
using System.Collections.ObjectModel;

namespace UnrelatedBarcodeStatusToVisibilityConverterNamespace
{
    //Visibility of the Green tick
    public class BarcodeStatusToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType == typeof(Visibility))
            {
                if (value is BarcodeStatus)
                {
                    switch ((BarcodeStatus)value)
                    {
                        case BarcodeStatus.Equal:
                            return Visibility.Visible;
                        case BarcodeStatus.NotEqual:
                            return Visibility.Hidden;
                        case BarcodeStatus.NotScanned:
                            return Visibility.Hidden;
                        case BarcodeStatus.Null:
                            return Visibility.Hidden;
                        case BarcodeStatus.NotLoggedIn:
                            return Visibility.Hidden;
                        case BarcodeStatus.LoggedIn:
                            return Visibility.Hidden;
                        case BarcodeStatus.Timeout:
                            return Visibility.Hidden;
                           
                        default:
                            return Visibility.Hidden;
                    }
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            else if (targetType == typeof(string))
            {
                if (value is BarcodeStatus)
                {
                    switch ((BarcodeStatus)value)
                    {
                        case BarcodeStatus.Equal:
                            return "Barcode is equal.";
                        case BarcodeStatus.NotEqual:
                            return "Barcode is not equal!";
                        case BarcodeStatus.NotScanned:
                            return "Waiting for barcode...";
                        case BarcodeStatus.Null:
                        default:
                            return null;
                    }
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    //Visibility of the Message 
    public class NotBarcodeStatusToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType == typeof(Visibility))
            {
                if (value is BarcodeStatus)
                {
                    switch ((BarcodeStatus)value)
                    {
                        case BarcodeStatus.NotScanned:
                            return Visibility.Visible;
                        case BarcodeStatus.Equal:
                            return Visibility.Hidden;
                        case BarcodeStatus.Null:
                            return Visibility.Hidden;
                        case BarcodeStatus.NotEqual:
                            return Visibility.Visible;
                        case BarcodeStatus.Duplicated:
                            return Visibility.Visible;
                        default:
                            return Visibility.Visible;
                    }
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            else if (targetType == typeof(string))
            {
                if (value is BarcodeStatus)
                {
                    switch ((BarcodeStatus)value)
                    {
                        case BarcodeStatus.NotScanned:
                            return "PLEASE SCAN BAG";
                        case BarcodeStatus.Equal:
                            return "SUCCESS";
                        case BarcodeStatus.Null:
                            return "waiting for bag";
                        case BarcodeStatus.NotEqual:
                            return "Barcode Mismatch, Scan again.";
                        case BarcodeStatus.NotLoggedIn:
                            return "Please Log In";
                        case BarcodeStatus.LoggedIn:
                            return "You are now logged in, please wait for products to stop at your station.";
                        case BarcodeStatus.Timeout:
                            return "REJECTED, You took too long.";
                        case BarcodeStatus.AttemptOut:
                            return "REJECTED, Exceeded scan attempts.";
                        case BarcodeStatus.Rejected:
                            return "Rejected";
                        case BarcodeStatus.Arriving:
                            return "Bag arriving to your station, please wait for it to stop.";
                        case BarcodeStatus.NoTrackingLabel:
                            return "No Tracking Label";
                        case BarcodeStatus.Duplicated:
                            return "Duplicated TL,Pls Change BOX";
                        default:
                            return "PLEASE SCAN";
                    }
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            //Change background colour of alert
            else if (targetType == typeof(Brush))
            {
                if (value is BarcodeStatus)
                {
                    switch ((BarcodeStatus)value)
                    {
                        case BarcodeStatus.NotScanned:
                            return "#CCF0C30F";
                        case BarcodeStatus.Equal:
                            return "#B227DB00";
                        case BarcodeStatus.Null:
                            return "#CCF0C30F";
                        case BarcodeStatus.NotEqual:
                            return "#B2FF0000";
                        case BarcodeStatus.NotLoggedIn:
                            return "#B2FF0000";
                        case BarcodeStatus.LoggedIn:
                            return "#B227DB00";
                        case BarcodeStatus.Timeout:
                            return "#B2FF0000";
                        case BarcodeStatus.AttemptOut:
                            return "#B2FF0000";
                        case BarcodeStatus.Rejected:
                            return "#B2FF0000";
                        case BarcodeStatus.Arriving:
                            return "#B227DB00";
                        case BarcodeStatus.Duplicated:
                            return "#B2FF0000";
                        default:
                            return "#CCF0C30F";
                    }
                }
                else
                {
                    throw new NotImplementedException();
                }

            }
            else
            {
                throw new NotImplementedException();
            }
        }
        
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class NotAreaStatusToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType == typeof(Visibility))
            {
                if (value is AreaStatus)
                {
                    switch ((AreaStatus)value)
                    {
                        case AreaStatus.Block:
                            return Visibility.Visible;
                        case AreaStatus.MemNoClear:
                            return Visibility.Visible;
                        case AreaStatus.EarlyTake:
                            return Visibility.Visible;
                        case AreaStatus.Null:
                            return Visibility.Hidden;
                        default:
                            return Visibility.Hidden;
                    }
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            else if (targetType == typeof(string))
            {
                if (value is AreaStatus)
                {
                    switch ((AreaStatus)value)
                    {
                        case AreaStatus.Block:
                            return "DO NOT BLOCK AREASENSOR";
                        case AreaStatus.EarlyTake:
                            return "EARLY PICKUP DETECTED.\r\nPLEASE REJECT IT";
                        case AreaStatus.Null:
                            return "PLEASE SCAN";
                        case AreaStatus.MemNoClear :
                            return "PLEASE RETRY WITH NEW BOX";
                        default:
                            return "PLEASE SCAN";
                    }
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            //Change background colour of alert
            else if (targetType == typeof(Brush))
            {
                if (value is AreaStatus)
                {
                    switch ((AreaStatus)value)
                    {
                        case AreaStatus.Block:
                            return "#B2FF0000";
                        case AreaStatus.MemNoClear:
                            return "#B2FF0000";
                        case AreaStatus.Null:
                            return "#B227DB00";
                        default:
                            return "#CCF0C30F";
                    }
                }
                else
                {
                    throw new NotImplementedException();
                }

            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    //Convert ErrorMessage to Visibility
    public class ErrorMessageToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType == typeof(Visibility))
            {

                if (value == null)
                {
                    return Visibility.Hidden;
                }
                else if (value.ToString() == String.Empty || value.ToString() == "" || value.ToString() == " ")
                {
                    return Visibility.Hidden;
                }
                else
                {
                    return Visibility.Visible;
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    //Convert Critical ErrorMessage to Visibility
    public class VisibilityConvertor : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType == typeof(Visibility))
            {

                if (value == null)
                {
                    return Visibility.Hidden;
                }
                else if (value.ToString() == String.Empty || value.ToString() == "" || value.ToString() == " ")
                {
                    return Visibility.Hidden;
                }
                else
                {
                    return Visibility.Visible;
                }
            }
            else if (value is String)
            {
                if (((string)value).Length > 0)
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Collapsed;
                }

            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
