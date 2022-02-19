$(function () {

    // Función para colocar "activo" el menú de administración en el header de la página.
    $(".adminMenu").addClass("active");

    // Función que se ejecuta cuando se quiere dar de alta un nuevo usuario.
    $(".newUserBtn").on("click", function () {
        AddEditUser();
    });

    // Función para inicializar la tabla de usuarios.
    UsersBuildTable();
});

/**
 * Función utilizada para construir la tabla asociada a los usuarios.
 */
function UsersBuildTable() {
    var dataTableProperties = {
        tableTarget: '#usersTbl',
        requestUri: "/Users/GetUsers",
        columnsDefinition: [
            { "orderable": false, "targets": -1 },
            { className: "text-end", "orderable": false, "targets": [4] }
        ],
        cellsDefinition: [
            { data: 'CollaboratorName', title: "Colaborador" },
            { data: 'Email', title: "Correo electr&oacute;nico" },
            { data: 'Username', title: "Nombre de usuario" },
            { data: 'RolUsuario', title: "Rol de usuario" },
            {
                data: null, render: function (data, type, row) {
                    var editBtn = '<button class="btn btn-outline-info btn-rounded editUserTbl" data-user_id=' + data.CollaboratorId + '><i class="fas fa-pen"></i></button>',
                        deleteBtn = '<button class="btn btn-outline-danger btn-rounded deleteUserTbl" data-user_id=' + data.CollaboratorId + '><i class="fas fa-trash"></i></button>';
                    return editBtn + ' ' + deleteBtn;
                }
            }
        ],
        sortedCallBack: usersSortedCallBack,
        initFunctionCallBack: usersInitCallBack,
        afterInitFunctionCallBack: usersInitCallBack,
        langUri: langUriDataTable
    };

    var table = BuildGenericDataTable(dataTableProperties);
    table.on('responsive-display', function (e, datatable, row, showHide, update) {
        UsersInitTableFn();
    });
}

/**
 * Función callback que se inicializa cuando carga la tabla de usuarios.
 */
var usersInitCallBack = function UsersInitTableFn() {
    EditUserBtn();
    DeleteUserBtn();

    $.extend($.validator.messages, {
        required: "Campo requerido",
        email: "Ingresa una dirección de correo válida"
    });
}

/**
 * Función callback para asociar las columnas con el nombre en Base de Datos para ordenar la información.
 * @param {any} sortedCol Número de columna que se quiere ordenar.
 */
var usersSortedCallBack = function SortedUsersCol(sortedCol) {
    switch (sortedCol) {
        case 0:
            sortedCol = "nombre";
            break;
        case 1:
            sortedCol = "correo";
            break;
        case 2:
            sortedCol = "usuario";
            break;
        case 3:
            sortedCol = "catRol.rolUsuario";
            break;
        default:
            sortedCol = "";
    };

    return sortedCol;
}

/**
 * Función que se ejecuta cuando se quiere realizar la edición de un usuario.
 */
function EditUserBtn() {
    $(".editUserTbl").on("click", function () {
        var userId = $(this).data("user_id");
        AddEditUser(userId);
    });
}

/**
 * Función que se ejecuta cuando se quiere eliminar a un usuario.
 */
function DeleteUserBtn() {
    $(".deleteUserTbl").on("click", function () {
        var userId = $(this).data("user_id");
        DeleteUserInformation(userId);
    });
}

/**
 * Función utilizada para mostrar el modal que contiene la información de un usuario o que permite
 * dar de alta uno nuevo (según sea el caso).
 * @param {any} userId Id asociado al usuario.
 */
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

/**
 * Función utilizada para actualizar o guardar (según sea el caso) la información asociada a un usuario.
 */
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

/**
 * Función utilizada para eliminar la información asociada a un usuario.
 * @param {any} collaboratorId Id asociado al usuario.
 */
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

/**
 * Función utilizada para recuperar el catálogo de áreas disponible en la aplicación.
 */
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