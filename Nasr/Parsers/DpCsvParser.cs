﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static vFalcon.Nasr.Models.DpCsvDataModel;

namespace vFalcon.Nasr.Parsers
{
    public class DpCsvParser
    {
        public DpCsvDataCollection ParseDpApt(string filePath)
        {
            var result = new DpCsvDataCollection();

            result.DpApt = FebCsvHelper.ProcessLines(
                filePath,
                fields => new DpApt
                {
                    EffDate = fields["EFF_DATE"],
                    DpName = fields["DP_NAME"],
                    Artcc = fields["ARTCC"],
                    DpComputerCode = fields["DP_COMPUTER_CODE"],
                    BodyName = fields["BODY_NAME"],
                    AptBodySeq = FebCsvHelper.ParseInt(fields["BODY_SEQ"]),
                    ArptId = fields["ARPT_ID"],
                    RwyEndId = fields["RWY_END_ID"],
                });

            return result;
        }

        public DpCsvDataCollection ParseDpBase(string filePath)
        {
            var result = new DpCsvDataCollection();

            result.DpBase = FebCsvHelper.ProcessLines(
                filePath,
                fields => new DpBase
                {
                    EffDate = fields["EFF_DATE"],
                    DpName = fields["DP_NAME"],
                    AmendmentNo = fields["AMENDMENT_NO"],
                    Artcc = fields["ARTCC"],
                    DpAmendEffDate = fields["DP_AMEND_EFF_DATE"],
                    RnavFlag = fields["RNAV_FLAG"],
                    GraphicalDpType = fields["GRAPHICAL_DP_TYPE"],
                    ServedArpt = fields["SERVED_ARPT"],
                });

            return result;
        }

        public DpCsvDataCollection ParseDpRte(string filePath)
        {
            var result = new DpCsvDataCollection();

            result.DpRte = FebCsvHelper.ProcessLines(
                filePath,
                fields => new DpRte
                {
                    EffDate = fields["EFF_DATE"],
                    DpName = fields["DP_NAME"],
                    Artcc = fields["ARTCC"],
                    DpComputerCode = fields["DP_COMPUTER_CODE"],
                    RoutePortionType = fields["ROUTE_PORTION_TYPE"],
                    RouteName = fields["ROUTE_NAME"],
                    RteBodySeq = FebCsvHelper.ParseInt(fields["BODY_SEQ"]),
                    TransitionComputerCode = fields["TRANSITION_COMPUTER_CODE"],
                    PointSeq = FebCsvHelper.ParseInt(fields["POINT_SEQ"]),
                    Point = fields["POINT"],
                    IcaoRegionCode = fields["ICAO_REGION_CODE"],
                    PointType = fields["POINT_TYPE"],
                    NextPoint = fields["NEXT_POINT"],
                    ArptRwyAssoc = fields["ARPT_RWY_ASSOC"],
                });

            return result;
        }

    }

    public class DpCsvDataCollection
    {
        public List<DpApt> DpApt { get; set; } = new();
        public List<DpBase> DpBase { get; set; } = new();
        public List<DpRte> DpRte { get; set; } = new();
    }
}
