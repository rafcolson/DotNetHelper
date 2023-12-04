using System.Runtime.Serialization.Json;
using System.Text.Json.Serialization;
using System.Net.Http.Headers;
using System.Diagnostics;
using System.Text.Json;

using static WinFormsLib.Utils;
using static WinFormsLib.Chars;
using static WinFormsLib.Constants;

namespace WinFormsLib
{
    public static class GeoLocator
    {
        private static HttpClient _httpClient { get; } = new();

        public enum GeoAreaDescription
        {
            [GlobalStringValue("Country")]
            Country,
            [GlobalStringValue("StateOrRegion")]
            StateOrRegion,
            [GlobalStringValue("TownCityOrCounty")]
            TownCityOrCounty,
            [GlobalStringValue("RoadOrAmenity")]
            RoadOrAmenity,
            [GlobalStringValue("SquareHamletOrQuarter")]
            SquareHamletOrQuarter,
            [GlobalStringValue("NeighbourhoodOrSuburb")]
            NeighbourhoodOrSuburb,
            [GlobalStringValue("Address")]
            Address
        }

        public class GeoArea
        {
            public string? AreaInfo { get; set; } = null;
            public GeoCoordinates? GeoCoords { get; set; } = null;

            public GeoArea() { }

            public GeoArea(string json)
            {
                if (string.IsNullOrEmpty(json))
                {
                    return;
                }
                try
                {
                    DataContractJsonSerializerSettings dcjss = new() { UseSimpleDictionaryFormat = true };
                    using MemoryStream ms = new(json.ToBytes());
                    DataContractJsonSerializer dcjs = new(typeof(GeoArea), dcjss);
                    if ((GeoArea?)dcjs.ReadObject(ms) is GeoArea ga)
                    {
                        AreaInfo = ga.AreaInfo;
                        GeoCoords = ga.GeoCoords;
                    }
                }
                catch {}
            }

            public override string ToString() => this.ToJson();
        }

        public class GeoAddress
        {
            public string house_number { get; set; } = EMPTY_STRING;
            public string road { get; set; } = EMPTY_STRING;
            public string amenity { get; set; } = EMPTY_STRING;
            public string neighbourhood { get; set; } = EMPTY_STRING;
            public string square { get; set; } = EMPTY_STRING;
            public string hamlet { get; set; } = EMPTY_STRING;
            public string quarter { get; set; } = EMPTY_STRING;
            public string town { get; set; } = EMPTY_STRING;
            public string city { get; set; } = EMPTY_STRING;
            public string city_district { get; set; } = EMPTY_STRING;
            public string county { get; set; } = EMPTY_STRING;
            public string state_district { get; set; } = EMPTY_STRING;
            public string state { get; set; } = EMPTY_STRING;
            public string region { get; set; } = EMPTY_STRING;
            public string postcode { get; set; } = EMPTY_STRING;
            public string country { get; set; } = EMPTY_STRING;
            public string country_code { get; set; } = EMPTY_STRING;
            public string leisure { get; set; } = EMPTY_STRING;
            public string suburb { get; set; } = EMPTY_STRING;
            public string GetCountry() => country;
            public string GetCountryCode() => country_code.ToUpper();
            public string GetStateOrRegion() => !string.IsNullOrEmpty(state) ? state : region;
            public string GetPostcode() => postcode;
            public string GetTownCityOrCounty() => !string.IsNullOrEmpty(town) ? town : !string.IsNullOrEmpty(city) ? city : county;
            public string GetNeighbourhoodOrSuburb() => !string.IsNullOrEmpty(neighbourhood) ? neighbourhood : suburb;
            public string GetSquareHamletOrQuarter() => !string.IsNullOrEmpty(square) ? square : !string.IsNullOrEmpty(hamlet) ? hamlet : quarter;
            public string GetRoadOrAmenity() => !string.IsNullOrEmpty(road) ? road : amenity;
            public string GetHouseNumber() => house_number;
            public string GetArea(GeoAreaDescription geoAreaDescription = GeoAreaDescription.Address, bool specify = true)
            {
                switch (geoAreaDescription)
                {
                    case (GeoAreaDescription.Country):
                        return country;
                    case (GeoAreaDescription.StateOrRegion):
                        string sr0 = GetStateOrRegion();
                        return specify && !string.IsNullOrEmpty(country_code) && !string.IsNullOrEmpty(sr0) ? $"{sr0} ({GetCountryCode()})" : sr0;
                    case (GeoAreaDescription.TownCityOrCounty):
                        string sr1 = GetStateOrRegion();
                        string townCityOrCounty = GetTownCityOrCounty();
                        return specify && !string.IsNullOrEmpty(sr1) && !string.IsNullOrEmpty(townCityOrCounty) ? $"{townCityOrCounty} ({sr1})" : townCityOrCounty;
                    case (GeoAreaDescription.NeighbourhoodOrSuburb):
                        string ns0 = GetNeighbourhoodOrSuburb();
                        return specify && !string.IsNullOrEmpty(postcode) && !string.IsNullOrEmpty(ns0) ? $"{ns0} ({postcode})" : ns0;
                    case (GeoAreaDescription.SquareHamletOrQuarter):
                        string squareHamletOrQuarter = GetSquareHamletOrQuarter();
                        return specify && !string.IsNullOrEmpty(postcode) && !string.IsNullOrEmpty(squareHamletOrQuarter) ? $"{squareHamletOrQuarter} ({postcode})" : squareHamletOrQuarter;
                    case (GeoAreaDescription.RoadOrAmenity):
                        string ra = GetRoadOrAmenity();
                        if (!specify)
                        {
                            return ra;
                        }
                        string ns1 = GetNeighbourhoodOrSuburb();
                        string suffix = string.IsNullOrEmpty(postcode) ? ns1 : string.IsNullOrEmpty(ns1) ? $"({postcode})" : $"{ns1} ({postcode})";
                        return !string.IsNullOrEmpty(suffix) ? $"{ra}, {suffix}" : ra;
                    case (GeoAreaDescription.Address):
                        string s0 = !string.IsNullOrEmpty(house_number) ? $"{GetRoadOrAmenity()} {house_number}" : GetRoadOrAmenity();
                        string s1 = GetNeighbourhoodOrSuburb();
                        s1 = string.IsNullOrEmpty(postcode) ? s1 : string.IsNullOrEmpty(s1) ? $"({postcode})" : $"{s1} ({postcode})";
                        string address = !string.IsNullOrEmpty(s1) ? $"{s0}, {s1}" : s0;
                        return specify && !string.IsNullOrEmpty(country_code) ? $"{address} [{GetCountryCode()}]" : address;
                    default:
                        return ToShortString();
                };
            }
            public string ToShortString() => GetArea(GeoAreaDescription.Address);
            public override string ToString() => this.ToJson();
        }

