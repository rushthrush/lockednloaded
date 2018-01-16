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

using System.Linq;

namespace LockedNLoaded.Models
{
    /// <summary>
    /// Implements IPlaceStore with a database.
    /// </summary>
    public class DbPlaceStore : IPlaceStore
    {
        private readonly ApplicationDbContext _dbcontext;

        public DbPlaceStore(ApplicationDbContext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        // [START create]
        public void Create(Place place)
        {
            var trackPlace = _dbcontext.Places.Add(place);
            _dbcontext.SaveChanges();
            place.Id = trackPlace.Id;
        }
        // [END create]
        public void Delete(long id)
        {
            Place place = _dbcontext.Places.Single(m => m.Id == id);
            _dbcontext.Places.Remove(place);
            _dbcontext.SaveChanges();
        }

        // [START list]
        public PlaceList List(int pageSize, string nextPageToken, string userId = null)
        {
            IQueryable<Place> query = _dbcontext.Places.OrderBy(book => book.Id);
            if (userId != null)
            {
                // Query for items created by the user
                query = query.Where(item => item.CreatedBy.Id == userId);
            }
            if (nextPageToken != null)
            {
                long previousId = long.Parse(nextPageToken);
                query = query.Where(item => item.Id > previousId);
            }
            var places = query.Take(pageSize).ToArray();
            return new PlaceList()
            {
                Places = places,
                NextPageToken = places.Count() == pageSize ? places.Last().Id.ToString() : null
            };
        }
        // [END list]

        public Place Read(long id)
        {
            return _dbcontext.Places.Single(m => m.Id == id);
        }

        public void Update(Place book)
        {
            _dbcontext.Entry(book).State = System.Data.Entity.EntityState.Modified;
            _dbcontext.SaveChanges();
        }
    }
}