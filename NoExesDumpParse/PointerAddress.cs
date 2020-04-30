using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoExesDumpParse
{
    class PointedAddress : IComparable<PointedAddress>
    {
        public PointedAddress(Address pointedAddress)
        {
            addr = pointedAddress;
            pointedfrom = new List<Address>();
        }
        int IComparable<PointedAddress>.CompareTo(PointedAddress obj)
        {
            IComparable<Address> iaddr = this.addr;
            return iaddr.CompareTo(obj.addr);
        }
        public Address addr;
        public List<Address> pointedfrom;
    }
}
