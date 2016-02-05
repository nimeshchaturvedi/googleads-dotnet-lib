﻿// Copyright 2015, Google Inc. All Rights Reserved.
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

using Google.Api.Ads.AdWords.v201509;
using Google.Api.Ads.Common.Lib;
using Google.Api.Ads.Common.Util;

using System;
using System.Linq;
using System.Text;

namespace Google.Api.Ads.AdWords.Util.BatchJob.v201509 {

  /// <summary>
  /// Utility methods to upload operations for a batch job, and download the
  /// results.
  /// </summary>
  public class BatchJobUtilities : BatchJobUtilitiesBase {

    /// <summary>
    /// Initializes a new instance of the <see cref="BatchJobUtilities"/>
    /// class.
    /// </summary>
    /// <param name="user">AdWords user to be used along with this
    /// utilities object.</param>
    public BatchJobUtilities(AdsUser user)
      : base(user) {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BatchJobUtilities"/>
    /// class.
    /// </summary>
    /// <param name="user">AdWords user to be used along with this
    /// utilities object.</param>
    /// <param name="useChunking">if the operations should be broken into
    /// smaller chunks before uploading to the server.</param>
    /// <param name="chunkSize">The chunk size to use for resumable upload.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="chunkSie"/>
    /// is not a multiple of 256KB.</exception>
    /// <remarks>Use chunking if your network is spotty for uploads, or if it
    /// has restrictions such as speed limits or timeouts. Chunking makes your
    /// upload reliable when the network is unreliable, but it is inefficient
    /// over a good connection, since an HTTPs request has to be made for every
    /// chunk being uploaded.</remarks>
    public BatchJobUtilities(AdsUser user, bool useChunking, int chunkSize)
      : base(user, useChunking, chunkSize) {
    }

    /// <summary>
    /// Gets the post body for sending a request.
    /// </summary>
    /// <param name="operations">The list of operations.</param>
    /// <returns>The POST body, for using in the web request.</returns>
    private string GetPostBody(Operation[] operations) {
      BatchJobMutateRequest request = new BatchJobMutateRequest() {
        operations = operations.ToArray()
      };
      return SerializationUtilities.SerializeAsXmlText(request);
    }

    /// <summary>
    /// Uploads the operations to a specified URL.
    /// </summary>
    /// <param name="url">The temporary URL returned by a batch job.</param>
    /// <param name="operations">The list of operations.</param>
    public void Upload(string url, Operation[] operations) {
      // Mark the usage.
      featureUsageRegistry.MarkUsage(FEATURE_ID);

      Upload(url, operations, false);
    }

    /// <summary>
    /// Uploads the operations to a specified URL.
    /// </summary>
    /// <param name="url">The temporary URL returned by a batch job.</param>
    /// <param name="operations">The list of operations.</param>
    /// <param name="resumePreviousUpload">True, if a previously interrupted
    /// upload should be resumed.</param>
    public void Upload(string url, Operation[] operations, bool resumePreviousUpload) {
      // Mark the usage.
      featureUsageRegistry.MarkUsage(FEATURE_ID);

      byte[] postBody = Encoding.UTF8.GetBytes(GetPostBody(operations));
      Upload(url, resumePreviousUpload, postBody);
    }

    /// <summary>
    /// Downloads the batch job results from a specified URL.
    /// </summary>
    /// <param name="url">The download URL from a batch job.</param>
    /// <returns>The results from the batch job.</returns>
    public BatchJobMutateResponse Download(string url) {
      return ParseResponse(DownloadResults(url));
    }

    /// <summary>
    /// Parses the response from Google Cloud Storage servers.
    /// </summary>
    /// <param name="contents">The response body.</param>
    /// <returns>A BatchJobMutateResponse object, generated by parsing the
    /// response from the server.</returns>
    private BatchJobMutateResponse ParseResponse(string contents) {
      return ParseResponse<BatchJobMutateResponseEnvelope>(contents).mutateResponse;
    }
  }
}
