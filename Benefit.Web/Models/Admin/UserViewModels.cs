using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using Benefit.DataTransfer.ViewModels;
using Benefit.Domain.Models;
using Benefit.Domain.Models.Enums;

namespace Benefit.Web.Models.Admin
{
    public class EditUserViewModel
    {
        public EditUserViewModel()
        {
            Addresses = new Collection<Address>();
        }
        public string Id { get; set; }
        [Required]
        public int ExternalNumber { get; set; }
        [Required]
        [MaxLength(64)]
        public string FullName { get; set; }
        public int? ReferalNumber { get; set; }
        public string ReferalId { get; set; }
        [MaxLength(10)]
        public string CardNumber { get; set; }
        [MaxLength(10)]
        public string NFCNumber { get; set; }
        public BusinessLevel? BusinessLevel { get; set; }
        public Status? Status { get; set; }
        public bool IsActive { get; set; }
        public bool IsCardVerified { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
        public DateTime RegisteredOn { get; set; }
        public ICollection<Address> Addresses { get; set; }
        public PaginatedList<Order> BenefitCardOrders { get; set; } 
        public PaginatedList<Order> OnlineOrders { get; set; } 
    }
}