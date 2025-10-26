using FlatTrade;
using FlatTrade.Common.Types.Base;
using StrategyEngine.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StrategyEngine
{
    internal interface IOrderProcessor
    {
        internal Task CancelOrder(CancelOrder cancelOrder);
        internal Task ModifyOrder(ModifyOrder modifyOrder);
        internal Task CreateOrder(CreateOrder createOrder);        
    }
}
