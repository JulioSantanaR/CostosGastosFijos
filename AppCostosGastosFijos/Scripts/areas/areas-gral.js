var areaModal = "#areaModal",
    areasTbl = "#areasTbl";

$(function () {

    // Función para colocar "activo" el menú de administración en el header de la página.
    $(".adminMenu").addClass("active");

    // Función que se ejecuta cuando se quiere dar de alta una nueva área.
    $(".newAreaBtn").on("click", function () {
        AddEditArea();
    });

    // Función para inicializar la tabla de áreas.
    AreasBuildTable();
});

/**
 * Función utilizada para construir la tabla asociada a las áreas.
 */
function AreasBuildTable() {
    var dataTableProperties = {
        tableTarget: areasTbl,
        requestUri: "/Areas/GetAreas",
        columnsDefinition: [
            { "orderable": false, "targets": -1 },
            { className: "text-end", "orderable": false, "targets": [1] }
        ],
        cellsDefinition: [
            { data: 'NameArea', title: "Nombre" },
            {
                data: null, render: function (data, type, row) {
                    var editBtn = '<button class="btn btn-outline-info btn-rounded editAreaTbl" data-area_id=' + data.AreaId + '><i class="fas fa-pen"></i></button>',
                        deleteBtn = '<button class="btn btn-outline-danger btn-rounded deleteAreaTbl" data-area_id=' + data.AreaId + '><i class="fas fa-trash"></i></button>';
                    return editBtn + ' ' + deleteBtn;
                }
            }
        ],
        sortedCallBack: areasSortedCallBack,
        initFunctionCallBack: areasInitCallBack,
        afterInitFunctionCallBack: areasInitCallBack,
        langUri: langUriDataTable
    };

    var table = BuildGenericDataTable(dataTableProperties);
    table.on('responsive-display', function (e, datatable, row, showHide, update) {
        AreasInitTableFn();
    });
}

/**
 * Función callback que se inicializa cuando carga la tabla de áreas.
 */
var areasInitCallBack = function AreasInitTableFn() {
    EditAreaBtn();
    DeleteAreaBtn();

    $.extend($.validator.messages, {
        required: "Campo requerido"
    });
}

/**
 * Función callback para asociar las columnas con el nombre en Base de Datos para ordenar la información.
 * @param {any} sortedCol Número de columna que se quiere ordenar.
 */
var areasSortedCallBack = function SortedAreasCol(sortedCol) {
    switch (sortedCol) {
        case 0:
            sortedCol = "nombre";
            break;
        default:
            sortedCol = "";
    };

    return sortedCol;
}

/**
 * Función que se ejecuta cuando se quiere realizar la edición de un área.
 */
function EditAreaBtn() {
    $(".editAreaTbl").on("click", function () {
        var areaId = $(this).data("area_id");
        AddEditArea(areaId);
    });
}

/**
 * Función que se ejecuta cuando se quiere eliminar un área.
 */
function DeleteAreaBtn() {
    $(".deleteAreaTbl").on("click", function () {
        var areaId = $(this).data("area_id");
        DeleteAreaInformation(areaId);
    });
}

/**
 * Función utilizada para mostrar el modal que contiene la información de un área o que permite
 * dar de alta una nueva (según sea el caso).
 * @param {any} areaId Id asociado al área.
 */
function AddEditArea(areaId) {
    areaId = areaId || null;
    $.post("/Areas/ShowArea", { areaId }, function (response) {
        $(areaModal).html("");
        $(areaModal).html(response);
        $(areaModal).modal("show");

        $(".saveArea").on("click", function () {
            SaveOrEditArea();
        });
    });
}

/**
 * Función utilizada para actualizar o guardar (según sea el caso) la información asociada a un área.
 */
function SaveOrEditArea() {
    var validData = $("#areaForm").valid();
    if (validData) {
        var existingArea = $("#areaId").val() !== "" && $("#areaId").val() !== "0";
        var areaInformation = {
            NameArea: $("#nameArea").val(),
            AreaId: $("#areaId").val()
        };
        $.ajax({
            url: "/Areas/SaveEditArea",
            type: "POST",
            data: areaInformation,
            success: function (response) {
                if (response !== null) {
                    if (response.successResponse) {
                        var successResult = existingArea ? "actualizada" : "agregada",
                            successMsg = "Área " + successResult + " correctamente";
                        swal("", successMsg, "success");
                        $(areaModal).modal("hide");
                        $(areasTbl).DataTable().ajax.reload();
                    } else {
                        var errorResult = existingArea ? "actualizar" : "guardar",
                            errorMsg = "Ocurrió un error al " + errorResult + " el área";
                        $(areaModal).modal("hide");
                        swal("", errorMsg, "error");
                    }
                }
            }
        });
    }
}

/**
 * Función utilizada para eliminar la información asociada a un área.
 * @param {any} areaId Id asociado al área.
 */
function DeleteAreaInformation(areaId) {
    swal({
        title: "¿Estás seguro?",
        text: "Una vez eliminada no podrás recuperar la información de esta área",
        icon: "warning",
        buttons: true,
        dangerMode: true,
    }).then((willDelete) => {
        if (willDelete) {
            $.ajax({
                url: "/Areas/DeleteAreaInformation",
                type: "POST",
                data: { areaId },
                dataType: "json",
                success: function (response) {
                    if (response !== null) {
                        swal("", "Área eliminada correctamente", "success");
                        $(areasTbl).DataTable().ajax.reload();
                    }
                }
            });
        }
    });
}