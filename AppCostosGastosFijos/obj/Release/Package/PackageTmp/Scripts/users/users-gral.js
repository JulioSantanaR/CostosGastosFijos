$(function () {
    $(".adminMenu").addClass("active");

    $(".newUserBtn").on("click", function () {
        AddEditUser();
    });
});

function InitTableFunctions() {
    EditUserBtn();
    DeleteUserBtn();

    $.extend($.validator.messages, {
        required: "Campo requerido",
        email: "Ingresa una dirección de correo válida"
    });
}

function EditUserBtn() {
    $(".editUserTbl").on("click", function () {
        var userId = $(this).data("user_id");
        AddEditUser(userId);
    });
}

function DeleteUserBtn() {
    $(".deleteUserTbl").on("click", function () {
        var userId = $(this).data("user_id");
        DeleteUserInformation(userId);
    });
}

function AddEditUser(userId) {
    userId = userId || null;
    $.post("/Users/ShowUser", { userId }, function (response) {
        $('#userModal').html("");
        $('#userModal').html(response);
        $("#userModal").modal("show");

        dselect(document.querySelector('#collaboratorArea'));

        $(".saveUser").on("click", function () {
            SaveOrEditUser();
        });
    });
}

function SaveOrEditUser() {
    var validData = $("#userForm").valid();
    if (validData) {
        var existingUser = $("#collaboratorId").val() !== "" && $("#collaboratorId").val() !== "0",
            areasList = GetAreasList();
        var userInformation = {
            CollaboratorId: $("#collaboratorId").val(),
            CollaboratorName: $("#collaboratorName").val(),
            Email: $("#collaboratorEmail").val(),
            Username: $("#collaboratorUsername").val(),
            RoleId: $("#collaboratorRole").val(),
            Areas: areasList
        };
        $.ajax({
            url: "/Users/SaveEditUser",
            type: "POST",
            data: userInformation,
            success: function (response) {
                if (response !== null) {
                    if (response.successResponse) {
                        var successResult = existingUser ? "actualizado" : "agregado",
                            successMsg = "Usuario " + successResult + " correctamente";
                        swal("", successMsg, "success");
                        $("#userModal").modal("hide");
                        $('#usersTbl').DataTable().ajax.reload();
                    } else {
                        var errorResult = existingUser ? "actualizar" : "guardar",
                            errorMsg = "Ocurrió un error al " + errorResult + " el usuario";
                        $("#userModal").modal("hide");
                        swal("", errorMsg, "error");
                    }
                }
            }
        });
    }
}

function DeleteUserInformation(collaboratorId) {
    swal({
        title: "¿Estás seguro?",
        text: "Una vez eliminado no podrás recuperar la información del usuario",
        icon: "warning",
        buttons: true,
        dangerMode: true,
    }).then((willDelete) => {
        if (willDelete) {
            $.ajax({
                url: "/Users/DeleteUserInformation",
                type: "POST",
                data: { collaboratorId },
                dataType: "json",
                success: function (response) {
                    if (response !== null) {
                        if (response.successResponse) {
                            swal("", "Usuario eliminado correctamente", "success");
                            $('#usersTbl').DataTable().ajax.reload();
                        } else {
                            var errorMsg = "Ocurrió un error al eliminar el usuario";
                            swal("", errorMsg, "error");
                        }
                    }
                }
            });
        }
    });
}

function GetAreasList() {
    var areasList = [],
        areasIds = $("#collaboratorArea").val();
    if (areasIds !== null && areasIds.length > 0) {
        for (var i = 0; i < areasIds.length; i++) {
            var singleArea = {
                AreaId: parseInt(areasIds[i])
            };
            areasList.push(singleArea);
        }
    }

    return areasList;
}