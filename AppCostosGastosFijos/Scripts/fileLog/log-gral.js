var logDataTable = $('#fileLogTbl');

$(function () {

    // Función para actualizar la tabla cada vez que se actualice un combo.
    OnChangeCombos();

    // Función para inicializar la tabla del log de archivos.
    FilesBuildTable();
});

/**
 * Función utilizada para construir la tabla asociada al log de archivos.
 */
function FilesBuildTable() {
    var dataTableProperties = {
        tableTarget: logDataTable,
        requestUri: "/FileLog/GetFileLog",
        columnsDefinition: [
            { "orderable": false, "targets": -1 },
            { "orderable": false, "targets": [6] },
            { className: "text-end", "orderable": false, "targets": [7] }
        ],
        cellsDefinition: [
            { data: 'FileName', title: "Nombre de archivo" },
            { data: 'CollaboratorName', title: "Colaborador" },
            { data: 'AreaName', title: "&Aacute;rea" },
            { data: 'FileTypeName', title: "Tipo de archivo" },
            { data: 'ChargeTypeName', title: "Tipo de carga" },
            { data: 'YearData', title: "A&ntilde;o" },
            {
                data: null, render: function (data, type, row) {
                    var options = { month: 'long', day: 'numeric', year: 'numeric', hour: '2-digit', minute: '2-digit', hour12: true };
                    var dateObj = new Date(parseInt(data.ChargeDate.substr(6))),
                        dateConversion = dateObj.toLocaleDateString('es-ES', options);
                    return dateConversion;
                }, title: "Fecha"
            },
            {
                data: null, render: function (data, type, row) {
                    var deleteBtn = '<button class="btn btn-outline-danger btn-rounded deleteFileTbl"'
                        + 'data-file_log_id=' + data.FileLogId + ' data-file_type_name=' + encodeURIComponent(data.FileTypeName)
                        + ' data-year_data=' + data.YearData + ' data-charge_type_name=' + encodeURIComponent(data.ChargeTypeName)
                        + ' data-charge_type_id=' + data.ChargeTypeId + '>'
                        + '<i class="fas fa-trash"></i></button>';
                    return deleteBtn;
                }
            }
        ],
        sortedCallBack: filesSortedCallBack,
        initFunctionCallBack: filesInitCallBack,
        afterInitFunctionCallBack: filesInitCallBack,
        langUri: langUriDataTable,
        extraParamFunctionCallBack: filesExtraParams
    };

    var table = BuildGenericDataTable(dataTableProperties);
    table.on('responsive-display', function (e, datatable, row, showHide, update) {
        FilesInitTableFn();
    });
}

/**
 * Función callback que se inicializa cuando carga la tabla del log de archivos.
 */
var filesInitCallBack = function FilesInitTableFn() {
    DeleteFileBtn();
}

/**
 * Función callback para asociar las columnas con el nombre en Base de Datos para ordenar la información.
 * @param {any} sortedCol Número de columna que se quiere ordenar.
 */
var filesSortedCallBack = function SortedFilesCol(sortedCol) {
    switch (sortedCol) {
        case 0:
            sortedCol = "nombreArchivo";
            break;
        case 1:
            sortedCol = "catColab.nombre";
            break;
        case 2:
            sortedCol = "catAreas.nombre";
            break;
        case 3:
            sortedCol = "tipoArchivo";
            break;
        case 4:
            sortedCol = "tipoCarga";
            break;
        case 5:
            sortedCol = "anio";
            break;
        default:
            sortedCol = "";
    };

    return sortedCol;
}

/**
 * Función utilizada para agregar parámetros adicionales a las propiedades de la tabla.
 * @param {any} dataTableObj Objeto que contiene las propiedades de la tabla a mostrar.
 */
var filesExtraParams = function FilesExtraParams(dataTableObj) {
    if (dataTableObj !== undefined && dataTableObj !== null) {
        var fileRequest = {
            FileTypeId: $("#fileTypeCatalog_Log").val(),
            AreaId: $("#areaData_Log").val(),
            Year: $("#yearData_Log").val(),
            ChargeTypeId: $("#chargeTypeCatalog_Log").val(),
            IsCollaborator: isCollaborator,
        };
        dataTableObj["fileRequest"] = fileRequest;
    }

    return dataTableObj;
}

/**
 * Función que se ejecuta cuando se quiere eliminar un archivo.
 */
function DeleteFileBtn() {
    $(".deleteFileTbl").on("click", function () {
        var logFileId = $(this).data("file_log_id"),
            logFileType = decodeURIComponent($(this).data("file_type_name")),
            yearData = $(this).data("year_data"),
            chargeTypeName = decodeURIComponent($(this).data("charge_type_name")),
            chargeTypeId = $(this).data("charge_type_id");
        var deleteFileRequest = {
            LogFileId: logFileId,
            LogFileType: logFileType,
            YearData: yearData,
            ChargeTypeData: chargeTypeId,
            ChargeTypeName: chargeTypeName
        };
        DeleteFileInformation(deleteFileRequest);
    });
}

/**
 * Función utilizada para eliminar la información asociada a un archivo en el historial de cargas.
 * @param {any} deleteFileRequest Objeto que contiene la información del archivo a eliminar.
 */
function DeleteFileInformation(deleteFileRequest) {
    swal({
        title: "¿Estás seguro?",
        text: "Una vez eliminado no podrás recuperar la información del archivo",
        icon: "warning",
        buttons: true,
        dangerMode: true,
    }).then((willDelete) => {
        if (willDelete) {
            $.ajax({
                url: "/FileLog/DeleteFileLog",
                type: "POST",
                data: { deleteFileRequest },
                dataType: "json",
                success: function (response) {
                    if (response !== null) {
                        if (response.successResponse) {
                            swal("", "Archivo eliminado correctamente", "success");
                            logDataTable.DataTable().ajax.reload();
                        } else {
                            var errorMsg = "Ocurrió un error al eliminar el archivo";
                            swal("", errorMsg, "error");
                        }
                    }
                }
            });
        }
    });
}

/**
 * Función que se ejecuta cada vez que cambia uno de los combos, para actualizar la información en la tabla de archivos.
 */
function OnChangeCombos() {
    $("#yearData_Log, #chargeTypeCatalog_Log, #fileTypeCatalog_Log, #areaData_Log").on('change', function () {
        logDataTable.DataTable().ajax.reload();
    });
}