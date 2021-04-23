
require "GameLogic.BehavitorTree.Base.BehavitorTree"

--配置所有的fsm在此
BehavitorTree.FSM["BTFSMSayTest"] = require "GameLogic.BehavitorTree.BTFSMSayTest"
BehavitorTree.FSM["BTFSMIdleTest"] = require "GameLogic.BehavitorTree.BTFSMIdleTest"
BehavitorTree.FSM["BTFSMMoveTest"] = require "GameLogic.BehavitorTree.BTFSMMoveTest"




BehavitorTree.CFG["BT_1001"] = require "GameLogic.BehavitorTree.BT_1001"