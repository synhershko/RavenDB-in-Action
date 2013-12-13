namespace Chapter06.Models
{
    public class Book
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public string Author { get; set; }

        public string Description { get; set; }

        public double Price { get; set; }

        public int Pages { get; set; }

        public int YearPublished { get; set; }
    }
}