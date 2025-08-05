using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using YiSha.Util;
using YiSha.Util.Extension;
using YiSha.Util.Model;

namespace YiSha.Admin.Web.Controllers
{
    public class FileController : BaseController
    {
        #region 上傳單個文件
        [HttpPost]
        [AuthorizeFilter]
        public async Task<TData<string>> UploadFile(int fileModule, IFormCollection fileList)
        {
            TData<string> obj = await FileHelper.UploadFile(fileModule, fileList.Files);
            return obj;
        }
        #endregion

        #region 删除單個文件
        [HttpPost]
        [AuthorizeFilter]
        public TData<string> DeleteFile(int fileModule, string filePath)
        {
            TData<string> obj = FileHelper.DeleteFile(fileModule, filePath);
            return obj;
        }
        #endregion

        #region 下載文件
        [HttpGet]
        public FileContentResult DownloadFile(string filePath, int delete = 1)
        {
            TData<FileContentResult> obj = FileHelper.DownloadFile(filePath, delete);
            if (obj.Tag == 1)
            {
                return obj.Data;
            }
            else
            {
                throw new Exception("下載失敗：" + obj.Message);
            }
        }
        #endregion
    }
}