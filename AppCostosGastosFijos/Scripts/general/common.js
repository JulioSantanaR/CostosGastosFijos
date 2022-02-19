/**
 * Función general o compartida utilizada para construir un data table, de acuerdo a las propiedades que se envíen.
 * @param {any} dataTableProperties Objeto que contiene las propiedades principales o necesarias para construir la tabla.
 */
function BuildGenericDataTable(dataTableProperties) {
    console.log("KKK");
    var tableTarget = dataTableProperties.tableTarget,
        requestUri = dataTableProperties.requestUri,
        columnsDefinition = dataTableProperties.columnsDefinition,
        cellsDefinition = dataTableProperties.cellsDefinition,
        sortedCallBack = dataTableProperties.sortedCallBack,
        initFunctionCallBack = dataTableProperties.initFunctionCallBack,
        afterInitFunctionCallBack = dataTableProperties.afterInitFunctionCallBack,
        columnOrder = dataTableProperties.columnOrder || 0,
        langUri = dataTableProperties.langUri,
        extraParamFunctionCallBack = dataTableProperties.extraParamFunctionCallBack;
    var dataTableObject = $(tableTarget).DataTable({
        "language": { "url": langUri },
        serverSide: true,
        processing: true,
        ajax: {
            type: "POST",
            url: requestUri,
            data: function () {
                var pageInfo = $(tableTarget).DataTable().page.info(),
                    searchValue = $(tableTarget).DataTable().search(),
                    sortedCol = $(tableTarget).dataTable().fnSettings().aaSorting[0][0],
                    sortedDir = $(tableTarget).dataTable().fnSettings().aaSorting[0][1];
                if (sortedCallBack !== undefined && sortedCallBack !== null) {
                    sortedCol = sortedCallBack(sortedCol);
                }

                var dataTableInfo = {
                    SearchValue: searchValue,
                    NumberOfRows: pageInfo.length,
                    RowsToSkip: pageInfo.start,
                    MakeServicesCountQuery: true,
                    SortName: sortedCol,
                    SortOrder: sortedDir
                };
                if (extraParamFunctionCallBack !== undefined && extraParamFunctionCallBack !== null) {
                    dataTableInfo = extraParamFunctionCallBack(dataTableInfo);
                }

                var params = { dataTableInfo };
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
        "aoColumns": cellsDefinition,
        columnDefs: columnsDefinition,
        "order": [[columnOrder, "asc"]],
        "initComplete": function (settings, json) {
            CallBackFunctions(initFunctionCallBack);
        },
        "fnDrawCallback": function (oSettings) {
            var pagination = $(this).closest('.dataTables_wrapper').find('.dataTables_paginate');
            pagination.toggle(this.api().page.info().pages > 1);
            CallBackFunctions(afterInitFunctionCallBack);
        }
    });

    return dataTableObject;
}

/**
 * Método utilizado para inicializar una función.
 * @param {any} callBackFunction Función que se pretende inicializar o ejecutar.
 */
function CallBackFunctions(callBackFunction) {
    if (callBackFunction !== undefined && callBackFunction !== null) {
        callBackFunction();
    }
}