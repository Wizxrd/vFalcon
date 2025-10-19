﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static vFalcon.Nasr.Models.NavCsvDataModel;

namespace vFalcon.Nasr.Parsers
{
    public class NavCsvParser
    {
        public NavCsvDataCollection ParseNavBase(string filePath)
        {
            var result = new NavCsvDataCollection();

            result.NavBase = FebCsvHelper.ProcessLines(
                filePath,
                fields => new NavBase
                {
                    EffDate = fields["EFF_DATE"],
                    NavId = fields["NAV_ID"],
                    NavType = fields["NAV_TYPE"],
                    StateCode = fields["STATE_CODE"],
                    City = fields["CITY"],
                    CountryCode = fields["COUNTRY_CODE"],
                    NavStatus = fields["NAV_STATUS"],
                    Name = fields["NAME"],
                    StateName = fields["STATE_NAME"],
                    RegionCode = fields["REGION_CODE"],
                    CountryName = fields["COUNTRY_NAME"],
                    FanMarker = fields["FAN_MARKER"],
                    Owner = fields["OWNER"],
                    Operator = fields["OPERATOR"],
                    NasUseFlag = fields["NAS_USE_FLAG"],
                    PublicUseFlag = fields["PUBLIC_USE_FLAG"],
                    NdbClassCode = fields["NDB_CLASS_CODE"],
                    OperHours = fields["OPER_HOURS"],
                    HighAltArtccId = fields["HIGH_ALT_ARTCC_ID"],
                    HighArtccName = fields["HIGH_ARTCC_NAME"],
                    LowAltArtccId = fields["LOW_ALT_ARTCC_ID"],
                    LowArtccName = fields["LOW_ARTCC_NAME"],
                    LatDeg = FebCsvHelper.ParseInt(fields["LAT_DEG"]),
                    LatMin = FebCsvHelper.ParseInt(fields["LAT_MIN"]),
                    LatSec = FebCsvHelper.ParseDouble(fields["LAT_SEC"]),
                    LatHemis = fields["LAT_HEMIS"],
                    LatDecimal = FebCsvHelper.ParseDouble(fields["LAT_DECIMAL"]),
                    LongDeg = FebCsvHelper.ParseInt(fields["LONG_DEG"]),
                    LongMin = FebCsvHelper.ParseInt(fields["LONG_MIN"]),
                    LongSec = FebCsvHelper.ParseDouble(fields["LONG_SEC"]),
                    LongHemis = fields["LONG_HEMIS"],
                    LongDecimal = FebCsvHelper.ParseDouble(fields["LONG_DECIMAL"]),
                    SurveyAccuracyCode = fields["SURVEY_ACCURACY_CODE"],
                    TacanDmeStatus = fields["TACAN_DME_STATUS"],
                    TacanDmeLatDeg = FebCsvHelper.ParseNullableInt(fields["TACAN_DME_LAT_DEG"]),
                    TacanDmeLatMin = FebCsvHelper.ParseNullableInt(fields["TACAN_DME_LAT_MIN"]),
                    TacanDmeLatSec = FebCsvHelper.ParseNullableDouble(fields["TACAN_DME_LAT_SEC"]),
                    TacanDmeLatHemis = fields["TACAN_DME_LAT_HEMIS"],
                    TacanDmeLatDecimal = FebCsvHelper.ParseNullableDouble(fields["TACAN_DME_LAT_DECIMAL"]),
                    TacanDmeLongDeg = FebCsvHelper.ParseNullableInt(fields["TACAN_DME_LONG_DEG"]),
                    TacanDmeLongMin = FebCsvHelper.ParseNullableInt(fields["TACAN_DME_LONG_MIN"]),
                    TacanDmeLongSec = FebCsvHelper.ParseNullableDouble(fields["TACAN_DME_LONG_SEC"]),
                    TacanDmeLongHemis = fields["TACAN_DME_LONG_HEMIS"],
                    TacanDmeLongDecimal = FebCsvHelper.ParseNullableDouble(fields["TACAN_DME_LONG_DECIMAL"]),
                    Elev = FebCsvHelper.ParseNullableDouble(fields["ELEV"]),
                    MagVarn = FebCsvHelper.ParseNullableInt(fields["MAG_VARN"]),
                    MagVarnHemis = fields["MAG_VARN_HEMIS"],
                    MagVarnYear = FebCsvHelper.ParseNullableInt(fields["MAG_VARN_YEAR"]),
                    SimulVoiceFlag = fields["SIMUL_VOICE_FLAG"],
                    PwrOutput = FebCsvHelper.ParseNullableInt(fields["PWR_OUTPUT"]),
                    AutoVoiceIdFlag = fields["AUTO_VOICE_ID_FLAG"],
                    MntCatCode = fields["MNT_CAT_CODE"],
                    VoiceCall = fields["VOICE_CALL"],
                    Chan = fields["CHAN"],
                    Freq = FebCsvHelper.ParseNullableDouble(fields["FREQ"]),
                    MkrIdent = fields["MKR_IDENT"],
                    MkrShape = fields["MKR_SHAPE"],
                    MkrBrg = FebCsvHelper.ParseNullableInt(fields["MKR_BRG"]),
                    AltCode = fields["ALT_CODE"],
                    DmeSsv = fields["DME_SSV"],
                    LowNavOnHighChartFlag = fields["LOW_NAV_ON_HIGH_CHART_FLAG"],
                    ZMkrFlag = fields["Z_MKR_FLAG"],
                    FssId = fields["FSS_ID"],
                    FssName = fields["FSS_NAME"],
                    FssHours = fields["FSS_HOURS"],
                    NotamId = fields["NOTAM_ID"],
                    QuadIdent = fields["QUAD_IDENT"],
                    PitchFlag = fields["PITCH_FLAG"],
                    CatchFlag = fields["CATCH_FLAG"],
                    SuaAtcaaFlag = fields["SUA_ATCAA_FLAG"],
                    RestrictionFlag = fields["RESTRICTION_FLAG"],
                    HiwasFlag = fields["HIWAS_FLAG"],
                });

            return result;
        }

