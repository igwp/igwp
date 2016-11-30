console.log('overwolfplugin.js loaded...')

function OverwolfPlugin(extraObjectNameInManifest, addNameToObject) {
  var _pluginInstance = null;
  var _extraObjectName = extraObjectNameInManifest;
  var _addNameToObject = addNameToObject;

  // public
  this.initialize = function(callback) {
    console.log('plugin initialize called...')
    return _initialize(callback);
  }
  
  this.initialized = function() {
    return _pluginInstance != null;
  };
  
  this.get = function() {
    return _pluginInstance;
  };

  // privates
  function _initialize(callback) {
    console.log('private plugin initialize called')
    var proxy = null;
    
    try {
      proxy = overwolf.extensions.current.getExtraObject;
    } catch(e) {
      console.error("overwolf.extensions.current.getExtraObject doesn't exist!");
      return callback(false);
    }
    
    console.log('proxy set to ' + proxy)
    
    proxy(_extraObjectName, function(result) {
      console.log('proxy called! returned ' + _extraObjectName)
      if (result.status != "success") {
        console.error("failed to create " + _extraObjectName + " object: " + result);
        return callback(false);
      }
      
      _pluginInstance = result.object;
      
      if (_addNameToObject) {
        _pluginInstance._PluginName_ = _extraObjectName;
      }
      
      return callback(true);
    });
  }
}