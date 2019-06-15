using System;
using System.Collections.Generic;
using System.Linq;

namespace BatisSuperHelper.Actions.ActionValidators
{
    public class FunctionBasedActionValidator : IActionValidator
    {
        private readonly Dictionary<string, List<Func<int, bool>>> _toExecute = new Dictionary<string, List<Func<int, bool>>>();

        public FunctionBasedActionValidator()
        {

        }

        public FunctionBasedActionValidator(Dictionary<string, List<Func<int, bool>>> functionDictionary)
        {
            _toExecute = functionDictionary;
        }

        public FunctionBasedActionValidator WithFunction(string target, Func<int, bool> function)
        {
            var funcList = new List<Func<int, bool>>();
            if (!_toExecute.ContainsKey(target))
            {
                _toExecute.Add(target, funcList);
            }

            funcList = _toExecute[target];
            funcList.Add(function);

            return this;
        }

        public FunctionBasedActionValidator WithFunctionList(string target, List<Func<int, bool>> functionList)
        {
            var funcList = new List<Func<int, bool>>();
            if (!_toExecute.ContainsKey(target))
            {
                _toExecute.Add(target, funcList);
            }

            funcList = _toExecute[target];
            funcList.AddRange(functionList);

            return this;
        }

        public bool CanJumpToQueryInLine(int lineNumber)
        {
            return _toExecute["jump"].All(e => e(lineNumber));
        }

        public bool CanRenameQueryInLin(int lineNumber)
        {
            return _toExecute["rename"].All(e => e(lineNumber));
        }
    }
}
