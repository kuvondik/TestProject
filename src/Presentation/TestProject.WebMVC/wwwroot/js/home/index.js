$(document).ready(function () {
    // Add the following code if you want the name of the file appear on select
    $(".custom-file-input").on("change", function () {
        var fileName = $(this).val().split("\\").pop();
        $(this).siblings(".custom-file-label").addClass("selected").html(fileName);
    });

    $("#tableSwitch").on("change", function () {
        if ($(this).prop("checked") == true) {
            $("#tableSelect").val(0).trigger('change');
            $("#tableSelect").prop("disabled", false);
        }
        else {
            $("#tableSelect").val(0).trigger('change');
            $("#tableSelect").prop("disabled", true);
        }
    });

    // Trigger change function to synchronize select state with switch
    $("#tableSwitch").trigger("change");

    // Table select
    CreateTableSelect();

    // Fetch preselected option
    FetchAndSetPreseletedTableOption();

    $("#tableSelect").on("change", function () {
        $(".table-select-error").remove();
    });

    // Configure validation
    $("#formUpload").validate({
        errorPlacement: function (error, element) {
            error.appendTo(element.closest('div'));
            error.addClass('text-danger');
        },
        rules: {
            fileUploadInput: {
                required: true,
            }
        },
        messages: {
            fileUploadInput: "Updload a file."
        }
    });

    // Load data button click event
    $("#btnLoadTable").click(function (e) {
        e.preventDefault();

        // Clear jsGrid data
        $("#jsGrid").jsGrid("option", "data", []);
        $("#jsGrid").jsGrid("option", "fields", []);

        // disable button
        $(this).prop("disabled", true);
        // add spinner to button
        $(this).html(
            '<i class="fa fa-spinner fa-spin" aria-hidden="true"></i> Loading data...'
        );

        var tableSelectSelected = $('#tableSelect').find(':selected');

        var tableSelectIsValid = typeof tableSelectSelected.val() !== 'undefined' &&
            tableSelectSelected.val() != 0 &&
            tableSelectSelected.text() != "" &&
            tableSelectSelected.text() != "null";

        if (tableSelectIsValid) {
            var tableName = tableSelectSelected.text();

            $.ajax({
                url: getAllDataUrl,
                type: 'GET',
                data: { tableName: tableName },
                dataType: 'json',
                contentType: 'application/json; charset=utf-8',
                success: function (result) {
                    if (result.success) {
                        var colNames = result.column_names;
                        var data = result.data;

                        currentTableName = tableName;
                        currentColumnNames = colNames;

                        // Load jsGrid
                        LoadJsGrid(colNames, data);
                    }
                    else
                        console.log(result.message);

                    // re-enable button
                    $("#btnLoadTable").prop("disabled", false);
                    // back button content
                    $("#btnLoadTable").html("Load table");
                },
                error: function (xhr, status, error) {
                    window.location.reload()
                },
                cache: false,
                error: function (xhr, status, error) {
                    // re-enable button
                    $("#btnLoadTable").prop("disabled", false);
                    // back button content
                    $("#btnLoadTable").html("Load table");

                    window.location.reload();
                }
            });
        }
        else {
            // show error
            var error = $('<label class="table-select-error text-danger">Select any table.</label>')
            $(".table-select-error").remove();
            $(".table-select-container").append(error);

            // Make grid empty
            $("#jsGrid").jsGrid("option", "data", []);

            // re-enable button
            $(this).prop("disabled", false);
            // back button content
            $(this).html("Load data");
        }
    });

    // Load file button click event
    $("#btnLoadFile").click(function (e) {
        e.preventDefault();

        $(".table-select-error").remove();

        // disable button
        $(this).prop("disabled", true);
        // add spinner to button
        $(this).html(
            '<i class="fa fa-spinner fa-spin" aria-hidden="true"></i> Loading file...'
        );

        if ($("#formUpload").valid() === true) {
            var args = new FormData();

            args.append("File", $("#fileUploadInput")[0].files[0]);

            var tableSelectSelected = $('#tableSelect').find(':selected');
            var tableSelectIsValid = typeof tableSelectSelected.val() !== 'undefined' &&
                tableSelectSelected.val() != 0 &&
                tableSelectSelected.text() != "null";
            if (tableSelectIsValid)
                args.append("TableName", tableSelectSelected.text());

            $.ajax({
                url: loadFileUrl,
                type: 'POST',
                data: args,
                contentType: false,
                dataType: 'json',
                processData: false,
                success: function (result) {
                    var colNames = result.column_names;
                    var data = result.data;
                    var totalCount = result.total_count;
                    var newRecordsCount = result.new_records_count;
                    var tableName = result.table_name;

                    currentTableName = tableName;
                    currentColumnNames = colNames;

                    LoadJsGrid(colNames, data);

                    // Refresh select

                    // If table name is not given while uploading a file,
                    // make tableSelect select a new table
                    if (!tableSelectIsValid) {
                        // Make sure table select is on
                        $("#tableSwitch").prop("checked", "checked").trigger("change");

                        // create the option and append to Select2
                        var option = new Option(tableName, 1, true, true);
                        $("#tableSelect").append(option).trigger('change');
                    }

                    // Show new records count
                    $(".new-records-count").html(newRecordsCount);

                    // Make delete table button enabled
                    $("#btnDeleteTable").prop("disabled", false);

                    // re-enable button
                    $("#btnLoadFile").prop("disabled", false);
                    // back button content
                    $("#btnLoadFile").html("Load file");
                },
                error: function (xhr, status, error) {
                    // Make grid empty
                    $("#jsGrid").jsGrid("option", "data", []);

                    // re-enable button
                    $("#btnLoadFile").prop("disabled", false);
                    // back button content
                    $("#btnLoadFile").html("Load file");

                    window.location.reload()
                },
                cache: false
            });
        }
        else {
            // re-enable button
            $(this).prop("disabled", false);
            // back button content
            $(this).html("Load file");
        }
    });

    // Delete table button click event
    $("#btnDeleteTable").click(function (e) {
        e.preventDefault();

        // disable button
        $(this).prop("disabled", true);
        // add spinner to button
        $(this).html(
            '<i class="fa fa-spinner fa-spin" aria-hidden="true"></i> Deleting table...'
        );

        if (currentTableName || currentTableName !== "") {
            $.ajax({
                url: deleteTableUrl,
                type: 'DELETE',
                data: { tableName: currentTableName },
                success: function (result) {
                    if (result.success) {
                        // Make grid empty
                        $("#jsGrid").jsGrid("option", "fields", []);
                        $("#jsGrid").jsGrid("option", "data", []);

                        // Set records count to 0
                        $(".new-records-count").html("0");
                        $(".total-count").html("0");

                        // Refresh select
                        FetchAndSetPreseletedTableOption();
                    } else {
                        console.log(result.message);
                    }

                    // re-enable button
                    $("#btnDeleteTable").prop("disabled", false);
                    // back button content
                    $("#btnDeleteTable").html("Delete table");
                },
                error: function (xhr, status, error) {
                    // re-enable button
                    $("#btnDeleteTable").prop("disabled", false);
                    // back button content
                    $("#btnDeleteTable").html("Delete table");

                    window.location.reload()
                },
                cache: false
            });
        }
        else {
            // re-enable button
            $(this).prop("disabled", false);
            // back button content
            $(this).html("Delete table");
        }
    });
    // JsGrid
    $("#jsGrid").jsGrid({
        width: '100%',
        height: 'auto',
        sorting: true,
        paging: true,
        pageSize: 8,
        pageButtonCount: 5,
        autoload: true,
        confirmDeleting: true,
        editing: true,
        //rowClick: $.noop,
        controller: {
            loadData: function (filter) {
                var d = $.Deferred();

                // server-side filtering
                var dataFilter = Object.values(filter);

                $.ajax({
                    type: "GET",
                    url: getFilteredDataUrl,
                    data: {
                        tableName: currentTableName,
                        columnNames: currentColumnNames,
                        dataFilter: dataFilter
                    },
                    dataType: "json"
                }).done(function (result) {
                    if (result.success) {
                        d.resolve(result.data);
                    }
                    else {
                        console.log(result.message);

                        d.reject();
                    }
                }).fail(function (msg) {
                    console.log("fail" + msg);
                    d.reject();
                });

                return d.promise();
            },
            insertItem: function (item) {
                var d = $.Deferred();

                // Make compatible with HomeViewModel
                var args = {};
                var dataSet = [];
                var data = Object.values(item);

                dataSet.push(data);
                args.table_name = currentTableName;
                args.column_names = currentColumnNames;
                args.data_set = dataSet;

                $.ajax({
                    type: "POST",
                    url: insertDataUrl,
                    data: JSON.stringify(args),
                    dataType: "json",
                    contentType: "application/json",
                    traditional: true,
                })
                    .done(function (response) {
                        if (response.success == true) {
                            $(".total-count").html(response.total_count);
                            d.resolve(response.data);
                        }
                        else {
                            console.log("Error occured!")
                            console.log(response.message);

                            d.reject();
                        }
                    })
                    .fail(function (msg) {
                        console.log("fail" + msg);
                        d.reject();
                    });

                return d.promise();
            },
            updateItem: function (item) {
                var d = $.Deferred();

                // Make compatible with HomeViewModel
                var args = {};
                var dataSet = [];
                var data = Object.values(item);

                dataSet.push(data);
                args.table_name = currentTableName;
                args.column_names = currentColumnNames;
                args.data_set = dataSet;

                $.ajax({
                    type: "PUT",
                    url: updateDataUrl,
                    data: JSON.stringify(args),
                    dataType: "json",
                    contentType: "application/json",
                    traditional: true,
                }).done(function (response) {
                    if (response.success == true) {
                        d.resolve(response.data);
                    }
                    else {
                        console.log("Error occured!")
                        console.log(response.message);

                        d.reject();
                    }
                }).fail(function (msg) {
                    console.log("fail" + msg);
                    d.reject();
                });

                return d.promise();
            },
            deleteItem: function (item) {
                console.log(item);
                return $.ajax({
                    url: deleteDataUrl,
                    method: "DELETE",
                    data: {
                        tableName: currentTableName,
                        id: item.Id
                    },
                    error: function (jqXHR, status, err) {
                        console.log("Error occured!")
                    },
                    success: function (result) {
                        if (result.success == true)
                            $(".total-count").html(result.total_count);
                        else
                            console.log("Error occured!")
                    }
                });
            }
        },
        fields: []
    });

    $(".config-panel input[type=checkbox]").on("click", function () {
        var $cb = $(this);
        $("#jsGrid").jsGrid("option", $cb.attr("id"), $cb.is(":checked"));
    });
});

