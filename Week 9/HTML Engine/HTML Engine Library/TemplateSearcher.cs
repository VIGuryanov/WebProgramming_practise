namespace HTML_Engine_Library
{
    internal static class TemplateSearcher
    {
        internal static (int, int)? FindForeachBlock(string template) =>FindBlock(template, "foreach");

        internal static (int, int)? FindIfBlock(string template)=>FindBlock(template, "if");

        internal static (int, int)? FindForBlock(string template)=>FindBlock(template, "for index");

        static (int, int)? FindBlock(string input, string begins)
        {
            var ind1 = input.IndexOf($"{{{{{begins}");
            if(ind1 == -1)
                return null;

            var parPairing = 1;
            for(int i=ind1+3+begins.Length;i< input.Length;i++)
            {
                var l = input[(i-1)..(i+1)];
                if (input[(i-1)..(i+1)] == "{{")
                {
                    parPairing++;
                    i+=1;
                }
                else if(input[(i-1)..(i+1)] == "}}")
                {
                    parPairing--;
                    i+=1;
                }
                if(parPairing == 0)
                    return(ind1, i);
            }
            return null;
        }

        internal static int? FindElsePos(string ifBlock)
        {         
            var beginFrom = ifBlock.IndexOf("then:");
            if(beginFrom == -1)
                throw new FormatException("No {then:} in if block}");

            var pairing = 1;

            for(int i=beginFrom+5;i<ifBlock.Length;i++)
            {
                var j = ifBlock[i..(i+5)];
                if (ifBlock[i..(i+5)]=="then:")
                { 
                    pairing++;
                    i+=4;
                }
                else if(ifBlock[i..(i+5)]=="else:")
                {
                    pairing--;
                    i+=4;
                }
                if(pairing == 0)
                {
                    return(i-=4);
                }
            }
            return null;
        }

        internal static List<string> FindAllBlocks(string template)
        {
            var res = new List<string>();

            var stack = new Stack<int>();

            var ind1 = template.IndexOf("{{");
            if(ind1 == -1)
                return res;

            stack.Push(ind1);

            var parPairing = 1;
            for(int i=ind1+2;i< template.Length;i++)
            {
                var l = template[(i-1)..(i+1)];
                if (template[(i-1)..(i+1)] == "{{")
                {
                    parPairing++;
                    stack.Push(i-1);
                    i+=1;
                }
                else if(template[(i-1)..(i+1)] == "}}")
                {
                    parPairing--;

                    var left = stack.Pop();
                    res.Add(template.Substring(left, i-left+1));
                    i+=1;
                }
                if(parPairing == 0)
                {
                    ind1 = template.Substring(i).IndexOf("{{");
                    if(ind1 == -1)
                        return res;

                    stack.Push(ind1);
                }
            }
            return res;
        }
    }
}
