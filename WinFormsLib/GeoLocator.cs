using System.Diagnostics;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using static WinFormsLib.Chars;
using static WinFormsLib.Constants;
using static WinFormsLib.Utils;

namespace WinFormsLib
{
    /// <summary>Provides geographic lookups, coordinate calculations, and video GPS metadata access.</summary>
    public static class GeoLocator
    {
        private static HttpClient _httpClient { get; } = new();

        /// <summary>Selects the address detail to display.</summary>
        public enum GeoAreaDescription
        {
            /// <summary>Displays the country.</summary>
            [GlobalStringValue("Country")]
            Country,
            /// <summary>Displays the state or region.</summary>
            [GlobalStringValue("StateOrRegion")]
            StateOrRegion,
            /// <summary>Displays the town, city, or county.</summary>
            [GlobalStringValue("TownCityOrCounty")]
            TownCityOrCounty,
            /// <summary>Displays the road or amenity.</summary>
            [GlobalStringValue("RoadOrAmenity")]
            RoadOrAmenity,
            /// <summary>Displays the square, hamlet, or quarter.</summary>
            [GlobalStringValue("SquareHamletOrQuarter")]
            SquareHamletOrQuarter,
            /// <summary>Displays the neighbourhood or suburb.</summary>
            [GlobalStringValue("NeighbourhoodOrSuburb")]
            NeighbourhoodOrSuburb,
            /// <summary>Displays a compact address.</summary>
            [GlobalStringValue("Address")]
            Address
        }

        /// <summary>Combines coordinates with an optional area description.</summary>
        public class GeoArea
        {
            /// <summary>Gets or sets the area description.</summary>
            public string? AreaInfo { get; set; } = null;
            /// <summary>Gets or sets the associated coordinates.</summary>
            public GeoCoordinates? GeoCoords { get; set; } = null;

            /// <summary>Initializes an empty area.</summary>
            public GeoArea() { }

            /// <summary>Initializes an area from coordinates and description.</summary>
            /// <param name="geoCoords">The associated coordinates.</param>
            /// <param name="areaInfo">The area description.</param>
            public GeoArea(GeoCoordinates? geoCoords = null, string? areaInfo = EMPTY_STRING)
            {
                GeoCoords = geoCoords;
                AreaInfo = areaInfo;
            }

            /// <summary>Initializes an area from JSON.</summary>
            /// <param name="json">The serialized data.</param>
            /// <remarks>Unreadable input leaves the area empty.</remarks>
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
                catch { }
            }

            /// <summary>Returns the object as JSON.</summary>
            public override string ToString() => this.ToJson();
        }

