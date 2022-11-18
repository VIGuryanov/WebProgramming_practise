using Microsoft.CSharp.RuntimeBinder;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Text.RegularExpressions;

namespace HTML_Engine_Library
{
    internal static class TemplateParser//Eternal model have highter priority in variabilityes. Possible solution - ignore foreach blocks in variability set
    {
        public static string ProcessMethods(string template, List<object> models)//start from for unrecognize forech skip
        {
            if (models.Count == 0)
                throw new ArgumentException("Empty model!");

            template = ProcessFor(template, models);
            template = ProcessForeach(template, models);
            template = ProcessVariability(template, models);
            template = ProcessCallingFunctions(template, models);
            return ProcessIf(template);
        }

        static string ProcessFor(string template, List<object> models)
        {
            var forBlockBorders = TemplateSearcher.FindForBlock(template);
            if (forBlockBorders is null)
                return template;

            var forBlock = template[forBlockBorders.Value.Item1..forBlockBorders.Value.Item2];

            var forParams = Regex.Match(forBlock, @"for\s{1}index\s{1}(.)*\s{1}to\s{1}(.)*\s{1}step\s{1}(.)*\s*");
            if (!forParams.Success)
                throw new FormatException("Invalid forindex syntax");

            var forLine =ProcessVariability(forParams.Value,models);
            forLine = ProcessCallingFunctions(forLine, models);

            int forInit = int.Parse(Regex.Match(forLine, @"for\s{1}index\s{1}(\d)*\s{1}to").Value.SkipLast(2).Skip(9).ToArray());
            int forMax = int.Parse(Regex.Match(forLine, @"to\s{1}(\d)*\s{1}step").Value.SkipLast(4).Skip(2).ToArray());
            int forStep = int.Parse(Regex.Match(forLine, @"step\s{1}(\d)*\s*").Value.Skip(4).ToArray());

            var replacement = new StringBuilder("");

            var sample = forBlock.Remove(forBlock.Length - 2, 2).Remove(0, 2 + forParams.Length);
            for(int i=forInit;i<forMax;i+=forStep)
                replacement.Append(sample.Replace("@index", i.ToString()));

            return ProcessFor(template.ReplaceFirst(forBlock, replacement.ToString()), models);
        }

        static string ProcessForeach(string template, List<object> models)
        {
            var foreachBlockBorders = TemplateSearcher.FindForeachBlock(template);
            if (foreachBlockBorders is null)
                return template;

            var foreachblock = template[foreachBlockBorders.Value.Item1..foreachBlockBorders.Value.Item2];

            var modelpath = Regex.Match(foreachblock.Remove(0, 13), "^.*").Value.TrimEnd();

            if (modelpath.Length == 0)
                throw new FormatException("Invalid foreach");
            var pathparts = modelpath.Split('.');

            var target = models[^1];
            for (int i = 0; i < pathparts.Length; i++)
            {
                var step1 = target.GetType().GetProperty(pathparts[i]);
                if (step1 == null)
                    return ProcessMethods(template.ReplaceFirst(foreachblock, ProcessMethods(foreachblock, models.Take(models.Count - 1).ToList())), models);

                target = step1.GetValue(target);
                if (target == null)
                    return ProcessMethods(template.ReplaceFirst(foreachblock, ProcessMethods(foreachblock, models.Take(models.Count - 1).ToList())), models);//Skip unrecognized foreach and start processing possible next foreach after it
            }

            var local = new StringBuilder();
            if (target is IEnumerable<object> t)
                foreach (var e in t)
                    local.Append(ProcessMethods(foreachblock.Remove(foreachblock.Length - 2, 2).Remove(0, 13 + modelpath.Length), models.Append(e).ToList()));
            else throw new InvalidOperationException("Foreach unavailable");

            return ProcessMethods(template.ReplaceFirst(foreachblock, local.ToString()), models);
        }

