var loggingEndPoint = "/api/log/benchmarkresults";

// [Manufacturer, Model]
var devices = [
    ["Lokia", "X423"],
    ["Lokia", "B24a"],
    ["Lokia", "94134"],
    ["Bamsung", "R2aa"],
    ["Bamsung", "Q41"],
    ["CTH", "9421"],
    ["CTH", "PPA"],
    ["CTH", "121"]
];

var benchmarks = [
    "Benchmark_A",
    "Benchmark_B",
    "Benchmark_C",
    "Benchmark_D",
    "Benchmark_E",
];

// [Data point name, min value, max value]
var dataPoints = [
    ["Data_A", 1, 10],
    ["Data_B", 10, 100],
    ["Data_C", 100, 1000],
    ["Data_D", 1000, 10000]
];

var maxRuns = 50;

function sendRandomDeviceData(numberOfEntries, sendCorruptedData) {
    var dataAsCSV = generateCSVHeader();

    var device = devices[randomIntFromInterval(0, devices.length - 1)];
    var runs = numberOfEntries === "" ? randomIntFromInterval(1, maxRuns) : numberOfEntries;

    var corruptedRow = -1;
    if (sendCorruptedData === true) {
        corruptedRow = randomIntFromInterval(1, numberOfEntries);
    }

    for (var run = 0; run < runs; run++) {
        var benchId = randomIntFromInterval(0, benchmarks.length - 1);
        var runData = (run != corruptedRow) ? generateRandomDataPoints() : "#$INVALIDDATA$#";
        dataAsCSV += "\n" + generateCSVRow(benchmarks[benchId], runData);
    }

    $.ajax({
        url: loggingEndPoint + "?Manufacturer=" + device[0] + "&Model=" + device[1],
        data: dataAsCSV,
        type: 'POST',
        error: function (a, b, c) {

        }
    });

    return [device, dataAsCSV];
}

function generateRandomDataPoints() {
    var data = [];
    for (var i = 0; i < dataPoints.length; i++) {
        data[i] = randomIntFromInterval(dataPoints[i][1], dataPoints[i][2]);
    }

    return data;
}

function randomIntFromInterval(min, max) {
    return Math.floor(Math.random() * (max - min + 1) + min);
}

function generateCSVHeader() {
    var header = "Benchmark";

    for (var i = 0; i < dataPoints.length; i++) {
        header += "," + dataPoints[i][0];
    }

    return header;
}

function generateCSVRow(benchmarkName, data) {
    return benchmarkName + "," + data;
}