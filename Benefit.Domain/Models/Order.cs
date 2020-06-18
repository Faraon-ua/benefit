using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Benefit.Domain.Models
{
    public enum OrderStatus
    {
        [Display(Name = "Нове замовлення", ShortName = "secondary")]
        [Description("Нове замовлення")]
        Created,
        [Display(Name = "Дані підтверджено. Очікує відправлення", ShortName = "info")]
        [Description("Дані підтверджено. Очікує відправлення")]
        AwaitingDelivery,
        [Display(Name = "Передано в службу доставки", ShortName = "presuccess")]
        [Description("Передано в службу доставки")]
        PassedToDelivery,
        [Display(Name = "Доставляється", ShortName = "presuccess")]
        [Description("Доставляється")]
        IsDelivering,
        [Display(Name = "Очікує в пункті самовивозу", ShortName = "danger")]
        [Description("Очікує в пункті самовивозу")]
        WaitingInSelfPickup,
        [Display(Name = "Отримано", ShortName = "success")]
        [Description("Отримано")]
        Finished,
        [Display(Name = "Не відпрацьовано продавцем", ShortName = "info")]
        [Description("Не відпрацьовано продавцем")]
        NotProcessedBySeller,
        miss8,
        miss9,
        [Display(Name = "Відправка прострочена", ShortName = "info")]
        [Description("Відправка прострочена")]
        OverduedDelivery,
        [Display(Name = "Не забрав посилку", ShortName = "danger")]
        [Description("Не забрав посилку")]
        PackageNotAquired,
        [Display(Name = "Відмовився від товару", ShortName = "default")]
        [Description("Відмовився від товару")]
        RefusedFromProducts,
        [Display(Name = "Відмінено адміністратором", ShortName = "info")]
        [Description("Відмінено адміністратором")]
        Abandoned,
        miss14,
        [Display(Name = "Некоректна ТТН", ShortName = "info")]
        [Description("Некоректна ТТН")]
        WrongTTH,
        [Display(Name = "Немає в наявності/Брак", ShortName = "default")]
        [Description("Немає в наявності/Брак")]
        NotAvailableOrDefect,
        [Display(Name = "Відміна. Не влаштовує оплата", ShortName = "default")]
        [Description("Відміна. Не влаштовує оплата")]
        UnsuitedPayment,
        [Display(Name = "Не вдалося зв'язатися з покупцем", ShortName = "info")]
        [Description("Не вдалося зв'язатися з покупцем")]
        NoncontactCustomer,
        [Display(Name = "Повернення", ShortName = "info")]
        [Description("Повернення")]
        Returning,
        [Display(Name = "Відміна. Не влаштовує товар", ShortName = "info")]
        [Description("Відміна. Не влаштовує товар")]
        UnacceptableProduct,
        miss21,
        miss22,
        miss23,
        [Display(Name = "Відміна. Не влаштовує доставка", ShortName = "info")]
        [Description("Відміна. Не влаштовує доставка")]
        UnacceptableShipping,
        [Display(Name = "Тестове замовлення", ShortName = "info")]
        [Description("Тестове замовлення")]
        Test,
        [Display(Name = "Обробляється менеджером", ShortName = "info")]
        [Description("Обробляється менеджером")]
        Processed,
        [Display(Name = "Вимагає доукомплектаціі", ShortName = "info")]
        [Description("Вимагає доукомплектаціі")]
        AddEquipmentNeeded,
        [Display(Name = "Некоректні контактні дані", ShortName = "danger")]
        [Description("Некоректні контактні дані")]
        WrongContactInfo,
        [Display(Name = "Відміна. Некоректна ціна на сайті", ShortName = "danger")]
        [Description("Відміна. Некоректна ціна на сайті")]
        WrongSitePrice,
        [Display(Name = "Закінчився термін резерву", ShortName = "default")]
        [Description("Закінчився термін резерву")]
        ReserveTimeOver,
        [Display(Name = "Відміна. Замовлення відновлено", ShortName = "info")]
        [Description("Відміна. Замовлення відновлено")]
        OrderRestored,
        [Display(Name = "Відміна. Не влаштовує розгрупування замовлення", ShortName = "info")]
        [Description("Відміна. Не влаштовує розгрупування замовлення")]
        UnacceptableOrderGrouping,
        [Display(Name = "Відміна. Не влаштовує вартість доставки", ShortName = "default")]
        [Description("Відміна. Не влаштовує вартість доставки")]
        UnacceptableShippingPrice,
        [Display(Name = "Відміна. Не влаштовує перевізник, спосіб доставки", ShortName = "default")]
        [Description("Відміна. Не влаштовує перевізник, спосіб доставки")]
        UnacceptableShippingType,
        [Display(Name = "Відміна. Не влаштовує термін доставки", ShortName = "default")]
        [Description("Відміна. Не влаштовує термін доставки")]
        UnacceptableShippingTime,
        [Display(Name = "Відміна. Клієнт бажає безготівкову оплату. Продавець не має такої можливості", ShortName = "default")]
        [Description("Відміна. Клієнт бажає безготівкову оплату. Продавець не має такої можливості")]
        UnacceptableNoncashPayment,
        [Display(Name = "Відміна. Не влаштовує передплата", ShortName = "default")]
        [Description("Відміна. Не влаштовує передплата")]
        UnacceptablePrePayment,
        [Display(Name = "Відміна. Не влаштовує якість товару", ShortName = "default")]
        [Description("Відміна. Не влаштовує якість товару")]
        UnacceptableProductQuality,
        [Display(Name = "Відміна. Не підійшли характеристик товару (колір, розмір)", ShortName = "default")]
        [Description("Відміна. Не підійшли характеристик товару (колір, розмір)")]
        UnacceptableProductOptions,
        [Display(Name = "Відміна. Клієнт передумав", ShortName = "default")]
        [Description("Відміна. Клієнт передумав")]
        CustomerRefused,
        [Display(Name = "Відміна. Купив на іншому сайті", ShortName = "default")]
        [Description("Відміна. Купив на іншому сайті")]
        AnotherSiteBought,
        [Display(Name = "Немає в наявності", ShortName = "default")]
        [Description("Немає в наявності")]
        NotAvailable,
        [Display(Name = "Брак", ShortName = "default")]
        [Description("Брак")]
        Defect,
        [Display(Name = "Відміна. Фейкове замовлення", ShortName = "info")]
        [Description("Відміна. Фейкове замовлення")]
        Fake,
        [Display(Name = "Відмінено клієнтом", ShortName = "info")]
        [Description("Відмінено клієнтом")]
        CustomerAbolished,
        [Display(Name = "Відновленний після обдзвону", ShortName = "info")]
        [Description("Відновленний після обдзвону")]
        RestoredAfterRecall,
        [Display(Name = "Обробляється менеджером(не вдалося зв'язатися 1-ий раз)", ShortName = "info")]
        [Description("Обробляється менеджером(не вдалося зв'язатися 1-ий раз)")]
        ContactFail1,
        [Display(Name = "Обробляється менеджером(не вдалося зв'язатися 2-ий раз)", ShortName = "info")]
        [Description("Обробляється менеджером(не вдалося зв'язатися 2-ий раз)")]
        ContactFail2,
        [Display(Name = "Відміна. Дубль замовлення", ShortName = "info")]
        [Description("Відміна. Дубль замовлення")]
        Duplicate
    }

    public enum OrderType
    {
        BenefitSite,
        BenefitCard,
        Rozetka,
        Allo
    }

    [Serializable]
    public class OrderVM
    {
        public OrderVM()
        {
            OrderProducts = new Collection<OrderProduct>();
        }
        public string SellerId { get; set; }
        public string SellerUrlName { get; set; }
        public string SellerPrimaryRegionName { get; set; }
        public double SellerUserDiscount { get; set; }
        public string SellerDiscountName { get; set; }
        public double? SellerDiscount { get; set; }
        public string SellerName { get; set; }
        public double Sum { get; set; }
        public ICollection<OrderProduct> OrderProducts { get; set; }

        public double GetOrderSum()
        {
            var sum = OrderProducts.Sum(
                entry =>
                    entry.ProductPrice * entry.Amount +
                    (entry.OrderProductOptions.Any()
                        ? entry.OrderProductOptions.Sum(option => option.ProductOptionPriceGrowth * option.Amount)
                        : entry.DbOrderProductOptions.Sum(option => option.ProductOptionPriceGrowth * option.Amount)));
            return sum;
        }
    }
    public class Order
    {
        public Order()
        {
            Id = Guid.NewGuid().ToString();
            Status = OrderStatus.Created;
            Transactions = new Collection<Transaction>();
            OrderProducts = new Collection<OrderProduct>();
        }
        public string Id { get; set; }
        public string ExternalId { get; set; }
        public int OrderNumber { get; set; }
        public double Sum { get; set; }
        public string Description { get; set; }
        public double PersonalBonusesSum { get; set; }
        public double PointsSum { get; set; }
        public string CardNumber { get; set; }
        [MaxLength(64)]
        public string ShippingName { get; set; }
        [MaxLength(64)]
        public string ShippingTrackingNumber { get; set; }
        [MaxLength(256)]
        [Required]
        public string ShippingAddress { get; set; }
        public double ShippingCost { get; set; }
        public DateTime Time { get; set; }
        public OrderType OrderType { get; set; }
        public PaymentType PaymentType { get; set; }
        public OrderStatus Status { get; set; }
        [MaxLength(128)]
        public string SellerId { get; set; }
        [MaxLength(128)]
        public string SellerName { get; set; }
        [NotMapped]
        public string SellerUrlName { get; set; }
        [NotMapped]
        public string SellerPrimaryRegionName { get; set; }
        [NotMapped]
        public double SellerUserDiscount { get; set; }
        public string SellerDiscountName { get; set; }
        public double? SellerDiscount { get; set; }
        [MaxLength(128)]
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
        public string UserName { get; set; }
        [MaxLength(20)]
        public string UserPhone { get; set; }
        [MaxLength(64)]
        public string PersonnelName { get; set; }
        public DateTime LastModified { get; set; }
        public string LastModifiedBy { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; }
        public virtual ICollection<OrderProduct> OrderProducts { get; set; }
        public virtual ICollection<OrderStatusStamp> OrderStatusStamps { get; set; }

        [NotMapped]
        public Transaction BonusPaymentTransaction
        {
            get { return Transactions.FirstOrDefault(entry => entry.Type == TransactionType.BonusesOrderPayment); }
        }
        [NotMapped]
        public bool IsRepeating { get; set; }
        [NotMapped]
        public string SellerPhone { get; set; }
        [NotMapped]
        public double SumWithDiscount
        {
            get { return Sum - SellerDiscount.GetValueOrDefault(0); }
        }
        public double GetOrderSum()
        {
            var sum = OrderProducts.Sum(
                entry =>
                    entry.ActualPrice * entry.Amount +
                    (entry.OrderProductOptions.Any()
                        ? entry.OrderProductOptions.Sum(option => option.ProductOptionPriceGrowth * option.Amount)
                        : entry.DbOrderProductOptions.Sum(option => option.ProductOptionPriceGrowth * option.Amount)));
            return sum;
        }

        public static Dictionary<OrderStatus, List<OrderStatus>> AvailableStatuses
        {
            get
            {
                return new Dictionary<OrderStatus, List<OrderStatus>>()
                     {
                        { OrderStatus.Created, new List<OrderStatus> { OrderStatus.Processed} },
                        { OrderStatus.Processed, new List<OrderStatus> { OrderStatus.AwaitingDelivery,
                                                                    OrderStatus.ContactFail1,
                                                                    OrderStatus.UnacceptableShippingType,
                                                                    OrderStatus.UnacceptableShippingPrice,
                                                                    OrderStatus.UnacceptableShippingTime,
                                                                    OrderStatus.UnsuitedPayment,
                                                                    OrderStatus.UnacceptableNoncashPayment,
                                                                    OrderStatus.UnacceptablePrePayment,
                                                                    OrderStatus.AnotherSiteBought,
                                                                    OrderStatus.UnacceptableProductOptions,
                                                                    OrderStatus.NotAvailable,
                                                                    OrderStatus.Defect,
                                                                    OrderStatus.WrongContactInfo,
                                                                    OrderStatus.CustomerRefused,
                                                                    OrderStatus.UnacceptableProduct,
                                                                    OrderStatus.UnacceptableOrderGrouping }},
                        { OrderStatus.AwaitingDelivery, new List<OrderStatus> { OrderStatus.Processed,
                                                                            OrderStatus.IsDelivering,
                                                                            OrderStatus.PassedToDelivery,
                                                                            OrderStatus.Finished,
                                                                            OrderStatus.CustomerRefused,
                                                                            OrderStatus.OverduedDelivery,
                                                                            OrderStatus.PackageNotAquired,
                                                                            OrderStatus.ReserveTimeOver }},
                        { OrderStatus.IsDelivering, new List<OrderStatus> { OrderStatus.WaitingInSelfPickup,
                                                                            OrderStatus.Processed,
                                                                            OrderStatus.Finished,
                                                                            OrderStatus.CustomerRefused }},
                        { OrderStatus.PassedToDelivery, new List<OrderStatus> { OrderStatus.WaitingInSelfPickup,
                                                                            OrderStatus.Processed,
                                                                            OrderStatus.Finished,
                                                                            OrderStatus.CustomerRefused }},
                        { OrderStatus.ContactFail1, new List<OrderStatus> { OrderStatus.AwaitingDelivery,
                                                                            OrderStatus.ContactFail2,
                                                                            OrderStatus.WrongSitePrice,
                                                                            OrderStatus.CustomerRefused,
                                                                            OrderStatus.AnotherSiteBought,
                                                                            OrderStatus.UnacceptableOrderGrouping,
                                                                            OrderStatus.UnacceptableShippingTime,
                                                                            OrderStatus.UnacceptableShippingPrice,
                                                                            OrderStatus.UnacceptableShippingType,
                                                                            OrderStatus.UnsuitedPayment,
                                                                            OrderStatus.UnacceptableNoncashPayment,
                                                                            OrderStatus.UnacceptablePrePayment,
                                                                            OrderStatus.UnacceptableProductQuality,
                                                                            OrderStatus.NotAvailable,
                                                                            OrderStatus.Defect }},
                        { OrderStatus.ContactFail2, new List<OrderStatus> { OrderStatus.AwaitingDelivery,
                                                                            OrderStatus.WrongSitePrice,
                                                                            OrderStatus.CustomerRefused,
                                                                            OrderStatus.AnotherSiteBought,
                                                                            OrderStatus.UnacceptableOrderGrouping,
                                                                            OrderStatus.UnacceptableShippingTime,
                                                                            OrderStatus.UnacceptableShippingPrice,
                                                                            OrderStatus.UnacceptableShippingType,
                                                                            OrderStatus.UnsuitedPayment,
                                                                            OrderStatus.UnacceptableNoncashPayment,
                                                                            OrderStatus.UnacceptablePrePayment,
                                                                            OrderStatus.UnacceptableProductQuality,
                                                                            OrderStatus.NotAvailable,
                                                                            OrderStatus.Defect }}
                    };
            }
        }

        public static Dictionary<string, string> StatusColorMapping
        {
            get
            {
                return new Dictionary<string, string>()
                     {
                        { "secondary", "#999" },
                        { "default", "#333" },
                        { "info", "#3860f6" },
                        { "success", "#12bd0d" },
                        { "presuccess", "#4bbfc6" },
                        { "danger", "#fb3f4c" }
                    };
            }
        }
    }
}
