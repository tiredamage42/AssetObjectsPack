using System.Collections.Generic;
using UnityEngine;
using System.Linq;
namespace AssetObjectsPacks {


    /*
    */


    [System.Serializable] public class AssetObject {
        /*

        const string testString = "b:Agitated:false & ((i:Stance:0 & s:String:turn) | (f:Something:1.5)) | s:StringVal:ssi";
        

        public struct BracketPair
        {
            public int startIndex, endIndex, depth;
            public BracketPair(int startIndex, int endIndex, int depth)
            {
                if (startIndex > endIndex) throw new System.ArgumentException("startIndex must be less than endIndex");
                this.startIndex = startIndex;
                this.endIndex = endIndex;
                this.depth = depth;
            }
        }
        public static IEnumerable<BracketPair> ParseParenthesisPairs(string text)
        {
            var startPositions = new Stack<int>();

            for (int i = 0; i < text.Length; i++) {

                if (text[i] == '(')
                {
                    startPositions.Push(i);
                }
                else if (text[i] == ')')
                {
                    if (startPositions.Count == 0) throw new System.ArgumentException(string.Format("mismatched end bracket at index {0}", i));
                    var depth = startPositions.Count - 1;
                    var start = startPositions.Pop();
                    yield return new BracketPair(start, i, depth);
                }
            }

            if (startPositions.Count > 0) {

                throw new System.ArgumentException(string.Format("mismatched start brackets, {0} total", startPositions.Count));
            }
        }


        



After which using the following:

static void Main(string[] args)
{
    for (string input = Console.ReadLine(); !string.IsNullOrWhiteSpace(input); input = Console.ReadLine())
    {
        foreach (var pairs in TextHelper.ParseBracketPairs(input))
            Console.WriteLine("Start: {0}, End: {1}, Depth: {2}", pairs.StartIndex, pairs.EndIndex, pairs.Depth);
    }
}
For the input [a][b[c[d]e]][f] you get:

Start: 0, End: 2, Depth: 0
Start: 7, End: 9, Depth: 2
Start: 5, End: 11, Depth: 1
Start: 3, End: 12, Depth: 0
Start: 13, End: 15, Depth: 0

        */
        /*
        
        static bool CheckPossibleParam(bool isParenthesisBlock, string paramBlock, CustomParameter[] paramChecks) {
            return isParenthesisBlock ? BlockMet(paramBlock, paramChecks) : ParamStringMet(paramBlock, paramChecks);
        }

        static bool BlockMet (string input, CustomParameter[] paramsCheck) {
        
            List<string> parenthesisSeperated = new List<string>();
            List<bool> isParenthesisElement = new List<bool>();
            

            //Debug.Log("getting pairs");
            
            IEnumerable<BracketPair> pairs = ParseParenthesisPairs(input).Where ( p => p.depth == 0 );

            if (pairs.Count() == 0) {
                //Debug.Log("is single");
            
                parenthesisSeperated.Add(input);
                isParenthesisElement.Add(false);
            }
            else {

                int startIndex = 0;

                foreach (var p in pairs) {
                    int start = p.startIndex;

                    int dif = start - startIndex;
                    if (dif > 0){   
                        parenthesisSeperated.Add(input.Substring(startIndex, dif));
                        isParenthesisElement.Add(false);
                    }

                    parenthesisSeperated.Add(input.Substring( p.startIndex + 1, (p.endIndex - 1) - p.startIndex ));
                    isParenthesisElement.Add(true);

                    startIndex = p.endIndex + 1;
                }
                if (startIndex != input.Length) {

                    parenthesisSeperated.Add(input.Substring(startIndex, input.Length - startIndex));
                    isParenthesisElement.Add(false);

                }
            }

            //Debug.Log("building chekc params");
            



            List<string> checkParams = new List<string>();
            List<bool> checkModeAnd = new List<bool>(); 
            List<bool> checkParamIsPar = new List<bool>();

            for (int i = 0; i < parenthesisSeperated.Count; i++) {

                string block = parenthesisSeperated[i];
                if (isParenthesisElement[i]) {
                    checkParams.Add(block);
                    checkParamIsPar.Add(true);
                    continue;
                }
                //Debug.Log("uhh");

                int lastCheckParamEndIndex = 0;

                int x = 0;
                while (x < block.Length) {

                

                //for (int x = 0; x < block.Length; x++) {
                    char c = block[x];
                    if (c == '|' || c == '&') {
                        if (x == 0) {
                            //lastCheckParamEndIndex = x + 2;
                        }
                        else {
                            int length = x - lastCheckParamEndIndex;
                            //Debug.Log("Length in loop: " + length);


                            checkParams.Add(block.Substring(lastCheckParamEndIndex, length));
                        }
                        lastCheckParamEndIndex = x + 2;
                        x++;

                        checkParamIsPar.Add(false);
                        checkModeAnd.Add( c == '&' );
                    }
                    if (x == block.Length - 1) {


                        if (block.Length - lastCheckParamEndIndex > 0) {
                            int length = block.Length - lastCheckParamEndIndex;
                            //Debug.Log("Length: " + length);

                            checkParams.Add(block.Substring(lastCheckParamEndIndex, length));

                            checkParamIsPar.Add(false);
                        }
                    }

                    x++;
                }
            }
            //Debug.Log("Checking count: " + checkParams.Count);                
            //Debug.Log("Checking count: " + checkModeAnd.Count);                
            //Debug.Log("Checking count: " + checkParamIsPar.Count);                
            
            //Debug.Log("Checking: " + checkParams[0]);                
            bool met = CheckPossibleParam(checkParamIsPar[0], checkParams[0], paramsCheck);
            bool lastCheckAnd = checkModeAnd.Count > 0 ? checkModeAnd[0] : true;
            
            //Debug.Log(lastCheckAnd ? "And" : "Or");

                
            for (int i = 1; i < checkParams.Count; i+=1) {
                //Debug.Log("Checking (in loop): " + checkParams[i]);      


                if ((lastCheckAnd && met) || (!lastCheckAnd && !met)) {
                    met = CheckPossibleParam(checkParamIsPar[i], checkParams[i], paramsCheck);
                }

                if ((lastCheckAnd && !met)) {
                    //Debug.Log("but checking and and already false");
                }
                if ((!lastCheckAnd && met)) {
                    //Debug.Log("but checking or and already true");
                }
        
                if (i < checkParams.Count -1) {
                    lastCheckAnd = checkModeAnd[i];
                    Debug.Log(lastCheckAnd ? "And" : "Or");
                }
            }
            return met;
        }


        static CustomParameter FindParamByName (string name, CustomParameter[] paramsCheck) {
            int l = paramsCheck.Length;
            for (int i = 0; i < l; i++) {
                if (name == paramsCheck[i].name) return paramsCheck[i];
            }
            Debug.LogWarning("Params List doesnt contain: " + name);
            return null;
        }



        

        static bool ParamStringMet (string parameter, CustomParameter[] paramsCheck) {
                string[] nameSplit = parameter.Split(':');
                string paramName = nameSplit[1];


                CustomParameter check = FindParamByName(paramName, paramsCheck);
                if (check == null || !check.MatchesParameter(nameSplit)) {
                    //Debug.Log("parameter failed" + paramName);
                    return false;
                }
                return true;
        }
        */
/*
        
        [System.Serializable] public class Condition {
            //ands
            public CustomParameter[] parameters;
            public bool ConditionMet (CustomParameter[] paramsCheck) {
                int l = parameters.Length;
                if (l == 0) return true;
                bool all_params_matched = true;
                for (int i = 0; i < l; i++) {
                    string name = parameters[i].name;
                    CustomParameter check = FindParamByName(name, paramsCheck);
                    //Debug.Log("checking parameter " + name);
                    if (check == null || !parameters[i].MatchesParameter(check)) {
                        //Debug.Log("parameter failed" + name);
                        all_params_matched = false;
                        break;
                    }
                }
                return all_params_matched;
            }
        }

        public string conditionsBlock = "";



        public bool PassesConditionCheck(CustomParameter[] paramsCheck) {
            if (conditionsBlock.IsEmpty()) return true;

            string input = conditionsBlock.Replace(" ", string.Empty);
            //Debug.Log("Start: " + input);
            

            return BlockMet(input, paramsCheck);
        }
        //ors
        public Condition[] conditions;
        public bool PassesConditionCheck (CustomParameter[] paramsCheck) {
            if (conditions.Length == 0) {
                //Debug.Log("[" + id + "] has no conditions");
                return true;
            }
            for (int i = 0; i < conditions.Length; i++ ) {
                if (conditions[i].ConditionMet(paramsCheck)) {
                    //Debug.Log("[" + id + "] condition " + i +  " met");   
                    return true;
                }
            }
            return false;
        }

 */

        public Object objRef;
        public int id;
        public CustomParameter[] parameters;
        Dictionary<string, CustomParameter> paramDict = new Dictionary<string, CustomParameter>();

        public CustomParameter this [string paramName] {
            get {
                int l = parameters.Length;
                if (paramDict.Count != l) {
                    paramDict.Clear();
                    for (int i = 0; i < l; i++) paramDict.Add(parameters[i].name, parameters[i]);
                }
                return paramDict[paramName];
            }
        }
    }
}