// Create table select
function CreateTableSelect() {
    $("#tableSelect").select2({
        ajax: {
            url: getAllTablesUrl,
            type: 'GET',
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            processResults: function (result) {
                if (result.success) {
                    if (result.data.length == 0)
                        $("#btnDeleteTable").prop("disabled", true);
                    else {
                        $("#tableSelect option[value='0']").remove();
                        $("#btnDeleteTable").prop("disabled", false);
                    }

                    var results = [];

                    for (var i = 0; i < result.data.length; i++) {
                        results.push({
                            id: i + 1,
                            // get table names to show
                            text: result.data[i].table_name
                        });
                    }

                    var result = {
                        results: results
                    };

                    return result;
                }
                else {
                    console.log("Error occured!");
                    return null;
                }
            },
            error: function (xhr, status, error) {
                window.location.reload()
            },
            cache: false
        },
        placeholder: "Select a table",
        width: '100%'
    });
}

// Load table and its data
function LoadJsGrid(columnNames, data) {
    var columns = [];

    // Table buttons
    columns.push({
        type: 'control',
        width: '80px'
    });

    // Hidden Id field
    columns.push({
        name: columnNames[0],
        css: 'hide',
        type: 'text',
        readOnly: true,
    });

    for (var i = 1; i < columnNames.length; i++) {
        columns.push({
            name: columnNames[i],
            title: columnNames[i],
            validate: {
                message: columnNames[i] + "field must have at least 2 characters!",
                validator: function (value) { return value.length > 1; }
            },
            width: '250px',
            type: 'text'
        });
    }

    // Declare jsGrid fields
    $("#jsGrid").jsGrid("option", "fields", columns);

    // Load data
    $("#jsGrid").jsGrid("option", "data", data);

    // Show counts
    $(".total-count").html(data.length);
}

