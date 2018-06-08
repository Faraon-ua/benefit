using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using System.Web.Optimization;
using WebGrease.Css.Extensions;

namespace Benefit.Web.Helpers
{
    public class FileVersionTransformer : IBundleTransform
    {
        public void Process(BundleContext context, BundleResponse response)
        {
            response.Files.ForEach(entry =>
            {
                var version = GetVersion(entry.IncludedVirtualPath);
                entry.IncludedVirtualPath += version;
            });
        }

        private string GetVersion(string filename)
        {
            if (HttpRuntime.Cache[filename] == null)
            {
                var physicalPath = HttpContext.Current.Server.MapPath(filename);
                var version = string.Format("?v={0}", new FileInfo(physicalPath).LastWriteTime.ToString("MMddHHmmss"));
                HttpRuntime.Cache.Add(filename, version, null,
                  DateTime.Now.AddMinutes(10), TimeSpan.Zero,
                  CacheItemPriority.Normal, null);
                return version;
            }
            else
            {
                return HttpRuntime.Cache[filename] as string;
            }
        }
    }

    public static class VersionedFileHtmlHelper
    {
        public static MvcHtmlString IncludeVersionedFile(this HtmlHelper helper, string filename)
        {
            string version = GetVersion(helper, filename);
            return MvcHtmlString.Create(filename + version);
        }

        private static string GetVersion(this HtmlHelper helper, string filename)
        {
            var context = helper.ViewContext.RequestContext.HttpContext;

            if (context.Cache[filename] == null)
            {
                var originalDirectory = AppDomain.CurrentDomain.BaseDirectory.Replace(@"bin\Debug\", string.Empty);
                var physicalPath = Path.Combine(originalDirectory, filename);
                var version = string.Format("?v={0}", new System.IO.FileInfo(physicalPath).LastWriteTime.ToString("MMddHHmmss"));
                context.Cache.Add(filename, version, null,
                  DateTime.Now.AddMinutes(5), TimeSpan.Zero,
                  CacheItemPriority.Normal, null);
                return version;
            }
            else
            {
                return context.Cache[filename] as string;
            }
        }
    }
}