        public class GeoObject
        {
            public string place_id { get; set; } = EMPTY_STRING;
            public string licence { get; set; } = EMPTY_STRING;
            public string osm_type { get; set; } = EMPTY_STRING;
            public string osm_id { get; set; } = EMPTY_STRING;
            public string lat { get; set; } = EMPTY_STRING;
            public string lon { get; set; } = EMPTY_STRING;
            public string display_name { get; set; } = EMPTY_STRING;
            public string cls { get; set; } = EMPTY_STRING;
            public string type { get; set; } = EMPTY_STRING;
            public string importance { get; set; } = EMPTY_STRING;
            public double[]? boundingbox { get; set; }
            public GeoAddress? address { get; set; }
            public string GetDisplayName() => display_name;
            public double GetLatitude() => lat.AsDouble();
            public double GetLongitude() => lon.AsDouble();
            public double[]? GetBoundingbox() => boundingbox;
            public GeoAddress? GetGeoAddress() => address;
            public GeoCoordinates? GetGeoCoordinates()
            {
                if (!string.IsNullOrEmpty(lat) && !string.IsNullOrEmpty(lon))
                {
                    try
                    {
                        return new(lat.AsDouble(), lon.AsDouble(), boundingbox);
                    }
                    catch (FormatException) { Debug.WriteLine($"GeoCoordinates invalid: {lat}, {lon}"); }
                }
                return null;
            }
            public override string ToString() => this.ToJson();
        }

        public class GeoCoordinates
        {
            private static JsonSerializerOptions JsonSerializerOptions => new()
            {
                WriteIndented = true,
                IgnoreReadOnlyFields = true,
                IgnoreReadOnlyProperties = true
            };

            private const double F = 60d;

            public double Latitude { get; set; }
            public double Longitude { get; set; }
            [JsonIgnore]
            public double[]? Boundingbox { get; set; }

            public string LatitudeWindDirection => GetLatitudeWindDirection(Latitude);
            public string LongitudeWindDirection => GetLongitudeWindDirection(Longitude);
            [JsonIgnore]
            public double[] LatitudeCoordinates => GetDegreesMinutesSeconds(Latitude);
            [JsonIgnore]
            public double[] LongitudeCoordinates => GetDegreesMinutesSeconds(Longitude);
            public int Radius => Boundingbox is double[] bb ? (int)Math.Round(GetRadius(bb)) : 10;
            public bool IsValid => Latitude != 0 && Longitude != 0;

            public static string GetLatitudeWindDirection(double latitude) => latitude >= 0d ? $"{N_UPPER}" : $"{S_UPPER}";

