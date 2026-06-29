using Ricebird.Framework.Configurations;
using System.Net;

namespace Ricebird.Clients.Services
{
    public class IpRegionOption : IOption
    {
        public List<IpRegion> Regions { get; set; } = [];

        public OptionSaveTo OptionSaveTo => OptionSaveTo.FileSystem;

        public string SaveKey => "ipRegions";
    }

    public class IpRegion
    {
        public string Name
        {
            get; set;
        } = string.Empty;

        public string[] Networks
        {
            get; set;
        } = [];

        private IPNetwork[]? _networks;
        public IPNetwork[] CidrNetworks
        {
            get
            {
                if (_networks == null)
                {
                    List<IPNetwork> r = [];
                    foreach (var item in Networks)
                    {
                        if (IPNetwork.TryParse(item, out IPNetwork n))
                        {
                            r.Add(n);
                        }
                    }

                    _networks = [.. r];
                }

                return _networks;
            }
        }

        public bool Contains(IPAddress ip)
        {
            foreach (var item in CidrNetworks)
            {
                if (item.Contains(ip))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
