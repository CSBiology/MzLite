#region license
// The MIT License (MIT)

// ParamContainer.cs

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
using System.ComponentModel;
using MzLite.Json;
using Newtonsoft.Json;

namespace MzLite.Model
{

    public abstract class ParamBase : INotifyPropertyChanged, INotifyPropertyChanging
    {
        
        private string cvUnitAccession;        
        private IConvertible value;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string CvUnitAccession 
        {
            get { return cvUnitAccession; }
            set
            {
                if (cvUnitAccession != value)
                {
                    NotifyPropertyChanging("CvUnitAccession");
                    this.cvUnitAccession = value;
                    NotifyPropertyChanged("CvUnitAccession");
                }
            }
        }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(ConvertibleConverter))]
        public IConvertible Value
        {
            get { return value; }
            set
            {
                if (this.value != value)
                {
                    NotifyPropertyChanging("Value");
                    this.value = value;
                    NotifyPropertyChanged("Value");
                }
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(string propertyName)
        {
            if ((this.PropertyChanged != null))
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region INotifyPropertyChanging Members

        public event PropertyChangingEventHandler PropertyChanging;

        protected void NotifyPropertyChanging(string propertyName)
        {
            if ((this.PropertyChanging != null))
            {
                this.PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
            }
        }

        #endregion
    }

    [JsonObject(MemberSerialization.OptIn)]
    public sealed class CvParam : ParamBase
    {

        private readonly string cvAccession;

        [JsonConstructor]
        public CvParam([JsonProperty("CvAccession")] string cvAccession)
        {
            if (string.IsNullOrWhiteSpace(cvAccession))
                throw new ArgumentNullException("cvAccession");

            this.cvAccession = cvAccession;
        }

        [JsonProperty(Required = Required.Always)]
        public string CvAccession { get { return cvAccession; } }

        public override string ToString()
        {
            if (CvUnitAccession == null)
                return string.Format("'{0}','{1}'",
                    CvAccession,
                    Value == null ? "null" : Value.ToString());
            else
                return string.Format("'{0}','{1}','{2}'",
                    CvAccession,
                    Value == null ? "null" : Value.ToString(),
                    CvUnitAccession);
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public sealed class UserParam : ParamBase
    {
        
        private readonly string name;
        
        [JsonConstructor]
        public UserParam([JsonProperty("Name")] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("name");
            this.name = name;
        }

        #region INamedItem Members

        [JsonProperty(Required = Required.Always)]
        public string Name
        {
            get
            {
                return name;
            }            
        }

        #endregion

        public override string ToString()
        {
            if (CvUnitAccession == null)
                return string.Format("'{0}','{1}'",
                    Name,
                    Value == null ? "null" : Value.ToString());
            else
                return string.Format("'{0}','{1}','{2}'",
                    Name,
                    Value == null ? "null" : Value.ToString(),
                    CvUnitAccession);
        }
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public sealed class ParamProperty : INotifyPropertyChanged, INotifyPropertyChanging
    {

        private readonly string name;
        private ParamBase param;

        [JsonConstructor]
        public ParamProperty([JsonProperty("Name")] string name)
            : base()
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("name");
            this.name = name;
        }

        public ParamProperty(string name, ParamBase p)
            : this(name)
        {
            if (p == null)
                throw new ArgumentNullException("p");
            this.param = p;
        }

        [JsonProperty(Required = Required.Always)]
        public string Name
        {
            get
            {
                return name;
            }
        }

        [JsonProperty(Required = Required.Always)]
        [JsonConverter(typeof(ParamBaseConverter))]
        public ParamBase Param
        {
            get { return param; }
            set
            {
                if (value == null)
                    throw new NullReferenceException("Value may not be null.");

                if (this.param != value)
                {
                    NotifyPropertyChanging("Param");
                    this.param = value;
                    NotifyPropertyChanged("Param");
                }
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            if ((this.PropertyChanged != null))
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region INotifyPropertyChanging Members

        public event PropertyChangingEventHandler PropertyChanging;

        private void NotifyPropertyChanging(string propertyName)
        {
            if ((this.PropertyChanging != null))
            {
                this.PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
            }
        }

        #endregion
    }

    public interface IParamContainer
    {
        CvParamCollection CvParams { get; }
        UserParamCollection UserParams { get; }
        ParamPropertyCollection ParamProperties { get; }
        UserDescriptionCollection UserDescriptions { get; }
    }

    public abstract class ParamContainer : IParamContainer
    {

        protected ParamContainer()
        {
            cvParams = new CvParamCollection();
            userParams = new UserParamCollection();
            paramProperties = new ParamPropertyCollection();
            userDescriptions = new UserDescriptionCollection();
        }

        private readonly CvParamCollection cvParams;
        private readonly UserParamCollection userParams;
        private readonly ParamPropertyCollection paramProperties;
        private readonly UserDescriptionCollection userDescriptions;

        [JsonProperty]
        public CvParamCollection CvParams { get { return cvParams; } }

        [JsonProperty]
        public UserParamCollection UserParams { get { return userParams; } }

        [JsonProperty]
        public ParamPropertyCollection ParamProperties { get { return paramProperties; } }

        [JsonProperty]
        public UserDescriptionCollection UserDescriptions { get { return userDescriptions; } }
    }

    public abstract class ParamBaseCollection<TParam> 
        : ObservableKeyedCollection<string, TParam> 
        where TParam : ParamBase
    {

        protected ParamBaseCollection() : base(StringComparer.InvariantCultureIgnoreCase) { }

        protected abstract override string GetKeyForItem(TParam item);
        
    }

    [JsonArray]
    public sealed class CvParamCollection : ParamBaseCollection<CvParam>
    {
        [JsonConstructor]
        internal CvParamCollection() { }

        protected override string GetKeyForItem(CvParam item)
        {
            if (item == null)
                throw new ArgumentNullException("item");
            return item.CvAccession;
        }        
    }

    [JsonArray]
    public sealed class UserParamCollection : ParamBaseCollection<UserParam>
    {
        [JsonConstructor]
        internal UserParamCollection() : base() { }

        protected override string GetKeyForItem(UserParam item)
        {
            if (item == null)
                throw new ArgumentNullException("item");           
            return item.Name;
        }       
    }

    [JsonArray]
    public sealed class ParamPropertyCollection : ObservableKeyedCollection<string, ParamProperty>
    {

        [JsonConstructor]
        internal ParamPropertyCollection() : base() { }

        protected override string GetKeyForItem(ParamProperty item)
        {
            if (item == null)
                throw new ArgumentNullException("item");
            return item.Name;
        }
    }
}
