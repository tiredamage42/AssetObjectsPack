using UnityEngine;
using System.Collections.Generic;
using System.Linq;
namespace AssetObjectsPacks {

    [System.Serializable] public class EventState {
        #if UNITY_EDITOR
        public const string assetObjectsField = "assetObjects";
        public const string nameField = "name";
        public const string conditionsBlockField = "conditionBlock";
        public const string subStatesField = "subStates";
        #endif
    
        
        public string conditionBlock;
        public EventState[] subStates;
        public AssetObject[] assetObjects;
        public string name;



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



        

        public bool PassesConditionCheck(CustomParameter[] paramsCheck) {
            if (conditionBlock.IsEmpty()) return true;

            //string input = conditionBlock.Replace(" ", string.Empty);
            //Debug.Log("Start: " + input);
            

            return BlockMet(conditionBlock.Replace(" ", string.Empty), paramsCheck);
        }


    }

    //[System.Serializable] public class EventStateCoupler {

    //    public int[] eventStateIndicies;
    //}


    [CreateAssetMenu(fileName = "New Asset Object Event", menuName = "Asset Objects Packs/Event", order = 2)]
    [System.Serializable] public class Event : ScriptableObject {

        #if UNITY_EDITOR
        //public const string asset_objs_field = "assetObjects", pack_id_field = "assetObjectPackID";
        public const string pack_id_field = "assetObjectPackID";
        public const string hiddenIDsField = "hiddenIDs";
        public const string multi_edit_instance_field = "multi_edit_instance";
        public const string baseStateField = "baseState";
        
        //used for multi anim editing and defaults in editor explorer
        public AssetObject multi_edit_instance;  
        public int[] hiddenIDs;
        #endif
    

        public EventState baseState;


        //public AssetObject[] assetObjects;


        void GetFilteredStates (EventState eventState, CustomParameter[] playerParameters, List<AssetObject> ret) {
            if (eventState.PassesConditionCheck(playerParameters)) {
                ret.AddRange(eventState.assetObjects);
                for (int i = 0; i < eventState.subStates.Length; i++) {
                    GetFilteredStates(eventState.subStates[i], playerParameters, ret);
                }
            }
        }




        public AssetObject[] GetFilteredStatesList(CustomParameter[] playerParameters) {

            List<AssetObject> ret = new List<AssetObject>();

            GetFilteredStates(baseState, playerParameters, ret);

            return ret.ToArray();

        }






        public int assetObjectPackID = -1;
    }
}





