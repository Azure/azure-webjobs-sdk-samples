var api = "api/log/single/";

function updateTable() {
    var table = "#results";

    removeAllTableRows(table);
    $(table).append('<tr><td colspan="5">Loading data...</td></tr>');

    var benchmarkName = $("#bench").val();

    $.ajax({
        url: api + benchmarkName,
        type: 'GET',
        success: function (data) {
            removeAllTableRows(table);
            for (var i = 0; i < data.length; i++) {
                var rowHtml = '<tr>' + dataRender(data[i]) + '</tr>';
                $(table).append(rowHtml);
            }
        },
        error: function () {
            removeAllTableRows(table);
            $(table).append('<tr><td colspan="5">Failed to retrieve data</td></tr>');
        }
    });
}

function dataRender(data) {
    var rowKeyParts = data.RowKey.split("_");
    var rowHtml =
        '<td>' + rowKeyParts[1] + ' ' + rowKeyParts[2] + '</td>' +
        '<td>' + data.Data_A + '</td>' +
        '<td>' + data.Data_B + '</td>' +
        '<td>' + data.Data_C + '</td>' +
        '<td>' + data.Data_D + '</td>';

    return rowHtml;
}

function removeAllTableRows(table) {
    $(table + ' > tbody > tr').remove();
}