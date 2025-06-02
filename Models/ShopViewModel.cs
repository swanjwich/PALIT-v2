using cce106_palit.Entity;

namespace cce106_palit.Models
{
    public class ShopViewModel
    {
        public IEnumerable<Category> Categories { get; set; }
        public IEnumerable<Product> Products { get; set; }
        public List<int> SelectedCategoryIds { get; set; }
        public decimal? MinPrice { get; set; } 
        public decimal? MaxPrice { get; set; }
        public string? SearchQuery { get; set; }

    }
}
