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
using System.Device.Location;
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
            new Key().WithElement("Place", id);

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
        public static Entity ToEntity(this Place place)
        {
            var entity =  new Entity()
            {
                Key = place.Id.ToKey(),
                ["LocationName"] = place.LocationName,
                ["ImageUrl"] = place.ImageUrl,
                ["Description"] = place.Description,
                ["Latitude"] = place.Coordinates.Latitude,
                ["Longitude"] = place.Coordinates.Longitude,
                ["UserRating"] = place.UserRating,
            };
            if (place.CreatedBy != null)
            {
                entity["CreatedById"] = place.CreatedBy.Id;
                entity["CreatedByName"] = place.CreatedBy.Name;
            }
            return entity;
        }

        // [END toentity]

        /// <summary>
        /// Unpack a book from a datastore entity.
        /// </summary>
        /// <param name="entity">An entity retrieved from datastore.</param>
        /// <returns>A book.</returns>
        public static Place ToPlace(this Entity entity) => new Place()
        {
            Id = entity.Key.Path.First().Id,
            LocationName = (string)entity["LocationName"],
            Coordinates = new GeoCoordinate((double)entity["Latitude"], (double)entity["Longitude"]),
            ImageUrl = (string)entity["ImageUrl"],
            Description = (string)entity["Description"],
            CreatedBy = new PlacesUser() { Name = (string)entity["CreatedByName"], Id = (string)entity["CreatedByid"] },
            UserRating = (long)(entity["UserRating"] == null ? 1 : entity["UserRating"])
        };

        public static double MaxLatitude(this GeoCoordinate coord, int distance)
        {
            double lat = 0;

            return lat;
        }
        public static double MinLatitude(this GeoCoordinate coord, int distance)
        {
            double lat = 0;

            return lat;
        }
        public static double MaxLongitude(this GeoCoordinate coord, int distance)
        {
            double lon = 0;

            return lon;
        }
        public static double MinLongitude(this GeoCoordinate coord, int distance)
        {
            double lon = 0;

            return lon;
        }
    }

    public class DatastorePlaceStore : IPlaceStore
    {
        private readonly string _projectId;
        private readonly DatastoreDb _db;

        /// <summary>
        /// Create a new datastore-backed bookstore.
        /// </summary>
        /// <param name="projectId">Your Google Cloud project id</param>
        public DatastorePlaceStore(string projectId)
        {
            _projectId = projectId;
            _db = DatastoreDb.Create(_projectId);
        }

        // [START create]
        public void Create(Place place)
        {
            var entity = place.ToEntity();
            entity.Key = _db.CreateKeyFactory("Place").CreateIncompleteKey();
            var keys = _db.Insert(new[] { entity });
            place.Id = keys.First().Path.First().Id;
        }
        // [END create]

        public void Delete(long id)
        {
            _db.Delete(id.ToKey());
        }

        // [START list]
        public PlaceList List(int pageSize, string nextPageToken, string userId = null, GeoCoordinate coordinates = null)
        {
            var query = new Query("Place") { Limit = pageSize };
            if (userId != null)
                query.Filter = Filter.Equal("CreatedById", userId);
            if (coordinates != null)
            {
                query.Filter = Filter.And(
                                Filter.GreaterThan("Latitude", coordinates.MaxLatitude(100)), 
                                Filter.GreaterThan("Latitude", coordinates.MinLatitude(100)),
                                Filter.GreaterThan("Longitude", coordinates.MaxLongitude(100)),
                                Filter.GreaterThan("Longitude", coordinates.MinLongitude(100))
                                );
            }
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
            //return _db.Lookup(id.ToKey())?.ToPlace();

            var tmp = _db.Lookup(id.ToKey());
            Place place = null;
            if (tmp != null)
                place = tmp.ToPlace();
            return place;
        }

        public void Update(Place place)
        {
            _db.Update(place.ToEntity());
        }
    }
}