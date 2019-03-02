using UnityEngine;
using System.Collections.Generic;
using System.Linq;
namespace AssetObjectsPacks {

    [System.Serializable] public class EventState {
        #if UNITY_EDITOR
        //for editor adding
        public bool isNew;
        public string name;
        #endif
    
        public string conditionBlock;
        public AssetObject[] assetObjects;
        public EventState[] subStates;

        public static IEnumerable<Vector2Int> ParseParenthesisPairs(string text) {
            var lastStartPos = -1;
            //var startPositions = new Stack<int>();
            int l = text.Length;

            for (int i = 0; i < l; i++) {
                if (text[i] == '(') {
                    lastStartPos = i;
                    //startPositions.Push(i);
                }
                else if (text[i] == ')') {
                    //if (startPositions.Count == 0) {
                    if (lastStartPos == -1) {

                        throw new System.ArgumentException(string.Format("mismatched end bracket at index {0}", i));
                    }
                    else {
                    //if (startPositions.Count == 1) {
                        int b = lastStartPos;
                        lastStartPos = -1;

                        yield return new Vector2Int(
                        b,
                            //startPositions.Pop(), 
                        i);
                        
                    } 
                }
            }
            //if (startPositions.Count > 0) 
            if (lastStartPos != -1) 
            {

                throw new System.ArgumentException(string.Format("mismatched start brackets{0}", ""));//, {0} total", startPositions.Count));
            }
        }



        static bool CheckPossibleParam(bool isParenthesisBlock, string paramBlock, Dictionary<string, CustomParameter> paramChecks) {
            return isParenthesisBlock ? BlockMet(paramBlock, paramChecks) : ParamStringMet(paramBlock, paramChecks);
        }

        static bool BlockMet (string input, Dictionary<string, CustomParameter> paramsCheck) {
        
            List<string> parenthesisSeperated = new List<string>();
            List<bool> isParenthesisElement = new List<bool>();

            //Debug.Log("getting pairs");            
            IEnumerable<Vector2Int> pairs = ParseParenthesisPairs(input);
            
            if (pairs.Count() == 0) {
                parenthesisSeperated.Add(input);
                isParenthesisElement.Add(false);
            }
            else {
                int startIndex = 0;

                foreach (var p in pairs) {
                    int pStartIndex = p.x;
                    int pEndIndex = p.y;
                    int start = pStartIndex;
                    int dif = start - startIndex;
                    if (dif > 0){   
                        parenthesisSeperated.Add(input.Substring(startIndex, dif));
                        isParenthesisElement.Add(false);
                    }
                    parenthesisSeperated.Add(input.Substring( pStartIndex + 1, (pEndIndex - 1) - pStartIndex ));
                    isParenthesisElement.Add(true);
                    startIndex = pEndIndex + 1;
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
                    char c = block[x];
                    if (c == '|' || c == '&') {
                        if (x != 0) checkParams.Add(block.Substring(lastCheckParamEndIndex, x - lastCheckParamEndIndex));
                        lastCheckParamEndIndex = x + 2;
                        x++;
                        checkParamIsPar.Add(false);
                        checkModeAnd.Add( c == '&' );
                    }
                    if (x == block.Length - 1) {
                        if (block.Length - lastCheckParamEndIndex > 0) {
                            int length = block.Length - lastCheckParamEndIndex;
                            checkParams.Add(block.Substring(lastCheckParamEndIndex, length));
                            checkParamIsPar.Add(false);
                        }
                    }
                    x++;
                }
            }
            bool met = CheckPossibleParam(checkParamIsPar[0], checkParams[0], paramsCheck);
            bool lastCheckAnd = checkModeAnd.Count > 0 ? checkModeAnd[0] : true;
                
            for (int i = 1; i < checkParams.Count; i+=1) {
                if ((lastCheckAnd && met) || (!lastCheckAnd && !met)) met = CheckPossibleParam(checkParamIsPar[i], checkParams[i], paramsCheck);
                if (i < checkParams.Count -1) lastCheckAnd = checkModeAnd[i];
            }
            return met;
        }

/*
        static CustomParameter FindParamByName (string name, CustomParameter[] paramsCheck) {
            int l = paramsCheck.Length;
            for (int i = 0; i < l; i++) {
                if (name == paramsCheck[i].name) return paramsCheck[i];
            }
            Debug.LogWarning("Params List doesnt contain: " + name);
            return null;
        }
 */


        static bool ParamStringMet (string parameter, Dictionary<string, CustomParameter> paramsCheck) {
                string[] nameSplit = parameter.Split(':');
                string paramName = nameSplit[0];


                CustomParameter check;// = paramsCheck[paramName];// FindParamByName(paramName, paramsCheck);
                if (!paramsCheck.TryGetValue(paramName, out check)) {
                    check = null;
                }

                
                
                if (check == null || !check.MatchesParameter(nameSplit)) {
                    //Debug.Log("parameter failed" + parameter);
                    return false;
                }
                return true;
        }



        

        public bool PassesConditionCheck(Dictionary<string, CustomParameter> paramsCheck) {
            string input = conditionBlock.Replace(" ", string.Empty);
            if (input.IsEmpty()) return true;

            //Debug.Log("Start: " + input);
            
            return BlockMet(conditionBlock.Replace(" ", string.Empty), paramsCheck);
        }


    }


    [CreateAssetMenu(fileName = "New Asset Object Event", menuName = "Asset Objects Packs/Event", order = 2)]
    [System.Serializable] public class Event : ScriptableObject {

        #if UNITY_EDITOR
        public const string pack_id_field = "assetObjectPackID";
        public const string multi_edit_instance_field = "multi_edit_instance";
        public const string hiddenIDsField = "hiddenIDs";
        public const string baseStateField = "baseState";
        
        //used for multi anim editing and defaults in editor explorer
        public AssetObject multi_edit_instance;  
        public int[] hiddenIDs;
        #endif
    

        public EventState baseState;
        public int assetObjectPackID = -1;


        void GetFilteredStates (EventState eventState, Dictionary<string, CustomParameter> parameters, List<AssetObject> ret) {
            if (eventState.PassesConditionCheck(parameters)) {
                
                ret.AddRange(eventState.assetObjects);
                
                int l = eventState.subStates.Length;
                for (int i = 0; i < l; i++) {
                    GetFilteredStates(eventState.subStates[i], parameters, ret);
                }
                
            }
        }
        public List<AssetObject> GetParamFilteredObjects(Dictionary<string, CustomParameter> parameters) {
            List<AssetObject> ret = new List<AssetObject>();
            GetFilteredStates(baseState, parameters, ret);

            if (ret.Count == 0) {
                Debug.LogWarning("Couldnt find any assets on: " + this.name);
            }
            return ret;
        }

    }
}





