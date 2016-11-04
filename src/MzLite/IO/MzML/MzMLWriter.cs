using System;
using System.Globalization;
using System.Xml;
using MzLite.Model;
using System.Linq;
using MzLite.MetaData;
using MzLite.MetaData.PSIMS;
using System.Collections.Generic;
using MzLite.Binary;

namespace MzLite.IO.MzML
{

    // TODO cv lookup
    // TODO param name lookup
    // TODO model only one run
    // TODO simplify write states, only speclist, chromlist
    // TODO write chromatogram list

    public sealed class MzMLWriter : IDisposable
    {

        private static readonly CultureInfo formatProvider = new CultureInfo("en-US");
        private readonly XmlWriter writer;
        private bool isClosed = false;

        public MzMLWriter(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            try
            {
                writer = XmlWriter.Create(path, new XmlWriterSettings() { Indent = true });
                writer.WriteStartDocument();
            }
            catch (Exception ex)
            {
                currentWriteState = MzMLWriteState.ERROR;
                throw new MzLiteIOException("Error init mzml output file.", ex);
            }
        }

        public void BeginMzML(MzLiteModel model)
        {
            try
            {
                if (model == null)
                    throw new ArgumentNullException("model");

                EnterWriteState(MzMLWriteState.INITIAL, MzMLWriteState.MZML);

                writer.WriteStartElement("mzML", "http://psi.hupo.org/ms/mzml");
                writer.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
                writer.WriteAttributeString("xsi", "schemaLocation", null, "http://psi.hupo.org/ms/mzml http://psidev.info/files/ms/mzML/xsd/mzML1.1.0.xsd");
                writer.WriteAttributeString("version", "1.1.0");

                WriteCvList();                

                // TODO scanSettingsList, also add to model

                WriteFileDescription(model.FileDescription);
                WriteList("dataProcessingList", model.DataProcessings, WriteDataProcessing, false);
                WriteList("softwareList", model.Software, WriteSoftware, false);
                WriteList("instrumentConfigurationList", model.Instruments, WriteInstrument, false);
                WriteList("sampleList", model.Samples, WriteSample);
            }
            catch (Exception ex)
            {
                currentWriteState = MzMLWriteState.ERROR;
                throw new MzLiteIOException("Error writing mzml output file.", ex);
            }
        }

        public void EndMzML()
        {
            try
            {
                LeaveWriteState(MzMLWriteState.MZML, MzMLWriteState.INITIAL);
                writer.WriteEndElement(); // </mzML>
            }
            catch (Exception ex)
            {
                currentWriteState = MzMLWriteState.ERROR;
                throw new MzLiteIOException("Error writing mzml output file.", ex);
            }
        }

        public void BeginRun(Run run)
        {
            try
            {
                if (run == null)
                    throw new ArgumentNullException("run");

                EnterWriteState(MzMLWriteState.MZML, MzMLWriteState.RUN);

                writer.WriteStartElement("run");
                WriteXmlAttribute("id", run.ID, true);

                if (run.Sample != null)
                {
                    WriteXmlAttribute("sampleRef", run.Sample.ID, true);
                }

                if (run.DefaultInstrument != null)
                {
                    WriteXmlAttribute("defaultInstrumentConfigurationRef", run.DefaultInstrument.ID, true);
                }

                WriteParamGroup(run);
            }
            catch (Exception ex)
            {
                currentWriteState = MzMLWriteState.ERROR;
                throw new MzLiteIOException("Error writing mzml output file.", ex);
            }
        }

        public void EndRun()
        {
            try
            {
                LeaveWriteState(MzMLWriteState.RUN, MzMLWriteState.MZML);
                writer.WriteEndElement(); // </run>
            }
            catch (Exception ex)
            {
                currentWriteState = MzMLWriteState.ERROR;
                throw new MzLiteIOException("Error writing mzml output file.", ex);
            }
        }

