using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace AssetObjectsPacks {
    public static class CustomScripting
    {
        static List<Vector2Int> ParsePairs(string text, string pairChars, string logErrors) {
            
            List<Vector2Int> r = new List<Vector2Int>();
            
            var lastStartPos = -1;
            
            int l = text.Length;
            
            for (int i = 0; i < l; i++) {
            
                if (text[i] == pairChars[0]) {
                    lastStartPos = i;
                }
                else if (text[i] == pairChars[1]) {
                    if (lastStartPos == -1) {

                        #if UNITY_EDITOR
                        logErrors += (":: mismatched " + pairChars[1] + " at index " + i + "\n\n");
                        #endif
                        
                        return null;
                    }
                    else {
                        r.Add( new Vector2Int(lastStartPos, i) );
                        lastStartPos = -1;
                    } 
                }
            } 
            if (lastStartPos != -1) {

                #if UNITY_EDITOR
                logErrors += (":: mismatched " + pairChars[0] + " at index " + lastStartPos + "\n\n");
                #endif
        
                return null;
            }
            return r;
        }

        static string StripAllSpace(string orig) {
            if (orig == null)
                return null;
            return System.Text.RegularExpressions.Regex.Replace(orig, @"\s+", string.Empty);
        }

        #region MESSAGE_SEND
        public static void ExecuteMessageBlock (int playerLayer, EventPlayer receiver, string messageBlock, Vector3 runtimePosition, string logErrors) {
            messageBlock = StripAllSpace(messageBlock);
            if (messageBlock == null || messageBlock.IsEmpty()) {
                return;
            }
            
            if (!messageBlock.Contains(";")) {
                #if UNITY_EDITOR
                logErrors += (" :: missing ';' in message block" + "\n\n");
                #endif
                return;
            }
            string[] individualMessages = messageBlock.Split(';');
            //skip last empty
            int l = individualMessages.Length - 1;
            for (int i = 0; i < l; i++) {
                BroadcastMessage(playerLayer, receiver, individualMessages[i], runtimePosition, logErrors);
            }
        }

        static void BroadcastMessage (int playerLayer, EventPlayer receiver, string message, Vector3 runtimePosition, string logErrors) {
            
            string[] split = message.Split('(');
            string msgName = split[0];
            string paramsS = split[1];
            
            int l = paramsS.Length;
            int lastIndex = l - 1;

            if (lastIndex == 0) {
                receiver.SendMessage(msgName, new object[] { playerLayer }, SendMessageOptions.RequireReceiver);
                return;
            }

            string allParamsString = paramsS.Substring(0, lastIndex);
            bool isMultipleParams = allParamsString.Contains(",");

            string[] parameterStrings = isMultipleParams ? allParamsString.Split(',') : new string[] { allParamsString };

            object[] parameters = new object[parameterStrings.Length + 1];
            parameters[0] = playerLayer;
            for (int i = 1; i < parameters.Length; i++) {
                parameters[i] = ParamFromString(parameterStrings[i - 1], runtimePosition, logErrors);
            }
            receiver.SendMessage(msgName, parameters, SendMessageOptions.RequireReceiver);
        } 

        const string cuePositionParamString = "position";
        const string nullParamString = "null";
        const string sFalse = "false", sTrue = "true";
   
        static object ParamFromString(string paramString, Vector3 runtimePosition, string logErrors) {
            string lower = paramString.ToLower();

            //check for position
            if (lower == cuePositionParamString) return runtimePosition;
            
            //check if null
            else if (lower == nullParamString) return null;
            
            //check if bool
            else if (lower == sFalse || lower == sTrue) return bool.Parse(lower);
            
            //check if string
            else if (lower.Contains("'")) {
                var pairs = ParsePairs(lower, "''", logErrors);
                if (pairs == null) return null;
                var p = pairs[0];
                return lower.Substring(p.x + 1, (p.y - 1) - p.x);
            }
            //check if float
            else if (lower.Contains(".")) {
                return float.Parse(lower);
            }

            //integer
            else {
                int r;
                if (int.TryParse(lower, out r)) {
                    return r;
                }
                return null;
            }
        }
        #endregion



        #region CHECK_STATEMENT_VALUE

        public static bool StatementValue(string statement, Dictionary<string, CustomParameter> paramsCheck, string logErrors, string logWarnings) {
            if (statement.IsEmpty()) return true;
            string input = StripAllSpace(statement);
            if (input.IsEmpty()) return true;

            bool statementTrue = BlockMet(input, paramsCheck, logErrors, logWarnings);
            
            #if UNITY_EDITOR
            if (!logErrors.IsEmpty()) logErrors = (" :: Start: " + input + "\n\n") + logErrors; 
            if (!logWarnings.IsEmpty()) logWarnings = (" :: Start: " + input + "\n\n") + logWarnings;
            #endif

            return statementTrue;
            
        }
        static bool BlockMet (string input, Dictionary<string, CustomParameter> paramsCheck, string logErrors, string logWarnings) {
        
            List<MiniStatementBlock> parenthesisSeperated = new List<MiniStatementBlock>();
            
            List<Vector2Int> pairs = ParsePairs(input, "()", logErrors);
            if (pairs == null) return false;
            
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
                            #if UNITY_EDITOR
                            logErrors += (" :: '" + c + c + "' defined prematurely" + "\n\n");
                            #endif
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

                    //check if leftover statement after last || or &&

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

            bool statementTrue = CheckPossibleParam(checkParams[0], paramsCheck, logErrors, logWarnings);

            bool lastCheckWasAND = checkModeAnd.Count > 0 ? checkModeAnd[0] : true;
                
            for (int i = 1; i < checkParams.Count; i+=1) {
            
                if ((lastCheckWasAND && statementTrue) || (!lastCheckWasAND && !statementTrue)) statementTrue = CheckPossibleParam(checkParams[i], paramsCheck, logErrors, logWarnings);
                if (i < checkParams.Count -1) lastCheckWasAND = checkModeAnd[i];
            }

            return statementTrue;
        }
        static bool CheckPossibleParam(MiniStatementBlock statement, Dictionary<string, CustomParameter> paramChecks, string logErrors, string logWarnings) {
            if (statement.isParenthesisBlock) {
                bool statementValue = BlockMet(statement.block, paramChecks, logErrors, logWarnings);
                return statement.negated ? !statementValue : statementValue;
            }
            else {
                string paramName, valueString;
                bool paramFound;
                object paramValue;
                bool val = ParamStringMet(statement.block, paramChecks, out paramName, out valueString, out paramValue, out paramFound);
                
                if (!paramFound) {
                    #if UNITY_EDITOR
                    logErrors += ("parameter failed" + statement.block + " :: name '"+ paramName +"' does not exist" + "\n\n");
                    #endif
                    return false;
                }

                bool final = statement.negated ? !val : val;

                #if UNITY_EDITOR
                if (!final) {
                    logWarnings += ("parameter failed" + statement.block + " :: paramVal: " + paramValue + " / checkVal: " + valueString + "\n\n" );
                }
                #endif

                return final;

            }
        }

        static bool ParamStringMet (string parameter, Dictionary<string, CustomParameter> paramsCheck, out string paramName, out string valueString, out object paramValue, out bool paramFound) {
            paramValue = null;
            CompareMode compareMode = GetCompareMode(parameter, out paramName, out valueString);
            CustomParameter check;
            paramFound = paramsCheck.TryGetValue(paramName, out check);
            return paramFound && MatchesParameter(check, compareMode, valueString, out paramValue);
        }

        public enum CompareMode { Equals, NotEquals, MoreThan, LessThan, MoreThanOrEqual, LessThenOrEqual }
        static bool CheckForCompare(string input, string compareCheck, out string paramName, out string valueString) {
            if (input.Contains(compareCheck)) {
                string[] split = input.Replace(compareCheck, "@").Split('@');
                paramName = split[0];
                valueString = split[1];
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

        static bool MatchesParameter(CustomParameter parameter, CompareMode compareMode, string valueStrig, out object paramVal){
            switch ( parameter.paramType ) {
                case CustomParameter.ParamType.IntValue:
                {
                    int checkVal = int.Parse(valueStrig);
                    int intValue = parameter.GetValue<int>();
                    paramVal = intValue;

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
                    paramVal = floatValue;

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
                    paramVal = boolValue;
                    if (parameter.name == valueStrig) {
                        return boolValue;
                    }
                    
                    bool checkVal = bool.Parse(valueStrig);
                    switch (compareMode) {
                        case CompareMode.Equals: return boolValue == checkVal;
                        case CompareMode.NotEquals: return boolValue != checkVal;
                    }
                    
                    return false;
                }
                case CustomParameter.ParamType.StringValue:{
                    string stringVal = parameter.GetValue<string>();
                    paramVal = stringVal;
                    switch (compareMode) {
                        case CompareMode.Equals: return stringVal == valueStrig;
                        case CompareMode.NotEquals: return stringVal != valueStrig;
                    }
                    return false;
                }
            }
            paramVal = null;
            return true; 
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
        #endregion
    }
}
