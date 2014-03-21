﻿using System;
using System.Web;
using System.Web.UI;

namespace Rocky.Web
{
    public sealed class EntityBinder<T>
    {
        private HttpContext context;

        public T Current
        {
            get
            {
                var page = PageBase.Current;
                return (T)page.GetDataItem();
            }
        }

        public EntityBinder()
        {
            context = HttpContext.Current;
            if (context == null)
            {
                throw new InvalidOperationException("context");
            }
        }

        public T Entity(IDataItemContainer container)
        {
            return (T)container.DataItem;
        }
    }
}