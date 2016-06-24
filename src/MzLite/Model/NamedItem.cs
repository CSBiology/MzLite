#region license
// The MIT License (MIT)

// NamedItem.cs

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
using Newtonsoft.Json;

namespace MzLite.Model
{

    /// <summary>
    /// An abstract base class of expansible description items that can be identified a name.
    /// </summary> 
    public abstract class NamedItem : ParamContainer, INotifyPropertyChanged, INotifyPropertyChanging
    {

        private string name;

        internal NamedItem(string name)
            : base()
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("name");
            this.name = name;
        }

        #region NamedItem Members

        [JsonProperty(Required = Required.Always)]
        public string Name
        {
            get
            {
                return name;
            }

            internal set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentNullException("Name cannot be null or empty.");

                if (this.name != value)
                {
                    NotifyPropertyChanging("Name");
                    this.name = value;
                    NotifyPropertyChanged("Name");
                }
            }
        }

        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(this, obj))
                return true;
            if (!this.GetType().Equals(obj.GetType()))
                return false;
            return this.name.Equals(((NamedItem)obj).name);
        }

        public override int GetHashCode()
        {
            return name.GetHashCode();
        }

        #endregion

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

    /// <summary>
    /// Base class of an observable collection of items that can be accessed by name. 
    /// </summary>    
    public abstract class ObservableNamedItemCollection<T> : ObservableKeyedCollection<string, T>
        where T : NamedItem
    {

        internal ObservableNamedItemCollection()
            : base()
        {
        }

        public void Rename(T item, string newName)
        {
            base.ChangeItemKey(item, newName);
            item.Name = newName;
        }

        protected override string GetKeyForItem(T item)
        {
            if (item == null)
                throw new ArgumentNullException("item");
            return item.Name;
        }

    }
}
