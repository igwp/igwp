// create an instance of the plugin
var plugin = new OverwolfPlugin("InGameProbability", true);

// initialize it
plugin.initialize(function(status) {
  if (status == false) {
    document.querySelector('#title').innerText = "Plugin couldn't be loaded??";
    return;
  }

  function addMessage(message) {
    var obj = document.createElement("div");
    obj.innerText = message;
    document.querySelector('#messages').appendChild(obj);
  }

  plugin.get().StartApp(function(success) {
    var obj = document.createElement("div");
    if(success === true) {
      obj.innerText = 'init successful'
    } else  {
      obj.innerText = 'init failed'
    }
    document.querySelector('#messages').appendChild(obj)
  });

    overwolf.windows.getCurrentWindow(function(result) {
        overwolf.windows.changePosition(result.window.id, 0, 16, function()   {
            overwolf.windows.changeSize(result.window.id, 1920, 400, null);
        });
    });
   
  //document.querySelector('#title').innerText =
  //  "Plugin " + plugin.get()._PluginName_ + " was loaded!";

    
  /////////
  /*
  plugin.get().onGlobalEvent.addListener(function(first, second) {
    addMessage("onGlobalEvent triggered: (" + first + ", " + second + ")");
  });
  
  addMessage("SampleProperty = " + plugin.get().SampleProperty);

  plugin.get().add(5, 4, function(result) {
    addMessage("5 + 4 = " + result);
  });
  
  plugin.get().triggerGlobalEvent();
    */
});