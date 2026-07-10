namespace HexaERP.Services
{
    class Error
    {
        public static string ErrorMsg(string file)
        {
            //string timeSpan = (DateTime.Now - DateTime.MinValue).TotalMilliseconds.ToString();

            //var fileName = Path.GetFileName(file.FileName);
            //var fileExtention = Path.GetExtension(fileName);

            //var fileNameWithoutName = Path.GetFileNameWithoutExtension(fileName);
            //string postedFile = fileNameWithoutName + "_" + timeSpan + fileExtention;
            //var path = Path.Combine(HttpContext.Current.Server.MapPath("~/UploadedFiles/"), postedFile);

            //file.SaveAs(path);
            return file;
        }
    }
}
