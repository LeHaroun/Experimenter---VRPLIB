using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Experimenter
{
    class ParseLoc
    {
        public Microsoft.Maps.MapControl.WPF.Location pin { get; set; }

        public List<int> Demand { get; set; }


        public List<Microsoft.Maps.MapControl.WPF.Location> Locations { get; set; }


        public ParseLoc(string file)
        {

            List<int> demand_ = new List<int>();
            List<Microsoft.Maps.MapControl.WPF.Location> locations = new List<Microsoft.Maps.MapControl.WPF.Location>();
            


            foreach (var line in file.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries))
            {

                demand_.Add(demand(line));

                locations.Add(location(line));

            }

            Demand = demand_;
            Locations = locations;


        }


        public ParseLoc(string file, bool state)
        {

            double[] locs = new double[2];

            List<int> demand_ = new List<int>();
            List<Microsoft.Maps.MapControl.WPF.Location> locations = new List<Microsoft.Maps.MapControl.WPF.Location>();
            


            foreach (var line in file.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries))
            {

                demand_.Add(demand(line));

                locations.Add(location(line));

            }

            Demand = demand_;
            Locations = locations;


        }
        private int demand(string line)
        {

            int demand = new int();

            string[] split = line.Split(';');

            demand = Convert.ToInt32(split[3]);

            return demand;

        }
        private Microsoft.Maps.MapControl.WPF.Location location (string line )
        {
            Microsoft.Maps.MapControl.WPF.Location loc = new Microsoft.Maps.MapControl.WPF.Location();


            string[] split = line.Split(';');

           

            loc.Latitude = Convert.ToDouble(split[1]);
            loc.Longitude = Convert.ToDouble(split[2]);


            return loc;


        }






    }
}
