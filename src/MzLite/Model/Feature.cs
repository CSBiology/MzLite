using System;
using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace MzLite.Model
{    
    public enum FeatureType
    {
        Chromatogram, MassSpectrum
    }

    public abstract class Feature<TPeakArray> : NamedItem 
        where TPeakArray : PeakArray
    {

        [JsonProperty("FeatureType", Required = Required.Always)]
        private readonly FeatureType featureType;

        private Feature() { }

        internal Feature(string name, FeatureType featureType) 
            : base(name)
        {
            this.featureType = featureType;
        }

        public FeatureType FeatureType { get { return featureType; } }

        public abstract TPeakArray PeakArray { get; }

        public abstract MassSpectrum AsMassSpectrum { get; }
        public abstract Chromatogram AsChromatogram { get; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public sealed class MassSpectrum : Feature<Peak1DArray>
    {

        private readonly PrecursorList precursors = new PrecursorList();
        private readonly ScanList scans = new ScanList();
        private readonly ProductList products = new ProductList();
        private readonly Peak1DArray peakArray = new Peak1DArray();

        public MassSpectrum(string name) 
            : base(name, FeatureType.MassSpectrum) { }

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

        public override MassSpectrum AsMassSpectrum
        {
            get { return this; }
        }

        public override Chromatogram AsChromatogram
        {
            get { throw new InvalidCastException(); }
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public sealed class Chromatogram : Feature<Peak2DArray>
    {
        private readonly Precursor precursor = new Precursor();
        private readonly Product product = new Product();
        private readonly Peak2DArray peakArray = new Peak2DArray();

        public Chromatogram(string name) 
            : base(name, FeatureType.Chromatogram) { }

        [JsonProperty]
        public Product Product { get { return product; } }

        [JsonProperty]
        public Precursor Precursor { get { return precursor; } }

        [JsonProperty]
        public override Peak2DArray PeakArray
        {
            get { return peakArray; }
        }

        public override MassSpectrum AsMassSpectrum
        {
            get { throw new InvalidCastException(); }
        }

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

        private readonly SelectedIonList selectedIons = new SelectedIonList();
        private readonly Activation activation = new Activation();

        public Precursor()
        {
        }

        [JsonProperty]
        public Activation Activation { get { return activation; } }

        [JsonProperty]
        public SelectedIonList SelectedIons { get { return selectedIons; } }
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

        public Scan() { }

        [JsonProperty]
        public ScanWindowList ScanWindows { get { return scanWindows; } }
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
