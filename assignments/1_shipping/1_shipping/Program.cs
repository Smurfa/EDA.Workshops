using System;
using System.Collections.Generic;
using System.Linq;

namespace shipping
{
    class Program
    {
        static void Main(string[] args)
        {
            var packages = new List<string> { "A", "A", "B", "A", "B", "B", "A", "B" };
            var app = new App();

            var result = app.Run(packages);

            Console.WriteLine($"Time to deliver: {result} h");
        }
    }

    public class App
    {
        private readonly List<Package> _deliveries = Enumerable.Empty<Package>().ToList();
        private readonly Queue<Package> _boatQueue = new Queue<Package>();
        private readonly Queue<Package> _truckQueue = new Queue<Package>();

        public int Run(IEnumerable<string> destinationInputs)
        {
            _deliveries.AddRange(destinationInputs.Select(x => new Package { Desination = x }));
            _deliveries.ForEach(x => _truckQueue.Enqueue(x));

            var trucks = new List<Transport> { new Transport(), new Transport() };
            var boat = new Transport();

            var time = 0;
            var run = _deliveries.Any(x => !x.Delivered);
            while (run)
            {
                Load(trucks, boat);
                
                time = Transport(trucks, boat, time);

                Offload(trucks, boat);

                run = _deliveries.Any(x => !x.Delivered);
            }

            return time;
        }

        private void Load(List<Transport> trucks, Transport boat)
        {
            foreach (var truck in trucks)
            {
                if (_truckQueue.Any() && !truck.Packages.Any() && !truck.Delivering && truck.Returning && truck.Eta == 0)
                {
                    var package = _truckQueue.Dequeue();

                    truck.Packages.Add(package);
                    truck.Delivering = true;
                    truck.Returning = false;
                    truck.Eta = package.Desination == "A" ? 1 : 5;
                }
            }

            if (_boatQueue.Any() && !boat.Packages.Any() && !boat.Delivering && boat.Returning && boat.Eta == 0)
            {
                var package = _boatQueue.Dequeue();

                boat.Packages.Add(package);
                boat.Delivering = true;
                boat.Returning = false;
                boat.Eta = 4;
            }
        }

        private int Transport(List<Transport> trucks, Transport boat, int time)
        {
            foreach (var truck in trucks)
            {
                if (truck.Delivering || truck.Returning)
                {
                    truck.Eta = Math.Clamp(truck.Eta - 1, 0 , 5);
                }
            }

            if (boat.Delivering || boat.Returning)
            {
                boat.Eta = Math.Clamp(boat.Eta - 1, 0, 4);
            }

            return ++time;
        }

        private void Offload(List<Transport> trucks, Transport boat)
        {
            foreach (var truck in trucks)
            {
                var package = truck.Packages.FirstOrDefault();
                if (truck.Delivering && truck.Eta == 0 && package?.Desination == "B")
                {
                    package.Delivered = true;

                    truck.Packages.Remove(package);
                    truck.Delivering = false;
                    truck.Returning = true;
                    truck.Eta = 5;
                }
                else if (truck.Delivering && truck.Eta == 0 && package?.Desination == "A")
                {
                    _boatQueue.Enqueue(package);

                    truck.Packages.Remove(package);
                    truck.Delivering = false;
                    truck.Returning = true;
                    truck.Eta = 1;
                }
            }

            if (boat.Delivering && boat.Eta == 0)
            {
                var package = boat.Packages.First();
                package.Delivered = true;

                boat.Packages.Remove(package);
                boat.Delivering = false;
                boat.Returning = true;
                boat.Eta = 4;
            }
        }
    }

    public class Package
    {
        public string Desination { get; set; }

        public bool Delivered { get; set; }
    }

    public class Transport
    {
        public List<Package> Packages { get; set; } = Enumerable.Empty<Package>().ToList();

        public bool Delivering { get; set; }

        public bool Returning { get; set; } = true; //NOTE: Ugly fugly

        public int Eta { get; set; }
    }
}
