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

using LockedNLoaded.Services;
using Microsoft.Practices.Unity;
using System;

namespace LockedNLoaded.App_Start
{
    /// <summary>
    /// Specifies the Unity configuration for the main container.
    /// </summary>
    public class UnityConfig
    {
        #region Unity Container

        private static readonly Lazy<IUnityContainer> s_container = new Lazy<IUnityContainer>(() =>
        {
            var container = new UnityContainer();
            RegisterTypes(container);
            return container;
        });

        /// <summary>
        /// Gets the configured Unity container.
        /// </summary>
        public static IUnityContainer GetConfiguredContainer()
        {
            return s_container.Value;
        }

        #endregion Unity Container

        /// <summary>Registers the type mappings with the Unity container.</summary>
        /// <param name="container">The unity container to configure.</param>
        public static void RegisterTypes(IUnityContainer container)
        {
            LibUnityConfig.RegisterTypes(container);
            var placeDetailLookup = new PlaceDetailLookup(LibUnityConfig.ProjectId);
            placeDetailLookup.CreateTopicAndSubscription();
            container.RegisterInstance<PlaceDetailLookup>(placeDetailLookup);

            container.RegisterInstance<ImageUploader>(
                new ImageUploader(
                  LibUnityConfig.GetConfigVariable("LockedNLoaded:BucketName")
                )
            );
        }
    }
}