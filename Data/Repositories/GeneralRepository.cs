namespace Data.Repositories
{
    using System;
    using System.IO;

    /// <summary>
    /// Clase asociada al repositorio que contiene operaciones generales.
    /// </summary>
    public class GeneralRepository
    {
        /// <summary>
        /// Método utilizado para guardar un mensaje específico en el archivo de log.
        /// </summary>
        /// <param name="message">Mensaje que se quiere guardar en el log.</param>
        public void WriteLog(string message)
        {
            try
            {
                FileStream fileStream = CreateLogFile();
                StreamWriter log = new StreamWriter(fileStream);
                log.WriteLine(DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt") + " " + message);
                log.Close();
                fileStream.Close();
            }
            catch (Exception ex)
            {
            }
        }

        /// <summary>
        /// Método utilizado para crear el directorio y archivo asociados al log de errores.
        /// </summary>
        /// <returns>Devuelve el archivo creado u obtenido de la ruta correspondiente.</returns>
        public FileStream CreateLogFile()
        {
            FileStream fileStream = null;
            string logFilePath = @"D:\CGFijos\Log\";
            logFilePath += "Log.txt";
            FileInfo logFileInfo = new FileInfo(logFilePath);
            DirectoryInfo logDirInfo = new DirectoryInfo(logFileInfo.DirectoryName);
            if (!logDirInfo.Exists)
            {
                logDirInfo.Create();
            }

            if (!logFileInfo.Exists)
            {
                fileStream = logFileInfo.Create();
            }
            else
            {
                fileStream = new FileStream(logFilePath, FileMode.Append);
            }

            return fileStream;
        }
    }
}