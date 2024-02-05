using BankAccount.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using Windows.UI.Xaml.Controls;


namespace BankAccount.View
{
    public sealed partial class AddTransactionPage : Page
    {
        public AddTransactionPage()
        {
            this.InitializeComponent();
            var container = ((App)App.Current).Container;
            DataContext = ActivatorUtilities.GetServiceOrCreateInstance(container, typeof(AddTransactionViewModel));
        }
    }
}