            public static string GetLongitudeWindDirection(double longitude) => longitude >= 0d ? $"{E_UPPER}" : $"{W_UPPER}";

            public static double GetDecimalDegrees(double[] degreesMinutesSeconds, string cardinalDirection)
            {
                int sign = cardinalDirection.ToUpper() == $"{N_UPPER}" || cardinalDirection.ToUpper() == $"{E_UPPER}" ? 1 : -1;
                return sign * (degreesMinutesSeconds[0] + degreesMinutesSeconds[1] / F + degreesMinutesSeconds[2] / (F * F));
            }

            public static double[] GetDegreesMinutesSeconds(double decimalDegrees)
            {
                double degAbs = Math.Abs(decimalDegrees);
                double deg = Math.Truncate(degAbs);
                double minTmp = (degAbs - deg) * F;
                double min = Math.Truncate(minTmp);
                double sec = (minTmp - min) * F;
                return new double[] { deg, min, sec };
            }

            public static double GetDistance(double lat1, double lon1, double lat2, double lon2)
            {
                double latMid = (lat1 + lat2) * 0.5;
                double latFac = 111132.92 - 559.82 * Math.Cos(2 * latMid) + 1.175 * Math.Cos(4 * latMid) - 0.0023 * Math.Cos(6 * latMid);
                double lonFac = 111412.84 * Math.Cos(latMid) - 93.5 * Math.Cos(3 * latMid) + 0.118 * Math.Cos(5 * latMid);
                double latDelta = Math.Abs(lat1 - lat2);
                double lonDelta = Math.Abs(lon1 - lon2);
                return Math.Sqrt(Math.Pow(latDelta * latFac, 2) + Math.Pow(lonDelta * lonFac, 2));
            }

            public static double GetRadius(double[] boundingbox)
            {
                return GetDistance(boundingbox[0], boundingbox[2], boundingbox[1], boundingbox[3]) * 0.5 * Math.Sqrt(0.5);
            }

            public static double[] GetBoundingbox(double latitude, double longitude, double radius)
            {
                double d = Math.Abs(radius / (111320 * Math.Sqrt(0.5)));
                return new double[] { latitude - d, latitude + d, longitude - d, longitude + d };
            }

            public static bool IsInside(double latitude, double longitude, double[] boundingbox)
            {
                return latitude > boundingbox[0] && latitude > boundingbox[1] && longitude > boundingbox[2] && longitude > boundingbox[3];
            }

            public static bool IsInside(GeoCoordinates geoCoordinates)
            {
                return geoCoordinates.Boundingbox is double[] bb && IsInside(geoCoordinates.Longitude, geoCoordinates.Longitude, bb);
            }

            public bool IsInside(double[] boundingbox) => IsInside(Latitude, Longitude, boundingbox);

            public GeoCoordinates() { }

            public GeoCoordinates(string json)
            {
                if (!string.IsNullOrEmpty(json) && JsonSerializer.Deserialize<GeoCoordinates?>(json, JsonSerializerOptions) is GeoCoordinates gc)
                {
                    Latitude = gc.Latitude;
                    Latitude = gc.Longitude;
                    Boundingbox = gc.Boundingbox;
                }
            }

            public GeoCoordinates(double latitude, double longitude, double[]? boundingbox = null)
            {
                Latitude = latitude;
                Longitude = longitude;
                Boundingbox = boundingbox is double[] bb ? (double[])bb.Clone() : GetBoundingbox(Latitude, Longitude, 10);
            }

            public GeoCoordinates(
                double[] latitudeCoordinates,
                double[] longitudeCoordinates,
                string latitudeWindDirection,
                string longitudeWindDirection,
                double[]? boundingbox = null
            )
            {
                Latitude = GetDecimalDegrees(latitudeCoordinates, latitudeWindDirection);
                Longitude = GetDecimalDegrees(longitudeCoordinates, longitudeWindDirection);
                Boundingbox = boundingbox is double[] bb ? (double[])bb.Clone() : GetBoundingbox(Latitude, Longitude, 10);
            }

            public string ToJson() => JsonSerializer.Serialize(this, JsonSerializerOptions);

            public string ToShortString() => !IsValid ? string.Empty : $"{Latitude.ToString(CULTURE_INFO_DEFAULT)}, {Longitude.ToString(CULTURE_INFO_DEFAULT)}";

