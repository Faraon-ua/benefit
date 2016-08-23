using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Benefit.Domain.Models.Enums;

namespace Benefit.Web.Models.Admin
{
    public class EditUserViewModel
    {
        public string Id { get; set; }
        [Required]
        public int ExternalNumber { get; set; }
        [Required]
        [MaxLength(64)]
        public string FullName { get; set; }
        [Required]
        public int ReferalNumber { get; set; }
        public string ReferalId { get; set; }
        [MaxLength(10)]
        public string CardNumber { get; set; }
        public BusinessLevel? BusinessLevel { get; set; }
        public Status? Status { get; set; }
        public bool IsActive { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
        public DateTime RegisteredOn { get; set; }
    }
}