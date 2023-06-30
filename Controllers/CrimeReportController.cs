using Microsoft.AspNetCore.Mvc;
using RestSharp;
using CrimeReports.Models;

namespace CrimeReports.Controllers
{
    public class CrimeReportController : Controller
    {
        private readonly string baseURL = "https://data.police.uk/api";
        // Handle error if no data is available at Data Police
        private readonly string noDataError = "NoDataError";
        // Handle exception errors like if the internet is not available
        private readonly string errorException = "ErrorException";

        //check for coordinates
        private readonly string outOfBoundsCoordinates = "OutOfBoundsError";
        private readonly string coordinatesOK = "coordinates_ok";
        private readonly string invalidCoordinates = "InvalidCoordinates";

        public ActionResult Home()
        {
            return View();
        }

        public async Task<ActionResult> CrimeStats(string latitude, string longitude, string date)
        {
            var check = CheckCoordinates(latitude, longitude);
            if (check == coordinatesOK)
            {
                // Used RestSharp library to use RESTful APIs
                var options = new RestClientOptions(baseURL);
                var client = new RestClient(options);
                var request = new RestRequest($"/crimes-street/all-crime?lat={latitude}&lng={longitude}&date={date}");
                var categoryRequest = new RestRequest($"crime-categories?date={date}");

                if (request != null)
                {
                    try
                    {
                        var reports = await client.GetAsync<List<Report>>(request);
                        if (reports != null && reports.Count>0)
                        {
                            var categoriesList = await client.GetAsync<List<Category>>(categoryRequest);
                            if (categoriesList != null)
                            {
                                //prepare the statistics report now - 
                                var summary = PrepareStatisticsReport(reports, categoriesList);
                                return View(summary);
                            }
                            else
                            {
                                //  No categories found (not possible as the data already exist)
                                return View(noDataError);
                            }
                        }
                        else
                        {
                            // No records to display for selected coordinates and date
                            return View(noDataError);
                        }
                    }
                    catch (Exception e)
                    {
                        // Some exception, typically due to internet connection
                        Console.Write("Error occured - " + e.Data);
                        return View(errorException);
                    }
                }
                else
                {
                    return View(noDataError);
                }
            }
            else if (check == outOfBoundsCoordinates)
            {
                return View(outOfBoundsCoordinates);
            }
            else
            {
                return View(invalidCoordinates);
            }
        }

        private string CheckCoordinates(string latitude, string longitude)
        {
            try
            {
                double lat = double.Parse(latitude);
                double lng = double.Parse(longitude);
                double minLatitude = 49.84;
                double maxLatitude = 58.67;
                double minLongitude = -8.65;
                double maxLongitude = 1.76;

                if (lat >= minLatitude && lng <= maxLatitude &&
                    lat >= minLongitude && lng <= maxLongitude)
                {
                    return coordinatesOK;
                }
                else
                {
                    return outOfBoundsCoordinates;
                }
            }
            catch
            {
                return invalidCoordinates;
            }
        }

        private static SummaryViewModel PrepareStatisticsReport(
            List<Report> reports,
            List<Category> categoriesList
            )
        {
            SummaryViewModel summary = new();
            summary.Summary = new();
            // now segregate the report using Dictionary<Area, Dictionary < category, count>>
            foreach (var report in reports)
            {
                string area = report.location.street.name;
                var categoryName = categoriesList.Find(x => x.url == report.category);
                string category = categoryName.name;
                if (summary.Summary.ContainsKey(area))
                {
                    if (summary.Summary[area].ContainsKey(category))
                    {
                        summary.Summary[area][category]++;
                    }
                    else
                    {
                        summary.Summary[area][category] = 1;
                    }
                }
                else
                {
                    summary.Summary[area] = new Dictionary<string, int> { { category, 1 } };
                }
            }
            return summary;
        }
    }
}