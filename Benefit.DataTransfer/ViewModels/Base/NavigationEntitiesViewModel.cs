﻿using System.Collections.Generic;
using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;

namespace Benefit.DataTransfer.ViewModels.Base
{
    public class NavigationEntitiesViewModel<T>
    {
        public NavigationEntitiesViewModel()
        {
            Items = new List<T>();
        }
        public CategoryVM Category { get; set; }
        public Seller Seller { get; set; }
        public List<T> CurrentRegionItems { get; set; }
        public List<T> Items { get; set; }
        public BreadCrumbsViewModel Breadcrumbs { get; set; }
    }
}