// Fetch the preselected item, and add to the control
function FetchAndSetPreseletedTableOption() {
    $('#tableSelect').select2("destroy");
    CreateTableSelect();

    var tableSelect = $('#tableSelect');

    $.ajax({
        url: getFirstTableUrl,
        type: 'GET',
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
    }).then(function (result) {
        if (result.success) {
            // create the option and append to Select2
            var option = new Option(result.data.table_name, 1, false, true);

            currentTableName = result.data.table_name;
            tableSelect.append(option).trigger('change');

            // manually trigger the `select2:select` event
            tableSelect.trigger({
                type: 'select2:select',
                params: {
                    data: {
                        id: 1,
                        full_name: result.data.table_name,
                    }
                }
            });

            if (currentTableName && currentTableName != "") {
                // Load data and JsGrid
                $.ajax({
                    url: getAllDataUrl,
                    type: 'GET',
                    data: { tableName: currentTableName },
                    dataType: 'json',
                    contentType: 'application/json; charset=utf-8',
                    success: function (result) {
                        var colNames = result.column_names;
                        var data = result.data;

                        currentColumnNames = colNames;

                        // Load jsGrid
                        LoadJsGrid(colNames, data);

                        // re-enable button
                        $("#btnLoadTable").prop("disabled", false);
                        // back button content
                        $("#btnLoadTable").html("Load table");
                    },
                    error: function (xhr, status, error) {
                        window.location.reload()
                    },
                    cache: false,
                    error: function (xhr, status, error) {
                        // re-enable button
                        $("#btnLoadTable").prop("disabled", false);
                        // back button content
                        $("#btnLoadTable").html("Load table");

                        window.location.reload();
                    }
                });
            }
        }
        else {
            console.log("No table is found!");
            // create the option and append to Select2
            var option = new Option(result.message, 0, true, true);

            tableSelect.append(option).trigger('change');

            // manually ttrigger the `select2:select` event
            tableSelect.trigger({
                type: 'select2:select',
                params: {
                    data: {
                        id: 0,
                        full_name: result.message,
                    }
                }
            });

            // Make delete table button disabled
            $("#btnDeleteTable").prop("disabled", true);
        }
    });
}