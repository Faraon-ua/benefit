using Benefit.Common.Extensions;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;
using Benefit.Domain.Models.XmlModels;
using Benefit.Web.Helpers;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;

namespace Benefit.Services.Import
{
    public class OneСImportService : BaseImportService
    {
        object lockObj = new object();
        protected override void ProcessImport(ExportImport importTask)
        {
            using (var db = new ApplicationDbContext())
            {
                System.Net.HttpWebRequest req = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(importTask.FileUrl);
                req.Timeout = 1000 * 60 * 5; // milliseconds
                System.Net.WebResponse res = req.GetResponse();
                Stream responseStream = res.GetResponseStream();
                var xml = XDocument.Load(responseStream);
                responseStream.Close();
                var rawXmlCategories = xml.Descendants("Группы").First().Elements().ToList();
                var resultXmlCategories = GetAllFiniteCategories(rawXmlCategories);
                var resultXmlCategoryIds = resultXmlCategories.Select(entry => entry.Element("Ид").Value);
                CreateAndUpdate1CCategories(resultXmlCategories, importTask.SellerId, db);
                db.SaveChanges();
                DeleteImportCategories(importTask.Seller, resultXmlCategories, SyncType.OneCCommerceMl, db);
                db.SaveChanges();

                var xmlProducts = xml.Descendants("Товары").First().Elements()
                    .Where(entry => entry.Element("Группы") != null)
                    .Where(entry => resultXmlCategoryIds.Contains(entry.Element("Группы").Element("Ид").Value)).ToList();
                var ids = xmlProducts.Select(entry => entry.Element("Ид").Value).ToList();
                var xmlProductsSkipped = xml.Descendants("Товары").First().Elements()
                    .Where(entry => !ids.Contains(entry.Element("Ид").Value)).ToList();
                AddAndUpdate1СProducts(xmlProducts.ToList(), importTask.SellerId, importTask.Seller.UrlName, db);
                db.SaveChanges();
                DeleteProducts(xmlProducts, importTask.SellerId, db);
                db.SaveChanges();
                //Task.Run(() => EmailService.SendImportResults(seller.Owner.Email, results));
                //images
                xml = XDocument.Load(importTask.FileUrl.Replace("import", "offers"));
                var xmlProductPrices = xml.Descendants("Предложение");
                var poductPrices = xmlProductPrices.Select(entry => new XmlProductPrice(entry)).ToList();
                var pricesResult = ProcessImportedProductPrices(poductPrices);
            }
        }

