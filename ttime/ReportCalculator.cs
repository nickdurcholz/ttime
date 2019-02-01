using System;
using System.Collections.Generic;
using LiteDB;

namespace ttime
{
    public class ReportCalculator
    {
        private readonly LiteDatabase _db;
        private readonly ReportingPeriod _period;
        private readonly DateTime _fromDate;
        private readonly DateTime _toDate;
        private readonly List<string> _tags;

        public ReportCalculator(LiteDatabase db, ReportingPeriod period, DateTime fromDate, DateTime toDate, List<string> tags)
        {
            _db = db;
            _period = period;
            _fromDate = fromDate;
            _toDate = toDate;
            _tags = tags;
        }

        public Report CreateReport()
        {
            throw new NotImplementedException();
        }
    }
}