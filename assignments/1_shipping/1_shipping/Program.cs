using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

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
            new Transport { Id = 0, Type = TransportType.Truck },
            new Transport { Id = 1, Type = TransportType.Truck },
            new Transport { Id = 2, Type = TransportType.Ship }
        };
        private readonly Queue<Package> _boatQueue = new Queue<Package>();
        private readonly Queue<Package> _truckQueue = new Queue<Package>();

        private readonly List<LogEvent> _logEvents = new List<LogEvent>();

        public (int totalTime, IEnumerable<LogEvent> events) Run(IEnumerable<string> destinationInputs)
        {
            _deliveries.AddRange(destinationInputs.Select((x, i) => new Package { Id = i, Desination = x, CurrentDestination = x == "A" ? Location.Port : Location.B }));
            _deliveries.ForEach(x => _truckQueue.Enqueue(x));

            var time = 0;
            var run = _deliveries.Any(x => !x.Delivered);
            while (run)
            {
                Load(time);

                time = Transport(time);

                Offload(time);

                run = _deliveries.Any(x => !x.Delivered);
            }

            return (time, _logEvents);
        }

        private void Load(int time)
        {
            foreach (var transport in _transports)
            {
                switch (transport.Type)
                {
                    case TransportType.Truck:
                        LoadTransport(transport, _truckQueue, time);
                        break;
                    case TransportType.Ship:
                        LoadTransport(transport, _boatQueue, time);
                        break;
                }
            }
        }

        private void LoadTransport(Transport transport, Queue<Package> packages, int time)
        {
            if (packages.Any() && !transport.Delivering && transport.Eta == 0)
            {
                var package = packages.Dequeue();

                _logEvents.Add(
                    new LogEvent(
                        "DEPART",
                        time,
                        transport.Id,
                        transport.Type.ToString().ToUpper(),
                        package.Location.ToString().ToUpper(),
                        package.CurrentDestination.ToString().ToUpper(),
                        new[] {
                            new LogCargo(
                                package.Id,
                                package.Desination,
                                package.Origin.ToString().ToUpper()) }));

                transport.Packages.Add(package);
                transport.Delivering = true;
                transport.Eta = CalculateEta(package.CurrentDestination);
            }
        }

        private int CalculateEta(Location desitnation)
        {
            switch (desitnation)
            {
                case Location.A:
                    return 4;
                case Location.B:
                    return 5;
                case Location.Port:
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

        private void Offload(int time)
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

                    _logEvents.Add(
                        new LogEvent(
                            "ARRIVE",
                            time,
                            transport.Id,
                            transport.Type.ToString().ToUpper(),
                            package.Location.ToString().ToUpper(),
                            null,
                            new[] {
                                new LogCargo(
                                    package.Id,
                                    package.Desination,
                                    package.Origin.ToString().ToUpper()) }));

                    _logEvents.Add(
                        new LogEvent(
                            "DEPART",
                            time,
                            transport.Id,
                            transport.Type.ToString().ToUpper(),
                            package.Location.ToString().ToUpper(),
                            transport.Type == TransportType.Truck ? Location.Factory.ToString().ToUpper() : Location.Port.ToString().ToUpper(),
                            null));
                }
            }
        }

        private void UpdatePackage(Package package)
        {
            switch (package.CurrentDestination)
            {
                case Location.A:
                case Location.B:
                    {
                        package.Delivered = true;
                        package.Location = package.CurrentDestination;
                        break;
                    }
                case Location.Port:
                    {
                        package.Location = package.CurrentDestination;
                        package.CurrentDestination = Location.A;
                        _boatQueue.Enqueue(package);
                        break;
                    }
            }
        }
    }

    public class Package
    {
        public int Id { get; set; }

        public string Desination { get; set; }

        public Location Origin { get; set; } = Location.Factory;

        public Location Location { get; set; } = Location.Factory;

        public Location CurrentDestination { get; set; }

        public bool Delivered { get; set; }
    }

    public class Transport
    {
        public int Id { get; set; }

        public TransportType Type { get; set; }

        public List<Package> Packages { get; set; } = Enumerable.Empty<Package>().ToList();

        public bool Delivering { get; set; }

        public int Eta { get; set; }
    }

    public enum Location
    {
        Factory,
        A,
        B,
        Port
    }

    public enum TransportType
    {
        Truck,
        Ship
    }

    public class LogEvent
    {
        public LogEvent(string @event, int time, int transportId, string kind, string location, string destination, IEnumerable<LogCargo> cargo)
        {
            Event = @event;
            Time = time;
            TransportId = transportId;
            Kind = kind;
            Location = location;
            Destination = destination;
            Cargo = cargo;
        }

        [JsonProperty("event")]
        public string Event { get; set; }

        [JsonProperty("time")]
        public int Time { get; set; }

        [JsonProperty("transport_id")]
        public int TransportId { get; set; }

        [JsonProperty("kind")]
        public string Kind { get; set; }

        [JsonProperty("location")]
        public string Location { get; set; }

        [JsonProperty("destination", NullValueHandling = NullValueHandling.Ignore)]
        public string Destination { get; set; }

        [JsonProperty("cargo", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<LogCargo> Cargo { get; set; }
    }

    public class LogCargo
    {
        public LogCargo(int id, string desitination, string origin)
        {
            Id = id;
            Desitination = desitination;
            Origin = origin;
        }

        [JsonProperty("cargo_id")]
        public int Id { get; set; }

        [JsonProperty("destination")]
        public string Desitination { get; set; }

        [JsonProperty("origin")]
        public string Origin { get; set; }
    }
}