        /// <summary>Represents address fields returned by a geographic lookup.</summary>
        public class GeoAddress
        {
            /// <summary>Gets or sets the house number returned by the lookup.</summary>
            public string house_number { get; set; } = EMPTY_STRING;
            /// <summary>Gets or sets the road name returned by the lookup.</summary>
            public string road { get; set; } = EMPTY_STRING;
            /// <summary>Gets or sets the amenity name returned by the lookup.</summary>
            public string amenity { get; set; } = EMPTY_STRING;
            /// <summary>Gets or sets the neighbourhood name returned by the lookup.</summary>
            public string neighbourhood { get; set; } = EMPTY_STRING;
            /// <summary>Gets or sets the square name returned by the lookup.</summary>
            public string square { get; set; } = EMPTY_STRING;
            /// <summary>Gets or sets the hamlet name returned by the lookup.</summary>
            public string hamlet { get; set; } = EMPTY_STRING;
            /// <summary>Gets or sets the quarter name returned by the lookup.</summary>
            public string quarter { get; set; } = EMPTY_STRING;
            /// <summary>Gets or sets the town name returned by the lookup.</summary>
            public string town { get; set; } = EMPTY_STRING;
            /// <summary>Gets or sets the city name returned by the lookup.</summary>
            public string city { get; set; } = EMPTY_STRING;
            /// <summary>Gets or sets the city district name returned by the lookup.</summary>
            public string city_district { get; set; } = EMPTY_STRING;
            /// <summary>Gets or sets the county name returned by the lookup.</summary>
            public string county { get; set; } = EMPTY_STRING;
            /// <summary>Gets or sets the state district name returned by the lookup.</summary>
            public string state_district { get; set; } = EMPTY_STRING;
            /// <summary>Gets or sets the state name returned by the lookup.</summary>
            public string state { get; set; } = EMPTY_STRING;
            /// <summary>Gets or sets the region name returned by the lookup.</summary>
            public string region { get; set; } = EMPTY_STRING;
            /// <summary>Gets or sets the postal code returned by the lookup.</summary>
            public string postcode { get; set; } = EMPTY_STRING;
            /// <summary>Gets or sets the country name returned by the lookup.</summary>
            public string country { get; set; } = EMPTY_STRING;
            /// <summary>Gets or sets the country code returned by the lookup.</summary>
            public string country_code { get; set; } = EMPTY_STRING;
            /// <summary>Gets or sets the leisure feature name returned by the lookup.</summary>
            public string leisure { get; set; } = EMPTY_STRING;
            /// <summary>Gets or sets the suburb name returned by the lookup.</summary>
            public string suburb { get; set; } = EMPTY_STRING;
            /// <summary>Returns the country name.</summary>
            public string GetCountry() => country;
            /// <summary>Returns the country code in uppercase.</summary>
            public string GetCountryCode() => country_code.ToUpper();
            /// <summary>Returns the state if available, otherwise the region.</summary>
            public string GetStateOrRegion() => !string.IsNullOrEmpty(state) ? state : region;
            /// <summary>Returns the postal code.</summary>
            public string GetPostcode() => postcode;
            /// <summary>Returns the first nonempty value among town, city, and county.</summary>
            public string GetTownCityOrCounty() => !string.IsNullOrEmpty(town) ? town : !string.IsNullOrEmpty(city) ? city : county;
            /// <summary>Returns the neighbourhood if available, otherwise the suburb.</summary>
            public string GetNeighbourhoodOrSuburb() => !string.IsNullOrEmpty(neighbourhood) ? neighbourhood : suburb;
            /// <summary>Returns the first nonempty value among square, hamlet, and quarter.</summary>
            public string GetSquareHamletOrQuarter() => !string.IsNullOrEmpty(square) ? square : !string.IsNullOrEmpty(hamlet) ? hamlet : quarter;
            /// <summary>Returns the road if available, otherwise the amenity.</summary>
            public string GetRoadOrAmenity() => !string.IsNullOrEmpty(road) ? road : amenity;
            /// <summary>Returns the house number.</summary>
            public string GetHouseNumber() => house_number;
            /// <summary>Formats the selected address detail.</summary>
            /// <param name="geoAreaDescription">The address detail to return.</param>
            /// <param name="specify">Whether to append additional location context where supported.</param>
            public string GetArea(GeoAreaDescription geoAreaDescription = GeoAreaDescription.Address, bool specify = true)
            {
                switch (geoAreaDescription)
                {
                    case GeoAreaDescription.Country:
                        return country;
                    case GeoAreaDescription.StateOrRegion:
                        string sr0 = GetStateOrRegion();
                        return specify && !string.IsNullOrEmpty(country_code) && !string.IsNullOrEmpty(sr0) ? $"{sr0} ({GetCountryCode()})" : sr0;
                    case GeoAreaDescription.TownCityOrCounty:
                        string sr1 = GetStateOrRegion();
                        string townCityOrCounty = GetTownCityOrCounty();
                        return specify && !string.IsNullOrEmpty(sr1) && !string.IsNullOrEmpty(townCityOrCounty) ? $"{townCityOrCounty} ({sr1})" : townCityOrCounty;
                    case GeoAreaDescription.NeighbourhoodOrSuburb:
                        string ns0 = GetNeighbourhoodOrSuburb();
                        return specify && !string.IsNullOrEmpty(postcode) && !string.IsNullOrEmpty(ns0) ? $"{ns0} ({postcode})" : ns0;
                    case GeoAreaDescription.SquareHamletOrQuarter:
                        string squareHamletOrQuarter = GetSquareHamletOrQuarter();
                        return specify && !string.IsNullOrEmpty(postcode) && !string.IsNullOrEmpty(squareHamletOrQuarter) ? $"{squareHamletOrQuarter} ({postcode})" : squareHamletOrQuarter;
                    case GeoAreaDescription.RoadOrAmenity:
                        string ra = GetRoadOrAmenity();
                        if (!specify)
                        {
                            return ra;
                        }
                        string ns1 = GetNeighbourhoodOrSuburb();
                        string suffix = string.IsNullOrEmpty(postcode) ? ns1 : string.IsNullOrEmpty(ns1) ? $"({postcode})" : $"{ns1} ({postcode})";
                        return !string.IsNullOrEmpty(suffix) ? $"{ra}, {suffix}" : ra;
                    case GeoAreaDescription.Address:
                        string s0 = !string.IsNullOrEmpty(house_number) ? $"{GetRoadOrAmenity()} {house_number}" : GetRoadOrAmenity();
                        string s1 = GetNeighbourhoodOrSuburb();
                        s1 = string.IsNullOrEmpty(postcode) ? s1 : string.IsNullOrEmpty(s1) ? $"({postcode})" : $"{s1} ({postcode})";
                        string address = !string.IsNullOrEmpty(s1) ? $"{s0}, {s1}" : s0;
                        return specify && !string.IsNullOrEmpty(country_code) ? $"{address} [{GetCountryCode()}]" : address;
                    default:
                        return ToShortString();
                }
                ;
            }
            /// <summary>Returns the compact address.</summary>
            public string ToShortString() => GetArea(GeoAreaDescription.Address);
            /// <summary>Returns the object as JSON.</summary>
            public override string ToString() => this.ToJson();
        }

