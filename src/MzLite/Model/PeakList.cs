using System;
using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace MzLite.Model
{
    
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class MassSpectrum : ModelItem
    {

        private readonly PrecursorList precursors = new PrecursorList();
        private readonly ScanList scans = new ScanList();
        private readonly ProductList products = new ProductList();
        
        [JsonConstructor]
        public MassSpectrum([JsonProperty("ID")] string id)
            : base(id) { }

        [JsonProperty]
        public PrecursorList Precursors { get { return precursors; } }

        [JsonProperty]
        public ScanList Scans { get { return scans; } }

        [JsonProperty]
        public ProductList Products { get { return products; } }               
    }

    [JsonObject(MemberSerialization.OptIn)]
    public sealed class Chromatogram : ModelItem
    {
        private readonly Precursor precursor = new Precursor();
        private readonly Product product = new Product();
        
        [JsonConstructor]
        public Chromatogram([JsonProperty("ID")] string id)
            : base(id) { }

        [JsonProperty]
        public Product Product { get { return product; } }

        [JsonProperty]
        public Precursor Precursor { get { return precursor; } }
              
    }

    [JsonObject(MemberSerialization.OptIn)]
    public sealed class IsolationWindow : ParamContainer { }

    [JsonObject(MemberSerialization.OptIn)]
    public abstract class IonSelectionMethod
    {

        private readonly IsolationWindow isolationWindow = new IsolationWindow();

        public IonSelectionMethod()
        {
        }

        [JsonProperty]
        public IsolationWindow IsolationWindow { get { return isolationWindow; } }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public sealed class Activation : ParamContainer { }

    [JsonObject(MemberSerialization.OptIn)]
    public sealed class SelectedIon : ParamContainer { }

    [JsonArray]
    public sealed class SelectedIonList : Collection<SelectedIon>
    {

        [JsonConstructor]
        internal SelectedIonList() { }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public sealed class Precursor : IonSelectionMethod
    {

        private readonly SpectrumReference spectrumReference;
        private readonly SelectedIonList selectedIons = new SelectedIonList();
        private readonly Activation activation = new Activation();

        public Precursor()
        {
        }

        [JsonConstructor]
        public Precursor([JsonProperty("SpectrumReference")] SpectrumReference spectrumReference)
        {
            this.spectrumReference = spectrumReference;
        }

        [JsonProperty]
        public Activation Activation { get { return activation; } }

        [JsonProperty]
        public SelectedIonList SelectedIons { get { return selectedIons; } }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public SpectrumReference SpectrumReference { get { return spectrumReference; } }
    }

    [JsonArray]
    public sealed class PrecursorList : Collection<Precursor>
    {

        [JsonConstructor]
        internal PrecursorList() { }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public sealed class ScanWindow : ParamContainer { }

    [JsonArray]
    public sealed class ScanWindowList : Collection<ScanWindow>
    {

        [JsonConstructor]
        internal ScanWindowList() { }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public sealed class Scan : ParamContainer
    {

        private readonly ScanWindowList scanWindows = new ScanWindowList();
        private readonly SpectrumReference spectrumReference;

        public Scan() { }

        [JsonConstructor]
        public Scan([JsonProperty("SpectrumReference")] SpectrumReference spectrumReference)
        {
            this.spectrumReference = spectrumReference;
        }

        [JsonProperty]
        public ScanWindowList ScanWindows { get { return scanWindows; } }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public SpectrumReference SpectrumReference { get { return spectrumReference; } }
    }

    [JsonArray]
    public sealed class ScanList : Collection<Scan>
    {

        [JsonConstructor]
        internal ScanList() { }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public sealed class Product : IonSelectionMethod
    {
        public Product()
            : base()
        { }
    }

    [JsonArray]
    public sealed class ProductList : Collection<Product>
    {

        [JsonConstructor]
        internal ProductList() { }
    }
}
