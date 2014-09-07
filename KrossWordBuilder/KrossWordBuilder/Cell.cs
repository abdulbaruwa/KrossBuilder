namespace KrossWordBuilder
{
    public class Cell
    {
        public int Row { get; set; }
        public int Col { get; set; }
        public string Character { get; set; }
        public bool IsFirstLetter { get; set; }
        public bool IsJunction { get; set; }
    }
}