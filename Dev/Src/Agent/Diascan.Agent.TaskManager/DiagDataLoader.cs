using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Diascan.Agent.Types;
using Diascan.Agent.Types.ModelCalculationDiagData;
using Diascan.NDT.Enums;
using DiCore.Lib.NDT.DataProviders;
using DiCore.Lib.NDT.DataProviders.CDM;
using DiCore.Lib.NDT.DiagnosticData;
using DiCore.Lib.NDT.Types;
using Standart.Hash.xxHash;
using FileMode = System.IO.FileMode;
using DiascanIO = Diascan.Utils.IO;

namespace Diascan.Agent.TaskManager
{
    public class DiagDataLoader
    {
        private string[] pointerFile;
        private string[] indexFile;
        private string[] dataFile;
        private string pathOmni;
        
        public void FindAllTypesInFolder(Calculation calculation, Action<object> loggerInfo)
        {
            var result = new List<IDiagData>();

            if (calculation.DataOutput.DataTypes.Contains(DataType.Nav) && calculation.DataOutput.DataTypes.Contains(DataType.NavSetup))
                calculation.NavigationInfo.State |= NavigationStateTypes.NavigationData;
            else
                calculation.DataOutput.DataTypes.Remove(DataType.Nav);

            foreach (var dataType in calculation.DataOutput.DataTypes)
            {
                var description = Constants.GetAllDescriptions().FirstOrDefault(type => type.DataType == dataType);
                if (description == null) continue;

                var fullPath = Directory.GetDirectories(calculation.DataLocation.InspectionFullPath,
                    $"*{description.DataDirSuffix.Remove(description.DataDirSuffix.Length - 1)}").FirstOrDefault();

                if (dataType.HasFlag(DataType.CDPA))
                {
                    var diagnosticData = new DiagnosticData();
                    diagnosticData.Open(calculation.DataLocation);

                    var directions = diagnosticData.GetCDpaDirections();

                    foreach (var direction in directions)
                    {
                        var desc = description.DataDirSuffix;
                        desc = desc.Remove(desc.Length - 4) + $@"{direction.Id:d3}\";
                        description.DataDirSuffix = desc;

                        fullPath = Directory.GetDirectories(calculation.DataLocation.InspectionFullPath,
                                $"*{description.DataDirSuffix.Remove(description.DataDirSuffix.Length - 1)}")
                            .FirstOrDefault();

                        result.Add(new CDpaDiagData(dataType, GetFilesPath(fullPath, description, loggerInfo),
                            direction.Id, direction.Angle, direction.EntryAngle, (enCdmDirectionName)direction.DirectionName));
                    }
                }
                else if (dataType.HasFlag(DataType.Cd360))
                {
                    var diagnosticData = new DiagnosticData();
                    diagnosticData.Open(calculation.DataLocation);

                    var directions = diagnosticData.GetCdmDirections();

                    foreach (var direction in directions)
                    {
                        var desc = description.DataDirSuffix;
                        desc = desc.Remove(desc.Length - 4) + $@"{direction.Id:d3}\";
                        description.DataDirSuffix = desc;

                        fullPath = Directory.GetDirectories(calculation.DataLocation.InspectionFullPath,
                                $"*{description.DataDirSuffix.Remove(description.DataDirSuffix.Length - 1)}")
                            .FirstOrDefault();

                        result.Add(new CdmDiagData(dataType, GetFilesPath(fullPath, description, loggerInfo),
                            direction.Id, direction.Angle, direction.EntryAngle, (enCdmDirectionName)direction.DirectionName));
                    }
                }
                else if (dataType.HasFlag(DataType.Mpm))
                    result.Add(new MpmDiagData(dataType, GetFilesPath(fullPath, description, loggerInfo)));
                else if (dataType.HasFlag(DataType.Ema))
                    result.Add(new EmaDiagData(dataType, GetFilesPath(fullPath, description, loggerInfo), calculation.DataOutput.Defectoscope, (int)(calculation.DataOutput.Diameter/25.4d)));
                else
                    result.Add(new DiagData(dataType, GetFilesPath(fullPath, description, loggerInfo)));
            }

            calculation.DiagDataList = result;
        }

        public FileHashed[] GetFilesPath(string fullPath, DiagdataDescription description, Action<object> loggerInfo)
        {
            if (!Directory.Exists(fullPath)) return null;
            try
            {
                pointerFile = description.PointerFileExt.Select(x => "*" + x).SelectMany(x => Directory.EnumerateFiles(fullPath, x)).ToArray();
                indexFile = Directory.GetFiles(fullPath, $"*{description.IndexFileExt}");
                dataFile = Directory.GetFiles(fullPath, $"*{description.DataFileExt}");
            }
            catch (Exception ex)
            {
                loggerInfo?.Invoke(ex.Message);
            }

            var filePaths = pointerFile.Concat(indexFile).Concat(dataFile);

            return filePaths.Select(oneFile => new FileHashed(oneFile)).ToArray();
        }

        public void CalcHash(Action<Calculation> updateAction, Action<object> loggerAction, Calculation calculation, CancellationTokenSource cts)
        {
            if (!Directory.Exists(calculation.DataLocation.BaseDirectory)) return;
            var totalFiles = calculation.DiagDataList.Sum(diagData => diagData.Files.Count);
            double count = 0;
            var updateCount = 1;
            foreach (var diagData in calculation.DiagDataList)
            {
                foreach (var file in diagData.Files)
                {
                    if (cts.IsCancellationRequested) return;

                    if (file.State)
                    {
                        count++;
                        continue;
                    }

                    try
                    {
                        using (var fileStream = new BufferedStream(System.IO.File.Open(file.FilePath, FileMode.Open, FileAccess.Read, FileShare.Read), 1048576))
                        {
                            file.Hashes = xxHash64.ComputeHash(fileStream);
                            file.State = true;
                            updateCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        loggerAction?.Invoke($"{ex.Message} : {ex}");
                        calculation.WorkState = enWorkState.Error;
                        return;
                    }
                    count++;
                    calculation.ProgressHashes = (count / totalFiles) * 100;
                    if (updateCount != 10) continue;

                    updateAction?.Invoke(calculation);
                    updateCount = 1;
                }
            }
        }

        public string FindOmni(string path)
        {
            try
            {
                var omniFileName = new DirectoryInfo(path).Name + ".omni";
                pathOmni = Directory.GetFiles(path, omniFileName).FirstOrDefault();
                if (pathOmni == string.Empty) return "Файл не найден";
            }
            catch (Exception )
            {
                //Logger.Logger.Info("Файл не найден: " + ex);
                pathOmni = "Файл не найден";
            }

            return pathOmni;
        }
    }
}
