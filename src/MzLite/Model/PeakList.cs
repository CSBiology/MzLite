using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Newtonsoft.Json;

namespace MzLite.Model
{    
    public enum PeakListType
    {
        Chromatogram, MassSpectrum
    }

    public abstract class PeakList : ParamContainer, IModelItem
    {
        private readonly PeakListType peakListType;
        
        private readonly string id;
        
        internal PeakList(string id, PeakListType peakListType)            
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException("id");
            this.id = id;
            this.peakListType = peakListType;
        }

        //[JsonProperty(Required = Required.Always)]
        public PeakListType PeakListType { get { return peakListType; } }

        [JsonProperty(Required = Required.Always)]
        public string ID { get { return id; } }

        [DebuggerBrowsableAttribute(DebuggerBrowsableState.Never)]
        public abstract MassSpectrum AsMassSpectrum { get; }

        [DebuggerBrowsableAttribute(DebuggerBrowsableState.Never)]
        public abstract Chromatogram AsChromatogram { get; }
    }

    public abstract class PeakList<TPeakArray> : PeakList
        where TPeakArray : PeakArray
    {
        
        internal PeakList(string nativeID, PeakListType peakListType)
            : base(nativeID, peakListType)           
        {                        
        }
        
        public abstract TPeakArray PeakArray { get; }
        
    }

    [JsonObject(MemberSerialization.OptIn)]
    public sealed class MassSpectrum : PeakList<Peak1DArray>
    {

        private readonly PrecursorList precursors = new PrecursorList();
        private readonly ScanList scans = new ScanList();
        private readonly ProductList products = new ProductList();
        private readonly Peak1DArray peakArray = new Peak1DArray();

        [JsonConstructor]
        public MassSpectrum([JsonProperty("ID")] string id)
            : base(id, PeakListType.MassSpectrum) { }

        [JsonProperty]
        public PrecursorList Precursors { get { return precursors; } }

        [JsonProperty]
        public ScanList Scans { get { return scans; } }

        [JsonProperty]
        public ProductList Products { get { return products; } }

        [JsonProperty]
        public override Peak1DArray PeakArray
        {
            get { return peakArray; }
        }

        [DebuggerBrowsableAttribute(DebuggerBrowsableState.Never)]
        public override MassSpectrum AsMassSpectrum
        {
            get { return this; }
        }

        [DebuggerBrowsableAttribute(DebuggerBrowsableState.Never)]
        public override Chromatogram AsChromatogram
        {
            get { throw new InvalidCastException(); }
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public sealed class Chromatogram : PeakList<Peak2DArray>
    {
        private readonly Precursor precursor = new Precursor();
        private readonly Product product = new Product();
        private readonly Peak2DArray peakArray = new Peak2DArray();

        [JsonConstructor]
        public Chromatogram([JsonProperty("ID")] string id)
            : base(id, PeakListType.Chromatogram) { }

        [JsonProperty]
        public Product Product { get { return product; } }

        [JsonProperty]
        public Precursor Precursor { get { return precursor; } }

        [JsonProperty]
        public override Peak2DArray PeakArray
        {
            get { return peakArray; }
        }

        [DebuggerBrowsableAttribute(DebuggerBrowsableState.Never)]
        public override MassSpectrum AsMassSpectrum
        {
            get { throw new InvalidCastException(); }
        }

        [DebuggerBrowsableAttribute(DebuggerBrowsableState.Never)]
        public override Chromatogram AsChromatogram
        {
            get { return this; }
        }
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

        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
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
