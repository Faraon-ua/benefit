﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Benefit.Domain.Models
{
    public enum ImageType
    {
        SellerLogo,
        SellerGallery,
        ProductGallery,
        NewsLogo,
        UserAvatar,
        CategoryLogo,
        SellerCatalog,
        SellerFavicon,
        SellerSlider,
        Banner,
        ProductDefault
    }
    public class Image
    {
        [Key]
        [MaxLength(128)]
        public string Id { get; set; }
        [Required]
        [MaxLength(256)]
        public string ImageUrl { get; set; }
        public bool IsAbsoluteUrl { get; set; }
        public bool IsImported { get; set; }
        public ImageType ImageType { get; set; }
        public int Order { get; set; }
        [MaxLength(128)]
        public string SellerId { get; set; }
        public Seller Seller { get; set; }
        [MaxLength(128)]
        public string ProductId { get; set; }
        public Product Product { get; set; }
        public ICollection<Product> DefaultProducts { get; set; }
    }
}
