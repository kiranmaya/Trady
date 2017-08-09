﻿using System;
using System.Collections.Generic;
using System.Linq;
using Trady.Analysis.Infrastructure;
using Trady.Core;

namespace Trady.Analysis.Pattern.Candlestick
{
    /// <summary>
    /// Reference: http://stockcharts.com/school/doku.php?id=chart_school:chart_analysis:candlestick_bearish_reversal_patterns#bearish_engulfing
    /// </summary>
    public class BullishEngulfingPattern<TInput, TOutput> : AnalyzableBase<TInput, (decimal Open, decimal High, decimal Low, decimal Close), bool?, TOutput>
    {
        DownTrendByTuple _downTrend;
        BearishByTuple _bearish;
        BullishByTuple _bullish;

        public BullishEngulfingPattern(IEnumerable<TInput> inputs, Func<TInput, (decimal Open, decimal High, decimal Low, decimal Close)> inputMapper, Func<TInput, bool?, TOutput> outputMapper, int downTrendPeriodCount = 3) : base(inputs, inputMapper, outputMapper)
        {
            var mappedInputs = inputs.Select(inputMapper);
            _downTrend = new DownTrendByTuple(mappedInputs.Select(i => (i.High, i.Low)), downTrendPeriodCount);

            var ocs = mappedInputs.Select(i => (i.Open, i.Close));
            _bearish = new BearishByTuple(ocs);
            _bullish = new BullishByTuple(ocs);

            DownTrendPeriodCount = downTrendPeriodCount;
        }

        public int DownTrendPeriodCount { get; private set; }

        protected override bool? ComputeByIndexImpl(IEnumerable<(decimal Open, decimal High, decimal Low, decimal Close)> mappedInputs, int index)
        {
			if (index < 1) return null;
            bool isEngulf = mappedInputs.ElementAt(index).Open < mappedInputs.ElementAt(index - 1).Close && mappedInputs.ElementAt(index).Close > mappedInputs.ElementAt(index - 1).Open;
			return (_downTrend[index - 1] ?? false) && _bearish[index - 1] && _bullish[index] && isEngulf;
        }
    }

    public class BullishEngulfingPatternByTuple : BullishEngulfingPattern<(decimal Open, decimal High, decimal Low, decimal Close), bool?>
    {
        public BullishEngulfingPatternByTuple(IEnumerable<(decimal Open, decimal High, decimal Low, decimal Close)> inputs, int downTrendPeriodCount = 3) 
            : base(inputs, i => i, (i, otm) => otm, downTrendPeriodCount)
        {
        }
    }

    public class BullishEngulfingPattern : BullishEngulfingPattern<Candle, AnalyzableTick<bool?>>
    {
        public BullishEngulfingPattern(IEnumerable<Candle> inputs, int downTrendPeriodCount = 3)
            : base(inputs, i => (i.Open, i.High, i.Low, i.Close), (i, otm) => new AnalyzableTick<bool?>(i.DateTime, otm), downTrendPeriodCount)
        {
        }
    }
}