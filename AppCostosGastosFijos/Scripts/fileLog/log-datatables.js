$(document).ready(function () {
    var table = $('#fileLogTbl').DataTable({
        "language": {
            "url": "//cdn.datatables.net/plug-ins/1.10.16/i18n/Spanish.json"
        },
        serverSide: true,
        processing: true,
        ajax: {
            type: "POST",
            url: "/FileLog/GetFileLog",
            data: function () {
                var tableTarget = "#fileLogTbl";
                var pageInfo = $(tableTarget).DataTable().page.info(),
                    searchValue = $(tableTarget).DataTable().search(),
                    sortedCol = $(tableTarget).dataTable().fnSettings().aaSorting[0][0],
                    sortedDir = $(tableTarget).dataTable().fnSettings().aaSorting[0][1];
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

                var fileRequest = {
                    FileTypeId: $("#fileTypeCatalog_Log").val(),
                    AreaId: $("#areaData_Log").val(),
                    Year: $("#yearData_Log").val(),
                    ChargeTypeId: $("#chargeTypeCatalog_Log").val(),
                    IsCollaborator: isCollaborator,
                }

                var dataTableInfo = {
                        SearchValue: searchValue,
                        NumberOfRows: pageInfo.length,
                        RowsToSkip: pageInfo.start,
                        MakeServicesCountQuery: true,
                        SortName: sortedCol,
                        SortOrder: sortedDir,
                        FileRequest: fileRequest
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
        "columnDefs": [
            { "orderable": false, "targets": -1 },
            { "orderable": false, "targets": [6] },
            { className: "text-end", "orderable": false, "targets": [7] }
        ],
        "order": [[0, "asc"]],
        "initComplete": function (settings, json) {
            var api = new $.fn.dataTable.Api(settings);
            api.columns([1]).visible(!isCollaborator);
            InitTableFunctions();
        },
        "fnDrawCallback": function (oSettings) {
            var pagination = $(this).closest('.dataTables_wrapper').find('.dataTables_paginate');
            pagination.toggle(this.api().page.info().pages > 1);
            InitTableFunctions();
        }
    });

    table.on('responsive-display', function (e, datatable, row, showHide, update) {
        InitTableFunctions();
    });
});