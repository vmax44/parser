using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Vmax44Parser.library;

namespace ParserLibrary
{
    public class ParsersManager
    {
        private Dictionary<int, string> AvailableParsers;

        public ParsersManager()
        {
            AvailableParsers=new Dictionary<int,string>();
            AvailableParsers.Add(2, "zappost.ru");
            AvailableParsers.Add(3, "autodoc.ru");
            AvailableParsers.Add(4, "exist.ru");
        }

        public Dictionary<int, string> GetAvailableParsers()
        {
            return AvailableParsers;
        }

        public string GetParserName(int id)
        {
            return AvailableParsers[id];
        }

        public IParser GetParserById(int id)
        {
            IParser parser=null;
            switch(id)
            {
                case 2: parser = new ParserZapPost();
                    break;
                case 3: parser = new ParserAutodoc();
                    break;
                case 4: parser = new ParserExist();
                    break;
            }
            return parser;
        }

        public List<IParser> GetParsersByArrayIds(int[] ids)
        {
            List<IParser> result = new List<IParser>();
            foreach (int id in ids)
            {
                result.Add(GetParserById(id));
            }
            return result;
        }
    }
}
