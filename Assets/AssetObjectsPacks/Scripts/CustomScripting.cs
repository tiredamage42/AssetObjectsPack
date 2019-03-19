using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace AssetObjectsPacks {
    public static class CustomScripting
    {
        public static List<Vector2Int> ParsePairs(string text, string pairChars, string debugName) {
            
            List<Vector2Int> r = new List<Vector2Int>();
            
            var lastStartPos = -1;
            
            int l = text.Length;
            
            for (int i = 0; i < l; i++) {
            
                if (text[i] == pairChars[0]) {
                    lastStartPos = i;
                }
                else if (text[i] == pairChars[1]) {
                    if (lastStartPos == -1) {
                        Debug.LogError(string.Format(debugName + ":: mismatched " + pairChars[1] + " at index", i));
                        return null;
                    }
                    else {
                        r.Add( new Vector2Int(lastStartPos, i) );
                        lastStartPos = -1;
                    } 
                }
            } 
            if (lastStartPos != -1) {
                Debug.LogError(string.Format(debugName + ":: mismatched " + pairChars[0] + " at index", lastStartPos));
                return null;
            }
            return r;
        }

        public static string StripAllSpace(string orig) {
            return System.Text.RegularExpressions.Regex.Replace(orig, @"\s+", string.Empty);
        }

        public static void ExecuteMessageBlockStep (int playerLayer, EventPlayer receiver, Cue cue, Vector3 runtimePosition, Cue.MessageEvent messageEvent) {    
            string stepBlock = cue.GetMessageBlock(messageEvent);
            stepBlock = StripAllSpace(stepBlock);
            if (stepBlock == null || stepBlock.IsEmpty()) {
                return;
            }
            ExecuteMessageBlock (playerLayer, receiver, stepBlock, cue, runtimePosition);
        }

        static void ExecuteMessageBlock (int playerLayer, EventPlayer receiver, string messageBlock, Cue cue, Vector3 runtimePosition) {
            if (!messageBlock.Contains(";")) {
                Debug.LogError(cue.name + " :: missing ';' in message block");
                return;
            }
            string[] individualMessages = messageBlock.Split(';');
            int l = individualMessages.Length - 1;
            for (int i = 0; i < l; i++) {
                //if (individualMessages[i].IsEmpty()) {
                //    continue;
                //}
                BroadcastMessage(playerLayer, receiver, individualMessages[i], cue, runtimePosition);
            }
        }

        const string cuePositionParamString = "cueposition";
        const string cueParamString = "cue";
        const string nullParamString = "null";
        const string sFalse = "false", sTrue = "true";
   
        static object ParamFromString(string paramString, Cue cue, Vector3 runtimePosition) {
            string lower = paramString.ToLower();

            if (lower == cuePositionParamString) return runtimePosition;
            else if (lower == cueParamString) return cue;
            else if (lower == nullParamString) return null;
            else if (lower == sFalse || lower == sTrue) return bool.Parse(lower);
                
            else if (lower.Contains("'")) {
                var pairs = ParsePairs(lower, "''", cue.name);
                if (pairs == null) return null;
                if (pairs.Count > 1) Debug.LogError(cue.name + " :: More than one string in value");
                var p = pairs[0];
                return lower.Substring(p.x + 1, (p.y - 1) - p.x);
            }
            else if (lower.Contains(".")) {
                return float.Parse(lower);
            }
            else {
                int r;
                if (int.TryParse(lower, out r)) {
                    return r;
                }
                return null;
            }
        }

        static void BroadcastMessage (int playerLayer, EventPlayer receiver, string message, Cue cue, Vector3 runtimePosition) {
            string[] split = message.Split('(');
            string msgName = split[0];
            string paramsS = split[1];
            
            int l = paramsS.Length;
            int lastIndex = l - 1;
            if (lastIndex == 0) {
                receiver.SendMessage(msgName, SendMessageOptions.RequireReceiver);
                return;
            }

            string parmChecks = paramsS.Substring(0, lastIndex);
            string[] paramStrings = parmChecks.Contains(",") ? parmChecks.Split(',') : new string[] { parmChecks };

            object[] parameters = new object[paramStrings.Length + 1];
            parameters[0] = playerLayer;
            for (int i = 1; i < parameters.Length; i++) {
                parameters[i] = ParamFromString(paramStrings[i - 1], cue, runtimePosition);
            }
            
            //object[] parameters = paramStrings.Length.Generate(i => ParamFromString(paramStrings[i], cue, runtimePosition)).ToArray(); 
            //Debug.Log("sendign mesage " + msgName + " / " + parameters.Length);
            //Debug.Log(receiver.name + " / " + parameters[0]);
            receiver.SendMessage(msgName, parameters, SendMessageOptions.RequireReceiver);
        }       


        static bool ParamStringMet (string parameter, Dictionary<string, CustomParameter> paramsCheck, bool debug, string debugName) {

            string paramName, valueString;
            CompareMode compareMode = GetCompareMode(parameter, out paramName, out valueString);
            
            CustomParameter check;
            if (!paramsCheck.TryGetValue(paramName, out check)) {
                check = null;
            }

            bool matchesParameter = false;
            if (check != null) {
                matchesParameter = MatchesParameter(check, compareMode, valueString, debug);           
            }

            if (check == null || !matchesParameter) {
                if (debug) {
                    string debugMsg = "parameter failed" + parameter;
                    if (check == null) {
                        debugMsg += " :: name '"+ paramName +"' does not exist";
                    }
                    Debug.Log(debugMsg);
                }
                return false;
            }
            return true;
        }

        public enum CompareMode {
            Equals, NotEquals, MoreThan, LessThan, MoreThanOrEqual, LessThenOrEqual
        }
        static bool CheckForCompare(string input, string compareCheck, out string paramName, out string valueString) {
            if (input.Contains(compareCheck)) {
                string[] split = input.Replace(compareCheck, "@").Split('@');
                
                valueString = split.Last();
                paramName = split[0];
                
                return true;
            }
            valueString = paramName = null;
            return false;
            
        }

         static CompareMode GetCompareMode(string full, out string paramName, out string valueString) {
 
            if (CheckForCompare(full, "==", out paramName, out valueString)) return CompareMode.Equals;
            else if (CheckForCompare(full, "!=", out paramName, out valueString)) return CompareMode.NotEquals;
            else if (CheckForCompare(full, "<=", out paramName, out valueString)) return CompareMode.LessThenOrEqual;
            else if (CheckForCompare(full, ">=", out paramName, out valueString)) return CompareMode.MoreThanOrEqual;
            else if (CheckForCompare(full, "<", out paramName, out valueString)) return CompareMode.LessThan;
            else if (CheckForCompare(full, ">", out paramName, out valueString)) return CompareMode.MoreThan;
            
            paramName = full;
            valueString = full;
            return CompareMode.Equals;
        }

       
        static bool MatchesParameter(CustomParameter parameter, CompareMode compareMode, string valueStrig, bool debug){
            switch ( parameter.paramType ) {
                case CustomParameter.ParamType.IntValue:
                {
                    int checkVal = int.Parse(valueStrig);
                    int intValue = parameter.GetValue<int>();

                    if (debug) {
                        Debug.Log("Params value: " + intValue + " " + compareMode.ToString() + " check value: " + checkVal);
                    }
                    switch (compareMode) {
                        case CompareMode.Equals: return intValue == checkVal;
                        case CompareMode.NotEquals: return intValue != checkVal;
                        case CompareMode.MoreThan: return intValue > checkVal;
                        case CompareMode.MoreThanOrEqual: return intValue >= checkVal;
                        case CompareMode.LessThan: return intValue < checkVal;
                        case CompareMode.LessThenOrEqual: return intValue <= checkVal;
                    }
                    return false;
                }
                case CustomParameter.ParamType.FloatValue:
                {
                    float checkVal = float.Parse(valueStrig);
                    float floatValue = parameter.GetValue<float>();
                    if (debug) {
                        Debug.Log("Params value: " + floatValue + " " + compareMode.ToString() + " check value: " + checkVal);
                    }
                    
                    switch (compareMode) {
                        case CompareMode.Equals: return floatValue == checkVal;
                        case CompareMode.NotEquals: return floatValue != checkVal;
                        case CompareMode.MoreThan: return floatValue > checkVal;
                        case CompareMode.MoreThanOrEqual: return floatValue >= checkVal;
                        case CompareMode.LessThan: return floatValue < checkVal;
                        case CompareMode.LessThenOrEqual: return floatValue <= checkVal;
                    }
                    return false;
                }
                case CustomParameter.ParamType.BoolValue:
                {

                    bool boolValue = parameter.GetValue<bool>();
                    if (parameter.name == valueStrig) {
                        return boolValue;
                    }
                    else {
                        bool checkVal = bool.Parse(valueStrig);
                        switch (compareMode) {
                            case CompareMode.Equals: return boolValue == checkVal;
                            case CompareMode.NotEquals: return boolValue != checkVal;
                        }
                    }
                    return false;
                }
                case CustomParameter.ParamType.StringValue:{
                    string stringVal = parameter.GetValue<string>();
                    string checkVal = valueStrig;
                    switch (compareMode) {
                        case CompareMode.Equals: return stringVal == checkVal;
                        case CompareMode.NotEquals: return stringVal != checkVal;
                    }
                    return false;
                }
            }
            return true; 
        }
        static bool CheckPossibleParam(MiniStatementBlock statement, Dictionary<string, CustomParameter> paramChecks, bool debug, string debugName) {
            bool val = statement.isParenthesisBlock ? BlockMet(statement.block, paramChecks, debug, debugName) : ParamStringMet(statement.block, paramChecks, debug, debugName);
            return statement.negated ? !val : val;
        }


        struct MiniStatementBlock {
            public bool isParenthesisBlock, negated;
            public string block;
            public MiniStatementBlock(string block, bool isParenthesisBlock, bool negated) {
                this.block = block;
                this.isParenthesisBlock = isParenthesisBlock;
                this.negated = negated;
            }
        }

        static bool BlockMet (string input, Dictionary<string, CustomParameter> paramsCheck, bool debug, string debugName) {
        
            List<MiniStatementBlock> parenthesisSeperated = new List<MiniStatementBlock>();
            List<Vector2Int> pairs = ParsePairs(input, "()", debugName);
            
            if (pairs.Count == 0) {
                parenthesisSeperated.Add(new MiniStatementBlock(input, false, false));
            }
            else {
                int startIndex = 0;

                foreach (var p in pairs) {
                    int pStartIndex = p.x;
                    int pEndIndex = p.y;
                    int start = pStartIndex;
                    int dif = start - startIndex;
                    if (dif > 0){   
                        parenthesisSeperated.Add(new MiniStatementBlock(input.Substring(startIndex, dif), false, false));
                    }

                    bool negate = input[pStartIndex - 1] == '!';

                    parenthesisSeperated.Add(new MiniStatementBlock(input.Substring( pStartIndex + 1, (pEndIndex - 1) - pStartIndex ), true, negate));
                    startIndex = pEndIndex + 1;
                }
                if (startIndex != input.Length) {
                    parenthesisSeperated.Add(new MiniStatementBlock(input.Substring(startIndex, input.Length - startIndex), false, false));
                }
            }

            List<MiniStatementBlock> checkParams = new List<MiniStatementBlock>();
            
            List<bool> checkModeAnd = new List<bool>(); 
            
            for (int i = 0; i < parenthesisSeperated.Count; i++) {

                if (parenthesisSeperated[i].isParenthesisBlock) {
                    checkParams.Add(parenthesisSeperated[i]);
                    continue;
                }

                string block = parenthesisSeperated[i].block;

                int lastCheckParamEndIndex = 0;

                int x = 0;
                while (x < block.Length) {
                    char c = block[x];

                    //got to or or and
                    if (c == '|' || c == '&') {
                        if (x == 0) {
                            Debug.LogError(debugName + " :: '" + c + c + "' defined prematurely");
                            return false;
                        }

                        bool negate = block[lastCheckParamEndIndex] == '!';
                        if (negate) {
                            lastCheckParamEndIndex++;
                        }

                        string blockBefore = block.Substring(lastCheckParamEndIndex, x - lastCheckParamEndIndex);

                        checkParams.Add(new MiniStatementBlock(blockBefore, false, negate));
                        lastCheckParamEndIndex = x + 2;
                        x++;

                        checkModeAnd.Add( c == '&' );
                    }

                    if (x == block.Length - 1) {
                        if (block.Length - lastCheckParamEndIndex > 0) {
                            bool negate = block[lastCheckParamEndIndex] == '!';
                            if (negate) {
                                lastCheckParamEndIndex++;
                            }
                            int length = block.Length - lastCheckParamEndIndex;
                            checkParams.Add(new MiniStatementBlock(block.Substring(lastCheckParamEndIndex, length), false, negate));
                        }
                    }
                    x++;
                }
            }

            bool met = CheckPossibleParam(checkParams[0], paramsCheck, debug, debugName);

            bool lastCheckAnd = checkModeAnd.Count > 0 ? checkModeAnd[0] : true;
                
            for (int i = 1; i < checkParams.Count; i+=1) {
                if ((lastCheckAnd && met) || (!lastCheckAnd && !met)) met = CheckPossibleParam(checkParams[i], paramsCheck, debug, debugName);
                if (i < checkParams.Count -1) lastCheckAnd = checkModeAnd[i];
            }
            return met;
        }

        public static bool StatementValue(string statement, Dictionary<string, CustomParameter> paramsCheck, bool debug, string debugName) {
            string input = StripAllSpace(statement);
            if (input.IsEmpty()) return true;
            if (debug) Debug.Log(debugName + " :: Start: " + input);
            return BlockMet(input, paramsCheck, debug, debugName);
        }
    }
}