        static string ProcessVariability(string template, List<object> models)
        {
            template = template.Replace("{{this}}", models[^1].ToString());

            for (int i = models.Count - 1; i >= 0; i--)
            {
                foreach (var property in models[i].GetType().GetProperties().Where(x => !x.GetIndexParameters().Any()))
                {
                    template = template.Replace(new string("{{" + $"{property.Name}" + "}}"), new string($"{property.GetValue(models[i])}"));

                    var indexed = Regex.Matches(template, new string("{{" + $@"{property.Name}(\[\d*\])+" + "}}"));
                    for (int j = 0; j < indexed.Count; j++)
                    {
                        var value = indexed[j].Value;
                        TryGetByIndex(value, models[i], out dynamic res);
                        template = template.Replace(value, new string($"{res}"));
                    }
                }

                if (!template.Contains("{{"))
                    break;
            }

            return template;
        }

        static bool TryGetByIndex(string toParse, object model, out dynamic res)
        {
            var indexParts = Regex.Matches(toParse, @"\[\d*\]");
            if (indexParts.Count == 0)
            {
                res = null;
                return false;
            }

            var propertyName = toParse.Remove(toParse.IndexOf('[')).Replace("{{", "");

            try
            {
                if(propertyName == "this")
                    res = model;
                else
                    res = model.GetType().GetProperty(propertyName).GetValue(model);
                for (int i = 0; i < indexParts.Count; i++)
                {
                    var index = int.Parse(indexParts[i].Value.Except(new[] { '[', ']' }).ToArray());
                    res = res[index];
                }
            }
            catch (RuntimeBinderException ex)
            {
                throw new InvalidOperationException($"Error during getting by index of '{toParse}'");
            }
            return true;
        }

        static string ProcessCallingFunctions(string template, List<object> models)
        {
            var leavedUnparsed = TemplateSearcher.FindAllBlocks(template);

            for (int i = 0; i < leavedUnparsed.Count; i++)
            {
                var val = leavedUnparsed[i];
                if (!Regex.Match(val, "{{.*}}").Success || TemplateSearcher.FindForeachBlock(val) != null || TemplateSearcher.FindIfBlock(val) != null)
                    continue;
                val = val.Remove(val.Length - 2, 2).Remove(0, 2);

                var pointSplited = val.Split('.').Where(x => x != "this").ToArray();

                foreach (var model in (models as IEnumerable<object>).Reverse())
                {
                    object? target;
                    if (Regex.Match(pointSplited[0], @"\[.*\]").Success && TryGetByIndex(pointSplited[0], model, out dynamic res))
                        target = res;
                    else
                    {
                        var possibleProperty = model.GetType().GetProperties().Where(x => x.Name == pointSplited[0]).FirstOrDefault();
                        if (possibleProperty == null)
                            continue;
                        target = possibleProperty.GetValue(model);
                    }

                    for (int j = 1; j < pointSplited.Length; j++)
                    {
                        if (Regex.Match(pointSplited[j], @"\[.*\]").Success && TryGetByIndex(pointSplited[j], target, out dynamic res2))
                            target = res2;
                        else
                        {
                            var step1 = target.GetType().GetProperty(pointSplited[j]);

                            if (step1 != null)
                            {
                                target = step1.GetValue(target);
                                continue;
                            }
                            target = BuiltInMethodsExecutor.TryExecute(target, pointSplited[j]);
                        }

                    }
                    if (target != null)
                    {
                        template = template.Replace($"{{{{{val}}}}}", target.ToString());

                        for (int j = i + 1; j < leavedUnparsed.Count; j++)//optimization
                            leavedUnparsed[j] = leavedUnparsed[j].Replace($"{{{{{val}}}}}", target.ToString());

                        break;
                    }
                }
            }
            return template;
        }

