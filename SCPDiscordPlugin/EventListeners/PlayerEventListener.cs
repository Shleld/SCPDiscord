﻿using Smod2.EventHandlers;
using Smod2.Events;
using System.Collections.Generic;

namespace SCPDiscord
{
    class PlayerEventListener : IEventHandlerPlayerJoin, IEventHandlerPlayerDie, IEventHandlerSpawn, IEventHandlerPlayerHurt, IEventHandlerPlayerPickupItem, 
        IEventHandlerPlayerDropItem, IEventHandlerNicknameSet, IEventHandlerInitialAssignTeam, IEventHandlerSetRole, IEventHandlerCheckEscape, IEventHandlerDoorAccess,
        IEventHandlerIntercom, IEventHandlerIntercomCooldownCheck, IEventHandlerPocketDimensionExit, IEventHandlerPocketDimensionEnter, IEventHandlerPocketDimensionDie, 
        IEventHandlerThrowGrenade, IEventHandlerInfected, IEventHandlerSpawnRagdoll, IEventHandlerLure, IEventHandlerContain106, IEventHandlerMedkitUse, IEventHandlerShoot,
        IEventHandler106CreatePortal, IEventHandler106Teleport, IEventHandlerElevatorUse, IEventHandlerHandcuffed , IEventHandlerPlayerTriggerTesla, IEventHandlerSCP914ChangeKnob,
        IEventHandlerRadioSwitch, IEventHandlerMakeNoise, IEventHandlerRecallZombie
    {
        private readonly SCPDiscord plugin;
        // First dimension is target player second dimension is attacking player
        private static readonly Dictionary<int,int> teamKillingMatrix = new Dictionary<int, int>
        {
            { 1, 3 },
            { 2, 4 },
            { 3, 1 },
            { 4, 2 }
        };

        public PlayerEventListener(SCPDiscord plugin)
        {
            this.plugin = plugin;
        }

        private static bool IsTeamDamage(int attackerTeam, int targetTeam)
        {
            if(attackerTeam == targetTeam)
            {
                return true;
            }
            foreach (KeyValuePair<int, int> team in teamKillingMatrix)
            {
                if (attackerTeam == team.Value && targetTeam == team.Key)
                {
                    return true;
                }
            }
            return false;
        }

        public void OnPlayerHurt(PlayerHurtEvent ev)
        {
            /// <summary>  
            /// This is called before the player is going to take damage.
            /// In case the attacker can't be passed, attacker will be null (fall damage etc)
            /// This may be broken into two events in the future
            /// </summary> 

            if (ev.Player == null || ev.Player.TeamRole.Role == Smod2.API.Role.UNASSIGNED)
            {
                return;
            }

            if (ev.Attacker == null || ev.Player.PlayerId == ev.Attacker.PlayerId)
            {
                Dictionary<string, string> noAttackerVar = new Dictionary<string, string>
                {
                    { "damage",             ev.Damage.ToString()                },
                    { "damagetype",         ev.DamageType.ToString()            },
                    { "playeripaddress",    ev.Player.IpAddress                 },
                    { "playername",         ev.Player.Name                      },
                    { "playerplayerid",     ev.Player.PlayerId.ToString()       },
                    { "playersteamid",      ev.Player.SteamId                   },
                    { "playerclass",        ev.Player.TeamRole.Role.ToString()  },
                    { "playerteam",         ev.Player.TeamRole.Team.ToString()  }
                };
                plugin.SendMessage(Config.GetArray("channels.onplayerhurt.noattacker"), "player.onplayerhurt.noattacker", noAttackerVar);
                return;
            }

            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "damage",             ev.Damage.ToString()                    },
                { "damagetype",         ev.DamageType.ToString()                },
                { "attackeripaddress",  ev.Attacker.IpAddress                   },
                { "attackername",       ev.Attacker.Name                        },
                { "attackerplayerid",   ev.Attacker.PlayerId.ToString()         },
                { "attackersteamid",    ev.Attacker.SteamId                     },
                { "attackerclass",      ev.Attacker.TeamRole.Role.ToString()    },
                { "attackerteam",       ev.Attacker.TeamRole.Team.ToString()    },
                { "playeripaddress",    ev.Player.IpAddress                     },
                { "playername",         ev.Player.Name                          },
                { "playerplayerid",     ev.Player.PlayerId.ToString()           },
                { "playersteamid",      ev.Player.SteamId                       },
                { "playerclass",        ev.Player.TeamRole.Role.ToString()      },
                { "playerteam",         ev.Player.TeamRole.Team.ToString()      }
            };

