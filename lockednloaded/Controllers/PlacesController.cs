// Copyright(c) 2016 Google Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not
// use this file except in compliance with the License. You may obtain a copy of
// the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
// WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
// License for the specific language governing permissions and limitations under
// the License.

using LockedNLoaded.Models;
using LockedNLoaded.Services;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace LockedNLoaded.Controllers
{
    public class PlacesController : Controller
    {
        /// <summary>
        /// How many places should we display on each page of the index?
        /// </summary>
        private const int _pageSize = 10;

        private readonly IPlaceStore _store;
        private readonly ImageUploader _imageUploader;

        public User CurrentUser => new User(this.User);

        public PlacesController(IPlaceStore store, ImageUploader imageUploader)
        {
            _store = store;
            _imageUploader = imageUploader;
        }

        // GET: Places
        public ActionResult Index(string nextPageToken)
        {
            return View(new ViewModels.Places.Index()
            {
                PlaceList = _store.List(_pageSize, nextPageToken)
            });
        }

        // GET: Places/Mine
        // [START mine]
        public ActionResult Mine(string nextPageToken)
        {
            if (Request.IsAuthenticated)
            {
                return View("Index", new ViewModels.Places.Index()
                {
                    // Fetch places created by the logged in user
                    PlaceList = _store.List(_pageSize, nextPageToken, CurrentUser.UserId)
                });
            }
            else
            {
                return RedirectToAction("Index");
            }
        }
        // [END mine]

        // GET: Places/Details/5
        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            Place place = _store.Read((long)id);
            if (place == null)
            {
                return HttpNotFound();
            }

            return View(place);
        }

        // [START create]
        // GET: Places/Create
        public ActionResult Create()
        {
            return ViewForm("Create", "Create");
        }

        // POST: Places/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        // [START create_place]
        public async Task<ActionResult> Create(Place place, HttpPostedFileBase image)
        {
            if (ModelState.IsValid)
            {
                place.CreatedBy = new PlacesUser(){ Id = CurrentUser.UserId, Name = CurrentUser.Name};

                _store.Create(place);

                // ...
                // [END creat_place]
                // If place image submitted, save image to Cloud Storage
                if (image != null)
                {
                    var imageUrl = await _imageUploader.UploadImage(image, place.Id);
                    place.ImageUrl = imageUrl;
                    _store.Update(place);
                }
                return RedirectToAction("Details", new { id = place.Id });
            }
            return ViewForm("Create", "Create", place);
        }
        // [END create]

        /// <summary>
        /// Dispays the common form used for the Edit and Create pages.
        /// </summary>
        /// <param name="action">The string to display to the user.</param>
        /// <param name="place">The asp-action value.  Where will the form be submitted?</param>
        /// <returns>An IActionResult that displays the form.</returns>
        private ActionResult ViewForm(string action, string formAction, Place place = null)
        {
            var form = new ViewModels.Places.Form()
            {
                Action = action,
                Place = place ?? new Place(),
                IsValid = ModelState.IsValid,
                FormAction = formAction
            };
            return View("/Views/Places/Form.cshtml", form);
        }

        // GET: Places/Edit/5
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            Place place = _store.Read((long)id);
            if (place == null)
            {
                return HttpNotFound();
            }
            return ViewForm("Edit", "Edit", place);
        }

        // POST: Places/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Place place, long id)
        {
            place.Id = id;
            if (ModelState.IsValid)
            {
                place.CreatedBy = new PlacesUser() { Id = CurrentUser.UserId, Name = CurrentUser.Name };
                _store.Update(place);
                return RedirectToAction("Details", new { id = place.Id });
            }
            return ViewForm("Edit", "Edit", place);
        }

        // POST: Places/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(long id)
        {
            // Delete place cover image from Cloud Storage if ImageUrl exists
            string imageUrlToDelete = _store.Read((long)id).ImageUrl;
            if (imageUrlToDelete != null)
            {
                await _imageUploader.DeleteUploadedImage(id);
            }
            _store.Delete(id);
            return RedirectToAction("Index");
        }
    }
}
