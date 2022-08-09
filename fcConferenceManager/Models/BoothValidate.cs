using Newtonsoft.Json;
using System.Collections.Generic;

namespace MAGI_API.Models
{
    public class BoothValidate
    {
        public class BoothList
        {          
            public int pkey { get; set; }
            public int Event_pkey { get; set; }
            public int BoothStatus_pkey { get; set; }
            public int PlayStation_pkey { get; set; }
            public int SortOrder { get; set; }
            public bool Active { get; set; }
            public string SettingName { get; set; }        
            public string Logo { get; set; }
            public string BoothID { get; set; }
            public string BoothColor { get; set; }
            public string SkypeID { get; set; }
            public string ProductName { get; set; }
            public string ProductTagName { get; set; }
            public int EventOrganizations_pkey { get; set; }
            public string WelComeText { get; set; }
            public string Person { get; set; }
            public string PlantID { get; set; }
            public string AudioFile { get; set; }           
            public string SignImage { get; set; }
            public string ScreenImage { get; set; }
            public string DeskImage { get; set; }
            public string FloorBackground { get; set; }
            public string WallBackground { get; set; }
            public string TableRack { get; set; }
            public string SelveRack { get; set; }           
        }

        public class Custom
        {
            public string name { get; set; }
            public string texture { get; set; }
            public bool visible { get; set; }
        }

        public class BoothFiles
        {
            public object texture { get; set; }
        }

        public class Scale
        {
            public int x { get; set; }
            public int y { get; set; }
            public int z { get; set; }
        }

        public class Position
        {
            public int x { get; set; }
            public int y { get; set; }
            public int z { get; set; }
        }

        public class Rotation
        {
            public int x { get; set; }
            public int y { get; set; }
            public int z { get; set; }
        }

        public class Additional
        {
            public string name { get; set; }
            public object texture { get; set; }
            public string model { get; set; }
            public IList<double> pos { get; set; }
        }

        public class Params
        {
            public int x { get; set; }
            public string y { get; set; }
            public int z { get; set; }
            public string targetName { get; set; }
        }

        public class Over
        {
            public string type { get; set; }
            [JsonProperty("params")]
            public Params _params { get; set; }
            public int time { get; set; }
        }

        public class Out
        {
            public string type { get; set; }
        }
       
        public class Click
        {
            public string type { get; set; }
            [JsonProperty("params")]
            public Params _params { get; set; }
        }

        public class Action
        {
            public Over over { get; set; }
            [JsonProperty("out")]
            public Out _out { get; set; }
            public Click click { get; set; }
        }

        public class Active
        {
            public string name { get; set; }
            public object texture { get; set; }
            public object model { get; set; }
            public IList<int> pos { get; set; }
            public IList<Action> actions { get; set; }
        }

        public class Pointer
        {
            public string type { get; set; }
            public int loop { get; set; }
            public object ease { get; set; }
            public int time { get; set; }
        }

        public class Animation
        {
            public Pointer pointer { get; set; }
        }

        public class Emmisive
        {
            public IList<object> stripes { get; set; }
        }
        public class Booth
        {
            public string boothName { get; set; }
            public string exhibitorName { get; set; }
            public string package { get; set; }
            public string id { get; set; }
            public IList<Custom> customs { get; set; }
            public string color { get; set; }
            public BoothFiles boothFiles { get; set; }
            public Scale scale { get; set; }
            public Position position { get; set; }
            public Rotation rotation { get; set; }
            public IList<Additional> additionals { get; set; }
            public IList<Active> actives { get; set; }
            public IList<Animation> animations { get; set; }
            public IList<Emmisive> emmisive { get; set; }
            public IList<object> lights { get; set; }
        }

        public class MainBooth
        {
            public IList<Booth> booths { get; set; }
            public IList<Additional> additionals { get; set; }
        }
    }
	
}