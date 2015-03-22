using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vmax44ParserConnectedLayer;

namespace Vmax44Parser
{
    abstract public class Parser: IDisposable
    {
        public string ParserType;
        public SelectFromStringList GetSelectedManufacturer;

        public abstract ParsedDataCollection detailParse(string detailCode);

        abstract public void Dispose();
    }
}
