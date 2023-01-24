
namespace TcpJsonClient
{
    class BoatClass
    {
        string Type = "Boat";
        private string _boat;
        private string _owner;
        private string _sailNumber;
        public string Boat
        {
            get { return _boat; }
            set { _boat = value; }
        }
        public string Owner
        {
            get { return _owner; }
            set { _owner = value; }
        }
        public string SailNumber
        {
            get { return _sailNumber; }
            set { _sailNumber = value; }
        }

        public BoatClass(string pBoatName, string pBoatOwner, string pSailNumber)
        {
            Boat = pBoatName;
            Owner = pBoatOwner;
            SailNumber = pSailNumber;

        }



    }

    
}
