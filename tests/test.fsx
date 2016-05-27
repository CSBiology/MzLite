#I "../bin"

//xcopy /Y "$(TargetDir)*.dll"  "$(SolutionDir)bin\"
//xcopy /Y "$(TargetDir)*.xml" "$(SolutionDir)bin\"

#r "Clearcore2.Compression.dll"
#r "Clearcore2.Data.AnalystDataProvider.dll"
#r "Clearcore2.Data.CommonInterfaces.dll"
#r "Clearcore2.Data.dll"
#r "Clearcore2.Data.WiffReader.dll"
#r "Clearcore2.InternalRawXYProcessing.dll"
#r "Clearcore2.Muni.dll"
#r "Clearcore2.ProjectUtilities.dll"
#r "Clearcore2.RawXYProcessing.dll"
#r "Clearcore2.StructuredStorage.dll"
#r "Clearcore2.Utility.dll"

#r "Newtonsoft.Json.dll"
#r "System.Data.SQLite.dll"

#r "MzLite.dll"
#r "MzLite.SQL.dll"
#r "MzLite.Wiff.dll"

open MzLite
open MzLite.Wiff
// Read MS_ScanStartTime using IParamEdit
open MzLite.MetaData  // Extension namespace



let wiffPath = @"D:\tmpData\20160212_MS_DHpsan009.wiff";
let runID = "sample=0"


// Create Param container (named)

// Create CV Param 

// Create User Param 

//// Create User description 
//let createUserDescirption (ipc:#Model.IParamContainer) (name:string) =    
//    
//    let userDes = new Model.UserDescription(name)
//    userDes.CvParams.Add(CvDes)
//    match ipc.UserDescriptions.Contains(userDes) with // Only to enforce uniquly named userDescription
//    | true  -> ipc
//    | false -> 
//               ipc.UserDescriptions.Add(userDes)
//               ipc
    
    
 
    

// Write mzLiteSql file 



// Read MS_ScanStartTime 
// Read MS_ScanStartTime plane
let readRetentionTime (scan:Model.Scan) =
    let valueRef = ref null;
    match scan.CvParams.TryGetItemByKey("MS:1000016",valueRef) with
    | true -> valueRef.Value.Value.ToDouble(new System.Globalization.CultureInfo("en-US"))
    | false -> nan


let readRetentionTime' (scan:Model.Scan) =
    let pe = scan.BeginParamEdit()    
    let valueConverter = pe.GetCvParam("MS:1000016")
    match valueConverter.GetDouble() with
    | v when v.HasValue -> v.Value
    | _ -> nan

//Read profile spectrum [MS:1000128]
let isProfile (scan:Model.Scan) =
    let pe = scan.BeginParamEdit()    
    pe.HasCvParam("MS:1000128")





let reader = new WiffFileReader(wiffPath) //,"D:/Source/Clearcore2.license.xml")

let spec = reader.ReadMassSpectrum("sample=0 experiment=0 scan=0")


// Create retention-time index
let retTimeIndex =
    reader.ReadMassSpectra(runID)
    |> Seq.map (fun ms -> 
        ms.Scans.[0]
        |> readRetentionTime',ms.ID
               )    
    |> Seq.toArray
    |> Array.sortBy fst


// Read peak array
let peaks = 
    let msID = "sample=0 experiment=0 scan=1000"
    reader.ReadSpectrumPeaks(msID).Peaks  // TODO: Peak description
                                          // TODO: Add param if Array is sorted 




reader.ReadMassSpectra(runID)
|> Seq.map (fun ms -> 
    ms.Scans.[0]
    |> isProfile  // .CvParams ,ms.ID
           )



// Read model - modify - write
let metaModel = reader.GetModel()

// modify

reader.SaveModel()






