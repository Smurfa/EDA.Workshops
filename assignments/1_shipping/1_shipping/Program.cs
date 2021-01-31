using System;
using System.Collections.Generic;
using System.Linq;

namespace shipping
{
    class Program
    {
        static void Main(string[] args)
        {
            var packages = new List<string> { "A", "B", "B" };
            var app = new App();

            var result = app.Run(packages);

            Console.WriteLine($"Time to deliver: {result} h");
        }
    }

    public class App
    {
        public int Run(IEnumerable<string> destinationInputs)
        {
            var packages = destinationInputs.Select(x => new Package { Desination = x }).ToList();
            if (packages.Count == 0 )
            {
                return 0;
            }

            var truckQueue = new Queue<Package>(packages);
            var boatQueue = new Queue<Package>();

            var trucks = new List<Transport> { new Transport(), new Transport() };
            var boat = new Transport();
            
            var run = packages.All(x => !x.Delivered);
            var time = 0;
            while (run)
            {
                Load(truckQueue, trucks, boatQueue, boat);

                Transport(trucks, boat);

                Offload(trucks, boat, boatQueue);

                run = packages.All(x => !x.Delivered);

                time++;
            }

            return time;
        }

        private void Load(Queue<Package> truckQueue, List<Transport> trucks, Queue<Package> boatQueue, Transport boat)
        {
            foreach (var truck in trucks)
            {
                if (truckQueue.Any() && !truck.Packages.Any() && !truck.Delivering && !truck.Returning)
                {
                    var package = truckQueue.Dequeue();

                    truck.Packages.Add(package);
                    truck.Delivering = true;
                    truck.Returning = false;
                    truck.Eta = package.Desination == "A" ? 1 : 5;
                }
            }

            if (boatQueue.Any() && !boat.Packages.Any() && !boat.Delivering && !boat.Returning)
            {
                var package = boatQueue.Dequeue();

                boat.Packages.Add(package);
                boat.Delivering = true;
                boat.Returning = false;
                boat.Eta = 4;
            }
        }

        private void Transport(List<Transport> trucks, Transport boat)
        {
            foreach (var truck in trucks)
            {
                if (truck.Delivering || truck.Returning)
                {
                    truck.Eta--;
                }
            }

            if (boat.Delivering || boat.Returning)
            {
                boat.Eta--;
            }
        }

        private void Offload(List<Transport> trucks, Transport boat, Queue<Package> boatQueue)
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
                    boatQueue.Enqueue(package);

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
                boat.Eta = 1;
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

        public bool Returning { get; set; }

        public int Eta { get; set; }
    }
}
