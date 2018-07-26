using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Text;
using System;

namespace Z_Manager.Managers
{
    public class NetworkManager
    {
        public static NetworkManager _instance;
        public static NetworkManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new NetworkManager();

                return _instance;
            }
        }

        private NetworkManager() { }

        /// <summary> Check connectivity to stable endpoint like Google </summary>
        public void CheckInternetConnectivity()
        {

        }

        /// <summary> Time a file download </summary>
        public void CheckInternetConnectionSpeed()
        {

        }

        /// <summary> Send UDP packet for another instance on the LAN to acknowledge </summary>
        public void CheckLocalNetwork()
        {

        }
    }
}