        /// <summary>Represents a geographic lookup result and its associated metadata.</summary>
        public class GeoObject
        {
            /// <summary>Gets or sets the lookup service's place identifier.</summary>
            public string place_id { get; set; } = EMPTY_STRING;
            /// <summary>Gets or sets the license text returned with the result.</summary>
            public string licence { get; set; } = EMPTY_STRING;
            /// <summary>Gets or sets the OpenStreetMap element type.</summary>
            public string osm_type { get; set; } = EMPTY_STRING;
            /// <summary>Gets or sets the OpenStreetMap element identifier.</summary>
            public string osm_id { get; set; } = EMPTY_STRING;
            /// <summary>Gets or sets the latitude as returned by the lookup.</summary>
            public string lat { get; set; } = EMPTY_STRING;
            /// <summary>Gets or sets the longitude as returned by the lookup.</summary>
            public string lon { get; set; } = EMPTY_STRING;
            /// <summary>Gets or sets the result's display name.</summary>
            public string display_name { get; set; } = EMPTY_STRING;
            /// <summary>Gets or sets the result's classification.</summary>
            public string cls { get; set; } = EMPTY_STRING;
            /// <summary>Gets or sets the result's type.</summary>
            public string type { get; set; } = EMPTY_STRING;
            /// <summary>Gets or sets the importance value returned with the result.</summary>
            public string importance { get; set; } = EMPTY_STRING;
            /// <summary>Gets or sets the bounding box in south, north, west, east order.</summary>
            public double[]? boundingbox { get; set; }
            /// <summary>Gets or sets the address details.</summary>
            public GeoAddress? address { get; set; }
            /// <summary>Returns the display name from the lookup result.</summary>
            public string GetDisplayName() => display_name;
            /// <summary>Parses the result's latitude in decimal degrees.</summary>
            public double GetLatitude() => lat.AsDouble();
            /// <summary>Parses the result's longitude in decimal degrees.</summary>
            public double GetLongitude() => lon.AsDouble();
            /// <summary>Returns the bounding box supplied by the lookup.</summary>
            public double[]? GetBoundingbox() => boundingbox;
            /// <summary>Returns the address supplied by the lookup, if present.</summary>
            public GeoAddress? GetGeoAddress() => address;
            /// <summary>Returns parsed coordinates, or null if either coordinate is missing or malformed.</summary>
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
            /// <summary>Returns the object as JSON.</summary>
            public override string ToString() => this.ToJson();
        }

