using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Text;
using System;

namespace Z_Manager.Managers
{
    public class ConfigurationManager
    {
        public static ConfigurationManager _instance;
        public static ConfigurationManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ConfigurationManager();

                return _instance;
            }
        }

        private ConfigurationManager() { }
    }
}
