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

//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using LockedNLoaded.Models;
using LockedNLoaded.Services;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace LockedNLoaded
{
    internal class FakeBookStore : IPlaceStore
    {
        public Place UpdatedBook = null;

        public void Create(Place book)
        {
            throw new NotImplementedException();
        }

        public void Delete(long id)
        {
            throw new NotImplementedException();
        }

        public PlaceList List(int pageSize, string nextPageToken, string userId = null)
        {
            throw new NotImplementedException();
        }

        public Place Read(long id)
        {
            return new Place()
            {
                Id = 3,
                Title = "test.js"
            };
        }

        public void Update(Place book)
        {
            UpdatedBook = book;
        }
    }

    public class BookDetailLookupTest
    {
        private static readonly string s_projectId;

        static BookDetailLookupTest()
        {
            s_projectId = System.Environment.GetEnvironmentVariable("LockedNLoaded:ProjectId");
            Assert.False(string.IsNullOrWhiteSpace(s_projectId), "Set the environment variable " +
                "LockedNLoaded:ProjectId to your google project id.");
        }

        private PlaceDetailLookup NewBookDetailLookup()
        {
            var options = new PlaceDetailLookup.Options();
            options.SubscriptionId += "-test";
            options.TopicId += "-test";
            return new PlaceDetailLookup(s_projectId, options);
        }

        [Fact]
        public void TestPubsub()
        {
            PlaceDetailLookup bookDetailLookup = NewBookDetailLookup();
            bookDetailLookup.CreateTopicAndSubscription();
            bookDetailLookup.EnquePlace(45);
            var cancel = new CancellationTokenSource();
            var pullTask = Task.Factory.StartNew(() => bookDetailLookup.PullLoop((long bookId) =>
            {
                Assert.Equal(45, bookId);
                cancel.Cancel();
            }, cancel.Token));
            pullTask.Wait();
        }

        [Fact]
        public void TestProcessBook()
        {
            FakeBookStore bookStore = new FakeBookStore();
            PlaceDetailLookup bookDetailLookup = NewBookDetailLookup();
            bookDetailLookup.ProcessBook(bookStore, 3);
            Assert.Equal(3, bookStore.UpdatedBook.Id);
            Assert.Equal("Test-Driven JavaScript Development", bookStore.UpdatedBook.Title);
            Assert.Equal("Christian Johansen", bookStore.UpdatedBook.Author);
        }

        [Fact]
        public void TestLoop()
        {
            PlaceDetailLookup bookDetailLookup = NewBookDetailLookup();
            bookDetailLookup.CreateTopicAndSubscription();
            var cancel = new CancellationTokenSource();
            var pullTask = bookDetailLookup.StartPullLoop(new FakeBookStore(), cancel.Token);
            cancel.CancelAfter(100);
            pullTask.Wait();
        }

        [Fact]
        public void TestUpdateBookFromJsonRedFern()
        {
            var json = System.IO.File.ReadAllText(@"testdata\RedFern.json");
            Place book = new Place();
            PlaceDetailLookup.UpdateBookFromJson(json, book);
            Assert.Equal("Where the Red Fern Grows", book.Title);
            Assert.Equal("Wilson Rawls", book.Author);
            Assert.Equal(new DateTime(1978, 1, 1), book.PublishedDate);
            Assert.Contains("Ozarks", book.Description);
            Assert.Equal("http://books.google.com/books/content?" +
                "id=-ddvPQAACAAJ&printsec=frontcover&img=1&zoom=1&source=gbs_api",
                book.ImageUrl);
        }

        [Fact]
        public void TestUpdateBookFromJsonBackendError()
        {
            var json = System.IO.File.ReadAllText(@"testdata\BackendError.json");
            Place book = new Place();
            PlaceDetailLookup.UpdateBookFromJson(json, book);
            Assert.Equal(null, book.Title);
            Assert.Equal(null, book.Author);
            Assert.Equal(null, book.PublishedDate);
            Assert.Equal(null, book.Description);
        }
    }
}