        /// <summary>Represents a coordinate pair and an optional bounding box.</summary>
        public class GeoCoordinates
        {
            private static JsonSerializerOptions JsonSerializerOptions => new()
            {
                WriteIndented = true,
                IgnoreReadOnlyFields = true,
                IgnoreReadOnlyProperties = true
            };

            private const double F = 60d;

            /// <summary>Gets or sets whether the coordinate pair is marked as valid.</summary>
            public bool IsValid { get; set; }
            /// <summary>Gets or sets the latitude in decimal degrees.</summary>
            public double Latitude { get; set; }
            /// <summary>Gets or sets the longitude in decimal degrees.</summary>
            public double Longitude { get; set; }
            /// <summary>Gets or sets the bounding box in south, north, west, east order.</summary>
            public double[]? Boundingbox { get; set; }


            /// <summary>Gets the north or south indicator for the latitude.</summary>
            [JsonIgnore]
            public string LatitudeWindDirection => GetLatitudeWindDirection(Latitude);
            /// <summary>Gets the east or west indicator for the longitude.</summary>
            [JsonIgnore]
            public string LongitudeWindDirection => GetLongitudeWindDirection(Longitude);
            /// <summary>Gets the absolute latitude in degrees, minutes, and seconds.</summary>
            [JsonIgnore]
            public double[] LatitudeCoordinates => GetDegreesMinutesSeconds(Latitude);
            /// <summary>Gets the absolute longitude in degrees, minutes, and seconds.</summary>
            [JsonIgnore]
            public double[] LongitudeCoordinates => GetDegreesMinutesSeconds(Longitude);
            /// <summary>Gets the rounded radius estimated from the bounding box, or 10 when no box is available.</summary>
            [JsonIgnore]
            public int Radius => Boundingbox is double[] bb ? (int)Math.Round(GetRadius(bb)) : 10;

            /// <summary>Returns N for a nonnegative latitude and S otherwise.</summary>
            /// <param name="latitude">The latitude in decimal degrees.</param>
            public static string GetLatitudeWindDirection(double latitude) => latitude >= 0d ? $"{N_UPPER}" : $"{S_UPPER}";

            /// <summary>Returns E for a nonnegative longitude and W otherwise.</summary>
            /// <param name="longitude">The longitude in decimal degrees.</param>
            public static string GetLongitudeWindDirection(double longitude) => longitude >= 0d ? $"{E_UPPER}" : $"{W_UPPER}";

            /// <summary>Converts degrees, minutes, and seconds to signed decimal degrees.</summary>
            /// <param name="degreesMinutesSeconds">An array containing degrees, minutes, and seconds in that order.</param>
            /// <param name="cardinalDirection">The hemisphere indicator. N and E give a positive sign, and other values give a negative sign.</param>
            public static double GetDecimalDegrees(double[] degreesMinutesSeconds, string cardinalDirection)
            {
                int sign = cardinalDirection.Equals($"{N_UPPER}", StringComparison.CurrentCultureIgnoreCase) || cardinalDirection.Equals($"{E_UPPER}", StringComparison.CurrentCultureIgnoreCase) ? 1 : -1;
                return sign * (degreesMinutesSeconds[0] + (degreesMinutesSeconds[1] / F) + (degreesMinutesSeconds[2] / (F * F)));
            }

            /// <summary>Splits the absolute decimal degree value into degrees, minutes, and seconds.</summary>
            /// <param name="decimalDegrees">The signed angle in decimal degrees.</param>
            public static double[] GetDegreesMinutesSeconds(double decimalDegrees)
            {
                double degAbs = Math.Abs(decimalDegrees);
                double deg = Math.Truncate(degAbs);
                double minTmp = (degAbs - deg) * F;
                double min = Math.Truncate(minTmp);
                double sec = (minTmp - min) * F;
                return [deg, min, sec];
            }

