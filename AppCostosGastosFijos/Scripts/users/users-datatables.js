$(document).ready(function () {
    var table = $('#usersTbl').DataTable({
        "language": {
            "url": "//cdn.datatables.net/plug-ins/1.10.16/i18n/Spanish.json"
        },
        serverSide: true,
        processing: true,
        ajax: {
            type: "POST",
            url: "/Users/GetUsers",
            data: function () {
                var tableTarget = "#usersTbl";
                var pageInfo = $(tableTarget).DataTable().page.info(),
                    searchValue = $(tableTarget).DataTable().search(),
                    sortedCol = $(tableTarget).dataTable().fnSettings().aaSorting[0][0],
                    sortedDir = $(tableTarget).dataTable().fnSettings().aaSorting[0][1];
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
                    default:
                        sortedCol = "";
                };

                var dataTableInfo = {
                        SearchValue: searchValue,
                        NumberOfRows: pageInfo.length,
                        RowsToSkip: pageInfo.start,
                        MakeServicesCountQuery: true,
                        SortName: sortedCol,
                        SortOrder: sortedDir
                    },
                    params = { dataTableInfo };
                return params;
            },
            dataFilter: function (data) {
                if (data != null) {
                    var json = jQuery.parseJSON(data);
                    if (json.data != "") {
                        json.recordsTotal = json.recordsTotal;
                        json.recordsFiltered = json.recordsTotal;
                        json.data = JSON.parse(json.data);
                        return JSON.stringify(json);
                    } else {
                        return JSON.stringify(json);
                    }
                }
            },
            async: true
        },
        responsive: true,
        "aoColumns": [
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
        "columnDefs": [
            { "orderable": false, "targets": -1 },
            { "orderable": false, "targets": [3] },
            { className: "text-end", "orderable": false, "targets": [4] }
        ],
        "order": [[0, "asc"]],
        "initComplete": function (settings, json) {
            InitTableFunctions();
        },
        "fnDrawCallback": function (oSettings) {
            InitTableFunctions();
        }
    });

    table.on('responsive-display', function (e, datatable, row, showHide, update) {
        InitTableFunctions();
    });
});