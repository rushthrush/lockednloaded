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

using System;
using System.Data.Entity.Infrastructure;

namespace LockedNLoaded.Models
{
    /// <summary>
    /// A DbContext factory that pulls the connection string from an environment variable
    /// or Web.config.  The environment variable overrides.
    /// </summary>
    public class ApplicationDbContextFactory : IDbContextFactory<ApplicationDbContext>
    {
        /// <summary>
        /// Check the environment variable, and create a new ApplicationDbContext accordingly.
        /// </summary>
        /// <returns></returns>
        public ApplicationDbContext Create()
        {
            if (LibUnityConfig.ChooseBookStoreFromConfig() == PlaceStoreFlag.MySql)
            {
                string envConnectionString = Environment.GetEnvironmentVariable(
                    "LockedNLoaded:ConnectionStringCloudSql");
                if (envConnectionString != null)
                {
                    // Pull the connection string from the environment variable.
                    return new ApplicationDbContext(
                        new MySql.Data.MySqlClient.MySqlConnection(envConnectionString));
                }
            }
            else if (LibUnityConfig.ChooseBookStoreFromConfig() == PlaceStoreFlag.SqlServer)
            {
                string envConnectionString = Environment.GetEnvironmentVariable(
                    "LockedNLoaded:ConnectionStringSqlServer");
                if (envConnectionString != null)
                {
                    // Pull the connection string from the environment variable.
                    return new ApplicationDbContext(
                        new System.Data.SqlClient.SqlConnection(envConnectionString));
                }
            }
            // Pulls connection string from Web.config.
            return new ApplicationDbContext();
        }
    }
}
