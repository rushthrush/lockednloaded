﻿// Copyright(c) 2016 Google Inc.
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
    public class DbBookStore : IPlaceStore
    {
        private readonly ApplicationDbContext _dbcontext;

        public DbBookStore(ApplicationDbContext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        // [START create]
        public void Create(Place book)
        {
            var trackBook = _dbcontext.Books.Add(book);
            _dbcontext.SaveChanges();
            book.Id = trackBook.Id;
        }
        // [END create]
        public void Delete(long id)
        {
            Place book = _dbcontext.Books.Single(m => m.Id == id);
            _dbcontext.Books.Remove(book);
            _dbcontext.SaveChanges();
        }

        // [START list]
        public PlaceList List(int pageSize, string nextPageToken, string userId = null)
        {
            IQueryable<Place> query = _dbcontext.Books.OrderBy(book => book.Id);
            if (userId != null)
            {
                // Query for books created by the user
                query = query.Where(book => book.CreatedById == userId);
            }
            if (nextPageToken != null)
            {
                long previousBookId = long.Parse(nextPageToken);
                query = query.Where(book => book.Id > previousBookId);
            }
            var books = query.Take(pageSize).ToArray();
            return new PlaceList()
            {
                Places = books,
                NextPageToken = books.Count() == pageSize ? books.Last().Id.ToString() : null
            };
        }
        // [END list]

        public Place Read(long id)
        {
            return _dbcontext.Books.Single(m => m.Id == id);
        }

        public void Update(Place book)
        {
            _dbcontext.Entry(book).State = System.Data.Entity.EntityState.Modified;
            _dbcontext.SaveChanges();
        }
    }
}