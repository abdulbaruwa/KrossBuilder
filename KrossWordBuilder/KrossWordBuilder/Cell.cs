using System;

namespace KrossWordBuilder
{
    public class Cell
    {
        public int Row { get; set; }
        public int Col { get; set; }
        public string Character { get; set; }
        public bool IsFirstLetter { get; set; }
        public bool IsJunction { get; set; }
        public Tuple<int, int> HorizontalPreceedingRelative { get; set; }
        public Tuple<int, int> VerticalPreceedingRelative { get; set; }
        public string[] WordH { get; set; }
        public int Index { get; set; }
        public int IndexV { get; set; }
        public string[] WordV { get; set; }
    }
}