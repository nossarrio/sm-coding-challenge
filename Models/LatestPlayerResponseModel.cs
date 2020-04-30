using System.Collections.Generic;
using System.Runtime.Serialization;

namespace sm_coding_challenge.Models
{
    public class LatestPlayerDataModel
    {
        public Receiving[] Receiving { get; set; }
        public Rushing[] Rushing { get; set; }

        public LatestPlayerDataModel()
        {

        }
    }

    [DataContract]
    public class Receiving
    {
        public PlayerModel Player { get; set; }        
    }

    [DataContract]
    public class Rushing
    {
        public PlayerModel Player { get; set; }
    }

}

