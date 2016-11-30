console.log('main.js loaded...')

// create an instance of the plugin
var plugin = new OverwolfPlugin("InGameProbability", true);

console.log('plugin created')

// initialize it
plugin.initialize(function(status) {
  console.log('plugin initialize returned ' + status)
  if (status == false) {
    document.querySelector('#title').innerText = "Plugin couldn't be loaded??";
    return;
  }

  function addMessage(message) {
    var obj = document.createElement("div");
    obj.innerText = message;
    document.querySelector('#messages').appendChild(obj);
  }

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

console.log('plugin initialize called...')

function registerEvents()   {
    overwolf.games.onGameInfoUpdated.addListener(function(info) {
        if(info && info.gameInfo)   {
            if(!info.gameInfo.isRunning)    {
                overwolf.windows.getCurrentWindow(function(result)  {
                    overwolf.windows.close(result.window.id);
                });
            }
        }
    });
    overwolf.games.events.onError.addListener(function(info)    {
        console.log("Error: " + JSON.stringify(info));
    });

    overwolf.games.events.onInfoUpdates2.addListener(function(info)    {
        console.log("info: " + JSON.stringify(info));
        if(info && info.info.game_info && info.info.game_info.teams)  {
            var teams = JSON.parse(decodeURIComponent(info.info.game_info.teams));

            initializePredictions(teams);
        }   else    {
            console.log("something was missing, couldnt get teams");
        }
    });
}

function setFeatures(callback)  {
    overwolf.games.events.setRequiredFeatures(['teams'], function(info) {
        if(info.status == 'error')  {
            console.log('could not set required features: ' + info.reason);
            window.setTimeout(function() {setFeatures(callback)}, 1000);
        }   else    {
            console.log('set required features!');
            console.log(JSON.stringify(info));
            if(callback)    {
                callback(true);
            }
        }
    })
}

function initializePredictions(teams)  {
    var blue = teams.filter(function (player) {
        return player.team == 100;
    });

    var red = teams.filter(function (player) {
        return player.team == 200;
    });

    var extractSummoner = function (player) {
        return player.summoner;
    };
	
	var championNames = teams.map(function (player) {
		return player.champion;
	});

    var blueNames = blue.map(extractSummoner);
    var redNames = red.map(extractSummoner);

    console.log('blue players: ' + JSON.stringify(blueNames));
    console.log('red players: ' + JSON.stringify(redNames));

    plugin.get().InitializeState(championNames, blueNames, redNames, function (success) {
        if (success) {
            console.log('successfully initialized state');
            plugin.get().StartApp(function (success) {
                if (success === true) {
                    console.log('successfully started app');
                    plugin.get().WinChanceChanged.addListener(function(blueChance, redChange)   {
                        console.log('win chance changed!');
                        document.querySelector('#blue_chance').innerText = Math.round(blueChance * 1000) / 10 + '%';
                        document.querySelector('#red_chance').innerText = Math.round(redChange * 1000) / 10 + '%';
                        document.querySelector('#root').style.visibility = 'visible';
                    });
                } else {
                    console.log('failed to start app!');
                }
            });
        }
        else
        {
            console.log('failed to initialize state')
        }
    });
}

overwolf.games.events.getInfo(function(info)    {
    if(info && info.res && info.res.game_info && info.res.game_info.teams) {
        var teams = JSON.parse(decodeURIComponent(info.res.game_info.teams));

        initializePredictions(teams);
    }   else    {
        registerEvents();
        setFeatures();
    }
});
