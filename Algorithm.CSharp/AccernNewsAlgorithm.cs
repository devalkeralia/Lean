/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); 
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using System;
using NodaTime;
using QuantConnect.Data;
using QuantConnect.Data.Custom;

namespace QuantConnect.Algorithm.CSharp
{
    /// <summary>
    /// Accern news data demonstration
    /// </summary>
    public class AccernNewsAlgorithm : QCAlgorithm
    {
        /// <summary>
        /// Add the Accern type to our algorithm and use its News Data.
        /// </summary>
        public override void Initialize()
        {
            SetStartDate(2012, 09, 26);  //Set Start Date
            SetEndDate(2012, 11, 27);    //Set End Date
            SetCash(100000);             //Set Strategy Cash
            AddData<Accern>("AAPL", Resolution.Second, DateTimeZone.Utc);
        }

        private int _sliceCount;
        public override void OnData(Slice slice)
        {
            var symbol = QuantConnect.Symbol.Create("AAPL", SecurityType.Base, Market.USA);
            if (!slice.ContainsKey(symbol)) return;

            var result = slice.Get<Accern>();
            Console.WriteLine("SLICE >> {0} : {1}", _sliceCount++, result[symbol]);
        }

        /// <summary>
        /// Trigger an event on a complete calendar event which has an actual value.
        /// </summary>
        private int _eventCount;
        public void OnData(Accern accernData)
        {
            Console.WriteLine("ONDATA >> {0}: {1}", _eventCount++, accernData);
        }

    }
}
