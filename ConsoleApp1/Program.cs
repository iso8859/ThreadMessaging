HttpClient client = new HttpClient();
HttpResponseMessage response = await client.GetAsync("https://filesamples.com/samples/document/csv/sample2.csv");
string fileContent = await response.Content.ReadAsStringAsync();
using (StringReader sr = new StringReader(fileContent))
{
    string? line = null;
    while ((line = sr.ReadLine())!=null)
    {
        string[] items = line.Split(",");
        // Process the items
    }
}

using (StreamReader sr = new StreamReader(response.Content.ReadAsStreamAsync()))