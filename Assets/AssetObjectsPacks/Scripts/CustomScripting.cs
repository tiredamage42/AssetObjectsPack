using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AssetObjectsPacks {

    public static class CustomScripting
    {


        /*
            //steps:

            on snap { when snap is completed } (only called when snap enabled)
            on play { when cue is ready plays }
            on end { when cue is done playing}


            on snap {
                CallMessage();
                SomethingElse(1.3, 1, 'string', false, null, cue, cueposition);
            }
            on play {
                Message(true);
            }


        */
        public const string onSnap = "onsnap", onPlay = "onplay", onEnd = "onend";

        static readonly List<string> stepNames = new List<string>() {
            onSnap, onPlay, onEnd
        };

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
                        int b = lastStartPos;
                        lastStartPos = -1;
                        r.Add( new Vector2Int(b, i) );
                    } 
                }
            } 
            
            if (lastStartPos != -1) 
            {
                Debug.LogError(string.Format(debugName + ":: mismatched " + pairChars[0] + " at index", lastStartPos));
                return null;
            }
            return r;
        }

        public static string StripAllSpace(string orig) {
            return System.Text.RegularExpressions.Regex.Replace(orig, @"\s+", string.Empty);
            //return orig.Replace(" ", string.Empty).Replace("\n", string.Empty).Trim();
        }

        static string GetMessageBlock(string messageBlocks, string stepFilter, string debugName) {
            
            messageBlocks = StripAllSpace(messageBlocks);
            if (messageBlocks.IsEmpty()) {
                return null;
            }

            IEnumerable<Vector2Int> pairs = ParsePairs(messageBlocks, "{}", debugName);
            if (pairs == null) {
                return null;
            }

            int startIndex = 0;
            foreach (var p in pairs) {
                int dif = p.x - startIndex;

                if (dif == 0) {
                    Debug.LogError(debugName + ":: no step name specified");
                    return null;
                }
                string stepName = messageBlocks.Substring(startIndex, dif).ToLower();

                if (!stepNames.Contains(stepName)) {
                    Debug.LogError(debugName + ":: Step name: " + stepName + " does not exist. // must be '" + onSnap + ", " + onPlay + ",  or " + onEnd);
                    return null;
                }

                startIndex = p.y + 1;
                if (stepName != stepFilter) {
                    continue;
                }
                return messageBlocks.Substring(p.x + 1, (p.y - 1) - p.x);
            }
            return null;
        }

        public static void ExecuteMessageBlockStep (GameObject receiver, Cue cue, Vector3 runtimePosition, string stepFilter) {
            string stepBlock = GetMessageBlock(cue.messagesBlock, stepFilter, cue.name);
            if (stepBlock == null || stepBlock.IsEmpty()) {
                return;
            }
            ExecuteMessageBlock (receiver, stepBlock, cue, runtimePosition);
        }

        static void ExecuteMessageBlock (GameObject receiver, string messageBlock, Cue cue, Vector3 runtimePosition) {
            if (!messageBlock.Contains(";")) {
                Debug.LogError(cue.name + " :: missing ';' in message block");
                return;
            }
            string[] individualMessages = messageBlock.Split(';');
            for (int i = 0; i  < individualMessages.Length; i++) {
                if (individualMessages[i].IsEmpty()) {
                    continue;
                }
                BroadcastMessage(receiver, individualMessages[i], cue, runtimePosition);
            }
        }

        const string cuePositionParamString = "cueposition";
        const string cueParamString = "cue";
        const string nullParamString = "null";
        const string sFalse = "false", sTrue = "true";
   
        static object ParamFromString(string paramString, Cue cue, Vector3 runtimePosition) {
            string lower = paramString.ToLower();

            //Debug.Log(paramString);

            if (lower == cuePositionParamString) {
              //  Debug.Log("position" + runtimePosition);
                return runtimePosition;
            }
            else if (lower == cueParamString) return cue;
            else if (lower == nullParamString) return null;
            else if (lower == sFalse || lower == sTrue) return bool.Parse(lower);
            else if (lower.Contains("'")) {
                List<Vector2Int> pairs = ParsePairs(lower, "''", cue.name);
                if (pairs == null) {
                    return null;
                }
                if (pairs.Count > 1) {
                    Debug.LogError(cue.name + " :: More than one string in value");
                }
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

        static void BroadcastMessage (GameObject receiver, string message, Cue cue, Vector3 runtimePosition) {
            string[] split = message.Split('(');
            string msgName = split[0];
            string paramsS = split[1];
            
            int l = paramsS.Length;
            if (l-1 == 0) {
                receiver.SendMessage(msgName, SendMessageOptions.RequireReceiver);
                return;
            }

            //Debug.Log(paramsS);

            string parmChecks = paramsS.Substring(0, l - 1);
//Debug.Log(parmChecks);
            

            string[] paramStrings = parmChecks.Contains(",") ? parmChecks.Split(',') : new string[] { parmChecks };
            l = paramStrings.Length;
            object[] parameters = new object[l];
            for (int i = 0; i < l; i++) parameters[i] = ParamFromString(paramStrings[i], cue, runtimePosition);
            //Debug.Log("sendign mesage " + msgName + " / " + parameters.Length);
            receiver.SendMessage(msgName, parameters, SendMessageOptions.RequireReceiver);
        }       








//param checks




        static bool ParamStringMet (string parameter, Dictionary<string, CustomParameter> paramsCheck, bool debug, string debugName) {

            string paramName, valueString;
            CompareMode compareMode = GetCompareMode(parameter, out paramName, out valueString);
            
            //string[] nameSplit = parameter.Split(':');
            //string paramName = nameSplit[0];
                
            
            CustomParameter check;
            if (!paramsCheck.TryGetValue(paramName, out check)) {
                check = null;
            }

            bool matchesParameter = false;
            if (check != null) {
                //string valueString;
                matchesParameter = MatchesParameter(check, compareMode, valueString);
                //matchesParameter = check.MatchesParameter(paramName, GetCompareMode(nameSplit[1], out valueString), valueString);
           
            }

            if (check == null || !matchesParameter) {
                if (debug) {
                    string debugMsg = "parameter failed" + parameter;
                    if (check == null) {
                        debugMsg += " :: name does not exist";
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
            if (input.Contains("==")) {
                string[] split = input.Replace(compareCheck, "@").Split('@');
                valueString = split.Last();
                paramName = split[0];
                return true;
            }
            valueString = paramName = null;
            return false;
            
        }

         static CompareMode GetCompareMode(string full, out string paramName, out string valueString) {

             
            if (CheckForCompare(full, "==", out paramName, out valueString)) {
                return CompareMode.Equals;
            }
            else if (CheckForCompare(full, "!=", out paramName, out valueString)) {
                return CompareMode.NotEquals;
            }
            else if (CheckForCompare(full, "<=", out paramName, out valueString)) {
                return CompareMode.LessThenOrEqual;
            }
            else if (CheckForCompare(full, ">=", out paramName, out valueString)) {
                return CompareMode.MoreThanOrEqual;
            }
            else if (CheckForCompare(full, "<", out paramName, out valueString)) {
                return CompareMode.MoreThan;
            }
            else if (CheckForCompare(full, ">", out paramName, out valueString)) {
                return CompareMode.LessThan;
            }
            else {
                paramName = full;
                valueString = full;
                return CompareMode.Equals;
            }
        }

       
        static bool MatchesParameter(CustomParameter parameter, CompareMode compareMode, string valueStrig){
            //if (pName != name) {
            //    Debug.LogWarning("Name Mismatch! " + pName + " / " + name);
            //    return false;
            //}
            switch ( parameter.paramType ) {
                case CustomParameter.ParamType.IntValue:
                {
                    int checkVal = int.Parse(valueStrig);
                    int intValue = parameter.GetValue<int>();
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
                    //return valueStrig == GetValue<string>();
                }
            }
            return true; 
        }
    static bool CheckPossibleParam(MiniStatementBlock statement, Dictionary<string, CustomParameter> paramChecks, bool debug, string debugName) {
    //static bool CheckPossibleParam(bool isParenthesisBlock, string paramBlock, Dictionary<string, CustomParameter> paramChecks, bool debug) {
    
            bool val = statement.isParenthesisBlock ? BlockMet(statement.block, paramChecks, debug, debugName) : ParamStringMet(statement.block, paramChecks, debug, debugName);
            return statement.negated ? !val : val;
        }


        struct MiniStatementBlock {
            public bool isParenthesisBlock;
            public string block;

            public bool negated;

            public MiniStatementBlock(string block, bool isParenthesisBlock, bool negated) {
                this.block = block;
                this.isParenthesisBlock = isParenthesisBlock;
                this.negated = negated;
            }
        }

        static bool BlockMet (string input, Dictionary<string, CustomParameter> paramsCheck, bool debug, string debugName) {
        
            List<MiniStatementBlock> parenthesisSeperated = new List<MiniStatementBlock>();
            //List<bool> isParenthesisElement = new List<bool>();
       
            List<Vector2Int> pairs = ParsePairs(input, "()", debugName);
            
            if (pairs.Count == 0) {
                parenthesisSeperated.Add(new MiniStatementBlock(input, false, false));
                //parenthesisSeperated.Add(input);
                //isParenthesisElement.Add(false);
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
                
                        //parenthesisSeperated.Add(input.Substring(startIndex, dif));
                        //isParenthesisElement.Add(false);
                    }

                    bool negate = input[pStartIndex - 1] == '!';

                    parenthesisSeperated.Add(new MiniStatementBlock(input.Substring( pStartIndex + 1, (pEndIndex - 1) - pStartIndex ), true, negate));
                    //parenthesisSeperated.Add(input.Substring( pStartIndex + 1, (pEndIndex - 1) - pStartIndex ));
                    //isParenthesisElement.Add(true);
                    startIndex = pEndIndex + 1;
                }


                if (startIndex != input.Length) {

                    parenthesisSeperated.Add(new MiniStatementBlock(input.Substring(startIndex, input.Length - startIndex), false, false));
                    //parenthesisSeperated.Add(input.Substring(startIndex, input.Length - startIndex));
                    //isParenthesisElement.Add(false);
                }
            }

            List<MiniStatementBlock> checkParams = new List<MiniStatementBlock>();
            
            //List<string> checkParams = new List<string>();
            List<bool> checkModeAnd = new List<bool>(); 
            //List<bool> checkParamIsPar = new List<bool>();

            for (int i = 0; i < parenthesisSeperated.Count; i++) {

                if (parenthesisSeperated[i].isParenthesisBlock) {
                    checkParams.Add(parenthesisSeperated[i]);
                    continue;
                }

                string block = parenthesisSeperated[i].block;

                //if (isParenthesisElement[i]) {
                
                    //checkParams.Add(block);
                    //checkParamIsPar.Add(true);
                
                    //continue;
                //}

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
                        
                        //checkParams.Add(block.Substring(lastCheckParamEndIndex, x - lastCheckParamEndIndex));
                        //checkParamIsPar.Add(false);
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

                            //checkParams.Add(block.Substring(lastCheckParamEndIndex, length));
                            //checkParamIsPar.Add(false);
                            checkParams.Add(new MiniStatementBlock(block.Substring(lastCheckParamEndIndex, length), false, negate));
                        

                        }
                    }
                    x++;
                }
            }


            //bool met = CheckPossibleParam(checkParamIsPar[0], checkParams[0], paramsCheck, debug);
            bool met = CheckPossibleParam(checkParams[0], paramsCheck, debug, debugName);

            
            bool lastCheckAnd = checkModeAnd.Count > 0 ? checkModeAnd[0] : true;
                
            for (int i = 1; i < checkParams.Count; i+=1) {
                //if ((lastCheckAnd && met) || (!lastCheckAnd && !met)) met = CheckPossibleParam(checkParamIsPar[i], checkParams[i], paramsCheck, debug);
                if ((lastCheckAnd && met) || (!lastCheckAnd && !met)) met = CheckPossibleParam(checkParams[i], paramsCheck, debug, debugName);
                
                if (i < checkParams.Count -1) lastCheckAnd = checkModeAnd[i];
            }
            return met;
        }

        

        public static bool StatementValue(string statement, Dictionary<string, CustomParameter> paramsCheck, bool debug, string debugName) {
            string input = StripAllSpace(statement);
            if (input.IsEmpty()) return true;
            if (debug) {
                Debug.Log(debugName + " :: Start: " + input);
            }
            return BlockMet(input, paramsCheck, debug, debugName);
        }
    }



}
