namespace Business.Services
{
    using System;
    using System.Collections.Generic;
    using Data.DAO;
    using Data.Models;
    using Data.Repositories;

    /// <summary>
    /// Clase intermedia entre el acceso a datos y la capa del cliente para manipular la información del catálogo de tipos de archivos.
    /// </summary>
    public static class FileTypeService
    {
        /// <summary>
        /// Método utilizado para recuperar el catálogo asociado a los tipos de archivos.
        /// </summary>
        /// <param name="percentageFile">Bandera para determinar si el archivo es del tipo porcentaje.</param>
        /// <returns>Devuelve la lista de tipos de archivos dados de alta en la aplicación.</returns>
        public static List<FileType> GetFileTypes(bool? percentageFile = null)
        {
            List<FileType> fileTypeCatalog = null;
            try
            {
                FileTypeDAO fileTypeDao = new FileTypeDAO();
                fileTypeCatalog = fileTypeDao.GetFileTypes(percentageFile);
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("GetFileTypes()." + "Error: " + ex.Message);
            }

            return fileTypeCatalog;
        }
    }
}