        public void BeginSpectrumList(int count)
        {
            try
            {
                if (count < 0)
                    throw new ArgumentOutOfRangeException("count");

                EnterWriteState(MzMLWriteState.RUN, MzMLWriteState.SPECTRUM_LIST);
                writer.WriteStartElement("spectrumList");
                WriteXmlAttribute("count", count.ToString(formatProvider));                
            }
            catch (Exception ex)
            {
                currentWriteState = MzMLWriteState.ERROR;
                throw new MzLiteIOException("Error writing mzml output file.", ex);
            }
        }

        public void EndSpectrumList()
        {
            try
            {
                LeaveWriteState(MzMLWriteState.SPECTRUM_LIST, MzMLWriteState.RUN);
                writer.WriteEndElement(); // </spectrumList>
            }
            catch (Exception ex)
            {
                currentWriteState = MzMLWriteState.ERROR;
                throw new MzLiteIOException("Error writing mzml output file.", ex);
            }
        }

        public void WriteSpectrum(MassSpectrum ms, Peak1DArray peaks, int index)
        {
            try
            {
                if (ms == null)
                    throw new ArgumentNullException("ms");
                if (peaks == null)
                    throw new ArgumentNullException("peaks");
                if (index < 0)
                    throw new ArgumentOutOfRangeException("idx");

                EnsureWriteState(MzMLWriteState.SPECTRUM_LIST);

                writer.WriteStartElement("spectrum");

                WriteXmlAttribute("id", ms.ID, true);
                WriteXmlAttribute("index", index.ToString(formatProvider), true);
                WriteXmlAttribute("dataProcessingRef", ms.DataProcessingReference, false);
                WriteXmlAttribute("sourceFileRef", ms.SourceFileReference, false);
                WriteXmlAttribute("defaultArrayLength", peaks.Peaks.Length.ToString(formatProvider), true);

                WriteParamGroup(ms);

                WriteList("scanList", ms.Scans, WriteScan);
                WriteList("precursorList", ms.Precursors, WritePrecursor);
                WriteList("productList", ms.Products, WriteProduct);

                WriteBinaryDataArrayList(peaks);

                writer.WriteEndElement();
                              
            }
            catch (Exception ex)
            {
                currentWriteState = MzMLWriteState.ERROR;
                throw new MzLiteIOException("Error writing mzml output file.", ex);
            }
        }                     

        public void Close()
        {
            if (!isClosed)
            {
                try
                {
                    EnterWriteState(MzMLWriteState.INITIAL, MzMLWriteState.CLOSED);

                    writer.WriteEndDocument();
                    writer.Flush();
                    writer.Close();
                    writer.Dispose();

                    isClosed = true;
                }
                catch (Exception ex)
                {
                    currentWriteState = MzMLWriteState.ERROR;
                    throw new MzLiteIOException("Error closing mzml output file.", ex);
                }
            }
        }

        #region xml writing helper

