using System.Collections.Generic;
using UnityEngine;
namespace AssetObjectsPacks {
    [System.Serializable] public abstract class AssetObject {
        public Object object_reference;
        public List<string> tags = new List<string>();
        public int id;

    }
}

