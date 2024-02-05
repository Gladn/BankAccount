using Windows.Networking.Connectivity;

namespace BankAccount.Service
{
    public interface INetworkAvalableService
    {
        bool IsInternetAvailableAsync();
    }

    public class NetworkAvalableService : INetworkAvalableService
    {
        public bool IsInternetAvailableAsync()
        {
            var connectionProfile = NetworkInformation.GetInternetConnectionProfile();
            if (connectionProfile != null)
            {
                var isConnected = connectionProfile.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess;
                return isConnected;
            }
            else
            {
                return false;
            }
        }
    }
}
