﻿using System.Collections.Generic;
using Benefit.Domain.Models;
using System.ComponentModel.DataAnnotations;

namespace Benefit.DataTransfer.ViewModels
{
    public class CompleteOrderViewModel
    {
        public CompleteOrderViewModel()
        {
            ShippingMethods = new List<ShippingMethod>();
            Addresses = new List<Address>();
            PaymentTypes = new List<PaymentType>();
        }
        public List<ShippingMethod> ShippingMethods { get; set; }
        public List<Address> Addresses { get; set; }
        public List<PaymentType> PaymentTypes { get; set; }
        [Required(ErrorMessage = "Оберіть метод доставки")]
        public string ShippingMethodId { get; set; }
        [Required(ErrorMessage = "Оберіть адресу доставки")]
        public string AddressId { get; set; }
        [Required(ErrorMessage = "Оберіть вид оплати")]
        public PaymentType? PaymentType { get; set; }
        public Order Order { get; set; }
        public string Comment { get; set; }
    }
}
