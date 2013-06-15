using System.Collections.Generic;
using Microsoft.Build.Framework;

namespace Chapter02Bookstore.Models
{
    public class Book
    {
        public string Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Author { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public double Price { get; set; }

        [Required]
        public int YearPublished { get; set; }

        public List<string> Departments { get; set; }
        public List<CustomerReview> CustomerReviews { get; set; }
    }
}