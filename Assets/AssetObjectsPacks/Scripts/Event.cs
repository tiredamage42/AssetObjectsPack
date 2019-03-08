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

        public List<AssetObject> GetAssetObjects () {
            List<AssetObject> r = new List<AssetObject>();
            int l = assetObjects.Length;
            for (int i = 0; i < l; i++) {

                if (assetObjects[i].solo) {
                    r.Clear();
                    r.Add(assetObjects[i]);
                    return r;
                }
                if (!assetObjects[i].mute) {
                    r.Add(assetObjects[i]);
                }
            }
            return r;

        }

        

/*


        static bool CheckPossibleParam(bool isParenthesisBlock, string paramBlock, Dictionary<string, CustomParameter> paramChecks, bool debug) {
            return isParenthesisBlock ? BlockMet(paramBlock, paramChecks, debug) : ParamStringMet(paramBlock, paramChecks, debug);
        }

        static bool BlockMet (string input, Dictionary<string, CustomParameter> paramsCheck, bool debug) {
        
            List<string> parenthesisSeperated = new List<string>();
            List<bool> isParenthesisElement = new List<bool>();
       
            IEnumerable<Vector2Int> pairs = CustomScripting.ParsePairs(input, "()", name);
            
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
            bool met = CheckPossibleParam(checkParamIsPar[0], checkParams[0], paramsCheck, debug);
            bool lastCheckAnd = checkModeAnd.Count > 0 ? checkModeAnd[0] : true;
                
            for (int i = 1; i < checkParams.Count; i+=1) {
                if ((lastCheckAnd && met) || (!lastCheckAnd && !met)) met = CheckPossibleParam(checkParamIsPar[i], checkParams[i], paramsCheck, debug);
                if (i < checkParams.Count -1) lastCheckAnd = checkModeAnd[i];
            }
            return met;
        }

        static bool ParamStringMet (string parameter, Dictionary<string, CustomParameter> paramsCheck, bool debug) {
                string[] nameSplit = parameter.Split(':');
                string paramName = nameSplit[0];

                CustomParameter check;
                
                if (!paramsCheck.TryGetValue(paramName, out check)) {
                    if (debug) {
                        Debug.Log("Parameter (" + paramName + ") des not exist on player");
                    }
                    check = null;
                }

                bool matchesParameter = false;
                if (check != null) {
                    string valueString;
                    //Debug.Log(nameSplit[0] + " / " + nameSplit[1]);
                    matchesParameter = check.MatchesParameter(nameSplit[0], GetCompareMode(nameSplit[1], out valueString), valueString);
                }
                if (check == null || !matchesParameter) {
                    if (debug) {
                        Debug.Log("parameter failed" + parameter);
                    }
                    return false;
                }
                return true;
        }

        static CustomParameter.CompareMode GetCompareMode(string compareAndValue, out string valString) {
            if (compareAndValue.StartsWith("<=")) {
                valString = compareAndValue.Substring(2);
                return CustomParameter.CompareMode.LessThenOrEqual;
            }
            else if (compareAndValue.StartsWith(">=")) {
                valString = compareAndValue.Substring(2);
                return CustomParameter.CompareMode.MoreThanOrEqual;
            }
            else if (compareAndValue.StartsWith(">")) {
                valString = compareAndValue.Substring(1);
                return CustomParameter.CompareMode.MoreThan;
            }
            else if (compareAndValue.StartsWith("<")) {
                valString = compareAndValue.Substring(1);
                return CustomParameter.CompareMode.LessThan;
            }
            else {
                valString = compareAndValue;
                return CustomParameter.CompareMode.Equals;
            }
        }



        

        public bool PassesConditionCheck(Dictionary<string, CustomParameter> paramsCheck, bool debug) {
            string input = conditionBlock.Replace(" ", string.Empty);
            if (input.IsEmpty()) return true;

            if (debug) {
                Debug.Log("Start: " + input);
            }

            
            return BlockMet(conditionBlock.Replace(" ", string.Empty), paramsCheck, debug);
        }
 */


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


        void GetFilteredStates (EventState eventState, Dictionary<string, CustomParameter> parameters, List<AssetObject> ret, bool debug) {
            //if (eventState.PassesConditionCheck(parameters, debug)) {
            if (CustomScripting.StatementValue(eventState.conditionBlock, parameters, debug, name)) {
                
                ret.AddRange(eventState.GetAssetObjects());
                
                int l = eventState.subStates.Length;
                for (int i = 0; i < l; i++) {
                    GetFilteredStates(eventState.subStates[i], parameters, ret, debug);
                }
            }
        }
        public List<AssetObject> GetParamFilteredObjects(Dictionary<string, CustomParameter> parameters) {
            List<AssetObject> ret = new List<AssetObject>();
            GetFilteredStates(baseState, parameters, ret, false);

            if (ret.Count == 0) {
                Debug.LogWarning("Couldnt find any assets on: " + this.name);
                //GetFilteredStates(baseState, parameters, ret, true);
                //Debug.Break();
            }
            return ret;
        }

    }
}





