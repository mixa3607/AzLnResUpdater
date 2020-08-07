using System;
using System.Collections.Generic;
using System.Linq;
using AzLn.Updater.DataModels;

namespace AzLn.Updater
{
    public class ResourceFilesInfoComparerService : IResourceFilesInfoComparerService
    {
        public ICompareResult Compare(IEnumerable<IResourceFileInfo> oldList, IEnumerable<IResourceFileInfo> newList)
        {
            var addedFiles = new List<IResourceFileInfo>();
            var updatedFiles = new List<IResourceFileInfo>();
            foreach (var newFile in newList)
            {
                switch (FindFile(newFile, oldList))
                {
                    case ESearchingResult.HashChanged:
                        updatedFiles.Add(newFile);
                        break;
                    case ESearchingResult.NotFound:
                        addedFiles.Add(newFile);
                        break;
                    case ESearchingResult.Identical: 
                        break;
                    default: 
                        throw new NotSupportedException();
                }
            }

            var deletedFiles = oldList.Where(x => newList.All(y => y.Path != x.Path)).ToArray();

            return new CompareResult()
            {
                DeletedFiles = deletedFiles,
                UpdatedFiles = updatedFiles,
                AddedFiles = addedFiles
            };
        }

        private static ESearchingResult FindFile(IResourceFileInfo targetFile,
            IEnumerable<IResourceFileInfo> filesCollection)
        {
            foreach (var collectionFile in filesCollection)
            {
                if (targetFile.Path == collectionFile.Path)
                {
                    return targetFile.Md5Hash == collectionFile.Md5Hash
                        ? ESearchingResult.Identical
                        : ESearchingResult.HashChanged;
                }
            }

            return ESearchingResult.NotFound;
        }

        private enum ESearchingResult
        {
            NotFound,
            Identical,
            HashChanged
        }
    }
}