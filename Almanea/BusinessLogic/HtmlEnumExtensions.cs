﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Almanea
{
    public static class HtmlEnumExtensions
    {
        public static MvcHtmlString EnumToString<T>(this HtmlHelper helper)
        {
            var values = Enum.GetValues(typeof(T)).Cast<int>();
            var enumDictionary = values.ToDictionary(value => Enum.GetName(typeof(T), value));

            return new MvcHtmlString(JsonConvert.SerializeObject(enumDictionary));
        }
    }
}