            /// <summary>Estimates the distance in metres between two geographic coordinates.</summary>
            /// <param name="lat1">The first latitude in decimal degrees.</param>
            /// <param name="lon1">The first longitude in decimal degrees.</param>
            /// <param name="lat2">The second latitude in decimal degrees.</param>
            /// <param name="lon2">The second longitude in decimal degrees.</param>
            public static double GetDistance(double lat1, double lon1, double lat2, double lon2)
            {
                double latMid = (lat1 + lat2) * 0.5 * (Math.PI / 180d);
                double latFac = 111132.92 - (559.82 * Math.Cos(2 * latMid)) + (1.175 * Math.Cos(4 * latMid)) - (0.0023 * Math.Cos(6 * latMid));
                double lonFac = (111412.84 * Math.Cos(latMid)) - (93.5 * Math.Cos(3 * latMid)) + (0.118 * Math.Cos(5 * latMid));
                double latDelta = Math.Abs(lat1 - lat2);
                double lonDelta = Math.Abs(lon1 - lon2);
                return Math.Sqrt(Math.Pow(latDelta * latFac, 2) + Math.Pow(lonDelta * lonFac, 2));
            }

            /// <summary>Estimates a radius from the bounding box diagonal.</summary>
            /// <param name="boundingbox">The bounds in south, north, west, east order.</param>
            public static double GetRadius(double[] boundingbox)
            {
                return GetDistance(boundingbox[0], boundingbox[2], boundingbox[1], boundingbox[3]) * 0.5 * Math.Sqrt(0.5);
            }

            /// <summary>Returns the bounding box supplied by the lookup.</summary>
            /// <param name="latitude">The latitude in decimal degrees.</param>
            /// <param name="longitude">The longitude in decimal degrees.</param>
            /// <param name="radius">The radius in metres.</param>
            public static double[] GetBoundingbox(double latitude, double longitude, double radius)
            {
                double d = Math.Abs(radius / (111320 * Math.Sqrt(0.5)));
                return [latitude - d, latitude + d, longitude - d, longitude + d];
            }

            /// <summary>Tests whether the supplied latitude and longitude are strictly inside the bounds.</summary>
            /// <param name="latitude">The latitude in decimal degrees.</param>
            /// <param name="longitude">The longitude in decimal degrees.</param>
            /// <param name="boundingbox">The bounds in south, north, west, east order.</param>
            public static bool IsInside(double latitude, double longitude, double[] boundingbox)
            {
                return latitude > boundingbox[0] && latitude < boundingbox[1]
                    && longitude > boundingbox[2] && longitude < boundingbox[3];
            }

            /// <summary>Tests whether the coordinates are strictly inside their stored bounds.</summary>
            /// <param name="geoCoordinates">The coordinates and bounding box to compare.</param>
            public static bool IsInside(GeoCoordinates geoCoordinates)
            {
                return geoCoordinates.Boundingbox is double[] bb && IsInside(geoCoordinates.Latitude, geoCoordinates.Longitude, bb);
            }

            /// <summary>Tests whether this coordinate pair is strictly inside the supplied bounds.</summary>
            /// <param name="boundingbox">The bounds in south, north, west, east order.</param>
            public bool IsInside(double[] boundingbox) => IsInside(Latitude, Longitude, boundingbox);

            /// <summary>Initializes an invalid coordinate pair.</summary>
            public GeoCoordinates() { }

            /// <summary>Initializes coordinates from JSON.</summary>
            /// <param name="json">The serialized data.</param>
            public GeoCoordinates(string json)
            {
                if (!string.IsNullOrEmpty(json) && JsonSerializer.Deserialize<GeoCoordinates?>(json, JsonSerializerOptions) is GeoCoordinates gc)
                {
                    IsValid = gc.IsValid;
                    Latitude = gc.Latitude;
                    Longitude = gc.Longitude;
                    Boundingbox = gc.Boundingbox;
                }
            }

            /// <summary>Initializes coordinates when both latitude and longitude are supplied.</summary>
            /// <param name="latitude">The latitude in decimal degrees.</param>
            /// <param name="longitude">The longitude in decimal degrees.</param>
            /// <param name="boundingbox">The bounds to copy, or null to calculate a default box.</param>
            public GeoCoordinates(double? latitude, double? longitude, double[]? boundingbox = null)
            {
                if (latitude != null && longitude != null)
                {
                    IsValid = true;
                    Latitude = (double)latitude;
                    Longitude = (double)longitude;
                    Boundingbox = boundingbox is double[] bb ? (double[])bb.Clone() : GetBoundingbox(Latitude, Longitude, 10);
                }
            }

