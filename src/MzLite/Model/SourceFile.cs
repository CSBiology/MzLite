#region license
// The MIT License (MIT)

// SourceFile.cs

// Copyright (c) 2016 Alexander Lüdemann
// alexander.luedemann@outlook.com
// luedeman@rhrk.uni-kl.de

// Computational Systems Biology, Technical University of Kaiserslautern, Germany
 

// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System;
using Newtonsoft.Json;

namespace MzLite.Model
{

    /// <summary>
    /// Expansible description of a source file.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn, IsReference = true)]
    public sealed class SourceFile : NamedModelItem
    {

        private string location;

        [JsonConstructor]
        public SourceFile(
            [JsonProperty("ID")] string id, 
            [JsonProperty("Name")] string name, 
            [JsonProperty("Location")] string location) 
            : base(id, name) 
        {
            if (string.IsNullOrWhiteSpace(location))
                throw new ArgumentNullException("location");
            this.location = location;
        }

        [JsonProperty(Required = Required.Always)]
        public string Location
        {
            get
            {
                return location;
            }

            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new NullReferenceException("Location cannot be null or empty.");

                if (value != location)
                {
                    NotifyPropertyChanging("Location");
                    this.location = value;
                    NotifyPropertyChanged("Location");
                }
            }
        }
    }

    /// <summary>
    /// The model item container for data files.
    /// </summary>
    [JsonArray]
    public sealed class SourceFileList : ObservableModelItemCollection<SourceFile>
    {
        [JsonConstructor]
        internal SourceFileList() : base() { }

    }
}