            if (IsTeamDamage((int)ev.Attacker.TeamRole.Team, (int)ev.Player.TeamRole.Team))
            {
                plugin.SendMessage(Config.GetArray("channels.onplayerhurt.friendlyfire"), "player.onplayerhurt.friendlyfire", variables);
                return;
            }

            plugin.SendMessage(Config.GetArray("channels.onplayerhurt.default"), "player.onplayerhurt.default", variables);
        }

        public void OnPlayerDie(PlayerDeathEvent ev)
        {
            /// <summary>  
            /// This is called before the player is about to die. Be sure to check if player is SCP106 (classID 3) and if so, set spawnRagdoll to false.
            /// In case the killer can't be passed, attacker will be null, so check for that before doing something.
            /// </summary> 

            if (ev.Player == null || ev.Player.TeamRole.Role == Smod2.API.Role.UNASSIGNED)
            {
                return;
            }

            if (ev.Killer == null || ev.Player.PlayerId == ev.Killer.PlayerId)
            {
                Dictionary<string, string> noKillerVar = new Dictionary<string, string>
                {
                    { "damagetype",         ev.DamageTypeVar.ToString()         },
                    { "spawnragdoll",       ev.SpawnRagdoll.ToString()          },
                    { "playeripaddress",    ev.Player.IpAddress                 },
                    { "playername",         ev.Player.Name                      },
                    { "playerplayerid",     ev.Player.PlayerId.ToString()       },
                    { "playersteamid",      ev.Player.SteamId                   },
                    { "playerclass",        ev.Player.TeamRole.Role.ToString()  },
                    { "playerteam",         ev.Player.TeamRole.Team.ToString()  }
                };
                plugin.SendMessage(Config.GetArray("channels.onplayerdie.nokiller"), "player.onplayerdie.nokiller", noKillerVar);
                return;
            }

            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "damagetype",         ev.DamageTypeVar.ToString()         },
                { "spawnragdoll",       ev.SpawnRagdoll.ToString()          },
                { "attackeripaddress",  ev.Killer.IpAddress                 },
                { "attackername",       ev.Killer.Name                      },
                { "attackerplayerid",   ev.Killer.PlayerId.ToString()       },
                { "attackersteamid",    ev.Killer.SteamId                   },
                { "attackerclass",      ev.Killer.TeamRole.Role.ToString()  },
                { "attackerteam",       ev.Killer.TeamRole.Team.ToString()  },
                { "playeripaddress",    ev.Player.IpAddress                 },
                { "playername",         ev.Player.Name                      },
                { "playerplayerid",     ev.Player.PlayerId.ToString()       },
                { "playersteamid",      ev.Player.SteamId                   },
                { "playerclass",        ev.Player.TeamRole.Role.ToString()  },
                { "playerteam",         ev.Player.TeamRole.Team.ToString()  }
            };

            if (IsTeamDamage((int)ev.Killer.TeamRole.Team, (int)ev.Player.TeamRole.Team))
            {
                plugin.SendMessage(Config.GetArray("channels.onplayerdie.friendlyfire"), "player.onplayerdie.friendlyfire", variables);
                return;
            }
            plugin.SendMessage(Config.GetArray("channels.onplayerdie.default"), "player.onplayerdie.default", variables);
        }

        public void OnPlayerPickupItem(PlayerPickupItemEvent ev)
        {
            /// <summary>  
            /// This is called when a player picks up an item.
            /// </summary> 
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "item",         ev.Item.ToString()                    },
                { "ipaddress",    ev.Player.IpAddress                   },
                { "name",         ev.Player.Name                        },
                { "playerid",     ev.Player.PlayerId.ToString()         },
                { "steamid",      ev.Player.SteamId                     },
                { "class",        ev.Player.TeamRole.Role.ToString()    },
                { "team",         ev.Player.TeamRole.Team.ToString()    }
            };
            plugin.SendMessage(Config.GetArray("channels.onplayerpickupitem"), "player.onplayerpickupitem", variables);
        }

        public void OnPlayerDropItem(PlayerDropItemEvent ev)
        {
            /// <summary>  
            /// This is called when a player drops up an item.
            /// </summary> 
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "item",         ev.Item.ToString()                    },
                { "ipaddress",    ev.Player.IpAddress                   },
                { "name",         ev.Player.Name                        },
                { "playerid",     ev.Player.PlayerId.ToString()         },
                { "steamid",      ev.Player.SteamId                     },
                { "class",        ev.Player.TeamRole.Role.ToString()    },
                { "team",         ev.Player.TeamRole.Team.ToString()    }
            };
            plugin.SendMessage(Config.GetArray("channels.onplayerdropitem"), "player.onplayerdropitem", variables);
        }

        public void OnPlayerJoin(PlayerJoinEvent ev)
        {
            /// <summary>  
            /// This is called when a player joins and is initialised.
            /// </summary> 
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "ipaddress",    ev.Player.IpAddress                   },
                { "name",         ev.Player.Name                        },
                { "playerid",     ev.Player.PlayerId.ToString()         },
                { "steamid",      ev.Player.SteamId                     },
                { "class",        ev.Player.TeamRole.Role.ToString()    },
                { "team",         ev.Player.TeamRole.Team.ToString()    }
            };
            plugin.SendMessage(Config.GetArray("channels.onplayerjoin"), "player.onplayerjoin", variables);
        }

        public void OnNicknameSet(PlayerNicknameSetEvent ev)
        {
            /// <summary>  
            /// This is called when a player attempts to set their nickname after joining. This will only be called once per game join.
            /// </summary> 
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "nickname",       ev.Nickname                         },
                { "ipaddress",      ev.Player.IpAddress                 },
                { "name",           ev.Player.Name                      },
                { "playerid",       ev.Player.PlayerId.ToString()       },
                { "steamid",        ev.Player.SteamId                   },
                { "class",          ev.Player.TeamRole.Role.ToString()  },
                { "team",           ev.Player.TeamRole.Team.ToString()  }
            };
            plugin.SendMessage(Config.GetArray("channels.onnicknameset"), "player.onnicknameset", variables);
        }

        public void OnAssignTeam(PlayerInitialAssignTeamEvent ev)
        {
            /// <summary>  
            /// Called when a team is picked for a player. Nothing is assigned to the player, but you can change what team the player will spawn as.
            /// <summary>
            if(ev.Team == Smod2.API.Team.NONE)
            {
                return;
            }

            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "ipaddress",      ev.Player.IpAddress                 },
                { "name",           ev.Player.Name                      },
                { "playerid",       ev.Player.PlayerId.ToString()       },
                { "steamid",        ev.Player.SteamId                   },
                { "class",          ev.Player.TeamRole.Role.ToString()  },
                { "team",           ev.Team.ToString()                  }
            };
            plugin.SendMessage(Config.GetArray("channels.onassignteam"), "player.onassignteam", variables);
        }

        public void OnSetRole(PlayerSetRoleEvent ev)
        {
            /// <summary>  
            /// Called after the player is set a class, at any point in the game. 
            /// <summary>  
            if(ev.Role == Smod2.API.Role.UNASSIGNED)
            {
                return;
            }
            
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "ipaddress",      ev.Player.IpAddress                 },
                { "name",           ev.Player.Name                      },
                { "playerid",       ev.Player.PlayerId.ToString()       },
                { "steamid",        ev.Player.SteamId                   },
                { "class",          ev.Player.TeamRole.Role.ToString()  },
                { "team",           ev.Player.TeamRole.Team.ToString()  }
            };
            plugin.SendMessage(Config.GetArray("channels.onsetrole"), "player.onsetrole", variables);
        }

        public void OnCheckEscape(PlayerCheckEscapeEvent ev)
        {
            /// <summary>  
            /// Called when a player is checking if they should escape (this is regardless of class)
            /// <summary>  
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "allowescape",    ev.AllowEscape.ToString()           },
                { "ipaddress",      ev.Player.IpAddress                 },
                { "name",           ev.Player.Name                      },
                { "playerid",       ev.Player.PlayerId.ToString()       },
                { "steamid",        ev.Player.SteamId                   },
                { "class",          ev.Player.TeamRole.Role.ToString()  },
                { "team",           ev.Player.TeamRole.Team.ToString()  }
            };

            if(ev.AllowEscape)
            {
                plugin.SendMessage(Config.GetArray("channels.oncheckescape.allowed"), "player.oncheckescape.allowed", variables);
            }
            else
            {
                plugin.SendMessage(Config.GetArray("channels.oncheckescape.denied"), "player.oncheckescape.denied", variables);
            }
        }

        public void OnSpawn(PlayerSpawnEvent ev)
        {
            /// <summary>  
            /// Called when a player spawns into the world
            /// <summary>  
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "spawnpos",       ev.SpawnPos.ToString()              },
                { "ipaddress",      ev.Player.IpAddress                 },
                { "name",           ev.Player.Name                      },
                { "playerid",       ev.Player.PlayerId.ToString()       },
                { "steamid",        ev.Player.SteamId                   },
                { "class",          ev.Player.TeamRole.Role.ToString()  },
                { "team",           ev.Player.TeamRole.Team.ToString()  }
            };

            plugin.SendMessage(Config.GetArray("channels.onspawn"), "player.onspawn", variables);
        }

        public void OnDoorAccess(PlayerDoorAccessEvent ev)
        {
            /// <summary>  
            /// Called when a player attempts to access a door that requires perms
            /// <summary> 
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "doorname",       ev.Door.Name                        },
                { "permission",     ev.Door.Permission                  },
                { "locked",         ev.Door.Locked.ToString()           },
                { "lockcooldown",   ev.Door.LockCooldown.ToString()     },
                { "open",           ev.Door.Open.ToString()             },
                { "ipaddress",      ev.Player.IpAddress                 },
                { "name",           ev.Player.Name                      },
                { "playerid",       ev.Player.PlayerId.ToString()       },
                { "steamid",        ev.Player.SteamId                   },
                { "class",          ev.Player.TeamRole.Role.ToString()  },
                { "team",           ev.Player.TeamRole.Team.ToString()  }
            };
            if (ev.Allow)
            {
                plugin.SendMessage(Config.GetArray("channels.ondooraccess.allowed"), "player.ondooraccess.allowed", variables);
            }
            else
            {
                plugin.SendMessage(Config.GetArray("channels.ondooraccess.denied"), "player.ondooraccess.denied", variables);
            }
        }

        public void OnIntercom(PlayerIntercomEvent ev)
        {
            /// <summary>  
            /// Called when a player attempts to use intercom.
            /// <summary>  
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "cooldowntime",   ev.CooldownTime.ToString()          },
                { "speechtime",     ev.SpeechTime.ToString()            },
                { "ipaddress",      ev.Player.IpAddress                 },
                { "name",           ev.Player.Name                      },
                { "playerid",       ev.Player.PlayerId.ToString()       },
                { "steamid",        ev.Player.SteamId                   },
                { "class",          ev.Player.TeamRole.Role.ToString()  },
                { "team",           ev.Player.TeamRole.Team.ToString()  }
            };

            plugin.SendMessage(Config.GetArray("channels.onintercom"), "player.onintercom", variables);
        }

        public void OnIntercomCooldownCheck(PlayerIntercomCooldownCheckEvent ev)
        {
            /// <summary>  
            /// Called when a player attempts to use intercom. This happens before the cooldown check.
            /// <summary>  
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "currentcooldown",    ev.CurrentCooldown.ToString()       },
                { "ipaddress",          ev.Player.IpAddress                 },
                { "name",               ev.Player.Name                      },
                { "playerid",           ev.Player.PlayerId.ToString()       },
                { "steamid",            ev.Player.SteamId                   },
                { "class",              ev.Player.TeamRole.Role.ToString()  },
                { "team",               ev.Player.TeamRole.Team.ToString()  }
            };

            plugin.SendMessage(Config.GetArray("channels.onintercomcooldowncheck"), "player.onintercomcooldowncheck", variables);
        }

        public void OnPocketDimensionExit(PlayerPocketDimensionExitEvent ev)
        {
            /// <summary>  
            /// Called when a player escapes from Pocket Demension
            /// <summary>  
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "ipaddress",          ev.Player.IpAddress                 },
                { "name",               ev.Player.Name                      },
                { "playerid",           ev.Player.PlayerId.ToString()       },
                { "steamid",            ev.Player.SteamId                   },
                { "class",              ev.Player.TeamRole.Role.ToString()  },
                { "team",               ev.Player.TeamRole.Team.ToString()  }
            };
            plugin.SendMessage(Config.GetArray("channels.onpocketdimensionexit"), "player.onpocketdimensionexit", variables);
        }

        public void OnPocketDimensionEnter(PlayerPocketDimensionEnterEvent ev)
        {
            /// <summary>  
            /// Called when a player enters Pocket Demension
            /// <summary>  
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "damage",             ev.Damage.ToString()                },
                { "ipaddress",          ev.Player.IpAddress                 },
                { "name",               ev.Player.Name                      },
                { "playerid",           ev.Player.PlayerId.ToString()       },
                { "steamid",            ev.Player.SteamId                   },
                { "class",              ev.Player.TeamRole.Role.ToString()  },
                { "team",               ev.Player.TeamRole.Team.ToString()  }
            };
            plugin.SendMessage(Config.GetArray("channels.onpocketdimensionenter"), "player.onpocketdimensionenter", variables);
        }

        public void OnPocketDimensionDie(PlayerPocketDimensionDieEvent ev)
        {
            /// <summary>  
            /// Called when a player enters the wrong way of Pocket Demension. This happens before the player is killed.
            /// <summary>  
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "ipaddress",          ev.Player.IpAddress                 },
                { "name",               ev.Player.Name                      },
                { "playerid",           ev.Player.PlayerId.ToString()       },
                { "steamid",            ev.Player.SteamId                   },
                { "class",              ev.Player.TeamRole.Role.ToString()  },
                { "team",               ev.Player.TeamRole.Team.ToString()  }
            };
            plugin.SendMessage(Config.GetArray("channels.onpocketdimensiondie"), "player.onpocketdimensiondie", variables);
        }

        public void OnThrowGrenade(PlayerThrowGrenadeEvent ev)
        {
            /// <summary>  
            /// Called after a player throws a grenade
            /// <summary>  
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "type",               ev.GrenadeType.ToString()           },
                { "ipaddress",          ev.Player.IpAddress                 },
                { "name",               ev.Player.Name                      },
                { "playerid",           ev.Player.PlayerId.ToString()       },
                { "steamid",            ev.Player.SteamId                   },
                { "class",              ev.Player.TeamRole.Role.ToString()  },
                { "team",               ev.Player.TeamRole.Team.ToString()  }
            };
            plugin.SendMessage(Config.GetArray("channels.onthrowgrenade"), "player.onthrowgrenade", variables);
        }

        public void OnPlayerInfected(PlayerInfectedEvent ev)
        {
            /// <summary>  
            /// Called when a player is cured by SCP-049
            /// <summary> 
            
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "damage",                 ev.Damage.ToString()                    },
                { "infecttime",             ev.InfectTime.ToString()                },
                { "attackeripaddress",      ev.Attacker.IpAddress                   },
                { "attackername",           ev.Attacker.Name                        },
                { "attackerplayerid",       ev.Attacker.PlayerId.ToString()         },
                { "attackersteamid",        ev.Attacker.SteamId                     },
                { "attackerclass",          ev.Attacker.TeamRole.Role.ToString()    },
                { "attackerteam",           ev.Attacker.TeamRole.Team.ToString()    },
                { "playeripaddress",        ev.Attacker.IpAddress                   },
                { "playername",             ev.Player.Name                          },
                { "playerplayerid",         ev.Player.PlayerId.ToString()           },
                { "playersteamid",          ev.Player.SteamId                       },
                { "playerclass",            ev.Player.TeamRole.Role.ToString()      },
                { "playerteam",             ev.Player.TeamRole.Team.ToString()      }
            };
            plugin.SendMessage(Config.GetArray("channels.onplayerinfected"), "player.onplayerinfected", variables);
        }

        public void OnSpawnRagdoll(PlayerSpawnRagdollEvent ev)
        {
            /// <summary>  
            /// Called when a ragdoll is spawned
            /// <summary>  
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "ipaddress",          ev.Player.IpAddress                 },
                { "name",               ev.Player.Name                      },
                { "playerid",           ev.Player.PlayerId.ToString()       },
                { "steamid",            ev.Player.SteamId                   },
                { "class",              ev.Role.ToString()                  },
                { "team",               ev.Player.TeamRole.Team.ToString()  }
            };
            plugin.SendMessage(Config.GetArray("channels.onspawnragdoll"), "player.onspawnragdoll", variables);
        }

        public void OnLure(PlayerLureEvent ev)
        {
            /// <summary>  
            /// Called when a player enters FemurBreaker
            /// <summary> 
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "allowcontain",       ev.AllowContain.ToString()          },
                { "ipaddress",          ev.Player.IpAddress                 },
                { "name",               ev.Player.Name                      },
                { "playerid",           ev.Player.PlayerId.ToString()       },
                { "steamid",            ev.Player.SteamId                   },
                { "class",              ev.Player.TeamRole.Role.ToString()  },
                { "team",               ev.Player.TeamRole.Team.ToString()  }
            };

            plugin.SendMessage(Config.GetArray("channels.onlure"), "player.onlure", variables);
        }

        public void OnContain106(PlayerContain106Event ev)
        {
            /// <summary>  
            /// Called when a player presses the button to contain SCP-106
            /// <summary>
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "activatecontainment",    ev.ActivateContainment.ToString()   },
                { "ipaddress",              ev.Player.IpAddress                 },
                { "name",                   ev.Player.Name                      },
                { "playerid",               ev.Player.PlayerId.ToString()       },
                { "steamid",                ev.Player.SteamId                   },
                { "class",                  ev.Player.TeamRole.Role.ToString()  },
                { "team",                   ev.Player.TeamRole.Team.ToString()  }
            };
            plugin.SendMessage(Config.GetArray("channels.oncontain106"), "player.oncontain106", variables);
        }

        public void OnMedkitUse(PlayerMedkitUseEvent ev)
        {
            /// <summary>  
            /// Called when a player uses Medkit
            /// <summary>

            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "recoveredhealth",        ev.RecoverHealth.ToString()         },
                { "ipaddress",              ev.Player.IpAddress                 },
                { "name",                   ev.Player.Name                      },
                { "playerid",               ev.Player.PlayerId.ToString()       },
                { "steamid",                ev.Player.SteamId                   },
                { "class",                  ev.Player.TeamRole.Role.ToString()  },
                { "team",                   ev.Player.TeamRole.Team.ToString()  }
            };
            plugin.SendMessage(Config.GetArray("channels.onmedkituse"), "player.onmedkituse", variables);
        }

        public void OnShoot(PlayerShootEvent ev)
        {
            /// <summary>  
            /// Called when a player shoots
            /// <summary>

            if (ev.Player == null)
            {
                return;
            }

            if(ev.Target == null)
            {
                Dictionary<string, string> noTargetVars = new Dictionary<string, string>
                {
                    { "weapon",                 ev.Weapon.ToString()                },
                    { "attackeripaddress",      ev.Player.IpAddress                 },
                    { "attackername",           ev.Player.Name                      },
                    { "attackerplayerid",       ev.Player.PlayerId.ToString()       },
                    { "attackersteamid",        ev.Player.SteamId                   },
                    { "attackerclass",          ev.Player.TeamRole.Role.ToString()  },
                    { "attackerteam",           ev.Player.TeamRole.Team.ToString()  }
                };
                plugin.SendMessage(Config.GetArray("channels.onshoot.notarget"), "player.onshoot.notarget", noTargetVars);
                return;
            }

            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "weapon",                 ev.Weapon.ToString()                },
                { "attackeripaddress",      ev.Player.IpAddress                 },
                { "attackername",           ev.Player.Name                      },
                { "attackerplayerid",       ev.Player.PlayerId.ToString()       },
                { "attackersteamid",        ev.Player.SteamId                   },
                { "attackerclass",          ev.Player.TeamRole.Role.ToString()  },
                { "attackerteam",           ev.Player.TeamRole.Team.ToString()  },
                { "playeripaddress",        ev.Target.IpAddress                 },
                { "playername",             ev.Target.Name                      },
                { "playerplayerid",         ev.Target.PlayerId.ToString()       },
                { "playersteamid",          ev.Target.SteamId                   },
                { "playerclass",            ev.Target.TeamRole.Role.ToString()  },
                { "playerteam",             ev.Target.TeamRole.Team.ToString()  }
            };

            if (ev.Player.SteamId != ev.Target.SteamId && IsTeamDamage((int)ev.Player.TeamRole.Team, (int)ev.Target.TeamRole.Team))
            {
                plugin.SendMessage(Config.GetArray("channels.onshoot.friendlyfire"), "player.onshoot.friendlyfire", variables);
            }

            plugin.SendMessage(Config.GetArray("channels.onshoot.default"), "player.onshoot.default", variables);
        }

        public void On106CreatePortal(Player106CreatePortalEvent ev)
        {
            /// <summary>  
            /// Called when SCP-106 creates a portal
            /// <summary>
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "ipaddress",              ev.Player.IpAddress                 },
                { "name",                   ev.Player.Name                      },
                { "playerid",               ev.Player.PlayerId.ToString()       },
                { "steamid",                ev.Player.SteamId                   },
                { "class",                  ev.Player.TeamRole.Role.ToString()  },
                { "team",                   ev.Player.TeamRole.Team.ToString()  }
            };
            plugin.SendMessage(Config.GetArray("channels.on106createportal"), "player.on106createportal", variables);
        }

        public void On106Teleport(Player106TeleportEvent ev)
        {
            /// <summary>  
            /// Called when SCP-106 teleports through portals
            /// <summary>

            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "ipaddress",              ev.Player.IpAddress                 },
                { "name",                   ev.Player.Name                      },
                { "playerid",               ev.Player.PlayerId.ToString()       },
                { "steamid",                ev.Player.SteamId                   },
                { "class",                  ev.Player.TeamRole.Role.ToString()  },
                { "team",                   ev.Player.TeamRole.Team.ToString()  }
            };
            plugin.SendMessage(Config.GetArray("channels.on106teleport"), "player.on106teleport", variables);
        }

        public void OnElevatorUse(PlayerElevatorUseEvent ev)
        {
            /// <summary>  
            /// Called when a player uses an elevator
            /// <summary>
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "elevatorname",           ev.Elevator.ElevatorType.ToString() },
                { "ipaddress",              ev.Player.IpAddress                 },
                { "name",                   ev.Player.Name                      },
                { "playerid",               ev.Player.PlayerId.ToString()       },
                { "steamid",                ev.Player.SteamId                   },
                { "class",                  ev.Player.TeamRole.Role.ToString()  },
                { "team",                   ev.Player.TeamRole.Team.ToString()  }
            };
            plugin.SendMessage(Config.GetArray("channels.onelevatoruse"), "player.onelevatoruse", variables);
        }

        public void OnHandcuffed(PlayerHandcuffedEvent ev)
        {
            /// <summary>  
            /// Called when a player handcuffs/releases another player
            /// <summary>
            if (ev.Owner != null)
            {
                Dictionary<string, string> variables = new Dictionary<string, string>
                {
                    { "cuffed",             ev.Handcuffed.ToString()                },
                    { "targetipaddress",    ev.Player.IpAddress                     },
                    { "targetname",         ev.Player.Name                          },
                    { "targetplayerid",     ev.Player.PlayerId.ToString()           },
                    { "targetsteamid",      ev.Player.SteamId                       },
                    { "targetclass",        ev.Player.TeamRole.Role.ToString()      },
                    { "targetteam",         ev.Player.TeamRole.Team.ToString()      },
                    { "playeripaddress",    ev.Owner.IpAddress                      },
                    { "playername",         ev.Owner.Name                           },
                    { "playerplayerid",     ev.Owner.PlayerId.ToString()            },
                    { "playersteamid",      ev.Owner.SteamId                        },
                    { "playerclass",        ev.Owner.TeamRole.Role.ToString()       },
                    { "playerteam",         ev.Owner.TeamRole.Team.ToString()       }
                };
                plugin.SendMessage(Config.GetArray("channels.onhandcuff.default"), "player.onhandcuff.default", variables);
            }
            else
            {
                Dictionary<string, string> variables = new Dictionary<string, string>
                {
                    { "cuffed",             ev.Handcuffed.ToString()                },
                    { "targetipaddress",    ev.Player.IpAddress                     },
                    { "targetname",         ev.Player.Name                          },
                    { "targetplayerid",     ev.Player.PlayerId.ToString()           },
                    { "targetsteamid",      ev.Player.SteamId                       },
                    { "targetclass",        ev.Player.TeamRole.Role.ToString()      },
                    { "targetteam",         ev.Player.TeamRole.Team.ToString()      }
                };
                plugin.SendMessage(Config.GetArray("channels.onhandcuff.nootherplayer"), "player.onhandcuff.nootherplayer", variables);
            }
        }

        public void OnPlayerTriggerTesla(PlayerTriggerTeslaEvent ev)
        {
            /// <summary>  
            /// Called when a player triggers a tesla gate
            /// <summary>
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "ipaddress",              ev.Player.IpAddress                 },
                { "name",                   ev.Player.Name                      },
                { "playerid",               ev.Player.PlayerId.ToString()       },
                { "steamid",                ev.Player.SteamId                   },
                { "class",                  ev.Player.TeamRole.Role.ToString()  },
                { "team",                   ev.Player.TeamRole.Team.ToString()  }
            };
            
            if(ev.Triggerable)
            {
                plugin.SendMessage(Config.GetArray("channels.onplayertriggertesla.default"), "player.onplayertriggertesla.default", variables);
            }
            else
            {
                plugin.SendMessage(Config.GetArray("channels.onplayertriggertesla.ignored"), "player.onplayertriggertesla.ignored", variables);
            }
        }

        public void OnSCP914ChangeKnob(PlayerSCP914ChangeKnobEvent ev)
        {
            /// <summary>  
            /// Called when a player changes the knob of SCP-914
            /// <summary>
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "setting",                ev.KnobSetting.ToString()           },
                { "ipaddress",              ev.Player.IpAddress                 },
                { "name",                   ev.Player.Name                      },
                { "playerid",               ev.Player.PlayerId.ToString()       },
                { "steamid",                ev.Player.SteamId                   },
                { "class",                  ev.Player.TeamRole.Role.ToString()  },
                { "team",                   ev.Player.TeamRole.Team.ToString()  }
            };
            plugin.SendMessage(Config.GetArray("channels.onscp914changeknob"), "player.onscp914changeknob", variables);
        }

        public void OnPlayerRadioSwitch(PlayerRadioSwitchEvent ev)
        {
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "setting",                ev.ChangeTo.ToString()              },
                { "ipaddress",              ev.Player.IpAddress                 },
                { "name",                   ev.Player.Name                      },
                { "playerid",               ev.Player.PlayerId.ToString()       },
                { "steamid",                ev.Player.SteamId                   },
                { "class",                  ev.Player.TeamRole.Role.ToString()  },
                { "team",                   ev.Player.TeamRole.Team.ToString()  }
            };
            plugin.SendMessage(Config.GetArray("channels.onplayerradioswitch"), "player.onplayerradioswitch", variables);
        }

        public void OnMakeNoise(PlayerMakeNoiseEvent ev)
        {
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "ipaddress",              ev.Player.IpAddress                 },
                { "name",                   ev.Player.Name                      },
                { "playerid",               ev.Player.PlayerId.ToString()       },
                { "steamid",                ev.Player.SteamId                   },
                { "class",                  ev.Player.TeamRole.Role.ToString()  },
                { "team",                   ev.Player.TeamRole.Team.ToString()  }
            };
            plugin.SendMessage(Config.GetArray("channels.onmakenoise"), "player.onmakenoise", variables);
        }

        public void OnRecallZombie(PlayerRecallZombieEvent ev)
        {
            Dictionary<string, string> variables = new Dictionary<string, string>
            {
                { "allowrecall",        ev.AllowRecall.ToString()               },
                { "playeripaddress",    ev.Player.IpAddress                     },
                { "playername",         ev.Player.Name                          },
                { "playerplayerid",     ev.Player.PlayerId.ToString()           },
                { "playersteamid",      ev.Player.SteamId                       },
                { "playerclass",        ev.Player.TeamRole.Role.ToString()      },
                { "playerteam",         ev.Player.TeamRole.Team.ToString()      },
                { "targetipaddress",    ev.Target.IpAddress                     },
                { "targetname",         ev.Target.Name                          },
                { "targetplayerid",     ev.Target.PlayerId.ToString()           },
                { "targetsteamid",      ev.Target.SteamId                       },
                { "targetclass",        ev.Target.TeamRole.Role.ToString()      },
                { "targetteam",         ev.Target.TeamRole.Team.ToString()      },
            };
            plugin.SendMessage(Config.GetArray("channels.onrecallzombie"), "player.onrecallzombie", variables);
        }
    }
}