            /// <summary>Initializes coordinates from degrees, minutes, seconds, and hemisphere indicators.</summary>
            /// <param name="latitudeCoordinates">The latitude in degrees, minutes, and seconds.</param>
            /// <param name="longitudeCoordinates">The longitude in degrees, minutes, and seconds.</param>
            /// <param name="latitudeWindDirection">The latitude hemisphere indicator.</param>
            /// <param name="longitudeWindDirection">The longitude hemisphere indicator.</param>
            /// <param name="boundingbox">The bounds to copy, or null to calculate a default box.</param>
            public GeoCoordinates(
                double[] latitudeCoordinates,
                double[] longitudeCoordinates,
                string latitudeWindDirection,
                string longitudeWindDirection,
                double[]? boundingbox = null
            )
            {
                IsValid = true;
                Latitude = GetDecimalDegrees(latitudeCoordinates, latitudeWindDirection);
                Longitude = GetDecimalDegrees(longitudeCoordinates, longitudeWindDirection);
                Boundingbox = boundingbox is double[] bb ? (double[])bb.Clone() : GetBoundingbox(Latitude, Longitude, 10);
            }

            /// <summary>Returns the coordinates and their stored metadata as indented JSON.</summary>
            public string ToJson() => JsonSerializer.Serialize(this, JsonSerializerOptions);

            /// <summary>Formats valid coordinates as an invariant decimal pair, or returns an empty string.</summary>
            public string ToShortString() => !IsValid ? string.Empty : $"{Latitude.ToString(CULTURE_INFO_DEFAULT)}, {Longitude.ToString(CULTURE_INFO_DEFAULT)}";

