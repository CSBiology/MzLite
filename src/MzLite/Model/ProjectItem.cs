
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
    public abstract class ProjectItemContainer<T> : ObservableNamedItemCollection<T>
        where T : class, IProjectItem
    {

        internal ProjectItemContainer()
            : base()
        {
        }
           
    }
}
