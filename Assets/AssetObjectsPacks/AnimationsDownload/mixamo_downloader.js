// Mixamo Animation downloadeer
// The following script make use of mixamo2 API to download all anims for a single character that you choose.

// -including three degrees of variation per parameter (not including overdrive which can be handled by unity)
// -includes packs

//paste the contents of this file to your chrome console after logging into mixamo

// The animations are saved with descriptive long names

/*

// y bot character
// to change: Download an animation and get the character ID from the Network tab

Read the Read Me instructions before changing the character
*/
const character = '4f5d21e1-4ccc-41f1-b35b-fb2547bd8493'

const bearer = localStorage.access_token

//uncomment if you only need to download certain ones
/*
const needed_indicies = [
    1905,
    2294,
    2301,
];
*/

var InitializeGetRequest = () => {
    return {
        method: 'GET',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${bearer}`,
            'X-Api-Key': 'mixamo2'
        }
    };
}


// retrieves json.details.gms_hash 
/*
name: "Praying"
description: "Buckled Stand And Praying"
id: "c9c69fb6-b96c-11e4-a802-0aaa78deedf9"
motion_id: "c9c69fb6-b96c-11e4-a802-0aaa78deedf9"
source: "system"
details:
    default_frame_length: 35
    duration: 1.17                        
    loopable: true
    supports_inplace: false
    gms_hash:
        arm-space: 0
        inplace: false
        mirror: false
        model-id: 103120902
        params: Array(3)
            0: (2) ["Pray Towards", -1..1]
            ...
        trim: (2) [0, 100]                    
*/
var getProduct = (animId) => {
    //console.log('getProduct animId=', animId);
    var productUrl = `https://www.mixamo.com/api/v1/products/${animId}?similar=0&character_id=${character}`;
    return fetch(productUrl, InitializeGetRequest()).then((res) => res.json()).then((json) => json).catch(() => Promise.reject('Failed to download product details'))
}
var getAnimationList = (page) => {
    var listUrl = `https://www.mixamo.com/api/v1/products?page=${page}&limit=96&order=&type=Motion%2CMotionPack&query=`;
    return fetch(listUrl, InitializeGetRequest()).then((res) => res.json()).then((json) => json).catch(() => Promise.reject('Failed to download animation list'))
}

var param_defaults_options = [-1, 0, 1];

var buildVariations = (original_params) => {
    var variations = [];
    //get the parameter count for variations (not includeing overdrive, speed can be overwritten in unity)            
    var l = original_params.length;
    var variational_params_length = l;
    for (var x = 0; x < l; x++) {
        if (original_params[x][0] == "Overdrive") {
            variational_params_length -= 1;
            break;
        }
    }
    if (variational_params_length == 0) {
        variations.push (original_params);
        return variations;
    }
    var variation_counts = Math.pow(3, variational_params_length);
    for (var i = 0; i < variation_counts; i++) {
        blank_variation = [];
        for (var x = 0; x < l; x++) {
            blank_variation.push(original_params[x]);
        }
        variations.push (blank_variation);
    }
        
    var param_change_sparsity = variation_counts / 3;
    for (var p = 0; p < l; p++) {
        if (original_params[p][0] == "Overdrive") continue;

        for (var i = 0; i < variation_counts; i++) {
            //maybe floor(i / (3^params_number+1))
            var target = param_defaults_options[ Math.floor(i / param_change_sparsity) % 3 ];
            variations[i][p] = [variations[i][p][0], target];
        }
        param_change_sparsity /= 3;
    }
    return variations;                
}

function toTitleCase(str) {
    return str.replace(/\w\S*/g, function(txt) {
        return txt.charAt(0).toUpperCase() + txt.substr(1).toLowerCase();
    }).replace(/ /g,'').replace(/[^a-z0-9]/gi,'');
}

var downloadAnimation = (character, current_anim, o, pack_name, just_packs) => {
    
    var page = o.page;
    var pages = o.pages;

    var log_prefix = '[' + page + '/' + pages + ']';
    
    if (current_anim.motions != null) {
        if (just_packs) {
            console.log(log_prefix + 'Checking pack ', current_anim.name)
            return downloadAnimLoop(character, { anims: current_anim.motions, page:o.page, pages:o.pages }, current_anim.name, just_packs);
        }
        console.log(log_prefix + 'Skipping pack ', current_anim.name)
        return Promise.resolve("");
    } 
    
    if (just_packs && !pack_name) {
        console.log(log_prefix + 'Skipping Loose anim ', current_anim.name)
        return Promise.resolve("");
    }
        
    var animId = current_anim.id;
    if (!animId) animId = current_anim.product_id; //motion_id
    
    if (!anim_trackers.includes(animId)) {        
        anim_trackers.push(animId)
    }
    else {
        console.log(log_prefix + "Duplicate:", current_anim.name)
        return Promise.resolve("");
    }

    return getProduct(animId)
        .then((product) => {
            var gms_hash = product.details.gms_hash;
            
            var variations = buildVariations(gms_hash.params);  

            var variations_length = variations.length;

            if (!variations_length) {
                console.error("No Variation?")
            }
            
            var start_anim_name = toTitleCase(product.name).replace(/ /g,'');
            
            if (product.details.loopable) 
                start_anim_name += '(LP)';

            
            if (pack_name) 
                start_anim_name += '(' + toTitleCase(pack_name).replace(/ /g,'') + ')';
  
            
            start_anim_name += '(' + toTitleCase(product.description).replace(/ /g,'') + ')';

            if (variations_length > 1) {
                console.log('starting variations', variations_length)
            }
            
            
            
            return exportVariationLoop(character, page, pages, variations, gms_hash, start_anim_name, variations_length > 1)
        })
    }
    

function sleeper(ms) {
    return function(x) {
        return new Promise(resolve => setTimeout(() => resolve(x), ms));
    };
}
      
var exportVariationLoop = (character, page, pages, variations, gms_hash, start_name, has_variations) => {
    
    
    if (!variations.length) {

        if (has_variations) {
            console.log("Variations done")
        }
        
        return Promise.resolve('variations done!');
    }


    
    var head = variations[0];
    variations = variations.slice(1);

    total_animations += 1;
    var final_product_name = '-' + String(total_animations) + '-' + start_name;

    if (has_variations) {
        for (var p = 0; p < head.length; p++) {
            var p_name = head[p][0];
            if (p_name == "Overdrive") continue;
            final_product_name += '(' + p_name.replace(/ /g,'') + '-' + String(head[p][1] + 1) + ')';
        }
    }

            
    var log_prefix = '[' + page + '/' + pages + ']';
    
    console.log(log_prefix + "Export:", final_product_name)

    //uncomment if you only need to download certain ones
    
    /*
    if (!needed_indicies.includes(total_animations)) {
        console.log("Skipping Not Needed:", final_product_name);
        return exportVariationLoop(character, page, pages, variations, gms_hash, start_name, has_variations) //loop
        .catch(() => Promise.reject("Unable to download animation " + animId))    
        //return Promise.resolve('variations done!');
    }
    */

    gms_hash.params = head;
    
    var _gms_hash = Object.assign({}, gms_hash, { params: gms_hash.params.map((param) => param[1]).join(',') })
                                   
    return exportAnimation(character, [_gms_hash], final_product_name)
        .then((json) => monitorAnimation(character, json))

        //uncomment if you're having performance problems:
        
        //.then(sleeper(3000))

        .then(() => exportVariationLoop(character, page, pages, variations, gms_hash, start_name, has_variations)) //loop
        .catch(() => Promise.reject("Unable to download animation " + animId))    
}

var downloadAnimLoop = (character, o, pack_name, just_packs) => {
    if (!o.anims.length) {
        if (pack_name) {
            console.log("downlaoded pack")
            return Promise.resolve('Downloaded pack!');
        }
        else {
            console.log("end of page");
            return downloadAnimsInPage(o.page + 1, o.pages, just_packs); // no anims left, get a new page
        }
    }
    
    var head = o.anims[0];
    o.anims = o.anims.slice(1);
    
    return downloadAnimation(character, head, o, pack_name, just_packs)    
        .then(() => downloadAnimLoop(character, o, pack_name, just_packs)) //loop
        .catch(() => Promise.reject("Something went wrong in  downloadAnimationsLoop"))
}

var total_animations = 0;
var anim_trackers = [];

var downloadAnimsInPage = (page, pages, just_packs) => {
    if (page > pages) {
        if (just_packs) {
            console.log('All packs have been downloaded, now going through other anims...');
            return downloadAnimsInPage(1, 100, false);
        }
        console.log('All pages have been downloaded')
        console.log('Total animations:', total_animations)
        return Promise.resolve('All pages have been downlaoded');
    }

    return getAnimationList(page)
        .then((json) => ({ anims: json.results, page: json.pagination.page, pages: json.pagination.num_pages }))
        .then((o) => downloadAnimLoop(character, o, false, just_packs))
        .catch((e) => Promise.reject("Unable to download all animations error ", e))
}

var start = () => {
    console.log('start');
    if (!character) {
        console.error("Please add a valid character ID at the beginnig of the script");
        return
    }
    downloadAnimsInPage(1, 100, true);
}
 
var exportAnimation = (character_id, gmsHashArray, product_name) => {
    var exportUrl = 'https://www.mixamo.com/api/v1/animations/export'
        
    var exportBody = {
        character_id,
        gms_hash: gmsHashArray, //[{ "model-id": 103120902, "mirror": false, "trim": [0, 100], "overdrive": 0, "params": "0,0,0", "arm-space": 0, "inplace": false }],
        preferences: { format: "fbx7_unity", skin: "false", fps: "60", reducekf: "0" },
        product_name,
        type: "Motion"
    };
    
    var exportInit = {
        method: 'POST',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${bearer}`,
            'X-Api-Key': 'mixamo2',
            'X-Requested-With': 'XMLHttpRequest'
        },
        body: JSON.stringify(exportBody)
    }
    return fetch(exportUrl, exportInit).then((res) => res.json()).then((json) => json)
}


var monitorAnimation = (character, export_result) => {

    var monitorUrl = `https://www.mixamo.com/api/v1/characters/${character}/monitor`;
    
    return fetch(monitorUrl, InitializeGetRequest())
        .then((res) => {
            switch (res.status) {
                case 404: {
                    var errorMsg = ('ERROR: Monitor got 404 error: ' + export_result.error + ' message=' + export_result.message);
                    console.error(errorMsg);
                    throw new Error(errorMsg);
                } break
                case 202:
                case 200: {
                    return res.json()
                } break
                default:
                    throw new Error('Response not handled', res);
            }
        }).
        then((msg) => {
            /*
                console.log(msg)
                job_type: "character_export"
                job_uuid: "b449b7c7-bcba-4a87-a0a3-9206b85a6143"
                message: "The job has been queued."
                status: "processing"
                type: "character"
                uuid: "4f5d21e1-4ccc-41f1-b35b-fb2547bd8493"
            */
            switch (msg.status) {
                case 'completed':
                    //console.log('Downloading: ', msg.job_result);
                    //if (downloadingTab) {
                    //    downloadingTab.close();
                    //}
                    //downloadingTab = window.open('', '_blank');
                    //downloadingTab.location.href = msg.job_result;

                    tab_link.href = msg.job_result;
                    tab_link.click();



                    return msg.job_result;
                    break;
                case 'processing':

                    //console.log('...');
                    return monitorAnimation(character);
                    break;// loop
                case 'failed':
                default:
                    var errorMsg = ('ERROR: Monitor status:' + msg.status + ' message:' + msg.message + 'result:' + JSON.stringify(msg.job_result));
                    console.error(errorMsg);
                    throw new Error(errorMsg);
            }
        }).catch((e) => Promise.reject("Unable to monitor job for character " + character + e))
}


// Workaround for downloading files from a promise
// NOTE that chrome will detect you are downloading multiple files in a single TAB. 
// Please allow it!
//var downloadingTab = window.open('', '_blank');

var tab_link = document.createElement("a");

start()



