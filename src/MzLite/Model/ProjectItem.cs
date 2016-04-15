using System.Collections.Generic;

namespace MzLite.Model
{

    /// <summary>
    /// Exposes an interface of project items that can be identified by a name.
    /// </summary>
    public interface IProjectItem : INamedItem {  }

    /// <summary>
    /// An abstract base class of expansible description project items that can be identified by a name.
    /// </summary>    
    public abstract class ProjectItem : NamedItem, IProjectItem
    {
       
        internal ProjectItem() : base() { }

        internal ProjectItem(string name)
            : base(name) {  }
        
    }

    /// <summary>
    /// Base class of collections of project items that can be accessed by name. 
    /// </summary>    
    public abstract class ProjectItemCollection<T> : NamedItemCollection<T>
        where T : class, IProjectItem
    {

        public ProjectItemCollection()
            : base()
        {
        }

        public ProjectItemCollection(IEnumerable<T> items)
            : base(items)
        {           
        }
              
    }
}