        static string ProcessIf(string template)
        {
            var ifBlockBorders = TemplateSearcher.FindIfBlock(template);
            if (ifBlockBorders is null)
                return template;

            var ifblock = template[ifBlockBorders.Value.Item1..ifBlockBorders.Value.Item2];

            var condition = Regex.Match(ifblock, @"\(.*\)").Value;
            condition = condition.Remove(condition.Length - 1, 1).Remove(0, 1).Replace(" ", "");

            var ifRes = ProcessCondition(condition);

            var elsePos = TemplateSearcher.FindElsePos(ifblock);
            elsePos ??= ifBlockBorders.Value.Item2 - 2;
            var notNullElsePos = (int)elsePos;

            if (ifRes)
                template = template.ReplaceFirst(ifblock, template[(ifblock.IndexOf("then:") + ifBlockBorders.Value.Item1 + 5)..(notNullElsePos + ifBlockBorders.Value.Item1)].ReplaceFirst("\t\t", ""));
            else
                template = template.ReplaceFirst(ifblock, template[(notNullElsePos + ifBlockBorders.Value.Item1 + 5)..(ifBlockBorders.Value.Item2 - 2)]);

            return ProcessIf(template);
        }

        static bool ProcessCondition(string condition)
        {
            (int, int)? parBorders = (0, 0);
            while (parBorders != null)
            {
                parBorders = condition.FindPar();
                if (parBorders != null)
                {
                    var segm = condition[parBorders.Value.Item1..(parBorders.Value.Item2 + 1)];
                    condition = condition.ReplaceFirst(segm, ProcessCondition(segm.Remove(segm.Length - 1, 1).Remove(0, 1)).ToString());
                }
            }

            if (bool.TryParse(condition, out bool result)) return result;

            if (TryParseLogicOperator(condition.Split("||"), "||", out result)) return result;
            if (TryParseLogicOperator(condition.Split("&&"), "&&", out result)) return result;
            if (TryParseLogicOperator(condition.Split("|"), "|", out result)) return result;
            if (TryParseLogicOperator(condition.Split("&"), "&", out result)) return result;

            if (TryCompare(condition.Split(">="), ">=", out result)) return result;
            if (TryCompare(condition.Split("<="), "<=", out result)) return result;
            if (TryCompare(condition.Split(">"), ">", out result)) return result;
            if (TryCompare(condition.Split("<"), "<", out result)) return result;
            if (TryCompare(condition.Split("="), "=", out result)) return result;
            if (TryCompare(condition.Split("!="), "!=", out result)) return result;

            throw new FormatException("Unexpected symbol");
        }

        static bool TryParseLogicOperator(string[] param, string oper, out bool result)
        {
            if (param.Length == 1)
            {
                result = false;
                return false;
            }

            result = ProcessCondition(param[0]);
            for (int i = 1; i < param.Length; i++)
                switch (oper)
                {
                    case "|":
                        result |= ProcessCondition(param[i]);
                        break;
                    case "||":
                        if (result == true)
                            return true;
                        result |= ProcessCondition(param[i]);
                        break;
                    case "&":
                        result &= ProcessCondition(param[i]);
                        break;
                    case "&&":
                        if (result == false)
                            return true;
                        result &= ProcessCondition(param[i]);
                        break;
                    default: throw new InvalidOperationException("Unsupported operator");
                };
            return true;
        }

        static bool TryCompare(string[] param, string oper, out bool result)
        {
            if (param.Length == 1)
            {
                result = false;
                return false;
            }
            if (param.Length != 2)
                throw new FormatException();
            switch (oper)
            {
                case "=": result = param[0] == param[1]; break;
                case "!=": result = param[0] == param[1]; break;
                default:
                    {
                        var conv1 = double.Parse(param[0]);
                        var conv2 = double.Parse(param[1]);
                        result = oper switch
                        {
                            ">=" => conv1 >= conv2,
                            "<=" => conv1 <= conv2,
                            ">" => conv1 > conv2,
                            "<" => conv1 < conv2,
                            _ => throw new InvalidOperationException(),
                        };
                    }
                    break;
            }
            return true;
        }
    }
}
