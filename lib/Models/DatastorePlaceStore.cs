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

using Google.Cloud.Datastore.V1;
using Google.Protobuf;
using System;
using System.Linq;

namespace LockedNLoaded.Models
{
    public static class DatastorePlaceStoreExtensionMethods
    {
        /// <summary>
        /// Make a datastore key given a book's id.
        /// </summary>
        /// <param name="id">A book's id.</param>
        /// <returns>A datastore key.</returns>
        public static Key ToKey(this long id) =>
            new Key().WithElement("Book", id);

        /// <summary>
        /// Make a book id given a datastore key.
        /// </summary>
        /// <param name="key">A datastore key</param>
        /// <returns>A book id.</returns>
        public static long ToId(this Key key) => key.Path.First().Id;

        /// <summary>
        /// Create a datastore entity with the same values as Place.
        /// </summary>
        /// <param name="place">The book to store in datastore.</param>
        /// <returns>A datastore entity.</returns>
        /// [START toentity]
        public static Entity ToEntity(this Place place) => new Entity()
        {
            Key = place.Id.ToKey(),
            ["Title"] = place.Title,
            ["PublishedDate"] = place.PublishedDate?.ToUniversalTime(),
            ["ImageUrl"] = place.ImageUrl,
            ["Description"] = place.Description,
            ["CreatedById"] = place.CreatedBy.Id,
            ["CreatedByName"] = place.CreatedBy.Name
        };
        // [END toentity]

        /// <summary>
        /// Unpack a book from a datastore entity.
        /// </summary>
        /// <param name="entity">An entity retrieved from datastore.</param>
        /// <returns>A book.</returns>
        public static Place ToPlace(this Entity entity) => new Place()
        {
            Id = entity.Key.Path.First().Id,
            Title = (string)entity["Title"],
            PublishedDate = (DateTime?)entity["PublishedDate"],
            ImageUrl = (string)entity["ImageUrl"],
            Description = (string)entity["Description"],
            CreatedBy = new PlacesUser() { Name = (string)entity["CreatedByName"], Id = (string)entity["CreatedByid"] }
            };
    }

    public class DatastoreBookStore : IPlaceStore
    {
        private readonly string _projectId;
        private readonly DatastoreDb _db;

        /// <summary>
        /// Create a new datastore-backed bookstore.
        /// </summary>
        /// <param name="projectId">Your Google Cloud project id</param>
        public DatastoreBookStore(string projectId)
        {
            _projectId = projectId;
            _db = DatastoreDb.Create(_projectId);
        }

        // [START create]
        public void Create(Place book)
        {
            var entity = book.ToEntity();
            entity.Key = _db.CreateKeyFactory("Book").CreateIncompleteKey();
            var keys = _db.Insert(new[] { entity });
            book.Id = keys.First().Path.First().Id;
        }
        // [END create]

        public void Delete(long id)
        {
            _db.Delete(id.ToKey());
        }

        // [START list]
        public PlaceList List(int pageSize, string nextPageToken, string userId = null)
        {
            var query = new Query("Book") { Limit = pageSize };
            if (userId != null)
                query.Filter = Filter.Equal("CreatedById", userId);
            if (!string.IsNullOrWhiteSpace(nextPageToken))
                query.StartCursor = ByteString.FromBase64(nextPageToken);
            var results = _db.RunQuery(query);
            return new PlaceList()
            {
                Places = results.Entities.Select(entity => entity.ToPlace()),
                NextPageToken = results.Entities.Count == query.Limit ?
                    results.EndCursor.ToBase64() : null
            };
        }
        // [END list]

        public Place Read(long id)
        {
            return _db.Lookup(id.ToKey())?.ToPlace();
        }

        public void Update(Place book)
        {
            _db.Update(book.ToEntity());
        }
    }
}