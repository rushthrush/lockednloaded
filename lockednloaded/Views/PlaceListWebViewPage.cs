using System.Web.Mvc;
using LockedNLoaded.Models;

namespace LockedNLoaded.Views
{
    // [START custom_web_view]
    public abstract class PlaceListWebViewPage<TModel> : WebViewPage<TModel>
    {
        public User CurrentUser => new User(this.User);
    }
    // [END custom_web_view]

    public abstract class PlaceListWebViewPage : WebViewPage { }
}
