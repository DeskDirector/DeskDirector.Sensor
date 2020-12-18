namespace DdManager.Sensor.NetworkInformation
{
    public record DynamicPortRange
    {
        public static readonly DynamicPortRange Default = new DynamicPortRange(32769, 32766);

        public uint Start { get; }

        public uint End { get; }

        public uint Count { get; }

        public DynamicPortRange(uint start, uint count)
        {
            Start = start;
            End = start + count - 1;
            Count = count;
        }
    }
}