        public int ProcessImportedProductPrices(IEnumerable<XmlProductPrice> xmlProductPrices)
        {
            using (var db = new ApplicationDbContext())
            {
                int productPricesUpdated = 0;
                var productIds = xmlProductPrices.Select(entry => entry.Id).ToList();
                var products = db.Products.Where(entry => productIds.Contains(entry.Id));
                Parallel.ForEach(products, (product) =>
                {
                    var xmlProductPrice = xmlProductPrices.First(entry => entry.Id == product.Id);
                    product.Price = xmlProductPrice.Price;
                    product.AvailableAmount = (int)xmlProductPrice.Amount;
                    productPricesUpdated++;
                });

                db.SaveChanges();
                return productPricesUpdated;
            }
        }
        private void DeleteProducts(List<XElement> xmlProducts, string sellerId, ApplicationDbContext db)
        {
            var currentSellerProductIds = db.Products.Where(entry => entry.SellerId == sellerId && entry.IsImported)
                .Select(entry => entry.Id).ToList();
            List<string> xmlProductIds = null;
            xmlProductIds = xmlProducts.Select(entry => entry.Element("Ид").Value).ToList();
            var productIdsToRemove = currentSellerProductIds.Except(xmlProductIds).Where(entry => entry != null).ToList();
            var productsToRemove = db.Products.Where(entry => entry.SellerId == sellerId && productIdsToRemove.Contains(entry.Id)).ToList();
            Parallel.ForEach(productsToRemove, (dbProduct) =>
            {
                dbProduct.AvailabilityState = ProductAvailabilityState.NotInStock;
            });
        }
        private void AddAndUpdate1СProducts(List<XElement> xmlProducts, string sellerId, string sellerUrl, ApplicationDbContext db)
        {
            var maxSku = db.Products.Max(entry => entry.SKU) + 1;
            var xmlProductIds = xmlProducts.Select(entry => entry.Element("Ид").Value).ToList();
            var dbProducts = db.Products.Where(entry => entry.SellerId == sellerId && entry.IsImported).ToList();
            var dbProductIds = dbProducts.Select(entry => entry.Id).ToList();
            var productIdsToAdd = xmlProductIds.Where(entry => !dbProductIds.Contains(entry)).ToList();
            //additional check all over DB
            productIdsToAdd = productIdsToAdd.Where(entry => !db.Products.Select(pr => pr.Id).Contains(entry)).ToList();
            var productIdsToUpdate = xmlProductIds.Where(dbProductIds.Contains).ToList();

            var productsToAddList = new List<Product>();
            var imagesToAddList = new List<Image>();

            var defaultImageIds = dbProducts.Select(entry => entry.DefaultImageId).Where(entry => entry != null).ToList();
            var imagesDbContext = new ImagesDBContext();
            imagesDbContext.BatchDeleteIn("IsImported = 1",
                new DeleteInModel { ColumnName = "ProductId", IncludeIn = true, Ids = dbProductIds },
                new DeleteInModel { ColumnName = "Id", IncludeIn = false, Ids = defaultImageIds });

            var affectedProducts = new List<Product>();
            Parallel.ForEach(productIdsToAdd, (productIdToAdd) =>
            {
                var xmlProduct = xmlProducts.First(entry => entry.Element("Ид").Value == productIdToAdd);
                var name = HttpUtility
                    .HtmlDecode(xmlProduct.Element("Наименование").Value.Replace("\n", "").Replace("\r", "").Trim())
                    .Truncate(256);
                var descr = xmlProduct.Element("Описание").GetValueOrDefault(string.Empty).Replace("\n", "<br/>");
                var urlName = name.Translit().Truncate(128);
                var product = new Product()
                {
                    Id = xmlProduct.Element("Ид").Value,
                    ExternalId = xmlProduct.Element("ШтрихКод").GetValueOrDefault(null),
                    Name = name,
                    UrlName = urlName,
                    CategoryId = xmlProduct.Element("Группы").Element("Ид").Value,
                    SellerId = sellerId,
                    Description = string.IsNullOrEmpty(descr) ? name : descr,
                    AvailabilityState = ProductAvailabilityState.AlwaysAvailable,
                    IsActive = true,
                    IsImported = true,
                    DoesCountForShipping = true,
                    AddedOn = DateTime.UtcNow,
                    LastModified = DateTime.UtcNow,
                    AltText = name.Truncate(100),
                    ShortDescription = name
                };

                var order = 0;
                lock (lockObj)
                {
                    productsToAddList.Add(product);

                    imagesToAddList.AddRange(xmlProduct.Elements("Картинка").Where(entry => !string.IsNullOrEmpty(entry.Value)).Select(xmlImage => new Image()
                    {
                        Id = Guid.NewGuid().ToString(),
                        ImageType = ImageType.ProductGallery,
                        ImageUrl = new Uri(SettingsService.BaseHostName).Append("FTP").Append("LocalUser").Append(sellerUrl).Append(xmlImage.Value).AbsoluteUri,
                        IsAbsoluteUrl = true,
                        Order = order++,
                        ProductId = product.Id,
                        IsImported = true
                    }));
                }
            });

            Parallel.ForEach(productIdsToUpdate, (productIdToUpdate) =>
            {
                var product = dbProducts.FirstOrDefault(entry => entry.Id == productIdToUpdate);
                var xmlProduct = xmlProducts.First(entry => entry.Element("Ид").Value == productIdToUpdate);

                product.ExternalId = xmlProduct.Element("ШтрихКод").GetValueOrDefault(null);
                product.IsImported = true;
                product.IsActive = true;
                product.CategoryId = xmlProduct.Element("Группы").Element("Ид").Value;
                product.AvailabilityState = ProductAvailabilityState.AlwaysAvailable;
                product.LastModified = DateTime.UtcNow;

                var order = 0;
                lock (lockObj)
                {
                    affectedProducts.Add(product);

                    imagesToAddList.AddRange(xmlProduct.Elements("Картинка").Where(entry => !string.IsNullOrEmpty(entry.Value)).Select(xmlImage => new Image()
                    {
                        Id = Guid.NewGuid().ToString(),
                        ImageType = ImageType.ProductGallery,
                        ImageUrl = new Uri(SettingsService.BaseHostName).Append("FTP").Append("LocalUser").Append(sellerUrl).Append(xmlImage.Value).AbsoluteUri,
                        IsAbsoluteUrl = true,
                        Order = order++,
                        ProductId = product.Id,
                        IsImported = true
                    }));
                }
            });
            foreach (var product in productsToAddList)
            {
                product.SKU = maxSku;
                product.UrlName = product.UrlName.Insert(0, maxSku++ + "-").Truncate(128);
            }
            affectedProducts.AddRange(productsToAddList);
            db.InsertIntoMembers(productsToAddList);
            db.SaveChanges();
            db.InsertIntoMembers(imagesToAddList);
            foreach (var product in affectedProducts)
            {
                var img = imagesToAddList.FirstOrDefault(entry => entry.ProductId == product.Id && entry.Order == 0);
                if (img != null)
                {
                    product.DefaultImageId = img.Id;
                }
            }
            foreach (var image in imagesToAddList)
            {
                var uri = new Uri(image.ImageUrl);
                var path = originalDirectory + uri.LocalPath;
                var ImagesService = new ImagesService(db);
                ImagesService.ResizeToSiteRatio(path, ImageType.ProductGallery);
            }
        }