        public NavCsvDataCollection ParseNavCkpt(string filePath)
        {
            var result = new NavCsvDataCollection();

            result.NavCkpt = FebCsvHelper.ProcessLines(
                filePath,
                fields => new NavCkpt
                {
                    EffDate = fields["EFF_DATE"],
                    NavId = fields["NAV_ID"],
                    NavType = fields["NAV_TYPE"],
                    StateCode = fields["STATE_CODE"],
                    City = fields["CITY"],
                    CountryCode = fields["COUNTRY_CODE"],
                    Altitude = FebCsvHelper.ParseNullableInt(fields["ALTITUDE"]),
                    Brg = FebCsvHelper.ParseInt(fields["BRG"]),
                    AirGndCode = fields["AIR_GND_CODE"],
                    ChkDesc = fields["CHK_DESC"],
                    ArptId = fields["ARPT_ID"],
                    StateChkCode = fields["STATE_CHK_CODE"],
                });

            return result;
        }

        public NavCsvDataCollection ParseNavRmk(string filePath)
        {
            var result = new NavCsvDataCollection();

            result.NavRmk = FebCsvHelper.ProcessLines(
                filePath,
                fields => new NavRmk
                {
                    EffDate = fields["EFF_DATE"],
                    NavId = fields["NAV_ID"],
                    NavType = fields["NAV_TYPE"],
                    StateCode = fields["STATE_CODE"],
                    City = fields["CITY"],
                    CountryCode = fields["COUNTRY_CODE"],
                    TabName = fields["TAB_NAME"],
                    RefColName = fields["REF_COL_NAME"],
                    RefColSeqNo = FebCsvHelper.ParseInt(fields["REF_COL_SEQ_NO"]),
                    Remark = fields["REMARK"],
                });

            return result;
        }

    }

    public class NavCsvDataCollection
    {
        public List<NavBase> NavBase { get; set; } = new();
        public List<NavCkpt> NavCkpt { get; set; } = new();
        public List<NavRmk> NavRmk { get; set; } = new();
    }
}
