using System;

namespace KrossWordBuilder
{
    public class InsertWordResult
    {
        public bool IsVertical { get; set; }
        public Tuple<int, int> StartCell { get; set; }
        public Tuple<int, int> EndCell { get; set; }
        public bool Inserted { get; set; }
        public string[] Word { get; set; }
    }
}