            public override string ToString()
            {
                if (!IsValid)
                {
                    return string.Empty;
                }
                string latRef = LatitudeWindDirection;
                string lonRef = LongitudeWindDirection;
                double[] latNum = LatitudeCoordinates;
                double[] lonNum = LongitudeCoordinates;
                double[] lat = new double[] { Math.Abs(latNum[0]), Math.Abs(latNum[1]), Math.Abs(latNum[2]) };
                double[] lon = new double[] { Math.Abs(lonNum[0]), Math.Abs(lonNum[1]), Math.Abs(lonNum[2]) };
                string latCoords = $"{lat[0]}{DEGREE_SIGN}{lat[1]}{SINGLE_QUOTE}{Math.Round(lat[2], 4).ToString(CULTURE_INFO_DEFAULT)}{DOUBLE_QUOTE}{latRef}";
                string lonCoords = $"{lon[0]}{DEGREE_SIGN}{lon[1]}{SINGLE_QUOTE}{Math.Round(lon[2], 4).ToString(CULTURE_INFO_DEFAULT)}{DOUBLE_QUOTE}{lonRef}";
                return $"{latCoords}{SPACE}{lonCoords}";
            }
        }

        private static async Task<GeoObject?> TryGetGeoObjectAsync(string input)
        {
            string uri = $"https://nominatim.openstreetmap.org/{input}";
            ProductHeaderValue product = new(Application.ProductName, Application.ProductVersion);
            _httpClient.DefaultRequestHeaders.UserAgent.Clear();
            _httpClient.DefaultRequestHeaders.UserAgent.Add(new(product));
            GeoObject? geoObject = null;
            try
            {
                using HttpResponseMessage response = await _httpClient.GetAsync(uri);
                response.EnsureSuccessStatusCode();
                byte[] json = await response.Content.ReadAsByteArrayAsync();
                //MessageBox.Show(await response.Content.ReadAsStringAsync());

                DataContractJsonSerializerSettings dcjss = new() { UseSimpleDictionaryFormat = true };
                using MemoryStream ms = new(json);
                try
                {
                    DataContractJsonSerializer dcjs = new(typeof(GeoObject[]), dcjss);
                    if ((GeoObject[]?)dcjs.ReadObject(ms) is GeoObject[] goa)
                    {
                        List<double> l = new();
                        foreach (GeoObject go in goa)
                        {
                            if (go.boundingbox is double[] bb)
                            {
                                l.Add(GeoCoordinates.GetRadius(bb));
                            }
                        }
                        int i = l.Any() ? l.IndexOf(l.Max()) : 0;
                        geoObject = goa[i];
                    }
                }
                catch (IndexOutOfRangeException)
                {
                    ms.Position = 0;
                    DataContractJsonSerializer dcjs = new(typeof(GeoObject), dcjss);
                    if ((GeoObject?)dcjs.ReadObject(ms) is GeoObject go0)
                    {
                        if (go0.GetGeoAddress() is GeoAddress ga)
                        {
                            geoObject = go0;
                        }
                    }
                }
                if (geoObject is GeoObject go1 && go1.GetGeoAddress() == null && go1.GetGeoCoordinates() is GeoCoordinates gc)
                {
                    geoObject = await GetGeoObjectAsync(gc.Latitude, gc.Longitude);
                }
            }
            catch (HttpRequestException)
            {
                Debug.WriteLine(Globals.GPSConnectionFailure);
                using Forms.MessageDialog md = new(Globals.GPSConnectionFailure, buttons: Buttons.DialogResultFlags.RetryCancel);
                if (md.ShowDialog() == DialogResult.Retry)
                {
                    return await TryGetGeoObjectAsync(input);
                }
            }
            return geoObject;
        }

        public static async Task<GeoObject?> GetGeoObjectAsync(string query) => await TryGetGeoObjectAsync($"search?format=json&q={query}");

        public static async Task<GeoObject?> GetGeoObjectAsync(double latitude, double longitude)
        {
            string lat = latitude.ToString(CULTURE_INFO_DEFAULT);
            string lon = longitude.ToString(CULTURE_INFO_DEFAULT);
            return await TryGetGeoObjectAsync($"reverse?format=json&lat={lat}&lon={lon}");
        }

        public static async Task<GeoCoordinates?> ExtractExifGPSCoords(string videoPath)
        {
            GeoCoordinates? geoCoords = null;
            try
            {
                string args = @$"-p {DOUBLE_QUOTE}{ExifWrapper.ToolFmtPath}{DOUBLE_QUOTE} -ee {DOUBLE_QUOTE}{videoPath}{DOUBLE_QUOTE}";
                ExifWrapper.Tool.StartInfo.Arguments = args;
                ExifWrapper.Tool.Start();
                string output = await ExifWrapper.Tool.StandardOutput.ReadToEndAsync();
                ExifWrapper.Tool.WaitForExit();
                Map<string, double> m = new(output);
                geoCoords = m.Any() ? new(m["lat"], m["lng"]) : new();
            }
            catch (Exception e) { Debug.WriteLine(e.Message); }
            return geoCoords;
        }

        public static void Dispose()
        {
            Debug.WriteLine($"Disposing 'GeoLocator'...");

            _httpClient.Dispose();
        }
    }
}
