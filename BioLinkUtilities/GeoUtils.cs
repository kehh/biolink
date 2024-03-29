﻿/*******************************************************************************
 * Copyright (C) 2011 Atlas of Living Australia
 * All Rights Reserved.
 * 
 * The contents of this file are subject to the Mozilla Public
 * License Version 1.1 (the "License"); you may not use this file
 * except in compliance with the License. You may obtain a copy of
 * the License at http://www.mozilla.org/MPL/
 * 
 * Software distributed under the License is distributed on an "AS
 * IS" basis, WITHOUT WARRANTY OF ANY KIND, either express or
 * implied. See the License for the specific language governing
 * rights and limitations under the License.
 ******************************************************************************/
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace BioLink.Client.Utilities {

    /// <summary>
    /// Utility geospatial functions 
    /// </summary>
    public static class GeoUtils {

        /* RegEx for matching Degree Decimal minutes (+Direction) */
        private static readonly Regex DegDecMRegex = new Regex(@"^(\d+).*?([\d.]+)'*\s*(N|S|E|W|n|e|s|w)\s*$");
        /* RegEx for matching Degree Minutes Seconds*/
        private static readonly Regex DmsRegex = new Regex("^(\\d+).*?([\\d.]+)'.*?([\\d.]+)\"*\\s*(N|S|E|W|n|e|s|w)\\s*$");
        /* RegEx for matching Degree Decimal minutes (No direction) */
        private static readonly Regex DegDecMNoDirRegex = new Regex(@"^([-+]?[0-9]+)(?:[^\d]+([-+]?[0-9]*\.?[0-9]+))?");

        /* Some useful constants used when doing coordinate calculations */
        public const double PI = Math.PI;
        public const double FOURTHPI = PI / 4;
        public const double DegreesToRads = PI / 180;
        public const double RadsToDegrees = 180.0 / PI;

        /// <summary>
        /// Degree symbol character
        /// </summary>
        public const string DegreeSymbol = "°";

        private static readonly DirectionRange[] FourPoints = new[] {
            new DirectionRange(0,45, "N"),
            new DirectionRange(45, 135, "E"),
            new DirectionRange(135, 225, "S"),
            new DirectionRange(225, 315, "W"),
            new DirectionRange(315, 361, "N")
        };

        private static readonly DirectionRange[] EightPoints = new [] {
            new DirectionRange(0,22.5, "N"),
            new DirectionRange(22.5, 67.5, "NE"),
            new DirectionRange(67.5,112.5, "E"),
            new DirectionRange(112.5,157.5, "SE"),
            new DirectionRange(157.5,202.5, "S"),
            new DirectionRange(202.5,247.5, "SW"),
            new DirectionRange(247.5,292.5, "W"),
            new DirectionRange(292.5,337.5, "NW"),
            new DirectionRange(337.5, 361, "N")
        };

        private static readonly DirectionRange[] SixteenPoints = new [] {
            new DirectionRange(0,11.25, "N"),
            new DirectionRange(11.25,33.75, "NNE"),
            new DirectionRange(33.75,56.25, "NE"),
            new DirectionRange(56.25,78.75, "ENE"),
            new DirectionRange(78.75,101.25, "E"),
            new DirectionRange(101.25,123.75, "ESE"),
            new DirectionRange(123.75,146.25, "SE"),
            new DirectionRange(146.25,168.75, "SSE"),
            new DirectionRange(168.75,191.25, "S"),
            new DirectionRange(191.25,213.75, "SSW"),
            new DirectionRange(213.75,236.25, "SW"),
            new DirectionRange(236.25,258.75, "WSW"),
            new DirectionRange(258.75,281.75, "W"),
            new DirectionRange(281.75,303.75, "WNW"),
            new DirectionRange(303.75,326.25, "NW"),
            new DirectionRange(326.25,348.75, "NNW"),
            new DirectionRange(348.75,361, "N")
        };

        private static readonly DirectionRange[] ThirtyTwoPoints = new [] {            
                new DirectionRange(0 , 5.625, "N"),
                new DirectionRange(5.625 ,16.875, "NbyE"),
                new DirectionRange(16.875, 28.125, "NNE"),
                new DirectionRange(28.125 , 39.375, "NEbyN"),
                new DirectionRange(39.375 , 50.625,"NE"),
                new DirectionRange(50.625 , 61.875, "NEbyE"),
                new DirectionRange(61.875 , 73.125,  "ENE"),
                new DirectionRange(73.125 , 84.375, "EbyN"),
                new DirectionRange(84.375 , 95.625,"E"),
                new DirectionRange(95.625 , 106.875, "EbyS"),
                new DirectionRange(106.875 , 118.125, "ESE"),
                new DirectionRange(118.125 , 129.375, "SEbyE"),
                new DirectionRange(129.375 , 140.625, "SE"),
                new DirectionRange(140.625 , 151.875, "SEbyS"),
                new DirectionRange(151.875 , 163.125, "SSE"),
                new DirectionRange(163.125 , 174.375, "SbyE"),
                new DirectionRange(174.375 , 185.625, "S"),
                new DirectionRange(185.625 , 196.875, "SbyW"),
                new DirectionRange(196.875 , 208.125, "SSW"),
                new DirectionRange(208.125 , 219.375, "SWbyS"),
                new DirectionRange(219.375 , 230.625,  "SW"),
                new DirectionRange(230.625 , 241.875, "SWbyW"),
                new DirectionRange(241.875 , 253.125, "WSW"),
                new DirectionRange(253.125 , 264.375, "WbyS"),
                new DirectionRange(264.375 , 275.625, "W"),
                new DirectionRange(275.625,286.875,"WbyN"),
                new DirectionRange(286.875,298.125,"WNW"),
                new DirectionRange(298.125,309.375,"NWbyW"),
                new DirectionRange(309.375,320.625,"NW"),
                new DirectionRange(320.625,331.875,"NWbyN"),
                new DirectionRange(331.875,343.125,"NNW"),
                new DirectionRange(343.125,354.375,"NbyW"),
                new DirectionRange(354.375,361,"N")
        };

        /// <summary>
        /// Attempts to extract a coordinate value as decimal from a variety of possible formats...
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static double? ParseCoordinate(object value) {

            if (value == null) {
                return null;
            }

            if (value is double) {
                return (double) value;
            }

            var str = value.ToString();

            // First try decimal degrees...
            if (str.IsNumeric()) {
                return Double.Parse(str);
            }

            // next try Degrees*minutes'Seconds", where * can be anything
            var matcher = DmsRegex.Match(str);
            if (matcher.Success) {
                var degrees = Int32.Parse(matcher.Groups[1].Value);
                var minutes = Int32.Parse(matcher.Groups[2].Value);
                var seconds = Int32.Parse(matcher.Groups[3].Value);
                var dir = matcher.Groups[4].Value;
                return DMSToDecDeg(degrees, minutes, seconds, dir);
            }

            // try degrees decimal minutes with direction...
            matcher = DegDecMRegex.Match(str);
            if (matcher.Success) {
                int degrees = Int32.Parse(matcher.Groups[1].Value);
                double minutes = Double.Parse(matcher.Groups[2].Value);
                string dir = matcher.Groups[3].Value;
                return DDecMDirToDecDeg(degrees, minutes, dir);
            }

            // try degrees decimal minutes (using sign as direction)
            matcher = DegDecMNoDirRegex.Match(str);
            if (matcher.Success) {
                int degrees = Int32.Parse(matcher.Groups[1].Value);
                double minutes = Double.Parse(matcher.Groups[2].Value);                
                return DDecMToDecDeg(degrees, minutes);
            }

            return null;
        }

        /// <summary>
        /// Given two points, works out a coarse bearing (Direction) from point 1 to point2. The coarseness is limited to a compass with either 4, 8, 16 or 32 points
        /// 
        /// </summary>
        /// <param name="nsLat1">Latitude for point 1</param>
        /// <param name="nsLong1">Longitude for point 1</param>
        /// <param name="nsLat2">Latitude for point 2</param>
        /// <param name="nsLong2">Longitude for point 2</param>
        /// <param name="niNumberOfPoints">The number of points on the compass from which the bearing will be taken. If 0, the actual bearing in degrees will be returned</param>
        /// <returns></returns>
        public static string GreatCircleArcDirection(double nsLat1, double nsLong1, double nsLat2, double nsLong2, int niNumberOfPoints) {
            // Calculate the Great Circle Arc direction between two points
            const double ndPi = Math.PI;
            double nsDirection;


            // Convert degrees to radians
            // Radians = Degrees * (Pi / 180)
            double ndRadLat1 = nsLat1 * (ndPi / 180);
            double ndRadLong1 = nsLong1 * (ndPi / 180) * -1;
            double ndRadLat2 = nsLat2 * (ndPi / 180);
            double ndRadLong2 = nsLong2 * (ndPi / 180) * -1;

            // Get distance between the points in radians
            double nsDistance = GreatCircleArcLength(nsLat1, nsLong1, nsLat2, nsLong2, DistanceUnits.None);

            if (Math.Abs(nsDistance - 0) < 0.000001) { // the points are very close together
                return "-";  // Return nothing
            }

            // Calculate direction in radians
            double ndPartial = (Math.Sin(ndRadLat2) - Math.Sin(ndRadLat1) * Math.Cos(nsDistance)) / (Math.Sin(nsDistance) * Math.Cos(ndRadLat1));
            // Arccos(X) = Atn(-X / Sqr(-X * X + 1)) + 1.5708
            // ndPartial = Math.Atan((ndPartial * -1) / Math.Sqr((ndPartial * -1) * ndPartial + 1)) + 1.5708;
            ndPartial = Math.Acos(ndPartial);

            if (Math.Sin(ndRadLong2 - ndRadLong1) <= 0) {
                nsDirection = ndPartial;
            } else {
                nsDirection = 2 * ndPi - ndPartial;
            }

            // Convert to degrees
            nsDirection = (180 / ndPi) * nsDirection;

            DirectionRange[] arr = null;

            // Convert to compass direction
            switch (niNumberOfPoints) {
                case 0: // ' Return the direction in degrees
                    return nsDirection + "";
                case 4:
                    arr = FourPoints;
                    break;
                case 8:
                    arr = EightPoints;
                    break;
                case 16:
                    arr = SixteenPoints;
                    break;
                case 32:
                    arr = ThirtyTwoPoints;
                    break;
            }
            if (arr != null) {
                foreach (DirectionRange r in arr) {
                    if (nsDirection >= r.StartAngle && nsDirection <= r.EndAngle) {
                        return r.Direction;
                    }
                }
                return "Direction not found for angle " + nsDirection;
            }

            return "Invalid number of points: " + niNumberOfPoints;
        }

        /// <summary>
        /// Returns the distance between two points on a curved plane with an assumed mean radius of 6371.1 KM (Earth)
        /// </summary>
        /// <param name="fLat1">Y1</param>
        /// <param name="fLong1">X1</param>
        /// <param name="fLat2">Y2</param>
        /// <param name="fLong2">X2</param>
        /// <param name="units">Should start with k for kilometres or m for Miles</param>
        /// <returns></returns>
        public static double GreatCircleArcLength(double fLat1, double fLong1, double fLat2, double fLong2, string units) {
            var distUnits = DistanceUnits.None;
            if (units.StartsWith("k", StringComparison.CurrentCultureIgnoreCase)) {
                distUnits = DistanceUnits.Kilometers;
            } else if (units.StartsWith("m", StringComparison.CurrentCultureIgnoreCase)) {
                distUnits = DistanceUnits.Miles;
            }

            return GreatCircleArcLength(fLat1, fLong1, fLat2, fLong2, distUnits);
        }

        /// <summary>
        /// Returns the distance between two points on a curved plane with an assumed mean radius of 6371.1 KM (Earth)
        /// </summary>
        /// <param name="fLat1">Y1</param>
        /// <param name="fLong1">X1</param>
        /// <param name="fLat2">Y2</param>
        /// <param name="fLong2">X2</param>
        /// <param name="units">Miles or Kilometres</param>
        /// <returns></returns>
        public static double GreatCircleArcLength(double fLat1, double fLong1, double fLat2, double fLong2, DistanceUnits units) {
            const double dfPi = Math.PI;

            double dfRadLat1 = fLat1 * (dfPi / 180);
            double dfRadLat2 = fLat2 * (dfPi / 180);
            double dfRadDeltaLong = (fLong2 - fLong1) * (dfPi / 180);
            double dfCosZ = Math.Sin(dfRadLat1) * Math.Sin(dfRadLat2) + Math.Cos(dfRadLat1) * Math.Cos(dfRadLat2) * Math.Cos(dfRadDeltaLong);

            if (Math.Abs(dfCosZ - 1.0) < 0.0000000001) {
                // The points were very close together
                return 0;
            } else {
                double dfDiv = Math.Sqrt((dfCosZ * -1) * dfCosZ + 1);
                if (dfDiv == 0) {
                    return 0;
                } else {
                    double dfACosZ = Math.Atan((dfCosZ * -1) / dfDiv) + 1.5708;
                    switch (units) {
                        case DistanceUnits.Kilometers:
                            return dfACosZ * (float)6371.1;	// Mean radius of Earth is 6371.1 KM
                        case DistanceUnits.Miles:
                            return dfACosZ * (float)3959;	// Mean radius of Earth is 3959 Miles
                        case DistanceUnits.None:
                            return dfACosZ;					// Users can apply their own Mean Radius to get different units
                        default:
                            throw new Exception("Unhandled distance unit type: " + units.ToString());
                    }
                }
            }
        }

        /// <summary>
        /// Convert a string in the format of Degrees Minutes and Seconds into a signed decimal degree value
        /// </summary>
        /// <param name="szIn">The string to convert</param>
        /// <param name="iCoordType">Latitude or Longitide</param>
        /// <param name="dfRet">The result of the conversion if successful</param>
        /// <param name="bszReason">If unsuccessful, the reason for failure</param>
        /// <returns></returns>
        public static bool DMSStrToDecDeg(string szIn, CoordinateType iCoordType, out double dfRet, out string bszReason) {

            int iDegrees = 0, iMinutes = 0;
            double dfSeconds = 0.0;
            char cDirection = '_';
            int i;
            bool isDMS = false;
            int iFirstLength = 2;

            dfRet = 0;
            bszReason = "";

            int lStrLen = szIn.Length;

            for (i = lStrLen - 1; i >= 0; i--) {

                if (("NSEW:;'\"" + DegreeSymbol).Contains(Char.ToUpper(szIn[i]))) {
                    isDMS = true;

                    if ("NSEW".Contains(Char.ToUpper(szIn[i]))) {
                        cDirection = Char.ToUpper(szIn[i]);
                        if ((Char.ToUpper(szIn[i]) == 'N') || (Char.ToUpper(szIn[i]) == 'S')) {
                            iFirstLength = 2;		// Latitudes can only have two digits (00-90)
                        } else {
                            iFirstLength = 3;		// Longitudes can have 3 (000-180)
                        }
                        //break;
                    }
                }
            }


            // -db- #19/01/00# Validate direction...
            if (!isDMS) {
                bszReason = string.Format("Coordinate to be converted incorrectly formatted [{0}] ! - try 'dd:mm:ss D'", szIn);
                return false;
            }

            if (cDirection == '_') {
                bszReason = String.Format("No valid direction in '{0}'. Try 'dd:mm:ss D' where D is either N,S,E or W.", szIn);
                return false;
            }

            switch (iCoordType) {
                case CoordinateType.Latitude:			// 0 = Latitude
                    if ((cDirection != 'N') && (cDirection != 'S')) {
                        bszReason = string.Format("Invalid direction for a Latitude. Expected 'N' or 'S' but got '{0}'.", cDirection);
                        return false;
                    }
                    break;
                case CoordinateType.Longitude:			// 1 = Longitude
                    if ((cDirection != 'E') && (cDirection != 'W')) {
                        bszReason = string.Format("Invalid direction for a Longitude. Expected 'E' or 'W' but got '{0}'.", cDirection);
                        return false;
                    }
                    break;
            }


            int idx = 0;

            // Get Degrees		
            while ((idx < szIn.Length) && !Char.IsDigit(szIn[idx])) idx++;		// find first numeric
            if (idx < szIn.Length) {										// Not at end of string
                i = 0;
                while (Char.IsDigit(szIn[idx + i]) && (i < iFirstLength)) i++;						// find last digit for this number
                string strBuf = szIn.Substring(idx, i);
                iDegrees = Int32.Parse(strBuf);
                idx += i;
            }

            // Get Minutes
            while ((idx < szIn.Length) && !Char.IsDigit(szIn[idx])) idx++;		// find first numeric
            if (idx < szIn.Length) {										// Not at end of string
                i = 0;
                while (Char.IsDigit(szIn[idx + i]) && (i < 2)) i++;		// find last digit for this number
                string szBuf = szIn.Substring(idx, i);
                iMinutes = Int32.Parse(szBuf);
                idx += i;
            }

            // Get Seconds
            while ((idx < szIn.Length) && !Char.IsDigit(szIn[idx])) idx++;		// find first numeric
            if (idx < szIn.Length) {										// Not at end of string
                i = 0;
                while (char.IsDigit(szIn[idx + i]) || (szIn[idx + i] == '.') && (i < 2)) i++;// find last digit for this number
                string szBuf = szIn.Substring(idx, i);

                dfSeconds = double.Parse(szBuf);
            }

            dfRet = (double)iDegrees + ((double)iMinutes / (double)60) + (dfSeconds / (double)3600);

            switch (cDirection) {
                case 'S':
                case 'W':
                    dfRet *= -1;								// Change sign for these directions
                    break;
            }

            return true;
        }


        /// <summary>
        /// Ported from BioLink 2.x
        /// Originally Adapted from VB code as follows...
        /// Convert Decimal Degrees to Degrees, Minutes, Seconds or Hemisphere (N,S,E,W)
        /// From Robert K. Colwell, Univ. of Connecticut, from TAXACOM list server posting,
        /// 30 July, 1996
        /// AbsDD = Abs(Decimal Degrees)
        /// Degrees = Int(AbsDD)
        /// Decimal Minutes = (AbsDD - Degrees) * 60
        /// Minutes = Int(Decimal Minutes)
        /// Decimal Seconds = (Decimal Minutes - Minutes) * 60
        /// Seconds = Round(Decimal Seconds)
        /// </summary>
        /// <param name="decdeg"></param>
        /// <param name="coordType"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static string DecDegToDMS(double decdeg, CoordinateType coordType, String format = "%D" + DegreeSymbol + "%m'%s\"%r") {

            var absDecDeg = Math.Abs(decdeg);
            var iDegrees = (int)Math.Floor(absDecDeg);
            double dfMinutes = (absDecDeg - iDegrees) * 60;
            var iMinutes = (int)Math.Floor(dfMinutes);
            var dfSeconds = (dfMinutes - iMinutes) * 60;
            var iSeconds = (int)Math.Round(dfSeconds);

            if (iSeconds > 59) {
                iMinutes++;
                iSeconds = 0;
            }

            var b = new StringBuilder();
            for (int i = 0; i < format.Length; ++i) {
                // Next character should be a place holder !
                if (format[i] == '%') {
                    i++;
                    switch (format[i]) {
                        case 'd':
                            b.Append(String.Format("{0:00}", iDegrees));
                            break;
                        case 'D':
                            b.Append(String.Format("{0}", iDegrees));
                            break;
                        case 'm':
                            b.Append(String.Format("{0:00}", iMinutes));
                            break;
                        case 'M':
                            b.Append(String.Format("{0}", iMinutes));
                            break;
                        case 's':
                            b.Append(String.Format("{0:00}", iSeconds));
                            break;
                        case 'S':
                            if (iSeconds != 0) {
                                b.Append(String.Format("{0}\"", iSeconds));
                            }
                            break;
                        case 'r':
                            if (coordType == CoordinateType.Latitude) {
                                b.Append(decdeg > 0 ? "N" : "S");
                            } else {
                                b.Append(decdeg > 0 ? "E" : "W");
                            }
                            break;
                        case 'R':
                            if (coordType == CoordinateType.Latitude) {
                                b.Append(decdeg > 0 ? "North" : "South");
                            } else {
                                b.Append(decdeg > 0 ? "East" : "West");
                            }
                            break;
                        default:
                            b.Append(format[i]);
                            break;
                    }

                } else if ((format[i] == '\\')) { // Next character is a special char (e.g. double quote)
                    b.Append(format[++i]);
                } else {
                    b.Append(format[i]); // Otherwise is part of the output string.
                }
            }

            return b.ToString();
        }

        /// <summary>
        /// Converts Degrees decimal minutes into decimal degrees
        /// </summary>
        /// <param name="degrees"></param>
        /// <param name="minutes"></param>
        /// <returns></returns>
        public static double DDecMToDecDeg(int degrees, double minutes) {
            return degrees + (minutes / 60.0) * (degrees < 0 ? -1 : 1);
        }

        /// <summary>
        /// Converts degrees minutes and direction to decimal degrees
        /// </summary>
        /// <param name="degrees"></param>
        /// <param name="minutes"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public static double DDecMDirToDecDeg(int degrees, double minutes, string direction) {
            int sign = ("sw".Contains(direction.ToLower()) ? -1 : 1);
            return (Math.Abs(degrees) + (minutes / 60)) * sign;
        }

        /// <summary>
        /// Converts a decimal degree value into its component degrees minutes and seconds, including direction
        /// </summary>
        /// <param name="decdeg"></param>
        /// <param name="coordType"></param>
        /// <param name="degrees"></param>
        /// <param name="minutes"></param>
        /// <param name="seconds"></param>
        /// <param name="direction"></param>
        public static void DecDegToDMS(double decdeg, CoordinateType coordType, out int degrees, out int minutes, out int seconds, out string direction) {
            degrees = (int)Math.Abs(decdeg);
            double leftover = (Math.Abs(decdeg) - degrees);
            minutes = (int)(leftover * 60);
            leftover = leftover - (minutes / 60.0);
            seconds = (int)Math.Round(leftover * 3600, MidpointRounding.AwayFromZero);

            if (seconds >= 60) {
                minutes++;
                seconds -= 60;
            }

            switch (coordType) {
                case CoordinateType.Latitude:
                    direction = decdeg < 0 ? "S" : "N";
                    break;
                case CoordinateType.Longitude:
                    direction = decdeg < 0 ? "W" : "E";
                    break;
                default:
                    throw new Exception();
            }
        }

        /// <summary>
        /// Converts a decimal degree value into degrees decimal minutes
        /// </summary>
        /// <param name="decdeg"></param>
        /// <param name="degrees"></param>
        /// <param name="minutes"></param>
        public static void DecDegToDDecM(double decdeg, out int degrees, out double minutes) {
            degrees = (int)Math.Abs(decdeg);
            double leftover = (Math.Abs(decdeg) - degrees);
            minutes = Math.Round(leftover * 60, 4);
            if (decdeg < 0) {
                degrees *= -1;
            }
        }

        /// <summary>
        /// Converts a decimal degree value int degrees minutes + direction
        /// </summary>
        /// <param name="decdeg"></param>
        /// <param name="degrees"></param>
        /// <param name="minutes"></param>
        /// <param name="direction"></param>
        /// <param name="coordType"></param>
        public static void DecDegToDDecMDir(double decdeg, out int degrees, out double minutes, out string direction, CoordinateType coordType) {
            DecDegToDDecM(decdeg, out degrees, out minutes);
            if (coordType == CoordinateType.Latitude) {
                direction = (decdeg < 0) ? "S" : "N";
            } else {
                direction = (decdeg < 0) ? "W" : "E";
            }
        }

        /// <summary>
        /// Converts component degrees, minutes and seconds (+ direction) into a decimal degree value
        /// </summary>
        /// <param name="degrees"></param>
        /// <param name="minutes"></param>
        /// <param name="seconds"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public static double DMSToDecDeg(int degrees, int minutes, double seconds, string direction) {
            if (direction == null) {
                direction = "N";
            }
            var decdeg = (double)degrees + ((double)minutes / 60.0) + (seconds / 3600.0);
            var sign = (double)("sw".Contains(direction.ToLower()) ? -1 : 1);
            return decdeg * sign;
        }

        /// <summary>
        /// Given a latitude returns a UTM designator
        /// </summary>
        /// <param name="lat"></param>
        /// <returns></returns>
        private static char UTMLetterDesignator(double lat) {
            //This routine determines the correct UTM letter designator for the given latitude
            //returns 'Z' if latitude is outside the UTM limits of 80N to 80S
            //Written by Chuck Gantz- chuck.gantz@globalstar.com.
            char letterDesignator;

            if ((80 >= lat) && (lat > 72)) letterDesignator = 'X';
            else if ((72 >= lat) && (lat > 64)) letterDesignator = 'W';
            else if ((64 >= lat) && (lat > 56)) letterDesignator = 'V';
            else if ((56 >= lat) && (lat > 48)) letterDesignator = 'U';
            else if ((48 >= lat) && (lat > 40)) letterDesignator = 'T';
            else if ((40 >= lat) && (lat > 32)) letterDesignator = 'S';
            else if ((32 >= lat) && (lat > 24)) letterDesignator = 'R';
            else if ((24 >= lat) && (lat > 16)) letterDesignator = 'Q';
            else if ((16 >= lat) && (lat > 8)) letterDesignator = 'P';
            else if ((8 >= lat) && (lat > 0)) letterDesignator = 'N';
            else if ((0 >= lat) && (lat > -8)) letterDesignator = 'M';
            else if ((-8 >= lat) && (lat > -16)) letterDesignator = 'L';
            else if ((-16 >= lat) && (lat > -24)) letterDesignator = 'K';
            else if ((-24 >= lat) && (lat > -32)) letterDesignator = 'J';
            else if ((-32 >= lat) && (lat > -40)) letterDesignator = 'H';
            else if ((-40 >= lat) && (lat > -48)) letterDesignator = 'G';
            else if ((-48 >= lat) && (lat > -56)) letterDesignator = 'F';
            else if ((-56 >= lat) && (lat > -64)) letterDesignator = 'E';
            else if ((-64 >= lat) && (lat > -72)) letterDesignator = 'D';
            else if ((-72 >= lat) && (lat > -80)) letterDesignator = 'C';
            else letterDesignator = 'Z'; //This is here as an error flag to show that the Latitude is outside the UTM limits

            return letterDesignator;
        }

        // converts lat/long to UTM coords.  Equations from USGS Bulletin 1532 
        // East Longitudes are positive, West longitudes are negative. 
        // North latitudes are positive, South latitudes are negative
        // Lat and Long are in decimal degrees
        // Does not take into account thespecial UTM zones between 0 degrees and 
        // 36 degrees longitude above 72 degrees latitude and a special zone 32 
        // between 56 degrees and 64 degrees north latitude 
        // Originally written by Chuck Gantz- chuck.gantz@globalstar.com. Ported to C by David Baird, 1998, Re-ported to c# David Baird 2010
        public static void LatLongToUTM(Ellipsoid referenceEllipsoid, double latitude, double longitude, out double northing, out double easting, out string zone) {
            double equatorialRadius = referenceEllipsoid.EquatorialRadius;
            double eccSquared = referenceEllipsoid.EccentricitySquared;
            const double k0 = 0.9996; // Magic constant :-(

            double longOrigin;

            double latRad = latitude * DegreesToRads;
            double longRad = longitude * DegreesToRads;

            // Outside bounds checking -db- 01/09/1998
            if (latitude < -90.0) {
                latitude = -90.0;
            }

            if (latitude > 90.0) {
                latitude = 90.0;
            }

            if (longitude < -180.0) {
                longitude = -180.0;
            }

            if (longitude > 180.0) {
                longitude = 180.0;
            }

            if (longitude > -6 && longitude <= 0) {
                longOrigin = -3;	//arbitrarily set origin at 0 longitude to 3W
            } else if (longitude < 6 && longitude > 0) {
                longOrigin = 3;
            } else {
                longOrigin = ((int)longitude / 6) * 6 + 3 * ((int)longitude / 6) / Math.Abs((int)longitude / 6);
            }

            double longOriginRad = longOrigin * DegreesToRads;

            //compute the UTM Zone from the latitude and longitude
            zone = string.Format("{0}{1}", (int)((longitude + 180) / 6) + 1, UTMLetterDesignator(latitude));

            double eccPrimeSquared = (eccSquared) / (1 - eccSquared);

            double n = equatorialRadius / Math.Sqrt(1 - eccSquared * Math.Sin(latRad) * Math.Sin(latRad));
            double T = Math.Tan(latRad) * Math.Tan(latRad);
            double c = eccPrimeSquared * Math.Cos(latRad) * Math.Cos(latRad);
            double a = Math.Cos(latRad) * (longRad - longOriginRad);

            double m = equatorialRadius * ((1 - eccSquared / 4 - 3 * eccSquared * eccSquared / 64 - 5 * eccSquared * eccSquared * eccSquared / 256) * latRad
                            - (3 * eccSquared / 8 + 3 * eccSquared * eccSquared / 32 + 45 * eccSquared * eccSquared * eccSquared / 1024) * Math.Sin(2 * latRad)
                            + (15 * eccSquared * eccSquared / 256 + 45 * eccSquared * eccSquared * eccSquared / 1024) * Math.Sin(4 * latRad)
                            - (35 * eccSquared * eccSquared * eccSquared / 3072) * Math.Sin(6 * latRad));

            easting = k0 * n * (a + (1 - T + c) * a * a * a / 6 + (5 - 18 * T + T * T + 72 * c - 58 * eccPrimeSquared) * a * a * a * a * a / 120) + 500000.0;

            northing = k0 * (m + n * Math.Tan(latRad) * (a * a / 2 + (5 - T + 9 * c + 4 * c * c) * a * a * a * a / 24 + (61 - 58 * T + T * T + 600 * c - 330 * eccPrimeSquared) * a * a * a * a * a * a / 720));
            if (latitude <= 0) {
                northing += 10000000.0; //10000000 meter offset for southern hemisphere
            }

        }

        private static readonly Regex ZoneRegex = new Regex(@"^(\d+)([A-Za-z]{1})$");


        //converts UTM coords to lat/long.  Equations from USGS Bulletin 1532 
        //East Longitudes are positive, West longitudes are negative. 
        //North latitudes are positive, South latitudes are negative
        //Lat and Long are in decimal degrees. 
        //Does not take into account the special UTM zones between 0 degrees 
        //and 36 degrees longitude above 72 degrees latitude and a special 
        //zone 32 between 56 degrees and 64 degrees north latitude
        //Written by Chuck Gantz- chuck.gantz@globalstar.com
        // Ported to C by David Baird, 1998, Re-ported to C# by David Baird 2010
        public static void UTMToLatLong(Ellipsoid referenceEllipsoid, double northing, double easting, string zone, out double latitude, out double longitude) {

            latitude = 0;
            longitude = 0;

            const double k0 = 0.9996;
            double a = referenceEllipsoid.EquatorialRadius;
            double eccSquared = referenceEllipsoid.EccentricitySquared;
            double e1 = (1 - Math.Sqrt(1 - eccSquared)) / (1 + Math.Sqrt(1 - eccSquared));
            // int NorthernHemisphere; //1 for northern hemispher, 0 for southern

            double x = easting - 500000.0;
            double y = northing;
            //if ( x <= 2875) x = 2875;		    // -db- 01/09/1998 To stop errors when Easting = 500000
            if (y == 0) y = 0.000000001;		// -db- 01/09/1998 To stop errors when Northing = 0

            var match = ZoneRegex.Match(zone);
            if (!match.Success) {
                return;
            }

            char zoneLetter = match.Groups[2].Value.ToUpper()[0];
            int zoneNumber = Int32.Parse(match.Groups[1].Value);

            if ((zoneLetter - 'N') > 0) {
                // NorthernHemisphere = 1;//point is in northern hemisphere
            } else {
                // NorthernHemisphere = 0;//point is in southern hemisphere
                y -= 10000000.0;//remove 10,000,000 meter offset used for southern hemisphere
            }

            double longOrigin = (zoneNumber - 1) * 6 - 180 + 3;

            double eccPrimeSquared = (eccSquared) / (1 - eccSquared);

            double m = y / k0;
            double mu = m / (a * (1 - eccSquared / 4 - 3 * eccSquared * eccSquared / 64 - 5 * eccSquared * eccSquared * eccSquared / 256));

            double phi1Rad = mu + (3 * e1 / 2 - 27 * e1 * e1 * e1 / 32) * Math.Sin(2 * mu)
                             + (21 * e1 * e1 / 16 - 55 * e1 * e1 * e1 * e1 / 32) * Math.Sin(4 * mu)
                             + (151 * e1 * e1 * e1 / 96) * Math.Sin(6 * mu);

            double n1 = a / Math.Sqrt(1 - eccSquared * Math.Sin(phi1Rad) * Math.Sin(phi1Rad));
            double t1 = Math.Tan(phi1Rad) * Math.Tan(phi1Rad);
            double c1 = eccPrimeSquared * Math.Cos(phi1Rad) * Math.Cos(phi1Rad);
            double r1 = a * (1 - eccSquared) / Math.Pow(1 - eccSquared * Math.Sin(phi1Rad) * Math.Sin(phi1Rad), 1.5);
            double d = x / (n1 * k0);

            latitude = phi1Rad - (n1 * Math.Tan(phi1Rad) / r1) * (d * d / 2 - (5 + 3 * t1 + 10 * c1 - 4 * c1 * c1 - 9 * eccPrimeSquared) * d * d * d * d / 24
                            + (61 + 90 * t1 + 298 * c1 + 45 * t1 * t1 - 252 * eccPrimeSquared - 3 * c1 * c1) * d * d * d * d * d * d / 720);
            latitude = latitude * RadsToDegrees;

            longitude = (d - (1 + 2 * t1 + c1) * d * d * d / 6 + (5 - 2 * c1 + 28 * t1 - 3 * c1 * c1 + 8 * eccPrimeSquared + 24 * t1 * t1)
                            * d * d * d * d * d / 120) / Math.Cos(phi1Rad);
            longitude = longOrigin + longitude * RadsToDegrees;
        }

        /// <summary>
        /// Returns the index of the supplied ellipsis name (or index) in the statuc array of possible ellipsoids
        /// </summary>
        /// <param name="nameOrIndex"></param>
        /// <returns></returns>
        public static int GetEllipsoidIndex(string nameOrIndex) {

            if (nameOrIndex.IsNumeric()) {
                var idx = Int32.Parse(nameOrIndex);
                if (idx <= 0 || idx >= ELLIPSOIDS.Length) {
                    throw new Exception("Index is outside of the range of allowed ellipsoid indexes: " + nameOrIndex + " (valid range is 1-" + (ELLIPSOIDS.Length - 1));
                }

                return idx;
            }
            foreach (Ellipsoid e in ELLIPSOIDS) {
                if (e.Name.Equals(nameOrIndex, StringComparison.CurrentCultureIgnoreCase)) {
                    return e.ID;
                }
            }
            return -1;
        }

        /// <summary>
        /// Each ellipsoid is an approximation for the Earths geometry by specifying a radius and an 'eccentricity squard' constant
        /// </summary>
        public static Ellipsoid[] ELLIPSOIDS = new[] {
	        new Ellipsoid( -1, "", 0, 0),
	        new Ellipsoid( 1, "Airy", 6377563, 0.00667054),
	        new Ellipsoid( 2, "Australian National", 6378160, 0.006694542),
	        new Ellipsoid( 3, "Bessel 1841", 6377397, 0.006674372),
	        new Ellipsoid( 4, "Bessel 1841 (Nambia) ", 6377484, 0.006674372),
	        new Ellipsoid( 5, "Clarke 1866", 6378206, 0.006768658),
	        new Ellipsoid( 6, "Clarke 1880", 6378249, 0.006803511),
	        new Ellipsoid( 7, "Everest", 6377276, 0.006637847),
	        new Ellipsoid( 8, "Fischer 1960 (Mercury) ", 6378166, 0.006693422),
	        new Ellipsoid( 9, "Fischer 1968", 6378150, 0.006693422),
	        new Ellipsoid( 10, "GRS 1967", 6378160, 0.006694605),
	        new Ellipsoid( 11, "GRS 1980", 6378137, 0.00669438),
	        new Ellipsoid( 12, "Helmert 1906", 6378200, 0.006693422),
	        new Ellipsoid( 13, "Hough", 6378270, 0.00672267),
	        new Ellipsoid( 14, "International", 6378388, 0.00672267),
	        new Ellipsoid( 15, "Krassovsky", 6378245, 0.006693422),
	        new Ellipsoid( 16, "Modified Airy", 6377340, 0.00667054),
	        new Ellipsoid( 17, "Modified Everest", 6377304, 0.006637847),
	        new Ellipsoid( 18, "Modified Fischer 1960", 6378155, 0.006693422),
	        new Ellipsoid( 19, "South American 1969", 6378160, 0.006694542),
	        new Ellipsoid( 20, "WGS 60", 6378165, 0.006693422),
	        new Ellipsoid( 21, "WGS 66", 6378145, 0.006694542),
	        new Ellipsoid( 22, "WGS-72", 6378135, 0.006694318),
	        new Ellipsoid( 23, "WGS-84", 6378137, 0.00669438),
	        new Ellipsoid( 24, "GDA", 6378137, 0.0066943800229)
        };

        /// <summary>
        /// Get an ellipsoid by name
        /// </summary>
        /// <param name="datum"></param>
        /// <returns></returns>
        public static Ellipsoid FindEllipsoidByName(string datum) {
            return ELLIPSOIDS.FirstOrDefault(e => e.Name.Equals(datum, StringComparison.InvariantCultureIgnoreCase));
        }

        /// <summary>
        /// Helper function for printing coordinates (both a lat and long)
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <param name="novalue"></param>
        /// <returns></returns>
        public static string FormatCoordinates(double? latitude, double? longitude, string novalue="") {
            if (latitude.HasValue && longitude.HasValue) {
                return string.Format("{0} {1}", DecDegToDMS(latitude.Value, CoordinateType.Latitude), DecDegToDMS(longitude.Value, CoordinateType.Longitude));
            }
            return novalue;
        }
    }

    /// <summary>
    /// This class holds parameters that approximate the Earths geometry
    /// </summary>
    public class Ellipsoid {

        public Ellipsoid(int id, string name, double radius, double eccentricity) {
            ID = id;
            Name = name;
            EquatorialRadius = radius;
            EccentricitySquared = eccentricity;
        }

        public int ID { get; private set; }
        public string Name { get; private set; }
        public double EquatorialRadius { get; private set; }
        public double EccentricitySquared { get; private set; }


    }

    /// <summary>
    /// Really should be called 'ordinate type'
    /// </summary>
    public enum CoordinateType {
        Latitude, Longitude
    }

    /// <summary>
    /// Differenet geometries that can be described
    /// </summary>
    public enum AreaType {
        Point = 1,
        Line = 2,
        Box = 3
    }

    /// <summary>
    /// Distance Units
    /// </summary>
    public enum DistanceUnits {
        Kilometers,
        Miles,
        None
    }

    /// <summary>
    /// Utility class that describes and labels (direction) an arc on a virtual compass
    /// </summary>
    public class DirectionRange {

        public DirectionRange(double start, double end, string dir) {
            StartAngle = start;
            EndAngle = end;
            Direction = dir;
        }

        public double StartAngle { get; set; }
        public double EndAngle { get; set; }
        public string Direction { get; set; }
    }
}
