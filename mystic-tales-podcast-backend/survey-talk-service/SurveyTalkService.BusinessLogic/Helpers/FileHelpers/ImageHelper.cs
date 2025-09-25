using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Drawing;
using SurveyTalkService.Common.AppConfigurations.App.interfaces;

namespace SurveyTalkService.BusinessLogic.Helpers.FileHelpers
{
    public class ImageHelper
    {
        private readonly FileIOHelper _fileIOHelper;

        public ImageHelper(FileIOHelper fileIOHelper)
        {
            _fileIOHelper = fileIOHelper;
        }

        public async Task<string> GenerateImageUrl(string folderPath, string fileName, string type)
        {
            return await _fileIOHelper.GeneratePresignedUrlAsync(FilePathHelper.CombinePaths(folderPath, fileName));
        }

        public async Task SaveBase64File(string base64Data, string folderPath, string fileName)
        {
            await _fileIOHelper.UploadBase64FileAsync(base64Data, folderPath, fileName);
        }

        public async Task CopyFile(string sourceFolder, string sourceFileName, string destinationFolder, string destinationFileName)
        {
            string sourceFilePath = FilePathHelper.CombinePaths(sourceFolder, sourceFileName);
            string destinationFilePath = FilePathHelper.CombinePaths(destinationFolder, destinationFileName);
            await _fileIOHelper.CopyFileToFileAsync(sourceFilePath, destinationFilePath);
        }

        public async Task DeleteFolder(string folderPath)
        {
            await _fileIOHelper.DeleteFolderAsync(folderPath);
        }

        public async Task DeleteFile(string folderName, string fileName)
        {
            string filePath = FilePathHelper.CombinePaths(folderName, fileName);
            await _fileIOHelper.DeleteFileAsync(filePath);
        }

        public async Task<string> GenerateImageBase64(string folderPath, string fileName, string type)
        {
            string filePath = FilePathHelper.CombinePaths(folderPath, fileName);
            var fileBytes = await _fileIOHelper.GetFileBytesAsync(filePath);
            if (fileBytes == null) return string.Empty;
            
            string mimeType = type switch
            {
                "jpg" or "jpeg" => "image/jpeg",
                "png" => "image/png",
                "gif" => "image/gif",
                _ => "application/octet-stream"
            };
            return $"data:{mimeType};base64,{Convert.ToBase64String(fileBytes)}";
        }

    }
}
