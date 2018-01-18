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

using System;
using System.ComponentModel.DataAnnotations;
using System.Device.Location;
using System.Web.Mvc;

namespace LockedNLoaded.Models
{
    // [START Place]
    [Bind(Include = "LocationName,  Coordinates, Description, ImageUrl")]
    public class Place
    {
        [Key]
        public long Id { get; set; }

        [Required]
        [Display(Name = "Location Name")]
        public string LocationName { get; set; }

        public string ImageUrl { get; set; }

        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        public PlacesUser CreatedBy { get; set; }

        [Required]
        public GeoCoordinate Coordinates { get; set; }
    }
    // [END Place]
}