        private void WriteXmlAttribute(
            string name, 
            string value, 
            bool required = false)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                if (required)
                    throw new MzLiteIOException("Value required for xml attribute: " + name);
                else
                    return;
            }
            else
            {
                writer.WriteAttributeString(name, value);
            }
        }

        private void WriteList<TItem>(
            string elementName, 
            ICollection<TItem> list,
            Action<TItem> writeItem, 
            bool skipEmpty = true)
        {

            int count = list.Count;

            if (skipEmpty && count == 0)
                return;

            writer.WriteStartElement(elementName);
            
            WriteXmlAttribute("count", count.ToString(formatProvider));
            
            foreach (var item in list)
            {
                writeItem.Invoke(item);
            }

            writer.WriteEndElement();
        }

        private void WriteList<TItem>(
            string elementName,
            ICollection<TItem> list,
            Action<TItem, int> writeItem,
            bool skipEmpty = true)
        {

            int count = list.Count;

            if (skipEmpty && count == 0)
                return;

            writer.WriteStartElement(elementName);

            WriteXmlAttribute("count", count.ToString(formatProvider));

            int idx = 0;
            foreach (var item in list)
            {
                writeItem.Invoke(item, idx);
                idx++;
            }

            writer.WriteEndElement();
        }

        #endregion

        #region write states

        private MzMLWriteState currentWriteState = MzMLWriteState.INITIAL;
        private readonly HashSet<MzMLWriteState> consumedWriteStates = new HashSet<MzMLWriteState>();

        private void EnsureWriteState(MzMLWriteState expectedWs)
        {
            if (currentWriteState == MzMLWriteState.ERROR)
                throw new MzLiteIOException("Current write state is ERROR.");
            if (currentWriteState == MzMLWriteState.CLOSED)
                throw new MzLiteIOException("Current write state is CLOSED.");
            if (currentWriteState != expectedWs)
                throw new MzLiteIOException("Invalid write state: expected '{0}' but current is '{1}'.", expectedWs, currentWriteState);
        }

        private void EnterWriteState(MzMLWriteState expectedWs, MzMLWriteState newWs)
        {
            if (consumedWriteStates.Contains(newWs))
                throw new MzLiteIOException("Can't reentering write state: '{0}'.", newWs);
            EnsureWriteState(expectedWs);
            currentWriteState = newWs;
            consumedWriteStates.Add(newWs);
        }

        private void LeaveWriteState(MzMLWriteState expectedWs, MzMLWriteState newWs)
        {
            EnsureWriteState(expectedWs);
            currentWriteState = newWs;
        }

        private enum MzMLWriteState
        {
            ERROR,
            INITIAL,
            CLOSED,
            MZML,
            RUN,
            SPECTRUM_LIST,
            CHROMATOGRAM_LIST
        }

        #endregion

        #region param writing

        private void WriteCvList()
        {
            writer.WriteStartElement("cvList");
            WriteXmlAttribute("count", "2");
            WriteCv("MS", "Proteomics Standards Initiative Mass Spectrometry Ontology", "3.79.0", "http://psidev.info/ms/mzML/psi-ms.obo");
            WriteCv("UO", "Unit Ontology", "1.15", "http://obo.cvs.sourceforge.net/obo/obo/ontology/phenotype/unit.obo");
            writer.WriteEndElement();
        }


        private void WriteCv(string id, string fullName, string version, string uri)
        {
            writer.WriteStartElement("cv");
            WriteXmlAttribute("id", id);
            WriteXmlAttribute("fullName", fullName);
            WriteXmlAttribute("version", version);
            WriteXmlAttribute("URI", uri);
            writer.WriteEndElement();
        }

        private void WriteParamGroup(IParamContainer pc)
        {
            var msParams = pc.CvParams.Where(x => IsMSParam(x) && HasValidOrEmptyUnit(x));

            foreach (var cvp in msParams)
            {
                writer.WriteStartElement("cvParam");

                WriteXmlAttribute("cvRef", "MS", true);
                WriteXmlAttribute("accession", cvp.CvAccession, true);
                WriteXmlAttribute("name", cvp.CvAccession, true);
                WriteXmlAttribute("value", cvp.GetStringOrDefault(), false);

                if (cvp.HasUnit())
                {
                    WriteXmlAttribute("unitCvRef", ParseCvRef(cvp.CvUnitAccession), true);
                    WriteXmlAttribute("unitAccession", cvp.CvUnitAccession, true);
                    WriteXmlAttribute("unitName", cvp.CvUnitAccession, true);
                }

                writer.WriteEndElement();
            }

            var userParams = pc.UserParams.Where(x => HasValidOrEmptyUnit(x));

            foreach (var up in userParams)
            {
                writer.WriteStartElement("userParam");

                WriteXmlAttribute("name", up.Name, true);
                WriteXmlAttribute("value", up.GetStringOrDefault(), false);

                if (up.HasUnit())
                {
                    WriteXmlAttribute("unitCvRef", ParseCvRef(up.CvUnitAccession), true);
                    WriteXmlAttribute("unitAccession", up.CvUnitAccession, true);
                    WriteXmlAttribute("unitName", up.CvUnitAccession, true);
                }

                writer.WriteEndElement();
            }
        }

        private static bool IsMSParam(CvParam p)
        {
            if (string.IsNullOrWhiteSpace(p.CvAccession))
                return false;
            else
                return p.CvAccession.StartsWith("MS:", StringComparison.InvariantCultureIgnoreCase);
        }

        private static bool HasValidOrEmptyUnit(ParamBase p)
        {
            return string.IsNullOrWhiteSpace(p.CvUnitAccession) ||
                p.CvUnitAccession.StartsWith("MS:", StringComparison.InvariantCultureIgnoreCase) ||
                p.CvUnitAccession.StartsWith("UO:", StringComparison.InvariantCultureIgnoreCase);
        }

        private static string ParseCvRef(string accession)
        {
            if (string.IsNullOrWhiteSpace(accession))
                return null;
            var split = accession.Split(':');
            if (split.Length > 0)
                return split.First().ToUpperInvariant();
            else
                return null;
        }

        #endregion

        #region binary data writing

        private void WriteBinaryDataArrayList(Peak1DArray peaks)
        {
            writer.WriteStartElement("binaryDataArrayList");
            WriteXmlAttribute("count", "2");

            UserDescription mzParams = new UserDescription("mzParams");
            mzParams
                .SetMzArray()
                .SetCompression(BinaryDataCompressionType.NoCompression)
                .SetBinaryDataType(peaks.MzDataType);

            double[] mzValues = peaks.Peaks.Select(x => x.Mz).ToArray();
            WriteBinaryDataArray(mzValues, peaks.MzDataType, mzParams);

            UserDescription intParams = new UserDescription("intParams");
            intParams
                .SetIntensityArray().NoUnit()
                .SetCompression(BinaryDataCompressionType.NoCompression)
                .SetBinaryDataType(peaks.IntensityDataType);

            double[] intValues = peaks.Peaks.Select(x => x.Intensity).ToArray();
            WriteBinaryDataArray(intValues, peaks.IntensityDataType, intParams);

            writer.WriteEndElement();
        }

        private void WriteBinaryDataArray(double[] values, BinaryDataType binaryDataType, UserDescription pars)
        {

            BinaryDataEncoder encoder = new BinaryDataEncoder(values.Length * 8);
            string base64 = encoder.EncodeBase64(values, BinaryDataCompressionType.NoCompression, binaryDataType);
            int len = base64.Length;

            writer.WriteStartElement("binaryDataArray");
            WriteXmlAttribute("encodedLength", len.ToString(formatProvider));

            WriteParamGroup(pars);

            writer.WriteStartElement("binary");
            writer.WriteString(base64);
            writer.WriteEndElement();

            writer.WriteEndElement();
        }  

        #endregion

        #region model writing

        private void WriteFileDescription(FileDescription fdesc)
        {            
            writer.WriteStartElement("fileDescription");

            writer.WriteStartElement("fileContent");
            WriteParamGroup(fdesc.FileContent);
            writer.WriteEndElement();

            WriteList("sourceFileList", fdesc.SourceFiles, WriteSourceFile);

            if (fdesc.Contact != null)
            {
                writer.WriteStartElement("contact");
                WriteParamGroup(fdesc.Contact);
                writer.WriteEndElement();
            }
            
            writer.WriteEndElement();
        }
        
        private void WriteSourceFile(SourceFile sf)
        {
            writer.WriteStartElement("sourceFile");

            WriteXmlAttribute("id", sf.ID, true);
            WriteXmlAttribute("name", sf.Name, true);
            WriteXmlAttribute("location", sf.Location, true);

            WriteParamGroup(sf);

            writer.WriteEndElement();
        }

        private void WriteDataProcessing(DataProcessing dp)
        {
            writer.WriteStartElement("dataProcessing");
            WriteXmlAttribute("id", dp.ID, true);           
            WriteParamGroup(dp);
                        
            int order = 1;
            foreach (var dps in dp.ProcessingSteps)
            {
                writer.WriteStartElement("processingMethod");
                WriteXmlAttribute("order", order.ToString(formatProvider));
                WriteParamGroup(dp);
                writer.WriteEndElement();

                if (dps.Software != null)
                {
                    writer.WriteStartElement("softwareRef");
                    WriteXmlAttribute("ref", dps.Software.ID, true);
                    writer.WriteEndElement();
                }

                order++;
            }            

            writer.WriteEndElement();
        }

        private void WriteSoftware(Software sw)
        {
            writer.WriteStartElement("software");
            WriteXmlAttribute("id", sw.ID, true);
            WriteXmlAttribute("version", "not supported");
            WriteParamGroup(sw);
            writer.WriteEndElement();
        }

        private void WriteInstrument(Instrument instr)
        {
            writer.WriteStartElement("instrumentConfiguration");
            WriteXmlAttribute("id", instr.ID, true);
            WriteParamGroup(instr);

            if (instr.Software != null)
            {
                writer.WriteStartElement("softwareRef");
                WriteXmlAttribute("ref", instr.Software.ID, true);
                writer.WriteEndElement();
            }
                
            // TODO scanSettingsRef            

            WriteList("componentList", instr.Components, WriteComponent, true);

            writer.WriteEndElement();
        }

        private void WriteComponent(Component comp, int index)
        {
            string elemName;

            if (comp is SourceComponent)
                elemName = "sourceComponent";
            if (comp is DetectorComponent)
                elemName = "detectorComponent";
            if (comp is AnalyzerComponent)
                elemName = "analyzerComponent";
            else
                return;

            writer.WriteStartElement(elemName);
            WriteXmlAttribute("order", index.ToString(formatProvider));
            WriteParamGroup(comp);
            writer.WriteEndElement();
        }
        
        private void WriteSample(Sample sample)
        {
            writer.WriteStartElement("sample");

            WriteXmlAttribute("id", sample.ID, true);
            WriteXmlAttribute("name", sample.Name, false);

            WriteParamGroup(sample);

            writer.WriteEndElement();
        }

        private void WriteProduct(Product p)
        {
            writer.WriteStartElement("product");
            if (p.IsolationWindow != null)
                WriteIsolationWindow(p.IsolationWindow);
            writer.WriteEndElement();
        }
        
        private void WritePrecursor(Precursor pc)
        {
            writer.WriteStartElement("precursor");

            if (pc.SpectrumReference != null)
                WriteSpectrumRef(pc.SpectrumReference);

            WriteList("selectedIonList", pc.SelectedIons, WriteSelectedIon);

            if (pc.IsolationWindow != null)
            {
                WriteIsolationWindow(pc.IsolationWindow);
            }

            if (pc.Activation != null)
            {
                writer.WriteStartElement("activation");
                WriteParamGroup(pc.Activation);
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        private void WriteIsolationWindow(IsolationWindow isolationWindow)
        {            
            writer.WriteStartElement("isolationWindow");
            WriteParamGroup(isolationWindow);
            writer.WriteEndElement();
        }

        private void WriteSelectedIon(SelectedIon ion)
        {
            writer.WriteStartElement("selectedIon");
            WriteParamGroup(ion);
            writer.WriteEndElement();            
        }

        private void WriteScanWindow(ScanWindow sw)
        {
            writer.WriteStartElement("scanWindow");
            WriteParamGroup(sw);
            writer.WriteEndElement();
        }

        private void WriteScan(Scan scan)
        {
            writer.WriteStartElement("scan");
            if (scan.SpectrumReference != null)
                WriteSpectrumRef(scan.SpectrumReference);
            WriteParamGroup(scan);
            WriteList("scanWindowList", scan.ScanWindows, WriteScanWindow);
            writer.WriteEndElement();
        }

        private void WriteSpectrumRef(SpectrumReference spectrumReference)
        {
            if (spectrumReference.IsExternal)
            {
                WriteXmlAttribute("sourceFileRef", spectrumReference.SourceFileID, true);
                WriteXmlAttribute("externalSpectrumID", spectrumReference.SpectrumID, true);
            }
            else
            {
                WriteXmlAttribute("spectrumRef", spectrumReference.SpectrumID, true);
            }
        } 

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Close();
        }

        #endregion
    }

}
