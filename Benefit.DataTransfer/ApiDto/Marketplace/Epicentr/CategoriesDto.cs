using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Benefit.DataTransfer.ApiDto.Marketplace.Epicentr
{
    public class CategoriesDto
    {
        public int page { get; set; }
        public int pages { get; set; }
        public IEnumerable<CategoryDto> items { get; set; }
    }
    public class CategoryDto
    {
        [DisplayName("Id")]
        public string code { get; set; }
        [DisplayName("Батьківска Id")]
        public string parentCode { get; set; }
        public IEnumerable<TranslationDto> translations { get; set; }
        [DisplayName("Назва")]
        public string name
        {
            get
            {
                return translations.FirstOrDefault(entry => entry.languageCode == "ua").title;
            }
        }
    }
    public class TranslationDto
    {
        public string title { get; set; }
        public string languageCode { get; set; }
    }
}
