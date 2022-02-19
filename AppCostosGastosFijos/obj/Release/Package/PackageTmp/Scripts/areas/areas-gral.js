var areaModal = "#areaModal",
    areasTbl = "#areasTbl";

$(function () {
    $(".adminMenu").addClass("active");

    $(".newAreaBtn").on("click", function () {
        AddEditArea();
    });
});

function InitTableFunctions() {
    EditAreaBtn();
    DeleteAreaBtn();

    $.extend($.validator.messages, {
        required: "Campo requerido"
    });
}

function EditAreaBtn() {
    $(".editAreaTbl").on("click", function () {
        var areaId = $(this).data("area_id");
        AddEditArea(areaId);
    });
}

function DeleteAreaBtn() {
    $(".deleteAreaTbl").on("click", function () {
        var areaId = $(this).data("area_id");
        DeleteAreaInformation(areaId);
    });
}

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