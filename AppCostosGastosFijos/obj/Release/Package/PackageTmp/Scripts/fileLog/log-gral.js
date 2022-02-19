var logDataTable = $('#fileLogTbl');

$(function () {
    OnChangeCombos();
});

function InitTableFunctions() {
    DeleteFileBtn();
}

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

function OnChangeCombos() {
    $("#yearData_Log, #chargeTypeCatalog_Log, #fileTypeCatalog_Log, #areaData_Log").on('change', function () {
        logDataTable.DataTable().ajax.reload();
    });
}