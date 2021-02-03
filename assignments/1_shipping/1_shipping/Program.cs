using System;
using System.Collections.Generic;
using System.Linq;

namespace shipping
{
    class Program
    {
        static void Main(string[] args)
        {
            var app = new App();

            var result = app.Run(args);

            Console.WriteLine($"Time to deliver: {result} h");
        }
    }

    public class App
    {
        private readonly List<Package> _deliveries = Enumerable.Empty<Package>().ToList();
        private readonly IEnumerable<Transport> _transports = new List<Transport>
        {
            new Transport { Type = TransportType.Truck },
            new Transport { Type = TransportType.Truck },
            new Transport { Type = TransportType.Boat }
        };
        private readonly Queue<Package> _boatQueue = new Queue<Package>();
        private readonly Queue<Package> _truckQueue = new Queue<Package>();

        public int Run(IEnumerable<string> destinationInputs)
        {
            _deliveries.AddRange(destinationInputs.Select(x => new Package { Desination = x, CurrentDestination = x == "A" ? Desitnation.Port : Desitnation.B }));
            _deliveries.ForEach(x => _truckQueue.Enqueue(x));

            var time = 0;
            var run = _deliveries.Any(x => !x.Delivered);
            while (run)
            {
                Load();

                time = Transport(time);

                Offload();

                run = _deliveries.Any(x => !x.Delivered);
            }

            return time;
        }

        private void Load()
        {
            foreach (var transport in _transports)
            {
                switch (transport.Type)
                {
                    case TransportType.Truck:
                        LoadTransport(transport, _truckQueue);
                        break;
                    case TransportType.Boat:
                        LoadTransport(transport, _boatQueue);
                        break;
                }
            }
        }

        private void LoadTransport(Transport transport, Queue<Package> packages)
        {
            if (packages.Any() && !transport.Delivering && transport.Eta == 0)
            {
                var package = packages.Dequeue();

                transport.Packages.Add(package);
                transport.Delivering = true;
                transport.Eta = CalculateEta(package.CurrentDestination);
            }
        }

        private int CalculateEta(Desitnation desitnation)
        {
            switch (desitnation)
            {
                case Desitnation.A:
                    return 4;
                case Desitnation.B:
                    return 5;
                case Desitnation.Port:
                    return 1;
                default:
                    return 0;
            }
        }

        private int Transport(int time)
        {
            foreach (var transport in _transports)
            {
                transport.Eta = Math.Clamp(transport.Eta - 1, 0, 5);
            }

            return ++time;
        }

        private void Offload()
        {
            foreach (var transport in _transports)
            {
                if (transport.Delivering && transport.Eta == 0)
                {
                    var package = transport.Packages.First();
                    transport.Packages.Remove(package);
                    transport.Delivering = false;
                    transport.Eta = CalculateEta(package.CurrentDestination);

                    UpdatePackage(package);
                }
            }
        }

        private void UpdatePackage(Package package)
        {
            switch (package.CurrentDestination)
            {
                case Desitnation.A:
                case Desitnation.B:
                    {
                        package.Delivered = true;
                        break;
                    }
                case Desitnation.Port:
                    {
                        package.CurrentDestination = Desitnation.A;
                        _boatQueue.Enqueue(package);
                        break;
                    }
            }
        }
    }

    public class Package
    {
        public string Desination { get; set; }

        public Desitnation CurrentDestination { get; set; }

        public bool Delivered { get; set; }
    }

    public class Transport
    {
        public TransportType Type { get; set; }

        public List<Package> Packages { get; set; } = Enumerable.Empty<Package>().ToList();

        public bool Delivering { get; set; }

        public int Eta { get; set; }
    }

    public enum Desitnation
    {
        A,
        B,
        Port
    }

    public enum TransportType
    {
        Truck,
        Boat
    }
}
