using System.Collections.ObjectModel;
using FixPortal.FixAtdl.Model.Elements;

namespace FixPortal.FixAtdl.Wpf.Controls;

// FP Enhancement: minimal stand-in for the original Atdl4net.Wpf.ViewModel.ViewModelListItemCollection.
// That type belonged to the ViewModel layer, which is not part of this port (a later task); this
// collection provides only the surface the Slider control actually needs (Count, indexer, and the
// two lookup helpers below) built directly on the core's own ListItem_t, so it can be swapped for
// a richer observable ViewModel-layer type later without changing Slider's usage of it.
public class ViewModelListItemCollection : Collection<ListItem_t>
{
    /// <summary>
    /// Gets the index of the first item flagged as selected, or -1 if none is selected.
    /// </summary>
    public int GetFirstSelectedEnumIdIndex()
    {
        for (int n = 0; n < Count; n++)
        {
            if (this[n].IsSelected)
            {
                return n;
            }
        }

        return -1;
    }

    /// <summary>
    /// Gets the index of the item with the supplied enum identifier, or -1 if not found.
    /// </summary>
    public int GetIndexOfEnumId(string enumId)
    {
        for (int n = 0; n < Count; n++)
        {
            if (this[n].EnumId == enumId)
            {
                return n;
            }
        }

        return -1;
    }
}
