using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Diascan.Agent.ModelDB;
using Diascan.NDT.Enums;
using DiCore.Lib.NDT.Carrier;
using DiCore.Lib.NDT.DataProviders;
using DiCore.Lib.NDT.DataProviders.CDM;
using DiCore.Lib.NDT.DiagnosticData;

namespace Diascan.Agent.DiagDataLoader
{
    public class DataLoader
    {
        private string[] pointerFile;
        private string[] indexFile;
        private string[] dataFile;
        private string pathOmni;
        public List<int> CancellableId;

        public DataLoader()
        {
            CancellableId = new List<int>();
        }

        public void FindAllTypesInFolder(string path, string omniPath, Calculation calculation)
        {
            var result = new List<DiagData>();

            var omniFileName = Path.GetFileName(omniPath)?.Split('.').FirstOrDefault();
            if (omniFileName == null) return;
            var sourceDataPath = $"{Path.Combine(path, omniFileName)}_1";

            var dataTypes = DiagnosticData.GetAvailableDataTypes(omniPath);

            if (dataTypes.Contains(DataType.Nav) && dataTypes.Contains(DataType.NavSetup))
                calculation.NavigationInfo.NavigationState |= NavigationStateTypes.NavigationData;
            else
                dataTypes.Remove(DataType.Nav);

            foreach (var dataType in dataTypes)
            {
                var description = Constants.GetAllDescriptions().FirstOrDefault(type => type.DataType == dataType);
                if (description == null) continue;

                var fullPath = Directory.GetDirectories(sourceDataPath,
                    $"*{description.DataDirSuffix.Remove(description.DataDirSuffix.Length - 1)}").FirstOrDefault();

                if (dataType.HasFlag(DataType.Cd360))
                {
                    var diagnosticData = new DiagnosticData();
                    diagnosticData.Open(omniPath);

                    var directions = diagnosticData.GetCdmDirections();

                    foreach (var direction in directions)
                    {
                        var desc = description.DataDirSuffix;
                        desc = desc.Remove(desc.Length - 4) + $@"{direction.Id:d3}\";
                        description.DataDirSuffix = desc;

                        fullPath = Directory.GetDirectories(sourceDataPath,
                                $"*{description.DataDirSuffix.Remove(description.DataDirSuffix.Length - 1)}")
                            .FirstOrDefault();

                        result.Add(new CdmDiagData(dataType, GetFilesPath(dataType, fullPath, direction),
                            direction.Id, direction.Angle, direction.EntryAngle, (enCdmDirectionName)direction.DirectionName));
                    }
                }
                else
                    result.Add(new DiagData(dataType, GetFilesPath(dataType, fullPath)));
            }

            calculation.DiagDataList = result;
        }

        public List<ModelDB.File> GetFilesPath(DataType dataType, string fullPath)
        {
            var description = Constants.GetAllDescriptions().FirstOrDefault(item => item.DataType == dataType);
            if (description == null) return null;

            if (!Directory.Exists(fullPath)) return null;
            try
            {
                pointerFile = Directory.GetFiles(fullPath, $"*{description.PointerFileExt}");
                indexFile = Directory.GetFiles(fullPath, $"*{description.IndexFileExt}");
                dataFile = Directory.GetFiles(fullPath, $"*{description.DataFileExt}");
            }
            catch (Exception ex)
            {
                Logger.Logger.Info(ex.Message);
            }

            var filePaths = pointerFile.Concat(indexFile).Concat(dataFile);

            return filePaths.Select(oneFile => new ModelDB.File(oneFile)).ToList();
        }

        public List<ModelDB.File> GetFilesPath(DataType dataType, string fullPath, Direction cdmDirection)
        {
            var description = Constants.GetAllDescriptions().FirstOrDefault(item => item.DataType == dataType);
            if (description == null) return null;

            if (!Directory.Exists(fullPath)) return null;
            try
            {
                pointerFile = Directory.GetFiles(fullPath, $"*{description.PointerFileExt}");
                indexFile = Directory.GetFiles(fullPath, $"*{description.IndexFileExt}");
                dataFile = Directory.GetFiles(fullPath, $"*{description.DataFileExt}");
            }
            catch (Exception ex)
            {
                Logger.Logger.Info(ex.Message);
            }

            var filePaths = pointerFile.Concat(indexFile).Concat(dataFile);

            return filePaths.Select(oneFile => new ModelDB.File(oneFile)).ToList();
        }

        public bool Hash(Calculation calculation)
        {
            if (!Directory.Exists(calculation.SourcePath)) return false;
            var md5 = MD5.Create();
            var hashFlag = true;
            var totalFiles = calculation.DiagDataList.Sum(diagData => diagData.Files.Count);
            double count = 0;
            foreach (var diagData in calculation.DiagDataList)
            {
                foreach (var file in diagData.Files)
                {
                    if (CancellableId.Any(q => q == calculation.Id))
                        return false;

                    if (!file.State)
                    {
                        try
                        {
                            using (var fileStream = new BufferedStream(
                                System.IO.File.Open(file.FilePath, FileMode.Open, FileAccess.Read, FileShare.Read),
                                1048576))
                            {
                                file.Hashes = md5.ComputeHash(fileStream);
                                file.State = true;
                                count++;
                                calculation.Helper.ProgressHashes = $"{Math.Round((count / totalFiles) * 100, 2)}%";
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Logger.Info(ex.Message);
                        }
                    }
                    if (file.Hashes == null)
                        hashFlag = false;
                }
            }
            return hashFlag;
        }

        public string FindOmni(string path)
        {
            try
            {
                var omniFileName = new DirectoryInfo(path).Name + ".omni";
                pathOmni = Directory.GetFiles(path, omniFileName).FirstOrDefault();
                if (pathOmni == string.Empty) return "Файл не найден";
            }
            catch (Exception ex)
            {
                Logger.Logger.Info("Файл не найден: " + ex);
                pathOmni = "Файл не найден";
            }

            return pathOmni;
        }
    }

    internal static class Helper
    {
        internal static Direction ToDirection(this CdmDiagData cdmDiagData)
        {
            return new Direction
            {
                Angle = cdmDiagData.Angle,
                EntryAngle = cdmDiagData.EntryAngle,
                DirectionName = (enDirectionName)cdmDiagData.DirectionName,
                Id = cdmDiagData.Id
            };
        }
    }
}