        private List<XElement> GetAllFiniteCategories(IEnumerable<XElement> xmlCategories)
        {
            var resultXmlCategories = new List<XElement>();
            var hadChildren = false;
            foreach (var rawXmlCategory in xmlCategories)
            {
                if (rawXmlCategory.Element("Группы") != null)
                {
                    resultXmlCategories.AddRange(rawXmlCategory.Element("Группы").Elements());
                    hadChildren = true;
                }
                else
                {
                    resultXmlCategories.Add(rawXmlCategory);
                }
            }
            if (hadChildren)
            {
                resultXmlCategories = GetAllFiniteCategories(resultXmlCategories);
            }
            return resultXmlCategories;
        }

        private void CreateAndUpdate1CCategories(List<XElement> xmlCategories, string sellerId, ApplicationDbContext db)
        {
            var hasNewContent = false;
            var sellerCats = new List<SellerCategory>();
            var seller = db.Sellers.Include(entry => entry.SellerCategories)
                .FirstOrDefault(entry => entry.Id == sellerId);
            var sellerCatsDiscount = seller.SellerCategories.Where(entry => entry.CustomDiscount.HasValue)
                .ToDictionary(entry => entry.CategoryId, entry => entry.CustomDiscount.Value);
            db.SellerCategories.RemoveRange(seller.SellerCategories);
            foreach (var xmlCategory in xmlCategories)
            {
                var catId = xmlCategory.Element("Ид").Value;
                var catName = xmlCategory.Element("Наименование").Value.Replace("\n", "").Replace("\r", "").Trim();
                var dbCategory = db.Categories.FirstOrDefault(entry => entry.Id == catId);
                if (dbCategory == null)
                {
                    if (!hasNewContent)
                    {
                        hasNewContent = true;
                    }

                    dbCategory = new Category()
                    {
                        Id = catId,
                        IsSellerCategory = true,
                        SellerId = sellerId,
                        Name = catName.Truncate(64),
                        UrlName = string.Format("{0}-{1}", catId, catName.Translit()).Truncate(128),
                        ExternalIds = catId,
                        IsActive = true,
                        LastModified = DateTime.UtcNow,
                        LastModifiedBy = "ImportFrom1C"
                    };
                    db.Categories.Add(dbCategory);
                }
                else
                {
                    sellerCats.Add(new SellerCategory()
                    {
                        CategoryId = dbCategory.MappedParentCategoryId,
                        SellerId = sellerId
                    });
                    dbCategory.ExternalIds = catId;
                    dbCategory.IsActive = true;
                    dbCategory.Name = catName.Truncate(64);
                    dbCategory.UrlName = string.Format("{0}-{1}", catId, catName.Translit()).Truncate(128);
                    db.Entry(dbCategory).State = EntityState.Modified;
                }
            }

            sellerCats = sellerCats.Where(entry => entry.CategoryId != null).Distinct(new SellerCategoryComparer()).ToList();
            foreach (var d in sellerCatsDiscount)
            {
                var sellerCat = sellerCats.FirstOrDefault(entry => entry.CategoryId == d.Key);
                if (sellerCat != null)
                {
                    sellerCat.CustomDiscount = d.Value;
                }
            }
            db.SellerCategories.AddRange(sellerCats);
            if (hasNewContent)
            {
                var importTask = db.ExportImports.FirstOrDefault(entry => entry.SellerId == sellerId);
                importTask.HasNewContent = true;
            }
        }
    }
}
