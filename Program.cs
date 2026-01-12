using Newtonsoft.Json;
using System.Text;

var currentDirectory = Directory.GetCurrentDirectory();
var storesDirectory = Path.Combine(currentDirectory, "stores");
// var salesFiles = FindFiles("stores");

var salesTotalDir = Path.Combine(currentDirectory, "salesTotalDir");
Directory.CreateDirectory(salesTotalDir);

var salesFiles = FindFiles(storesDirectory);

/* foreach (var file in salesFiles)
{
  Console.WriteLine(file);
} */

var salesTotal = CalculateSalesTotal(salesFiles);

// File.WriteAllText(Path.Combine(salesTotalDir, "totals.txt"), String.Empty);
File.AppendAllText(Path.Combine(salesTotalDir, "totals.txt"), $"{salesTotal}{Environment.NewLine}");

// Detailed summary
WriteSalesSummaryReport(salesFiles, storesDirectory, salesTotalDir);

IEnumerable<string> FindFiles(string folderName)
{
  List<string> salesFiles = new List<string>();

  var foundFiles = Directory.EnumerateFiles(folderName, "*", SearchOption.AllDirectories);

  foreach (var file in foundFiles)
  {
    // The file name will contain the full path, so only check the end of it
    // if (file.EndsWith("sales.json"))
    var extension = Path.GetExtension(file);
    if (extension == ".json")
    {
      salesFiles.Add(file);
    }
  }

  return salesFiles;
}

double CalculateSalesTotal(IEnumerable<string> salesFiles)
{
  double salesTotal = 0;

  // Loop over each file path in salesFiles
  foreach (var file in salesFiles)
  {
    // Read the contents of the file
    string salesJson = File.ReadAllText(file);

    // Parse the contents as JSON
    SalesData? data = JsonConvert.DeserializeObject<SalesData?>(salesJson);

    // Add the amount found in the Total field to the salesTotal variable
    salesTotal += data?.Total ?? 0;
  }

  return salesTotal;
}

void WriteSalesSummaryReport(IEnumerable<string> salesFiles, string storesDirectory, string outputDirectory)
{
  // compute per-file totals
  var fileTotals = new List<(string RelativeName, double Total)>();
  double grandTotal = 0;

  foreach (var file in salesFiles)
  {
    string salesJson = File.ReadAllText(file);

    double total = 0;

    var data = JsonConvert.DeserializeObject<SalesData?>(salesJson);
    if (data is not null && data.Total != 0)
    {
      total = data.Total;
    }
    else
    {
      var totalData = JsonConvert.DeserializeObject<SalesTotal?>(salesJson);
      if (totalData is not null)
      {
        total = totalData.OverallTotal;
      }
    }

    grandTotal += total;

    // var nameOnly = Path.GetFileName(file);
    var relativeName = Path.GetRelativePath(storesDirectory, file);
    fileTotals.Add((relativeName, total));
  }

  var sb = new StringBuilder();

  sb.AppendLine("Sales Summary");
  sb.AppendLine("----------------------------");
  sb.AppendLine($" Total Sales: {grandTotal:C}");
  sb.AppendLine();
  sb.AppendLine(" Details:");

  foreach (var item in fileTotals)
  {
    sb.AppendLine($"  {item.RelativeName}: {item.Total:C}");
  }

  var reportPath = Path.Combine(outputDirectory, "sales-summary.txt");
  File.WriteAllText(reportPath, sb.ToString());
}
record SalesData(double Total);
record SalesTotal(double OverallTotal);