            /// <summary>Formats valid coordinates as degrees, minutes, seconds, and hemisphere indicators, or returns an empty string.</summary>
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
                double[] lat = [Math.Abs(latNum[0]), Math.Abs(latNum[1]), Math.Abs(latNum[2])];
                double[] lon = [Math.Abs(lonNum[0]), Math.Abs(lonNum[1]), Math.Abs(lonNum[2])];
                string latCoords = $"{lat[0]}{DEGREE_SIGN}{lat[1]}{SINGLE_QUOTE}{Math.Round(lat[2], 4).ToString(CULTURE_INFO_DEFAULT)}{DOUBLE_QUOTE}{latRef}";
                string lonCoords = $"{lon[0]}{DEGREE_SIGN}{lon[1]}{SINGLE_QUOTE}{Math.Round(lon[2], 4).ToString(CULTURE_INFO_DEFAULT)}{DOUBLE_QUOTE}{lonRef}";
                return $"{latCoords}{SPACE}{lonCoords}";
            }
        }

        private static async Task<GeoObject?> TryGetGeoObjectAsync(string input)
        {
            string uri = $"https://nominatim.openstreetmap.org/{input}";
            string productName = Application.ProductName ?? "UnknownProduct";
            string productVersion = Application.ProductVersion ?? "1.0";
            ProductHeaderValue product = new(productName, productVersion);
            _httpClient.DefaultRequestHeaders.UserAgent.Clear();
            _httpClient.DefaultRequestHeaders.UserAgent.Add(new(product));
            GeoObject? geoObject = null;
            try
            {
                using HttpResponseMessage response = await _httpClient.GetAsync(uri);
                _ = response.EnsureSuccessStatusCode();
                byte[] json = await response.Content.ReadAsByteArrayAsync();
                //MessageBox.Show(await response.Content.ReadAsStringAsync());

                DataContractJsonSerializerSettings dcjss = new() { UseSimpleDictionaryFormat = true };
                using MemoryStream ms = new(json);
                try
                {
                    DataContractJsonSerializer dcjs = new(typeof(GeoObject[]), dcjss);
                    if ((GeoObject[]?)dcjs.ReadObject(ms) is GeoObject[] goa)
                    {
                        List<double> l = [];
                        foreach (GeoObject go in goa)
                        {
                            if (go.boundingbox is double[] bb)
                            {
                                l.Add(GeoCoordinates.GetRadius(bb));
                            }
                        }
                        int i = l.Count != 0 ? l.IndexOf(l.Max()) : 0;
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

        /// <summary>Searches for a location by text.</summary>
        /// <param name="query">The search text.</param>
        /// <returns>The lookup result, if available.</returns>
        public static async Task<GeoObject?> GetGeoObjectAsync(string query) => await TryGetGeoObjectAsync($"search?format=json&q={query}");

        /// <summary>Looks up an address by latitude and longitude.</summary>
        /// <param name="latitude">The latitude in decimal degrees.</param>
        /// <param name="longitude">The longitude in decimal degrees.</param>
        /// <returns>The lookup result, if available.</returns>
        public static async Task<GeoObject?> GetGeoObjectAsync(double latitude, double longitude)
        {
            string lat = latitude.ToString(CULTURE_INFO_DEFAULT);
            string lon = longitude.ToString(CULTURE_INFO_DEFAULT);
            return await TryGetGeoObjectAsync($"reverse?format=json&lat={lat}&lon={lon}");
        }

        /// <summary>Reads GPS coordinates from video metadata using ExifTool.</summary>
        /// <param name="videoPath">The path to the video file.</param>
        /// <returns>The coordinates, an invalid coordinate pair for an empty result, or null if the read fails.</returns>
        public static GeoCoordinates? ReadVideoGPSCoords(string videoPath)
        {
            GeoCoordinates? geoCoords = null;
            try
            {
                using Process tool = ExifWrapper.CreateToolProcess();
                tool.StartInfo.ArgumentList.Add("-p");
                tool.StartInfo.ArgumentList.Add(ExifWrapper.ToolFmtPath);
                tool.StartInfo.ArgumentList.Add("-ee");
                tool.StartInfo.ArgumentList.Add(videoPath);
                _ = tool.Start();
                string output = tool.StandardOutput.ReadToEnd();
                tool.WaitForExit();
                Map<string, double> m = new(output);
                geoCoords = m.Any() ? new(m["lat"], m["lng"]) : new();
            }
            catch (Exception e) { Debug.WriteLine(e.Message); }
            return geoCoords;
        }

        /// <summary>Writes or clears GPS coordinates in the video's metadata using ExifTool.</summary>
        /// <param name="videoPath">The path to the video file.</param>
        /// <param name="geoCoords">The coordinates to write, or null to clear the tags.</param>
        /// <remarks>Passing null or invalid coordinates clears the GPS tags. ExifTool overwrites the original file.</remarks>
        /// <returns><see langword="true"/> if ExifTool exits with code zero.</returns>
        public static bool WriteVideoGPSCoords(string videoPath, GeoCoordinates? geoCoords)
        {
            string keysTag = "-Keys:GPSCoordinates=";
            string userDataTag = "-UserData:GPSCoordinates=";

            if (geoCoords is GeoCoordinates gc && gc.IsValid)
            {
                string latitude = gc.Latitude.ToString("+00.0000000;-00.0000000", CULTURE_INFO_DEFAULT);
                string longitude = gc.Longitude.ToString("+000.0000000;-000.0000000", CULTURE_INFO_DEFAULT);
                string gps = $"{latitude}{longitude}/";
                keysTag += gps;
                userDataTag += gps;
            }

            using Process tool = ExifWrapper.CreateToolProcess();
            tool.StartInfo.ArgumentList.Add("-overwrite_original");
            tool.StartInfo.ArgumentList.Add(keysTag);
            tool.StartInfo.ArgumentList.Add(userDataTag);
            tool.StartInfo.ArgumentList.Add(videoPath);
            _ = tool.Start();
            tool.StandardOutput.BaseStream.CopyTo(Stream.Null);
            tool.WaitForExit();

            return tool.ExitCode == 0;
        }

        /// <summary>Disposes the shared HTTP client used by geographic lookups.</summary>
        public static void Dispose()
        {
            Debug.WriteLine($"Disposing 'GeoLocator'...");

            _httpClient.Dispose();
        }
    }
}
